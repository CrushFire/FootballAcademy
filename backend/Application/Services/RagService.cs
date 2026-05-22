using Core.Entities;
using Core.Enums;
using Core.Enums.Match;
using Core.Interfaces.Services;
using Core.Models.RagModel;
using Core.Results;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Embeddings;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace Application.Services
{
    public class RagService : IRagService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly ArticleKnowledgeService _articles;

        private string OpenAiKey => _config["OpenAI:ApiKey"] ?? "";

        public RagService(ApplicationDbContext context, IConfiguration config, ArticleKnowledgeService articles)
        {
            _context = context;
            _config = config;
            _articles = articles;
        }

        // ─────────────────────────────────────────────────────
        // ЧАТЫ
        // ─────────────────────────────────────────────────────

        public async Task<Result<List<AiChatResponse>>> GetChatsAsync(long trainerId)
        {
            var chats = await _context.AiChats
                .Where(c => c.TrainerId == trainerId)
                .Include(c => c.Messages)
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();

            var result = chats.Select(c =>
            {
                var title = c.Title;
                if (title == "Новый чат")
                {
                    var firstUser = c.Messages.OrderBy(m => m.CreatedAt).FirstOrDefault(m => m.Role == "user");
                    if (firstUser != null)
                        title = firstUser.Content.Length > 50 ? firstUser.Content[..50] + "…" : firstUser.Content;
                }
                return new AiChatResponse { Id = c.Id, Title = title, CreatedAt = c.CreatedAt, UpdatedAt = c.UpdatedAt };
            }).ToList();

            return Result<List<AiChatResponse>>.Success(result);
        }

        public async Task<Result<AiChatDetailResponse>> GetChatAsync(long chatId, long trainerId)
        {
            var chat = await _context.AiChats
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == chatId && c.TrainerId == trainerId);

            if (chat == null)
                return Result<AiChatDetailResponse>.Failure("Чат не найден", 404);

            return Result<AiChatDetailResponse>.Success(new AiChatDetailResponse
            {
                Id = chat.Id,
                Title = chat.Title,
                CreatedAt = chat.CreatedAt,
                Messages = chat.Messages.Select(m => new AiMessageResponse
                {
                    Id = m.Id,
                    Role = m.Role,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt,
                }).ToList(),
            });
        }

        public async Task<Result<AiChatResponse>> CreateChatAsync(long trainerId)
        {
            var chat = new AiChat
            {
                TrainerId = trainerId,
                Title = "Новый чат",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };
            _context.AiChats.Add(chat);
            await _context.SaveChangesAsync();

            return Result<AiChatResponse>.Success(new AiChatResponse
            {
                Id = chat.Id,
                Title = chat.Title,
                CreatedAt = chat.CreatedAt,
                UpdatedAt = chat.UpdatedAt,
            });
        }

        public async Task<Result<bool>> DeleteChatAsync(long chatId, long trainerId)
        {
            var chat = await _context.AiChats
                .FirstOrDefaultAsync(c => c.Id == chatId && c.TrainerId == trainerId);

            if (chat == null)
                return Result<bool>.Failure("Чат не найден", 404);

            _context.AiChats.Remove(chat);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        // ─────────────────────────────────────────────────────
        // СООБЩЕНИЯ
        // ─────────────────────────────────────────────────────

        public async Task<Result<AiMessageResponse>> SendMessageAsync(long chatId, long trainerId, string message)
        {
            if (string.IsNullOrEmpty(OpenAiKey))
                return Result<AiMessageResponse>.Failure("OpenAI API key не настроен", 500);

            var chat = await _context.AiChats
                .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .FirstOrDefaultAsync(c => c.Id == chatId && c.TrainerId == trainerId);

            if (chat == null)
                return Result<AiMessageResponse>.Failure("Чат не найден", 404);

            // Сохраняем сообщение пользователя
            var userMsg = new AiMessage
            {
                ChatId = chatId,
                Role = "user",
                Content = message,
                CreatedAt = DateTime.UtcNow,
            };
            _context.AiMessages.Add(userMsg);

            var isFirstMessage = chat.Messages.Count == 0 && chat.Title == "Новый чат";

            await _context.SaveChangesAsync();

            // Строим историю для GPT (последние 20 сообщений)
            var history = chat.Messages
                .TakeLast(20)
                .Select(m => m.Role == "user"
                    ? (OpenAI.Chat.ChatMessage)new UserChatMessage(m.Content)
                    : new AssistantChatMessage(m.Content))
                .ToList();

            // RAG: умный поиск — сначала по ключевым словам, затем векторный fallback
            var ragContext = await BuildRagContextAsync(message, trainerId);

            var systemPrompt = BuildSystemPrompt(ragContext);

            var allMessages = new List<OpenAI.Chat.ChatMessage> { new SystemChatMessage(systemPrompt) };
            allMessages.AddRange(history);
            allMessages.Add(new UserChatMessage(message));

            // Отправить в GPT
            var client = new OpenAIClient(OpenAiKey);
            var chatClient = client.GetChatClient("gpt-4.1-mini");
            var completion = await chatClient.CompleteChatAsync(allMessages);
            var answer = completion.Value.Content[0].Text;

            // Сохраняем ответ AI
            var aiMsg = new AiMessage
            {
                ChatId = chatId,
                Role = "assistant",
                Content = answer,
                CreatedAt = DateTime.UtcNow,
            };
            _context.AiMessages.Add(aiMsg);

            chat.UpdatedAt = DateTime.UtcNow;

            if (isFirstMessage)
                chat.Title = await GenerateChatTitleAsync(client, message, answer);

            await _context.SaveChangesAsync();

            return Result<AiMessageResponse>.Success(new AiMessageResponse
            {
                Id = aiMsg.Id,
                Role = aiMsg.Role,
                Content = aiMsg.Content,
                CreatedAt = aiMsg.CreatedAt,
            });
        }

        // СИСТЕМНЫЙ ПРОМПТ

        private async Task<string> GenerateChatTitleAsync(OpenAIClient client, string userMessage, string assistantAnswer)
        {
            try
            {
                var chatClient = client.GetChatClient("gpt-4.1-mini");
                var prompt = $"Придумай короткое название для чата (3-6 слов, без кавычек) на основе этого диалога:\nПользователь: {userMessage}\nАссистент: {assistantAnswer[..Math.Min(200, assistantAnswer.Length)]}";
                var result = await chatClient.CompleteChatAsync(new List<OpenAI.Chat.ChatMessage> { new UserChatMessage(prompt) });
                var title = result.Value.Content[0].Text.Trim().Trim('"').Trim('«', '»');
                return title.Length > 60 ? title[..60] + "…" : title;
            }
            catch
            {
                return userMessage.Length > 50 ? userMessage[..50] + "…" : userMessage;
            }
        }

        private async Task<string> BuildRagContextAsync(string message, long trainerId)
        {
            try
            {
                var msg = message.ToLowerInvariant();
                var msgWords = msg.Split(new[] { ' ', ',', '.', '?', '!', ':', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 3).ToList();

                var sb = new StringBuilder();

                // ── 1. Спортсмены по имени ───────────────────────────────────────
                var allEmbeddings = await _context.PlayerEmbeddings.ToListAsync();
                if (allEmbeddings.Count > 0)
                {
                    var byName = msgWords.Count > 0
                        ? allEmbeddings.Where(e => msgWords.Any(w => e.SourceText.ToLowerInvariant().Contains(w))).ToList()
                        : new List<PlayerEmbedding>();

                    if (byName.Count > 0)
                    {
                        sb.AppendLine("=== СПОРТСМЕНЫ (по имени) ===");
                        foreach (var e in byName) sb.AppendLine(e.SourceText);
                        sb.AppendLine("=== КОНЕЦ ===");
                    }
                    else
                    {
                        // ── 2. По команде ────────────────────────────────────────
                        var allTeams = await _context.Teams.ToListAsync();
                        var matchedTeam = allTeams.FirstOrDefault(t =>
                            msg.Contains(t.Name.ToLowerInvariant()) ||
                            t.Name.ToLowerInvariant().Split(new[] { '-', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                .Any(w => w.Length > 2 && msg.Contains(w)));

                        if (matchedTeam != null)
                        {
                            var ids = await _context.Sportsmen.Where(s => s.TeamId == matchedTeam.Id).Select(s => s.Id).ToListAsync();
                            var byTeam = allEmbeddings.Where(e => e.SportsmanId.HasValue && ids.Contains(e.SportsmanId.Value)).ToList();
                            if (byTeam.Count > 0)
                            {
                                sb.AppendLine($"=== СПОРТСМЕНЫ (команда «{matchedTeam.Name}») ===");
                                foreach (var e in byTeam) sb.AppendLine(e.SourceText);
                                sb.AppendLine("=== КОНЕЦ ===");
                            }
                        }

                        // ── 3. По группе ─────────────────────────────────────────
                        var allGroups = await _context.Groups.ToListAsync();
                        var matchedGroup = allGroups.FirstOrDefault(g =>
                            msg.Contains(g.Name.ToLowerInvariant()) ||
                            g.Name.ToLowerInvariant().Split(new[] { '-', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                .Any(w => w.Length > 2 && msg.Contains(w)));

                        if (matchedGroup != null)
                        {
                            var ids = await _context.SportsmanGroups.Where(sg => sg.GroupId == matchedGroup.Id).Select(sg => sg.SportsmanId).ToListAsync();
                            var byGroup = allEmbeddings.Where(e => e.SportsmanId.HasValue && ids.Contains(e.SportsmanId.Value)).ToList();
                            if (byGroup.Count > 0)
                            {
                                sb.AppendLine($"=== СПОРТСМЕНЫ (группа «{matchedGroup.Name}») ===");
                                foreach (var e in byGroup) sb.AppendLine(e.SourceText);
                                sb.AppendLine("=== КОНЕЦ ===");
                            }
                        }

                        // ── 4. Векторный fallback ────────────────────────────────
                        if (sb.Length == 0)
                        {
                            var queryVec = await GetEmbeddingAsync(message);
                            var pgVec = new Vector(queryVec);
                            var byVector = await _context.PlayerEmbeddings
                                .OrderBy(e => e.Embedding.CosineDistance(pgVec))
                                .Take(5)
                                .ToListAsync();
                            if (byVector.Count > 0)
                            {
                                sb.AppendLine("=== СПОРТСМЕНЫ (по смыслу запроса) ===");
                                foreach (var e in byVector) sb.AppendLine(e.SourceText);
                                sb.AppendLine("=== КОНЕЦ ===");
                            }
                        }
                    }
                }

                // ── 5. Матчи ─────────────────────────────────────────────────────
                var matchKeywords = new[] { "матч", "игра", "результат", "победа", "поражение", "ничья", "соревнован", "турнир", "кубок", "лига", "счёт", "состав", "lineup" };
                if (matchKeywords.Any(k => msg.Contains(k)) || msgWords.Any(w => matchKeywords.Any(k => k.Contains(w))))
                {
                    var matches = await _context.Matches
                        .Include(m => m.HomeTeam)
                        .OrderByDescending(m => m.Date)
                        .Take(20)
                        .ToListAsync();
                    if (matches.Count > 0)
                    {
                        sb.AppendLine("=== МАТЧИ ===");
                        foreach (var m in matches)
                        {
                            var status = m.Status == MatchStatus.Finished ? $"Результат: {m.Result}" : $"Статус: {m.Status}";
                            sb.AppendLine($"{m.Date:dd.MM.yyyy} | {m.HomeTeam?.Name ?? "?"} vs {m.OpponentTeamName} | {m.Type} | {status}{(string.IsNullOrEmpty(m.TrainerComment) ? "" : " | " + m.TrainerComment)}");
                        }
                        sb.AppendLine("=== КОНЕЦ ===");
                    }
                }

                // ── 6. Тренировки / расписание ───────────────────────────────────
                var trainKeywords = new[] { "тренировк", "занятие", "расписани", "план", "нагрузк", "упражнени" };
                if (trainKeywords.Any(k => msg.Contains(k)))
                {
                    var trainings = await _context.Trainings
                        .Include(t => t.Group)
                        .OrderByDescending(t => t.Date)
                        .Take(15)
                        .ToListAsync();
                    if (trainings.Count > 0)
                    {
                        sb.AppendLine("=== ТРЕНИРОВКИ ===");
                        foreach (var t in trainings)
                            sb.AppendLine($"{t.Date:dd.MM.yyyy HH:mm} | Группа: {t.Group?.Name ?? "?"} | Тип: {t.Type}{(string.IsNullOrEmpty(t.OtherInformation) ? "" : " | " + t.OtherInformation)}");
                        sb.AppendLine("=== КОНЕЦ ===");
                    }
                }

                // ── 7. Команды / группы (общая сводка) ──────────────────────────
                var structKeywords = new[] { "команда", "группа", "состав", "список", "все игрок", "все спортсмен", "академи" };
                if (structKeywords.Any(k => msg.Contains(k)) && sb.Length == 0)
                {
                    var teams = await _context.Teams.Include(t => t.Sportsmen).ToListAsync();
                    if (teams.Count > 0)
                    {
                        sb.AppendLine("=== КОМАНДЫ ===");
                        foreach (var t in teams)
                            sb.AppendLine($"{t.Name} ({t.AgeGroup}) — {t.Sportsmen.Count} игроков");
                        sb.AppendLine("=== КОНЕЦ ===");
                    }

                    var groups = await _context.Groups
                        .Include(g => g.SportsmanGroups)
                        .Include(g => g.Trainer)
                        .ToListAsync();
                    if (groups.Count > 0)
                    {
                        sb.AppendLine("=== ГРУППЫ ===");
                        foreach (var g in groups)
                            sb.AppendLine($"{g.Name} — тренер: {g.Trainer?.FIO ?? "?"}, игроков: {g.SportsmanGroups.Count}");
                        sb.AppendLine("=== КОНЕЦ ===");
                    }
                }

                // ── 8. Статьи из базы знаний ────────────────────────────────────
                if (_articles.HasArticles)
                {
                    var articleChunks = await _articles.SearchAsync(message, topN: 3);
                    if (articleChunks.Count > 0)
                    {
                        sb.AppendLine("=== СПРАВОЧНЫЕ МАТЕРИАЛЫ ===");
                        foreach (var chunk in articleChunks)
                            sb.AppendLine($"[{chunk.Title}]\n{chunk.Text}");
                        sb.AppendLine("=== КОНЕЦ ===");
                    }
                }

                return sb.ToString();
            }
            catch
            {
                return "";
            }
        }

        private async Task<string> VectorFallbackAsync(string message)
        {
            try
            {
                var queryVec = await GetEmbeddingAsync(message);
                var pgVec = new Vector(queryVec);
                var similar = await _context.PlayerEmbeddings
                    .OrderBy(e => e.Embedding.CosineDistance(pgVec))
                    .Take(5)
                    .ToListAsync();
                return similar.Count > 0 ? FormatRagContext(similar, "похожие по запросу") : "";
            }
            catch { return ""; }
        }

        private static string FormatRagContext(List<PlayerEmbedding> embeddings, string label)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== ДАННЫЕ ИЗ БАЗЫ ({label}, {embeddings.Count} игроков) ===");
            foreach (var e in embeddings)
                sb.AppendLine(e.SourceText);
            sb.AppendLine("=== КОНЕЦ ДАННЫХ ===");
            return sb.ToString();
        }

        private static string BuildSystemPrompt(string ragContext = "")
        {
            var context = string.IsNullOrEmpty(ragContext) ? "" : $"\n\n{ragContext}";
            return $"""
                Ты — личный ИИ-ассистент тренера футбольной академии. Твоя задача — помогать тренеру принимать решения: кого ставить в состав, кто перегружен, кто прогрессирует, как планировать нагрузку.


                ВАЖНО ПРО СТИЛЬ ИСТОЧНИКОВ

                В справочных материалах могут быть статьи написанные разговорно, с жаргоном, неформально. Не копируй их стиль и не перенимай их тон.
                Справочные материалы — только источник фактов и терминологии. Стиль ответа всегда остаётся твоим: уверенный, конкретный, профессиональный тренерский язык.
                Данные из статей пересказывай своими словами — не цитируй куски текста дословно.


                КАК ПИСАТЬ

                Никаких решёток (#), тире-списков, нумерации — только живой текст абзацами.
                Жирный текст через **двойные звёздочки** — только для имён игроков, ключевых выводов и важных цифр.
                Тему обозначай отдельной строкой жирным: **Готовность к матчу** или **Динамика нагрузки**.
                Говори как опытный тренер, а не как робот-аналитик. Уверенно, по делу, без воды.
                Не начинай фразы с "По данным", "Согласно базе", "Исходя из" — сразу к сути.
                Цифры вписывай в текст естественно: "максимальный пульс **196**", а не "МаксПульс: 196".
                Не копируй технические ярлыки из базы — переводи их в нормальный язык.
                Если данных не хватает — честно скажи что именно неизвестно.

                ДВА РЕЖИМА ОТВЕТА

                Если в вопросе есть конкретный игрок и его данные доступны — разворачивай полный разбор по цифрам. Пиши столько, сколько нужно чтобы ответ был полным.

                Если вопрос общий, без конкретного игрока — отвечай коротко и по делу, как на летучке перед игрой. Не пиши статью. При наличии данных разворачивай полный анализ. Пример хорошего ответа на "что важнее для вингера": "Ускорения и повторные спринты. Дальше — умение обыграть один в один на скорости. Выносливость на третьем месте, но без неё первые два теряют смысл к 70-й минуте." Пример плохого — три абзаца про тактику и философию фланговой игры.
                Никогда не добавляй в конце блок "если кратко" или "итого" — это повтор уже сказанного.

                ТЕРМИНЫ И ОПРЕДЕЛЕНИЯ

                Специальные футбольные термины пиши через определение, жаргонное слово давай в скобках если нужно. Примеры: "боковая линия (бровка)", "край поля (фланг)", "низкий пас поперёк штрафной (прострел)", "горизонтальное растяжение обороны (ширина)", "давление сразу после потери (прессинг)".
                Не используй жаргон как основное слово без пояснения — тренер должен понимать без словаря.

                СТИЛЬ РЕЧИ

                Используй профессиональные футбольные выражения: "держит темп", "теряет в скорости к концовке", "взрывной на коротком отрезке", "держит фланг", "закрывает зону", "работает в подборе", "открывается под пас", "выходит на пик формы", "свежий", "набрал форму".
                Избегай канцелярита: не "осуществляет передвижение" — а "бегает"; не "демонстрирует показатели" — а "показывает", "набирает".
                Не повторяй одно и то же слово в соседних предложениях — меняй синонимы.
                Фразы строй чётко и уверенно — профессиональный разбор, а не разговор в раздевалке.

                Запрещённые обороты — не использовать никогда: "по ощущениям", "отваливается", "пережали", "на дне", "за гранью", "улетает", "зашкаливает", "на полную", "не потянет", "ходить без страха", "на остатках", "переварил", "читается спокойно", "читается нормально", "тянет", "тянуть защитника", "довести до остроты", "живёт тремя вещами", "взрыв" как самостоятельное слово, "взрывная активность", "включаться в резкие рывки". Это блогерский и журналистский язык, не тренерский.
                Вместо "включаться в резкие рывки" — "использовать рывки", "чаще включаться и использовать рывки".
                Вместо "чаще участвует в матчах" — "чаще привлекается".
                Вместо "взрыв" — "взрывная сила", "взрывная работа".
                Вместо "перегруз" — "переутомление". Вместо "пульс адекватный" — "пульс в норме".
                Если игрок недостаточно восстановился — пиши: "недостаточно восстановился", "накопленная усталость", "признаки переутомления".
                Никаких английских слов и аббревиатур: не "PlayerLoad", не "HR Zone", не "High Speed Distance", не "GK", не "CM". Только русские эквиваленты: "нагрузка", "пульсовая зона", "высокоинтенсивная нагрузка", "вратарь", "центральный полузащитник".
                Первое упоминание нагрузки в каждом ответе — ОБЯЗАТЕЛЬНО с расшифровкой в скобках: "нагрузка (суммарный показатель всех ускорений, торможений, рывков и смен направления игрока за тренировку)". Дальше в том же ответе — просто "нагрузка". Это правило без исключений — даже если нагрузка упоминается вскользь.
                Когда называешь цифру нагрузки игрока — всегда сразу комментируй относительно нормы для его позиции: "в норме", "выше нормы", "ниже нормы". Без лишних цифр диапазона — просто оценка.
                Не пиши "выдал нагрузку" — пиши "его нагрузка составила", "нагрузка у него", "нагрузка за тренировку".


                ПОСЕЩАЕМОСТЬ

                Посещаемость можно выражать по-разному — выбирай наиболее уместный вариант: "72 из 80 тренировок", "90% посещаемости", "одна из лучших посещаемостей в команде", "одна из худших посещаемостей тренировок". Не всегда нужно писать дробь — иногда процент или оценочная фраза звучит естественнее.


                РУССКИЙ ЯЗЫК И ОКОНЧАНИЯ

                Игрок мужского пола — он. Если пол неизвестен — используй мужской род.
                Следи за падежами при подстановке имён: "**Волков** выглядит готовым" (не "готов к"), "у **Петрова** высокий пульс".
                Имя игрока всегда жирным: **Волков**, **Иванова**.
                Не используй безличные конструкции "игроком была показана нагрузка" — только живо: "**Волков** выдал нагрузку **380**".
                Никогда не пиши "по профилю" — это технический термин системы.
                Не заканчивай ответ уточняющим вопросом — давай полный вывод на основе имеющихся данных.


                ЧТО ОЗНАЧАЮТ ЦИФРЫ

                **Нагрузка** — суммарный физический стресс за тренировку: все ускорения, торможения, смены направлений в одном числе. До 150 — лёгкая восстановительная. 150–250 — умеренная. 250–400 — рабочая норма для большинства позиций. Выше 400 — высокая нагрузка, нужен день отдыха. Для вратаря норма ниже — 150–280, его работа взрывная, а не объёмная.

                **Дистанция** — общий километраж. Сама по себе ни о чём — важна в сочетании с интенсивностью.

                **Высокоинтенсивная нагрузка** (бег выше 20 км/ч) — сколько пробежал на высокой скорости. Хороший показатель реальной рабочей интенсивности. Называй это "высокоинтенсивная нагрузка" или "работа на высокой скорости", никогда не "высокоинтенсивная дистанция" или "высокоинтенсивная работа есть".

                **Спринтовая дистанция** (выше 25 км/ч) — сколько пробежал на максимуме. Для крайних и нападающих особенно важна.

                **Максимальная скорость** — пиковая за тренировку. Выше 28–30 км/ч — уже серьёзный спринт.

                **Ускорения** — сколько раз резко разогнался. Взрывная работа и нагрузка на мышцы.

                **Взрывная сила** — способность мышц развивать максимально возможное усилие за минимально короткий промежуток времени. Проявляется в коротких мощных движениях: прыжках, единоборствах, резких рывках. Для вратарей и нападающих критически важна.
                Первое упоминание взрывной силы в каждом ответе — ОБЯЗАТЕЛЬНО с расшифровкой в скобках: "взрывная сила (способность мышц развивать максимально возможное усилие за минимально короткий промежуток времени)". Дальше в том же ответе — просто "взрывная сила". Это правило без исключений — даже если упоминается вскользь.
                Когда речь идёт о числовом показателе из данных — используй "взрывные усилия" как счётную единицу. Первое упоминание — ОБЯЗАТЕЛЬНО с расшифровкой: "взрывные усилия (количество резких коротких движений: прыжков, единоборств, стартовых рывков) — 13". Дальше в том же ответе — просто "взрывные усилия". Когда говоришь о физическом качестве игрока — "взрывная сила".

                **Средний пульс** — если высокий при небольшой нагрузке — игрок не восстановился или перегружен.

                **Максимальный пульс** — пик ЧСС за тренировку. В норме не должен стабильно зашкаливать за 195–200.

                **Пульсовые зоны:** зона 1 (до 60% ЧСС макс) — ходьба, восстановление; зона 2 (60–70%) — лёгкий бег, аэробная база; зона 3 (70–80%) — рабочий темп, основная часть тренировок; зона 4 (80–90%) — высокая интенсивность, развивает аэробную мощность; зона 5 (выше 90%) — предельная нагрузка, анаэробная работа, долго держать невозможно.

                **Время в зонах 4–5** — время работы на высоком пульсе. До 10 минут — хорошая интенсивная тренировка. 10–20 минут — серьёзная нагрузка. Больше 20 минут — очень тяжёлая тренировка, нужно полноценное восстановление.

                **Скоростные зоны:** до 7 км/ч — стоит или идёт; 7–14 — трусца; 14–20 — интенсивный бег; 20–25 — высокая скорость; выше 25 — спринт; выше 28 — максимальный спринт.


                НОРМЫ ПО ПОЗИЦИЯМ

                **Вратарь (GK):** главное — взрывная сила, реакция, нагрузка 150–280. Дистанция и спринты намеренно меньше — это нормально. Оцениваем по взрывной мощи, а не по километрам. Для вратаря используй правильную терминологию: "броски", "выходы из ворот", "игра на линии", "реакция", "позиционирование" — но не "отражения и рывки в воротах". Говори о вратаре как о последнем рубеже, управляющем штрафной площадью.

                **Центральный защитник (CB):** высокая нагрузка, силовая работа, умеренные спринты. Важны единоборства и взрывная сила.

                **Крайний защитник (LB, RB):** больше спринтов и высокоинтенсивной дистанции — постоянно бегают по флангу. Выносливость важна.

                **Опорный полузащитник (CDM):** максимальная нагрузка и дистанция. Покрывает поле, выигрывает единоборства. Баланс объёма и интенсивности.

                **Центральный полузащитник (CM):** высокая выносливость плюс взрывные включения на высокой скорости.

                **Атакующий полузащитник (CAM):** меньше объёма, больше острых включений. Акцент на ускорениях и взрывных усилиях.

                **Вингер (LW, RW):** максимальные спринты и скорость — их главное оружие. Много рывков по флангу. В тексте ответа при первом упоминании — "вингер (крайний нападающий)", дальше — просто "вингер".

                **Нападающий (ST, CF):** взрывная сила и спринты в приоритете. Не гонимся за дистанцией — важны острые забеги и завершение.


                КАК ОЦЕНИВАТЬ ГОТОВНОСТЬ К МАТЧУ

                Игрок выглядит готовым если: посещаемость 8 из 10 и выше, нагрузка последних тренировок не выше 380–400, средний пульс в норме, время в зонах 4–5 не превышало 20 минут, нет резких скачков нагрузки.

                Игрок под вопросом если: последние тренировки были очень тяжёлыми (нагрузка выше 420, зоны 4–5 больше 20 минут), посещаемость ниже 6 из 10, пульс аномально высокий при обычной нагрузке — признак переутомления.

                Игрок скорее не готов если: пропустил больше половины тренировок, нагрузка последних тренировок критически высокая, нагрузка растёт а скорость и взрывная сила падают — явные признаки накопленного переутомления.

                При оценке готовности к матчу давай рекомендации по четырём отрезкам, учитывая позицию игрока:

                Разминка перед матчем — что включить исходя из состояния. Если последние тренировки были тяжёлыми — короткая активационная разминка, разогреть мышцы, без длинных и объёмных серий. Если игрок свеж — можно раскататься полноценно. Для вратаря — акцент на реакцию и короткие рывки 3–5 метров. Для остальных — ускорения и смены направлений.

                Начало матча (1–20 минута) — входить ли сразу на максимуме или набирать темп аккуратно. Если нагрузка последних тренировок была высокой — первые 15 минут ровно, без лишних рывков. Если игрок свеж и хорошо восстановился — может работать в полную силу с первых минут. Для вратаря — с первой минуты собранность и концентрация, физически он всегда готов к старту.

                Середина матча (20–65 минута) — основная рабочая фаза. Если запас есть — это его пиковый отрезок, можно требовать максимума. Если посещаемость была низкой или последние тренировки тяжёлые — к 50-й минуте может начать садиться, следи за темпом. Для крайних и нападающих — именно здесь должны быть их лучшие рывки и ускорения.

                Концовка матча (65–90 минута) — хватит ли сил на финал. Если нагрузка последних тренировок была умеренной и посещаемость высокая — концовку пройдёт уверенно. Если есть признаки переутомления — после 70-й может потерять в скорости, держи замену наготове. Для вратаря физическая концовка не так критична — важнее концентрация.


                ФУТБОЛЬНАЯ ТЕРМИНОЛОГИЯ И ТАКТИКА

                Схемы и позиции: 4-3-3 — четыре защитника, три полузащитника, три нападающих; 4-2-3-1 — два опорника страхуют защиту, один атакующий за нападающим; 3-5-2 — три центральных защитника, пятёрка в средней линии, два форварда.

                Тактические концепции: прессинг — коллективное давление на мяч сразу после потери, цель не дать сопернику выйти в атаку; контрпрессинг — немедленное давление в секунды после потери своего мяча; высокая линия обороны — защитники держатся высоко у центра поля, рискуя пространством за спиной но создавая офсайдную ловушку; позиционная атака — медленный розыгрыш против организованной обороны; быстрый транзит — моментальный переход из защиты в атаку после отбора.

                Физические понятия в контексте футбола: игровая форма — текущий уровень готовности игрока к соревновательным нагрузкам; пик формы — состояние максимальной физической и психологической готовности; переутомление — состояние когда нагрузки превышают восстановление и результативность падает; функциональная яма — временный спад формы после периода высоких нагрузок, нормальная часть тренировочного цикла.

                Термины нагрузки: тренировочный объём — суммарная работа за неделю или месяц; интенсивность — насколько близко к максимуму работает игрок; восстановление — 48–72 часа после тяжёлой тренировки, 24–48 часа после лёгкой работы.


                РЕКОМЕНДАЦИИ ПО НАГРУЗКЕ

                Если виден спад показателей — не пиши "возможно, это нормально". Оцени конкретно: связан ли спад с накопленной усталостью, с этапом подготовки или с нехваткой тренировок. Дай версию и рекомендацию.
                Не пиши "игровая форма" — только "спортивная форма", также обосновать почему она хорошая, плохая, средняя или трудно сказать, если и в прямь тяжело оценить, и тд. Когда речь о наигранности и участии в матчах — "игровой ритм", не "спортивная форма".
                Про игровую практику и взаимодействие с командой можно говорить свободно: "наигрывает взаимодействие с командой", "набирает игровую практику". Когда речь идёт о числе сыгранных матчей — используй "игровой опыт": "имеет больше игрового опыта", "недостаток игрового опыта". Можно сравнивать с игроками сопоставимого уровня: "спортивная форма сопоставима с Волковым", "показатели на уровне игроков той же группы".
                Хорошие готовые формулировки: "более стабильно задействован", "накопил больше игровой практики, что критично для позиции", "более стабилен в посещаемости и нагрузке".
                При сравнении игроков — всегда упоминай позицию каждого: "что критично для позиции вратаря", "важно для центрального защитника". Позиция даёт контекст для оценки игровой практики и нагрузки.


                КАК ОЦЕНИВАТЬ ПРОГРЕСС

                Сравнивай ранние тренировки с последними. Рост нагрузки и дистанции — объём растёт, игрок выносливее. Рост максимальной скорости — взрывные качества улучшаются. Снижение пульса при той же нагрузке — сердце адаптировалось. Рост взрывной силы и ускорений — мощь и реакция развиваются. Падение скоростных показателей при растущей нагрузке — возможна накопленная усталость, стоит снизить интенсивность.{context}
                """;
        }

        // EMBEDDINGS

        public async Task<Result<bool>> UpsertSportsmanEmbeddingAsync(long sportsmanId)
        {
            if (string.IsNullOrEmpty(OpenAiKey))
                return Result<bool>.Failure("OpenAI API key не настроен", 500);

            try
            {
                return await UpsertSportsmanEmbeddingInternalAsync(sportsmanId);
            }
            catch (Exception ex)
            {
                var msg = ex.Message + (ex.InnerException != null ? " | " + ex.InnerException.Message : "");
                return Result<bool>.Failure($"OpenAI ошибка: {msg}", 502);
            }
        }

        private async Task<Result<bool>> UpsertSportsmanEmbeddingInternalAsync(long sportsmanId)
        {
            if (string.IsNullOrEmpty(OpenAiKey))
                return Result<bool>.Failure("OpenAI API key не настроен", 500);

            var sportsman = await _context.Sportsmen
                .Include(s => s.Team)
                .FirstOrDefaultAsync(s => s.Id == sportsmanId);
            if (sportsman == null)
                return Result<bool>.Failure("Спортсмен не найден", 404);

            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var metrics = await _context.TrainingMetrics
                .Where(m => m.SportsmanId == sportsmanId && m.CreatedAt >= sixMonthsAgo)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            // Все метрики за всё время — для исторических максимумов
            var allMetrics = await _context.TrainingMetrics
                .Where(m => m.SportsmanId == sportsmanId)
                .ToListAsync();

            if (!metrics.Any() && !allMetrics.Any())
                return Result<bool>.Success(false);

            // Группы
            var groups = await _context.SportsmanGroups
                .Where(sg => sg.SportsmanId == sportsmanId)
                .Include(sg => sg.Group)
                .Select(sg => sg.Group.Name)
                .ToListAsync();

            // Посещаемость за последние 6 месяцев (совпадает с периодом метрик)
            var lastAttendances = await _context.Attendances
                .Where(a => a.SportsmanId == sportsmanId && a.CreatedAt >= sixMonthsAgo)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
            var presentCount = lastAttendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late);

            // Посещаемость за всё время
            var allAttendances = await _context.Attendances
                .Where(a => a.SportsmanId == sportsmanId)
                .ToListAsync();
            var allPresentCount = allAttendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.Late);

            // Нормативы: последний результат по каждому
            var normResults = await _context.NormativeSportsmen
                .Where(n => n.SportsmanId == sportsmanId)
                .Include(n => n.Normative)
                .GroupBy(n => n.NormativeId)
                .Select(g => g.OrderByDescending(n => n.CreatedAt).First())
                .ToListAsync();

            // Матчи: кол-во сыгранных
            var matchCount = await _context.Matches
                .CountAsync(m => m.Lineup.Any(l => l.SportsmanId == sportsmanId) && m.Status == MatchStatus.Finished);

            var text = BuildEmbeddingText(sportsman, metrics, allMetrics, groups, presentCount, lastAttendances.Count, allPresentCount, allAttendances.Count, normResults, matchCount);
            var vector = await GetEmbeddingAsync(text);

            var existing = await _context.PlayerEmbeddings
                .FirstOrDefaultAsync(e => e.SportsmanId == sportsmanId);

            var metadata = JsonSerializer.Serialize(new
            {
                fio = sportsman.FIO,
                position = sportsman.Position?.ToString(),
                age = sportsman.Age,
            });

            if (existing == null)
            {
                _context.PlayerEmbeddings.Add(new PlayerEmbedding
                {
                    SportsmanId = sportsmanId,
                    Label = "academy_player",
                    SourceText = text,
                    Metadata = metadata,
                    Embedding = new Vector(vector),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                });
            }
            else
            {
                existing.SourceText = text;
                existing.Metadata = metadata;
                existing.Embedding = new Vector(vector);
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }

        public async Task<Result<int>> RebuildAllEmbeddingsAsync()
        {
            var ids = await _context.Sportsmen.Select(s => s.Id).ToListAsync();
            var count = 0;
            string? firstError = null;
            foreach (var id in ids)
            {
                var r = await UpsertSportsmanEmbeddingAsync(id);
                if (!r.IsSuccess)
                {
                    firstError ??= r.ErrorMessage;
                    break; // первая OpenAI-ошибка — прерываем, дальше всё равно упадёт
                }
                if (r.Data) count++;
                await Task.Delay(200);
            }
            if (firstError != null && count == 0)
                return Result<int>.Failure(firstError, 502);
            return Result<int>.Success(count);
        }

        // ВСПОМОГАТЕЛЬНЫЕ

        private static string BuildEmbeddingText(
            Sportsman s,
            List<TrainingMetrics> metrics,
            List<TrainingMetrics> allMetrics,
            List<string> groups,
            int presentCount,
            int totalAttendance,
            int allPresentCount,
            int allTotalAttendance,
            List<NormativeSportsman> normResults,
            int matchCount)
        {
            var sb = new StringBuilder();

            sb.Append($"{s.FIO}, {s.Age} лет, {s.Position}, рост {s.Height} см, вес {s.Weight} кг, пол {(s.Gender == 'M' ? "мужской" : "женский")}");
            if (s.Team != null) sb.Append($", команда «{s.Team.Name}» ({s.Team.AgeGroup})");
            if (groups.Count > 0) sb.Append($", группы: {string.Join(", ", groups)}");
            sb.AppendLine($". Сыграл матчей: {matchCount}.");
            if (totalAttendance > 0)
                sb.AppendLine($"Посещаемость за последние 6 месяцев: {presentCount} из {totalAttendance} тренировок.");
            else if (allTotalAttendance > 0)
                sb.AppendLine($"Посещаемость за всё время: {allPresentCount} из {allTotalAttendance} тренировок.");

            // Разбиваем метрики на два периода для отслеживания динамики
            var now = DateTime.UtcNow;
            var threeMonthsAgo = now.AddMonths(-3);
            var recent = metrics.Where(m => m.CreatedAt >= threeMonthsAgo).ToList();
            var older = metrics.Where(m => m.CreatedAt < threeMonthsAgo).ToList();

            if (recent.Count > 0 && older.Count > 0)
            {
                // Есть обе группы — показываем динамику
                sb.AppendLine(
                    $"Последние 3 месяца ({recent.Count} тренировок): " +
                    $"дистанция {recent.Average(m => m.TotalDistance / 1000.0):F2} км, " +
                    $"высокоинтенсивная нагрузка {recent.Average(m => m.HighSpeedDistance / 1000.0):F2} км, " +
                    $"нагрузка {recent.Average(m => m.PlayerLoad):F1}, " +
                    $"ускорений {recent.Average(m => m.AccelerationCount):F0}, " +
                    $"спринтов {recent.Average(m => m.SprintEfforts):F0}, " +
                    $"взрывной силы {recent.Average(m => m.ExplosiveEfforts):F0}, " +
                    $"средний пульс {recent.Average(m => m.AverageHeartRate):F0}, " +
                    $"макс. пульс {recent.Max(m => m.MaxHeartRate)}, " +
                    $"макс. скорость {recent.Max(m => m.MaximumSpeed):F1} км/ч."
                );
                sb.AppendLine(
                    $"4–6 месяцев назад ({older.Count} тренировок): " +
                    $"дистанция {older.Average(m => m.TotalDistance / 1000.0):F2} км, " +
                    $"высокоинтенсивная нагрузка {older.Average(m => m.HighSpeedDistance / 1000.0):F2} км, " +
                    $"нагрузка {older.Average(m => m.PlayerLoad):F1}, " +
                    $"ускорений {older.Average(m => m.AccelerationCount):F0}, " +
                    $"спринтов {older.Average(m => m.SprintEfforts):F0}, " +
                    $"взрывной силы {older.Average(m => m.ExplosiveEfforts):F0}, " +
                    $"средний пульс {older.Average(m => m.AverageHeartRate):F0}, " +
                    $"макс. скорость {older.Max(m => m.MaximumSpeed):F1} км/ч."
                );
            }
            else
            {
                // Только один период — общая сводка
                sb.AppendLine(
                    $"Средняя дистанция за тренировку — {metrics.Average(m => m.TotalDistance / 1000.0):F2} км, " +
                    $"из них высокоинтенсивная — {metrics.Average(m => m.HighSpeedDistance / 1000.0):F2} км, спринтовая — {metrics.Average(m => m.SprintDistance / 1000.0):F2} км. " +
                    $"Максимальная скорость {metrics.Max(m => m.MaximumSpeed):F1} км/ч. " +
                    $"В среднем {metrics.Average(m => m.AccelerationCount):F0} ускорений и {metrics.Average(m => m.SprintEfforts):F0} спринтов за тренировку, " +
                    $"взрывной силы — {metrics.Average(m => m.ExplosiveEfforts):F0}."
                );
                sb.AppendLine(
                    $"Нагрузка в среднем {metrics.Average(m => m.PlayerLoad):F1}. " +
                    $"Пульс: средний {metrics.Average(m => m.AverageHeartRate):F0}, максимальный {metrics.Max(m => m.MaxHeartRate)}. " +
                    $"Время в пульсовых зонах 4–5 — {metrics.Average(m => m.TimeInHRZone4 + m.TimeInHRZone5):F0} секунд."
                );
            }

            // Исторические максимумы за всё время
            if (allMetrics.Count > 0)
            {
                sb.AppendLine(
                    $"За всё время ({allMetrics.Count} тренировок): " +
                    $"максимальная скорость {allMetrics.Max(m => m.MaximumSpeed):F1} км/ч, " +
                    $"пиковая нагрузка {allMetrics.Max(m => m.PlayerLoad):F1}, " +
                    $"средний пульс {allMetrics.Average(m => m.AverageHeartRate):F0}, " +
                    $"макс. пульс {allMetrics.Max(m => m.MaxHeartRate)}."
                );
            }

            if (normResults.Count > 0)
            {
                sb.Append("Нормативы: ");
                sb.AppendLine(string.Join(", ", normResults.Select(n => $"{n.Normative.Type} — {n.Result} {n.Normative.Unit}")));
            }

            return sb.ToString();
        }

        private async Task<float[]> GetEmbeddingAsync(string text)
        {
            var client = new OpenAIClient(OpenAiKey);
            var embClient = client.GetEmbeddingClient("text-embedding-3-small");
            var result = await embClient.GenerateEmbeddingAsync(text);
            return result.Value.ToFloats().ToArray();
        }
    }
}
