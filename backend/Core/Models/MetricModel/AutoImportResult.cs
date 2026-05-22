namespace Core.Models.MetricModel
{
    public class AutoImportResult
    {
        public int Processed { get; set; }     // Сколько файлов обработано
        public int Failed { get; set; }        // Сколько файлов не обработалось
        public int MetricsImported { get; set; } // Сколько метрик (строк из xlsx) добавлено в БД
        public string Message { get; set; } = string.Empty;
    }
}
