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

        public async Task<Result<List<BroadcastResponse>>> GetBroadcastsAsync(Filter? filter)
        {
            var broadcasts = await _context.Broadcasts
                .ApplyFilter(filter)
                .ToListAsync();

            var broadcastIds = broadcasts.Select(b => b.Id).ToList();

            var counts = await _context.Messages
                .Where(m => m.BroadcastId != null && broadcastIds.Contains(m.BroadcastId!.Value))
                .GroupBy(m => m.BroadcastId!.Value)
                .Select(g => new { BroadcastId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.BroadcastId, x => x.Count);

            var result = broadcasts.Select(b =>
            {
                var r = _mapper.Map<BroadcastResponse>(b);
                r.RecipientsCount = counts.GetValueOrDefault(b.Id, 0);
                return r;
            }).ToList();

            return Result<List<BroadcastResponse>>.Success(result);
        }

        public async Task<Result<BroadcastDetailsResponse>> GetBroadcastDetailsAsync(long broadcastId, Filter? filter)
        {
            var broadcast = await _context.Broadcasts.FirstOrDefaultAsync(b => b.Id == broadcastId);
            if (broadcast == null)
                return Result<BroadcastDetailsResponse>.Failure("Рассылка не найдена", 404);

            var recipients = await _context.Messages
                .Where(m => m.BroadcastId == broadcastId)
                .ApplyFilter(filter)
                .Join(_context.Users,
                    m => m.ReceiverId,
                    u => u.Id,
                    (m, u) => new { m, u })
                .GroupJoin(_context.Sportsmen,
                    x => x.u.Id,
                    s => s.UserId,
                    (x, sportsmen) => new { x.m, x.u, Sportsman = sportsmen.FirstOrDefault() })
                .GroupJoin(_context.Personal,
                    x => x.u.Id,
                    p => p.UserId,
                    (x, personals) => new { x.m, x.u, x.Sportsman, Personal = personals.FirstOrDefault() })
                .Select(x => new BroadcastRecipientDto
                {
                    UserId = x.m.ReceiverId,
                    UserName = x.Sportsman != null ? x.Sportsman.FIO : x.Personal != null ? x.Personal.FIO : x.u.Login,
                    IsRead = x.m.IsRead
                })
                .ToListAsync();

            var response = new BroadcastDetailsResponse
            {
                Id = broadcast.Id,
                Title = broadcast.Title,
                Text = broadcast.Text,
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
