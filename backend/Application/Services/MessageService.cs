using Application.Utils;
using AutoMapper;
using Core.Entities;
using Core.Enums;
using Core.Interfaces.Services;
using Core.Models;
using Core.Models.BroadcastModel;
using Core.Models.MessageModel;
using Core.Results;
using DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class MessageService : IMessageService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly INotificationService _notification;

        public MessageService(ApplicationDbContext context, IMapper mapper, INotificationService notification)
        {
            _context = context;
            _mapper = mapper;
            _notification = notification;
        }

        // Сообщения

        public async Task<Result<List<DialogPreviewResponse>>> GetDialogListAsync(long userId)
        {
            // Все сообщения где пользователь — отправитель или получатель (личные, не рассылки)
            var msgs = await _context.Messages
                .Where(m => m.BroadcastId == null && (m.SenderId == userId || m.ReceiverId == userId))
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            // Уникальные собеседники: для каждого берём последнее сообщение + считаем непрочитанные
            var partnersMap = new Dictionary<long, Message>();
            var unreadCounts = new Dictionary<long, int>();
            foreach (var msg in msgs)
            {
                var partnerId = msg.SenderId == userId ? msg.ReceiverId : msg.SenderId;
                if (!partnersMap.ContainsKey(partnerId))
                    partnersMap[partnerId] = msg;
                if (msg.ReceiverId == userId && !msg.IsRead)
                    unreadCounts[partnerId] = (unreadCounts.GetValueOrDefault(partnerId)) + 1;
            }

            if (partnersMap.Count == 0)
                return Result<List<DialogPreviewResponse>>.Success(new List<DialogPreviewResponse>());

            var partnerIds = partnersMap.Keys.ToList();
            var users = await _context.Users
                .Where(u => partnerIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var sportsmenFio = await _context.Sportsmen
                .Where(s => partnerIds.Contains(s.UserId))
                .ToDictionaryAsync(s => s.UserId, s => s.FIO);

            var personalFio = await _context.Personal
                .Where(p => partnerIds.Contains(p.UserId))
                .ToDictionaryAsync(p => p.UserId, p => p.FIO);

            var result = partnersMap.Select(kvp =>
            {
                var msg = kvp.Value;
                var partnerId = kvp.Key;
                users.TryGetValue(partnerId, out var user);
                var fio = sportsmenFio.ContainsKey(partnerId) ? sportsmenFio[partnerId]
                        : personalFio.ContainsKey(partnerId) ? personalFio[partnerId]
                        : user?.Login ?? string.Empty;
                return new DialogPreviewResponse
                {
                    UserId = partnerId,
                    UserName = fio,
                    UserRole = user?.Role ?? string.Empty,
                    LastMessage = msg.Text.Length > 80 ? msg.Text[..80] : msg.Text,
                    LastMessageAt = msg.CreatedAt,
                    HasUnread = unreadCounts.GetValueOrDefault(partnerId) > 0,
                    UnreadCount = unreadCounts.GetValueOrDefault(partnerId),
                };
            })
            .OrderByDescending(d => d.LastMessageAt)
            .ToList();

            return Result<List<DialogPreviewResponse>>.Success(result);
        }

        public async Task<Result<MessageResponse>> SendAsync(long senderId, SenderRole senderRole, MessageSendRequest req)
        {
            var receiverExists = await _context.Users.AnyAsync(u => u.Id == req.ReceiverId);
            if (!receiverExists)
                return Result<MessageResponse>.Failure("Получатель не найден", 404);

            var message = new Message
            {
                SenderId = senderId,
                SenderRole = senderRole,
                ReceiverId = req.ReceiverId,
                Text = req.Text
            };

            await _context.Messages.AddAsync(message);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<MessageResponse>(message);

            // Пушим сообщение получателю
            await _notification.NotifyUserAsync(req.ReceiverId, "ReceiveMessage", response);

            return Result<MessageResponse>.Success(response);
        }

        public async Task<Result<DialogResponse>> GetDialogAsync(long userId, long withUserId, Filter? filter)
        {
            var messages = await _context.Messages
                .Where(m =>
                    (m.SenderId == userId && m.ReceiverId == withUserId) ||
                    (m.SenderId == withUserId && m.ReceiverId == userId))
                .ApplyFilter(filter)
                .ToListAsync();

            return Result<DialogResponse>.Success(new DialogResponse
            {
                WithUserId = withUserId,
                Messages = _mapper.Map<List<MessageResponse>>(messages)
            });
        }

        public async Task<Result<bool>> MarkReadAsync(long messageId, long userId)
        {
            var message = await _context.Messages.FirstOrDefaultAsync(m => m.Id == messageId);
            if (message == null)
                return Result<bool>.Failure("Сообщение не найдено", 404);

            if (message.ReceiverId != userId)
                return Result<bool>.Failure("Нет доступа", 403);

            message.IsRead = true;
            await _context.SaveChangesAsync();

            // Уведомляем отправителя что сообщение прочитано
            await _notification.NotifyUserAsync(message.SenderId, "MessageRead", messageId);

            return Result<bool>.Success(true);
        }

        // Пометить сообщение от рассылки прочитанным для текущего пользователя
        public async Task<Result<bool>> MarkBroadcastReadAsync(long broadcastId, long userId)
        {
            var msg = await _context.Messages
                .FirstOrDefaultAsync(m => m.BroadcastId == broadcastId && m.ReceiverId == userId);
            if (msg == null) return Result<bool>.Failure("Рассылка не найдена для этого пользователя", 404);
            if (msg.IsRead) return Result<bool>.Success(true);
            msg.IsRead = true;
            await _context.SaveChangesAsync();
            // Уведомляем автора рассылки что один получатель прочитал
            await _notification.NotifyUserAsync(msg.SenderId, "BroadcastRead", new { broadcastId, userId });
            return Result<bool>.Success(true);
        }

        public async Task<Result<int>> GetTotalCountAsync()
        {
            var messages = await _context.Messages.CountAsync();
            var broadcasts = await _context.Broadcasts.CountAsync();
            return Result<int>.Success(messages + broadcasts);
        }

        // Массовая рассылка сообщений

        public async Task<Result<BroadcastResponse>> SendBroadcastAsync(long createdById, SenderRole createdByRole, BroadcastCreateRequest req)
        {
            // Защита от дублей: одна рассылка с тем же Title от того же автора за последние 10 секунд
            var recentDuplicate = await _context.Broadcasts.AnyAsync(b =>
                b.CreatedById == createdById &&
                b.Title == req.Title &&
                b.CreatedAt >= DateTime.UtcNow.AddSeconds(-10));

            if (recentDuplicate)
                return Result<BroadcastResponse>.Failure("Такая рассылка уже была отправлена", 409);

            var broadcast = new Broadcast
            {
                Title = req.Title,
                Text = req.Text,
                CreatedById = createdById,
                CreatedByRole = createdByRole,
                TargetType = req.TargetType,
                TargetId = req.TargetId,
                ExpireAt = req.ExpireAt
            };

            await _context.Broadcasts.AddAsync(broadcast);
            await _context.SaveChangesAsync();

            var recipientIds = await ResolveRecipientsAsync(req.TargetType, req.TargetId, createdById);

            // Пакетная вставка всех сообщений за один раз
            var messages = recipientIds.Select(receiverId => new Message
            {
                SenderId = createdById,
                SenderRole = createdByRole,
                ReceiverId = receiverId,
                Text = req.Text,   // снимок текста на момент рассылки
                BroadcastId = broadcast.Id
            }).ToList();

            await _context.Messages.AddRangeAsync(messages);
            await _context.SaveChangesAsync();

            var response = _mapper.Map<BroadcastResponse>(broadcast);
            response.RecipientsCount = messages.Count;

            // Пушим уведомление каждому получателю
            var notification = new { broadcastId = broadcast.Id, title = broadcast.Title, text = broadcast.Text };
            foreach (var recipientId in recipientIds)
                await _notification.NotifyUserAsync(recipientId, "ReceiveBroadcast", notification);

            return Result<BroadcastResponse>.Success(response);
        }

        public async Task<Result<List<BroadcastResponse>>> GetBroadcastsAsync(Filter? filter, long? forUserId = null)
        {
            IQueryable<Broadcast> query = _context.Broadcasts;

            // Если forUserId задан — возвращаем только рассылки где этот юзер является получателем.
            // Используется в колокольчике уведомлений, чтобы админ не видел свои же отправленные рассылки
            // как «непрочитанные входящие». Без этого параметра — возвращаем все (для админ-страницы).
            if (forUserId.HasValue)
            {
                var userBroadcastIds = await _context.Messages
                    .Where(m => m.BroadcastId != null && m.ReceiverId == forUserId.Value)
                    .Select(m => m.BroadcastId!.Value)
                    .Distinct()
                    .ToListAsync();
                query = query.Where(b => userBroadcastIds.Contains(b.Id));
            }

            var broadcasts = await query.ApplyFilter(filter).ToListAsync();

            var broadcastIds = broadcasts.Select(b => b.Id).ToList();

            var counts = await _context.Messages
                .Where(m => m.BroadcastId != null && broadcastIds.Contains(m.BroadcastId!.Value))
                .GroupBy(m => m.BroadcastId!.Value)
                .Select(g => new { BroadcastId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BroadcastId, x => x.Count);

            // Подтягиваем ФИО создателей рассылок (Sportsman / Personal / fallback User.Login)
            var creatorIds = broadcasts.Select(b => b.CreatedById).Distinct().ToList();
            var creatorNames = await BuildUserNamesAsync(creatorIds);

            // Если запрос от лица юзера — подтягиваем индивидуальный статус IsRead.
            Dictionary<long, bool> readStatus = new();
            if (forUserId.HasValue)
            {
                readStatus = await _context.Messages
                    .Where(m => m.BroadcastId != null && broadcastIds.Contains(m.BroadcastId!.Value) && m.ReceiverId == forUserId.Value)
                    .Select(m => new { BroadcastId = m.BroadcastId!.Value, m.IsRead })
                    .ToDictionaryAsync(x => x.BroadcastId, x => x.IsRead);
            }

            var result = broadcasts.Select(b =>
            {
                var r = _mapper.Map<BroadcastResponse>(b);
                r.RecipientsCount = counts.GetValueOrDefault(b.Id, 0);
                r.CreatedByName = creatorNames.GetValueOrDefault(b.CreatedById);
                r.IsReadByMe = forUserId.HasValue ? readStatus.GetValueOrDefault(b.Id, false) : (bool?)null;
                return r;
            }).ToList();

            return Result<List<BroadcastResponse>>.Success(result);
        }

        // Возвращает мапу userId → отображаемое имя (FIO из Sportsman/Personal, иначе Login)
        private async Task<Dictionary<long, string>> BuildUserNamesAsync(List<long> userIds)
        {
            if (userIds.Count == 0) return new Dictionary<long, string>();
            var users = await _context.Users.Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Login }).ToListAsync();
            var sportsmen = await _context.Sportsmen.Where(s => userIds.Contains(s.UserId))
                .Select(s => new { s.UserId, s.FIO }).ToListAsync();
            var personals = await _context.Personal.Where(p => userIds.Contains(p.UserId))
                .Select(p => new { p.UserId, p.FIO }).ToListAsync();
            var map = new Dictionary<long, string>();
            foreach (var u in users) map[u.Id] = u.Login;
            foreach (var s in sportsmen) map[s.UserId] = s.FIO;
            foreach (var p in personals) map[p.UserId] = p.FIO;
            return map;
        }

        public async Task<Result<BroadcastDetailsResponse>> GetBroadcastDetailsAsync(long broadcastId, Filter? filter)
        {
            var broadcast = await _context.Broadcasts.FirstOrDefaultAsync(b => b.Id == broadcastId);
            if (broadcast == null)
                return Result<BroadcastDetailsResponse>.Failure("Рассылка не найдена", 404);

            // EF Core 9 не транслирует GroupJoin → FirstOrDefault() в SQL.
            // Поэтому делаем простой Join к Users + подзапросы для ФИО через коррелированные SELECT.
            var recipients = await _context.Messages
                .Where(m => m.BroadcastId == broadcastId)
                .ApplyFilter(filter)
                .Join(_context.Users,
                    m => m.ReceiverId,
                    u => u.Id,
                    (m, u) => new BroadcastRecipientDto
                    {
                        UserId = m.ReceiverId,
                        UserName =
                            _context.Sportsmen.Where(s => s.UserId == u.Id).Select(s => s.FIO).FirstOrDefault()
                            ?? _context.Personal.Where(p => p.UserId == u.Id).Select(p => p.FIO).FirstOrDefault()
                            ?? u.Login,
                        IsRead = m.IsRead,
                    })
                .ToListAsync();

            var creatorNames = await BuildUserNamesAsync(new List<long> { broadcast.CreatedById });
            var response = new BroadcastDetailsResponse
            {
                Id = broadcast.Id,
                Title = broadcast.Title,
                Text = broadcast.Text,
                CreatedById = broadcast.CreatedById,
                CreatedByName = creatorNames.GetValueOrDefault(broadcast.CreatedById),
                CreatedAt = broadcast.CreatedAt,
                ExpireAt = broadcast.ExpireAt,
                Recipients = recipients
            };

            return Result<BroadcastDetailsResponse>.Success(response);
        }

        public async Task<Result<bool>> DeleteBroadcastAsync(long broadcastId)
        {
            var broadcast = await _context.Broadcasts.FirstOrDefaultAsync(b => b.Id == broadcastId);
            if (broadcast == null)
                return Result<bool>.Failure("Рассылка не найдена", 404);

            var messages = await _context.Messages.Where(m => m.BroadcastId == broadcastId).ToListAsync();
            _context.Messages.RemoveRange(messages);
            _context.Broadcasts.Remove(broadcast);
            await _context.SaveChangesAsync();

            return Result<bool>.Success(true);
        }

        // Вспомогательные методы

        private async Task<List<long>> ResolveRecipientsAsync(BroadcastTargetType targetType, long? targetId, long senderId)
        {
            IQueryable<long> query = targetType switch
            {
                BroadcastTargetType.Team =>
                    _context.Sportsmen
                        .Where(s => s.TeamId == targetId)
                        .Select(s => s.UserId),

                BroadcastTargetType.Group =>
                    _context.SportsmanGroups
                        .Where(sg => sg.GroupId == targetId)
                        .Select(sg => sg.Sportsman.UserId),

                BroadcastTargetType.Individual =>
                    _context.Users
                        .Where(u => u.Id == targetId)
                        .Select(u => u.Id),

                // Personal — все сотрудники (trainer + medical), не сами спортсмены
                BroadcastTargetType.Personal =>
                    _context.Users
                        .Where(u => u.Role == "personal")
                        .Select(u => u.Id),

                // Trainers — только тренеры (Personal.Type = Trainer)
                BroadcastTargetType.Trainers =>
                    _context.Personal
                        .Where(p => p.Type == Core.Enums.PersonalType.Trainer)
                        .Select(p => p.UserId),

                // Medical — только мед. персонал (Personal.Type = Medical)
                BroadcastTargetType.Medical =>
                    _context.Personal
                        .Where(p => p.Type == Core.Enums.PersonalType.Medical)
                        .Select(p => p.UserId),

                // All — только спортсмены
                _ => _context.Sportsmen.Select(s => s.UserId)
            };

            return await query
                .Where(id => id != senderId)
                .Distinct()
                .ToListAsync();
        }
    }
}
