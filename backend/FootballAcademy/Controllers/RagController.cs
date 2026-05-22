using Application.Services;
using Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FootballAcademy.Controllers
{
    [ApiController]
    [Route("rag")]
    [Authorize]
    public class RagController : BaseController
    {
        private readonly IRagService _rag;
        private readonly ArticleKnowledgeService _articles;

        public RagController(IRagService rag, ArticleKnowledgeService articles)
        {
            _rag = rag;
            _articles = articles;
        }

        private long TrainerId =>
            long.Parse(User.FindFirstValue("personalId")
                ?? throw new UnauthorizedAccessException("personalId не найден в токене"));

        // ── Чаты ──

        /// <summary>Список всех чатов тренера (по убыванию даты)</summary>
        [HttpGet("chats")]
        public async Task<IActionResult> GetChats()
        {
            var result = await _rag.GetChatsAsync(TrainerId);
            return result.IsSuccess
                ? Ok(new { data = result.Data })
                : StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        /// <summary>Открыть конкретный чат с историей сообщений</summary>
        [HttpGet("chats/{chatId}")]
        public async Task<IActionResult> GetChat(long chatId)
        {
            var result = await _rag.GetChatAsync(chatId, TrainerId);
            return result.IsSuccess
                ? Ok(new { data = result.Data })
                : StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        /// <summary>Создать новый пустой чат</summary>
        [HttpPost("chats")]
        public async Task<IActionResult> CreateChat()
        {
            var result = await _rag.CreateChatAsync(TrainerId);
            return result.IsSuccess
                ? Ok(new { data = result.Data })
                : StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        /// <summary>Удалить чат со всеми сообщениями</summary>
        [HttpDelete("chats/{chatId}")]
        public async Task<IActionResult> DeleteChat(long chatId)
        {
            var result = await _rag.DeleteChatAsync(chatId, TrainerId);
            return result.IsSuccess
                ? Ok(new { data = result.Data })
                : StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        // ── Сообщения ──

        /// <summary>Отправить сообщение в чат, получить ответ AI</summary>
        [HttpPost("chats/{chatId}/messages")]
        public async Task<IActionResult> SendMessage(long chatId, [FromBody] string message)
        {
            var result = await _rag.SendMessageAsync(chatId, TrainerId, message);
            return result.IsSuccess
                ? Ok(new { data = result.Data })
                : StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        // ── Embeddings ──

        /// <summary>Пересчитать embedding одного спортсмена (после импорта метрик)</summary>
        [HttpPost("embedding/{sportsmanId}")]
        public async Task<IActionResult> UpsertEmbedding(long sportsmanId)
        {
            var result = await _rag.UpsertSportsmanEmbeddingAsync(sportsmanId);
            return result.IsSuccess
                ? Ok(new { data = result.Data })
                : StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        /// <summary>Пересчитать embeddings для всех спортсменов (первичное заполнение)</summary>
        [HttpPost("embedding/rebuild-all")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> RebuildAll()
        {
            var result = await _rag.RebuildAllEmbeddingsAsync();
            return result.IsSuccess
                ? Ok(new { data = result.Data, message = $"Обновлено {result.Data} embeddings" })
                : StatusCode(result.StatusCode, new { error = result.ErrorMessage });
        }

        /// <summary>Переиндексировать статьи из папки knowledge без рестарта</summary>
        [HttpPost("knowledge/reindex")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> ReindexKnowledge()
        {
            var count = await _articles.ReindexAsync();
            return Ok(new { message = $"Переиндексировано {count} чанков" });
        }
    }
}
