using Pgvector;

namespace Core.Entities
{
    public class PlayerEmbedding
    {
        public long Id { get; set; }

        // null = эталонный профиль позиции (знаменитый игрок / шаблон)
        public long? SportsmanId { get; set; }

        // "academy_player" | "position_template"
        public string Label { get; set; } = null!;

        // Текст, из которого был сгенерирован embedding (для отладки)
        public string SourceText { get; set; } = null!;

        // JSON: позиция, фио, доп. данные
        public string Metadata { get; set; } = "{}";

        // Вектор 1536 измерений (text-embedding-3-small)
        public Vector Embedding { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Sportsman? Sportsman { get; set; }
    }
}
