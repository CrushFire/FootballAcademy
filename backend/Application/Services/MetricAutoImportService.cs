using Core.Interfaces.Services;
using Core.Models.MetricModel;
using Core.Results;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services
{
    public class MetricAutoImportService
    {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer;
        public bool AutoImportEnabled { get; set; } = true;

        public MetricAutoImportService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            // По умолчанию автоимпорт включён — стартуем таймер сразу
            if (AutoImportEnabled) InitializeAutoImport();
        }

        public void SetAutoImport(bool enabled)
        {
            AutoImportEnabled = enabled;
            if (enabled && _timer == null)
            {
                InitializeAutoImport();
            }
            else if (!enabled && _timer != null)
            {
                _timer?.Dispose();
                _timer = null;
            }
        }

        private void InitializeAutoImport()
        {
            var now = DateTime.Now;
            var nextRun = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddDays(1);
            var delay = nextRun - now;
            _timer = new Timer(async _ => await AutoImportMetricsAsync(), null, delay, TimeSpan.FromDays(1));
        }

        private async Task AutoImportMetricsAsync()
        {
            if (!AutoImportEnabled || !Directory.Exists("/app/uploads/metrics")) return;

            using var scope = _serviceProvider.CreateScope();
            var metricService = scope.ServiceProvider.GetRequiredService<IMetricsService>();

            var files = Directory.GetFiles("/app/uploads/metrics", "*.xlsx");
            foreach (var filePath in files)
            {
                try
                {
                    await metricService.ImportMetricsFromFileAsync(filePath);
                    var archivePath = Path.Combine("/app/uploads/metrics", "archive");
                    Directory.CreateDirectory(archivePath);
                    var destPath = Path.Combine(archivePath, Path.GetFileName(filePath));
                    if (File.Exists(destPath)) File.Delete(destPath);
                    File.Move(filePath, destPath);
                }
                catch { }
            }
        }

        public async Task<Result<AutoImportResult>> RunAutoImportNowAsync()
        {
            const string folder = "/app/uploads/metrics";

            // Создаём папку если её нет (первый запуск или новое окружение)
            if (!Directory.Exists(folder))
            {
                try { Directory.CreateDirectory(folder); }
                catch (Exception ex)
                {
                    return Result<AutoImportResult>.Failure($"Не удалось создать директорию: {ex.Message}", 500);
                }
            }

            using var scope = _serviceProvider.CreateScope();
            var metricService = scope.ServiceProvider.GetRequiredService<IMetricsService>();

            var files = Directory.GetFiles(folder, "*.xlsx");

            if (files.Length == 0)
            {
                return Result<AutoImportResult>.Success(new AutoImportResult
                {
                    Processed = 0,
                    Failed = 0,
                    Message = "Хранилище не содержит новых метрик"
                });
            }

            int processed = 0;
            int failed = 0;
            int metricsImported = 0;
            string? lastError = null;

            foreach (var filePath in files)
            {
                try
                {
                    var importResult = await metricService.ImportMetricsFromFileAsync(filePath);
                    if (!importResult.IsSuccess)
                    {
                        // Парсер вернул ошибку — оставляем файл в metrics/ (не двигаем в archive)
                        failed++;
                        lastError = importResult.ErrorMessage;
                        continue;
                    }

                    // Парсим число метрик из строки вида "Импортировано N метрик"
                    var msg = importResult.Data ?? "";
                    var m = System.Text.RegularExpressions.Regex.Match(msg, @"\d+");
                    if (m.Success && int.TryParse(m.Value, out var n))
                    {
                        metricsImported += n;
                    }

                    var archivePath = Path.Combine(folder, "archive");
                    Directory.CreateDirectory(archivePath);
                    var destPath = Path.Combine(archivePath, Path.GetFileName(filePath));
                    if (File.Exists(destPath)) File.Delete(destPath);
                    File.Move(filePath, destPath);
                    processed++;
                }
                catch (Exception ex)
                {
                    failed++;
                    lastError = ex.Message;
                }
            }

            return Result<AutoImportResult>.Success(new AutoImportResult
            {
                Processed = processed,
                Failed = failed,
                MetricsImported = metricsImported,
                Message = failed == 0
                    ? $"Импортировано тренировок: {processed}, метрик: {metricsImported}"
                    : $"Импортировано тренировок: {processed}, метрик: {metricsImported}, ошибок: {failed} ({lastError})"
            });
        }
    }

}
