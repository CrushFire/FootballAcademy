using Core.Models.RagModel;
using Core.Results;

namespace Core.Interfaces.Services
{
    public interface IRagService
    {
        // ── Чаты ──
        Task<Result<List<AiChatResponse>>> GetChatsAsync(long trainerId);
        Task<Result<AiChatDetailResponse>> GetChatAsync(long chatId, long trainerId);
        Task<Result<AiChatResponse>> CreateChatAsync(long trainerId);
        Task<Result<bool>> DeleteChatAsync(long chatId, long trainerId);

        // ── Сообщения ──
        Task<Result<AiMessageResponse>> SendMessageAsync(long chatId, long trainerId, string message);

        // ── Embeddings ──
        Task<Result<bool>> UpsertSportsmanEmbeddingAsync(long sportsmanId);
        Task<Result<int>> RebuildAllEmbeddingsAsync();
    }
}
