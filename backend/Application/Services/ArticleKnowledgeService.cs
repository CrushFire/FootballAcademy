using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenAI;
using System.Text;
using System.Text.Json;
using UglyToad.PdfPig;

namespace Application.Services
{
    /// <summary>
    /// Загружает .txt статьи из папки /knowledge при старте, индексирует их в памяти через OpenAI embeddings.
    /// При запросе — векторный поиск, возвращает топ-N релевантных чанков.
    /// </summary>
    public class ArticleKnowledgeService : IHostedService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<ArticleKnowledgeService> _logger;

        private string OpenAiKey => _config["OpenAI:ApiKey"] ?? "";

        private readonly List<ArticleChunk> _chunks = new();

        public ArticleKnowledgeService(IConfiguration config, ILogger<ArticleKnowledgeService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            // Запускаем индексацию в фоне — не блокируем старт приложения
            _ = Task.Run(() => IndexAsync(cancellationToken), cancellationToken);
            return Task.CompletedTask;
        }

        private async Task IndexAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(OpenAiKey))
            {
                _logger.LogWarning("ArticleKnowledgeService: OpenAI key не настроен, статьи не будут проиндексированы.");
                return;
            }

            var knowledgeDir = Path.Combine(AppContext.BaseDirectory, "knowledge");
            if (!Directory.Exists(knowledgeDir))
            {
                _logger.LogInformation("ArticleKnowledgeService: папка /knowledge не найдена, пропускаем.");
                return;
            }

            var txtFiles = Directory.GetFiles(knowledgeDir, "*.txt");
            var pdfFiles = Directory.GetFiles(knowledgeDir, "*.pdf");
            var files = txtFiles.Concat(pdfFiles).ToArray();

            if (files.Length == 0)
            {
                _logger.LogInformation("ArticleKnowledgeService: нет файлов в /knowledge.");
                return;
            }

            _logger.LogInformation("ArticleKnowledgeService: индексируем {Count} файлов...", files.Length);

            foreach (var file in files)
            {
                try
                {
                    var ext = Path.GetExtension(file).ToLowerInvariant();
                    var title = Path.GetFileNameWithoutExtension(file);
                    var cacheFile = file + ".cache";

                    // Если кэш свежее исходника — грузим из кэша без OpenAI
                    if (File.Exists(cacheFile) &&
                        File.GetLastWriteTimeUtc(cacheFile) >= File.GetLastWriteTimeUtc(file))
                    {
                        var cached = await LoadCacheAsync(cacheFile);
                        if (cached.Count > 0)
                        {
                            _chunks.AddRange(cached);
                            _logger.LogInformation("ArticleKnowledgeService: '{Title}' — {N} чанков из кэша", title, cached.Count);
                            continue;
                        }
                    }

                    // Нет кэша или устарел — читаем файл и индексируем
                    var text = ext == ".pdf"
                        ? ExtractTextFromPdf(file)
                        : await File.ReadAllTextAsync(file, Encoding.UTF8, cancellationToken);

                    if (string.IsNullOrWhiteSpace(text)) continue;
                    var chunks = SplitIntoChunks(text, title, maxChunkLen: 800);

                    foreach (var chunk in chunks)
                    {
                        chunk.Embedding = await GetEmbeddingAsync(chunk.Text);
                        await Task.Delay(150, cancellationToken);
                    }

                    _chunks.AddRange(chunks);
                    await SaveCacheAsync(cacheFile, chunks);
                    _logger.LogInformation("ArticleKnowledgeService: '{Title}' — {N} чанков (проиндексировано)", title, chunks.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "ArticleKnowledgeService: ошибка при индексации {File}", file);
                }
            }

            _logger.LogInformation("ArticleKnowledgeService: готово, всего {Total} чанков.", _chunks.Count);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        /// <summary>Векторный поиск по запросу, возвращает топ-N чанков.</summary>
        public async Task<List<ArticleChunk>> SearchAsync(string query, int topN = 3)
        {
            if (_chunks.Count == 0 || string.IsNullOrEmpty(OpenAiKey))
                return new List<ArticleChunk>();

            float[] queryVec;
            try { queryVec = await GetEmbeddingAsync(query); }
            catch { return new List<ArticleChunk>(); }

            return _chunks
                .Select(c => (chunk: c, score: CosineSimilarity(queryVec, c.Embedding!)))
                .OrderByDescending(x => x.score)
                .Where(x => x.score > 0.35f)
                .Take(topN)
                .Select(x => x.chunk)
                .ToList();
        }

        public bool HasArticles => _chunks.Count > 0;

        /// <summary>Переиндексировать все файлы без рестарта контейнера.</summary>
        public async Task<int> ReindexAsync()
        {
            _chunks.Clear();
            await IndexAsync(CancellationToken.None);
            return _chunks.Count;
        }

        // ── helpers ──

        private static string ExtractTextFromPdf(string filePath)
        {
            var sb = new StringBuilder();
            using var doc = PdfDocument.Open(filePath);
            foreach (var page in doc.GetPages())
            {
                var words = page.GetWords();
                sb.AppendLine(string.Join(" ", words.Select(w => w.Text)));
                sb.AppendLine();
            }
            return sb.ToString();
        }

        private static List<ArticleChunk> SplitIntoChunks(string text, string title, int maxChunkLen)
        {
            var paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var chunks = new List<ArticleChunk>();
            var current = new StringBuilder();

            foreach (var para in paragraphs)
            {
                var clean = para.Trim();
                if (clean.Length == 0) continue;

                if (current.Length + clean.Length > maxChunkLen && current.Length > 0)
                {
                    chunks.Add(new ArticleChunk { Title = title, Text = current.ToString().Trim() });
                    current.Clear();
                }
                current.AppendLine(clean);
            }

            if (current.Length > 0)
                chunks.Add(new ArticleChunk { Title = title, Text = current.ToString().Trim() });

            return chunks;
        }

        private async Task<float[]> GetEmbeddingAsync(string text)
        {
            var client = new OpenAIClient(OpenAiKey);
            var embClient = client.GetEmbeddingClient("text-embedding-3-small");
            var result = await embClient.GenerateEmbeddingAsync(text);
            return result.Value.ToFloats().ToArray();
        }

        private static async Task SaveCacheAsync(string cacheFile, List<ArticleChunk> chunks)
        {
            var data = chunks.Select(c => new CacheEntry { Title = c.Title, Text = c.Text, Embedding = c.Embedding! }).ToList();
            var json = JsonSerializer.Serialize(data);
            await File.WriteAllTextAsync(cacheFile, json, Encoding.UTF8);
        }

        private static async Task<List<ArticleChunk>> LoadCacheAsync(string cacheFile)
        {
            try
            {
                var json = await File.ReadAllTextAsync(cacheFile, Encoding.UTF8);
                var data = JsonSerializer.Deserialize<List<CacheEntry>>(json);
                if (data == null) return new List<ArticleChunk>();
                return data.Select(e => new ArticleChunk { Title = e.Title, Text = e.Text, Embedding = e.Embedding }).ToList();
            }
            catch { return new List<ArticleChunk>(); }
        }

        private class CacheEntry
        {
            public string Title { get; set; } = "";
            public string Text { get; set; } = "";
            public float[] Embedding { get; set; } = Array.Empty<float>();
        }

        private static float CosineSimilarity(float[] a, float[] b)
        {
            if (a.Length != b.Length) return 0f;
            float dot = 0, normA = 0, normB = 0;
            for (int i = 0; i < a.Length; i++)
            {
                dot += a[i] * b[i];
                normA += a[i] * a[i];
                normB += b[i] * b[i];
            }
            var denom = MathF.Sqrt(normA) * MathF.Sqrt(normB);
            return denom == 0 ? 0f : dot / denom;
        }
    }

    public class ArticleChunk
    {
        public string Title { get; set; } = "";
        public string Text { get; set; } = "";
        public float[]? Embedding { get; set; }
    }
}
