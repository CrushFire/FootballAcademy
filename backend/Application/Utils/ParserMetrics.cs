using Core.Entities;
using Core.Models;
using Core.Models.MetricModel;
using Core.Results;
using DataAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Utils
{
    public class ParserMetrics
    {
        private readonly ApplicationDbContext _context;

        public ParserMetrics(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<MetricCreateRequest>>> ParseExcelFileAsync(IFormFile file)
        {
            try
            {
                // Парсим имя файла
                var fileInfo = ParseFileName(file.FileName);
                if (!fileInfo.IsSuccess)
                {
                    return Result<List<MetricCreateRequest>>.Failure(fileInfo.ErrorMessage, 400);
                }

                // Ищем группу по имени из файла (как есть)
                var groupName = fileInfo.Data.GroupName;
                var group = await _context.Groups
                    .FirstOrDefaultAsync(g => g.Name == groupName);

                if (group == null)
                {
                    return Result<List<MetricCreateRequest>>.Failure($"Группа '{groupName}' не найдена", 404);
                }

                // Ищем тренировку.
                // Тренировка создаётся ДО или в начале занятия. Файл выгружается ПОСЛЕ
                // (может прилететь в течение 3 часов). Значит тренировка попадает в окно [файл - 3ч ; файл].
                // Если в окно попадает несколько — берём ближайшую по времени к моменту файла.
                var windowFrom = fileInfo.Data.DateTime.AddHours(-3);
                var windowTo   = fileInfo.Data.DateTime;

                var candidates = await _context.Trainings
                    .Where(t =>
                        t.GroupId == group.Id &&
                        t.Type == fileInfo.Data.Type &&
                        t.Date >= windowFrom &&
                        t.Date <= windowTo)
                    .ToListAsync();

                var training = candidates
                    .OrderBy(t => Math.Abs((t.Date - fileInfo.Data.DateTime).Ticks))
                    .FirstOrDefault();

                if (training == null)
                {
                    return Result<List<MetricCreateRequest>>.Failure(
                        $"Тренировка не найдена. Дата файла: {fileInfo.Data.DateTime}, Группа: {group.Name}, " +
                        $"Тип: {fileInfo.Data.Type}. Искал в окне [{windowFrom:yyyy-MM-dd HH:mm} ; {windowTo:yyyy-MM-dd HH:mm}]. " +
                        $"Создайте тренировку сначала.", 404);
                }

                // Парсим Excel
                var metrics = await ParseExcelContent(file, training.Id, group.Id);

                return metrics;
            }
            catch (Exception ex)
            {
                return Result<List<MetricCreateRequest>>.Failure($"Ошибка парсинга: {ex.Message}", 500);
            }
        }

        private Result<FileNameInfo> ParseFileName(string fileName)
        {
            // Формат: Группа_{ИмяГруппы}_{Тип}_{YYYY-MM-DD}_{HH-MM}_{Порядковый}.xlsx
            // ИмяГруппы — любое из БД (например U14-А, U16-Б, U12), может содержать буквы/цифры/дефис.
            // Регулярка матчит "Группа_" + (имя группы) + "_" + (тип) + "_" + (дата) + "_" + (время) + "_" + (порядковый)
            var regex = new Regex(@"^Группа_([^_]+)_([^_]+)_(\d{4}-\d{2}-\d{2})_(\d{2}-\d{2})_(\d+)\.xlsx$");
            var match = regex.Match(fileName);

            if (!match.Success)
            {
                return Result<FileNameInfo>.Failure(
                    "Неверный формат имени файла. Ожидается: Группа_{ИмяГруппы}_{Тип}_{YYYY-MM-DD}_{HH-MM}_{Порядковый}.xlsx " +
                    "(пример: Группа_U14-А_Игровая_2026-05-21_15-30_001.xlsx)", 400);
            }

            var groupName = match.Groups[1].Value;
            var type = match.Groups[2].Value;
            var date = match.Groups[3].Value;
            var time = match.Groups[4].Value;

            // Парсим дату и время
            var dateTimeStr = $"{date} {time.Replace('-', ':')}";
            if (!DateTime.TryParseExact(dateTimeStr, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateTime))
            {
                return Result<FileNameInfo>.Failure("Неверный формат даты или времени в имени файла", 400);
            }

            return Result<FileNameInfo>.Success(new FileNameInfo
            {
                GroupName = groupName,
                Type = type,
                DateTime = dateTime
            });
        }

        private async Task<Result<List<MetricCreateRequest>>> ParseExcelContent(IFormFile file, long trainingId, long groupId)
        {
            var metrics = new List<MetricCreateRequest>();

            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    var rowCount = worksheet.Dimension.Rows;

                    // Предполагаем что первая строка - заголовки
                    var headers = GetHeaders(worksheet);

                    // Парсим строки (начиная со 2-й)
                    for (int row = 2; row <= rowCount; row++)
                    {
                        var athleteName = GetCellValue(worksheet, row, headers, "Athlete Name", "Игрок");
                        if (string.IsNullOrEmpty(athleteName))
                            continue;

                        // Ищем спортсмена по FIO
                        var sportsman = await FindSportsmanByName(athleteName, groupId);
                        if (sportsman == null)
                        {
                            return Result<List<MetricCreateRequest>>.Failure(
                                $"Спортсмен '{athleteName}' не найден в группе. Проверьте ФИО в системе.", 404);
                        }

                        var metric = new MetricCreateRequest
                        {
                            TrainingId = trainingId,
                            SportsmanId = sportsman.Id,
                            Duration = ParseInt(GetCellValue(worksheet, row, headers, "Duration (s)", "Длительность (мин)")) * 60,
                            TotalDistance = ParseInt(GetCellValue(worksheet, row, headers, "Total Distance (m)", "Общая дистанция (м)")),
                            LowSpeedDistance = ParseInt(GetCellValue(worksheet, row, headers, "Low Speed Distance (m)", "Дистанция низкой интенсивности (м)")),
                            ModerateSpeedDistance = ParseInt(GetCellValue(worksheet, row, headers, "Moderate Speed Distance (m)", "Дистанция средней интенсивности (м)")),
                            HighSpeedDistance = ParseInt(GetCellValue(worksheet, row, headers, "High Speed Running Distance (m)", "Дистанция высокой интенсивности (м)")),
                            SprintDistance = ParseInt(GetCellValue(worksheet, row, headers, "Sprint Distance (m)", "Sprint Distance (м)")),
                            TimeInSpeedZone1 = ParseInt(GetCellValue(worksheet, row, headers, "Time In Speed Zone 1 (s)", "Время в зоне S1 (с)")),
                            TimeInSpeedZone2 = ParseInt(GetCellValue(worksheet, row, headers, "Time In Speed Zone 2 (s)", "Время в зоне S2 (с)")),
                            TimeInSpeedZone3 = ParseInt(GetCellValue(worksheet, row, headers, "Time In Speed Zone 3 (s)", "Время в зоне S3 (с)")),
                            TimeInSpeedZone4 = ParseInt(GetCellValue(worksheet, row, headers, "Time In Speed Zone 4 (s)", "Время в зоне S4 (с)")),
                            TimeInSpeedZone5 = ParseInt(GetCellValue(worksheet, row, headers, "Time In Speed Zone 5 (s)", "Время в зоне S5 (с)")),
                            TimeInSpeedZone6 = ParseInt(GetCellValue(worksheet, row, headers, "Time In Speed Zone 6 (s)", "Время в зоне S6 (с)")),
                            TimeInSpeedZone7 = ParseInt(GetCellValue(worksheet, row, headers, "Time In Speed Zone 7 (s)", "Время в зоне S7 (с)")),
                            DistanceInSpeedZone1 = ParseInt(GetCellValue(worksheet, row, headers, "Distance In Speed Zone 1 (m)", "Дистанция в зоне S1 (м)")),
                            DistanceInSpeedZone2 = ParseInt(GetCellValue(worksheet, row, headers, "Distance In Speed Zone 2 (m)", "Дистанция в зоне S2 (м)")),
                            DistanceInSpeedZone3 = ParseInt(GetCellValue(worksheet, row, headers, "Distance In Speed Zone 3 (m)", "Дистанция в зоне S3 (м)")),
                            DistanceInSpeedZone4 = ParseInt(GetCellValue(worksheet, row, headers, "Distance In Speed Zone 4 (m)", "Дистанция в зоне S4 (м)")),
                            DistanceInSpeedZone5 = ParseInt(GetCellValue(worksheet, row, headers, "Distance In Speed Zone 5 (m)", "Дистанция в зоне S5 (м)")),
                            DistanceInSpeedZone6 = ParseInt(GetCellValue(worksheet, row, headers, "Distance In Speed Zone 6 (m)", "Дистанция в зоне S6 (м)")),
                            DistanceInSpeedZone7 = ParseInt(GetCellValue(worksheet, row, headers, "Distance In Speed Zone 7 (m)", "Дистанция в зоне S7 (м)")),
                            AverageSpeed = ParseDouble(GetCellValue(worksheet, row, headers, "Average Speed (km/h)", "Средняя скорость (км/ч)")),
                            MaximumSpeed = ParseDouble(GetCellValue(worksheet, row, headers, "Maximum Speed (km/h)", "Максимальная скорость (км/ч)")),
                            AccelerationCount = ParseInt(GetCellValue(worksheet, row, headers, "Accelerations", "Количество ускорений")),
                            DecelerationCount = ParseInt(GetCellValue(worksheet, row, headers, "Decelerations", "Количество торможений")),
                            MaxAcceleration = ParseDouble(GetCellValue(worksheet, row, headers, "Max Acceleration (m/s²)", "Макс ускорение (м/с²)")),
                            MaxDeceleration = ParseDouble(GetCellValue(worksheet, row, headers, "Max Deceleration (m/s²)", "Макс торможение (м/с²)")),
                            SprintEfforts = ParseInt(GetCellValue(worksheet, row, headers, "Sprint Efforts", "Количество спринтов")),
                            HighSpeedEfforts = ParseInt(GetCellValue(worksheet, row, headers, "High Speed Efforts", "Усилия высокой скорости")),
                            PlayerLoad = ParseDouble(GetCellValue(worksheet, row, headers, "PlayerLoad", "Player Load")),
                            Energy = ParseDouble(GetCellValue(worksheet, row, headers, "Energy (kJ)", "Energy (kJ)")),
                            WorkRatio = ParseDouble(GetCellValue(worksheet, row, headers, "Work Ratio", "Work Ratio")),
                            MetabolicPower = ParseDouble(GetCellValue(worksheet, row, headers, "Metabolic Power Average (W/kg)", "Metabolic Power (W/kg)")),
                            Impacts = ParseInt(GetCellValue(worksheet, row, headers, "Impacts", "Impact Count")),
                            AverageSatellites = ParseInt(GetCellValue(worksheet, row, headers, "Average Satellites", "Среднее число спутников")),
                            AverageHDOP = ParseDouble(GetCellValue(worksheet, row, headers, "Average HDOP", "Average HDOP")),
                            Position = GetCellValue(worksheet, row, headers, "Position", "Игровая позиция") ?? "",
                            PositionGroup = GetCellValue(worksheet, row, headers, "Position Group", "Группа позиций") ?? "",
                            ExplosiveEfforts = ParseInt(GetCellValue(worksheet, row, headers, "Explosive Efforts", "Взрывные действия")),
                            AverageHeartRate = ParseInt(GetCellValue(worksheet, row, headers, "Average Heart Rate (bpm)", "Средний пульс (уд/мин)")),
                            MaxHeartRate = ParseInt(GetCellValue(worksheet, row, headers, "Max Heart Rate (bpm)", "Макс пульс (уд/мин)")),
                            HeartRateExertion = ParseDouble(GetCellValue(worksheet, row, headers, "Heart Rate Exertion", "Нагрузка по пульсу")),
                            TimeInHRZone1 = ParseInt(GetCellValue(worksheet, row, headers, "Time In HR Zone 1 (s)", "Время в зоне HR1 (с)")),
                            TimeInHRZone2 = ParseInt(GetCellValue(worksheet, row, headers, "Time In HR Zone 2 (s)", "Время в зоне HR2 (с)")),
                            TimeInHRZone3 = ParseInt(GetCellValue(worksheet, row, headers, "Time In HR Zone 3 (s)", "Время в зоне HR3 (с)")),
                            TimeInHRZone4 = ParseInt(GetCellValue(worksheet, row, headers, "Time In HR Zone 4 (s)", "Время в зоне HR4 (с)")),
                            TimeInHRZone5 = ParseInt(GetCellValue(worksheet, row, headers, "Time In HR Zone 5 (s)", "Время в зоне HR5 (с)")),
                            TimeInHRZone6 = ParseInt(GetCellValue(worksheet, row, headers, "Time In HR Zone 6 (s)", "Время в зоне HR6 (с)")),
                            TimeInHRRedZone = ParseInt(GetCellValue(worksheet, row, headers, "Time In HR Red Zone (s)", "Время в красной зоне HR (с)"))
                        };

                        metrics.Add(metric);
                    }
                }
            }

            return Result<List<MetricCreateRequest>>.Success(metrics);
        }

        private Dictionary<string, int> GetHeaders(ExcelWorksheet worksheet)
        {
            var headers = new Dictionary<string, int>();
            var colCount = worksheet.Dimension.Columns;

            for (int col = 1; col <= colCount; col++)
            {
                var headerValue = worksheet.Cells[1, col].Value?.ToString();
                if (!string.IsNullOrEmpty(headerValue))
                {
                    headers[headerValue] = col;
                }
            }

            return headers;
        }

        private string GetCellValue(ExcelWorksheet worksheet, int row, Dictionary<string, int> headers, params string[] possibleHeaders)
        {
            foreach (var header in possibleHeaders)
            {
                if (headers.TryGetValue(header, out var col))
                {
                    return worksheet.Cells[row, col].Value?.ToString();
                }
            }
            return null;
        }

        private async Task<Sportsman> FindSportsmanByName(string athleteName, long groupId)
        {
            // Пробуем найти точное совпадение
            var sportsman = await _context.Sportsmen
                .FirstOrDefaultAsync(s => s.SportsmanGroups.Any(sg => sg.GroupId == groupId) && s.FIO == athleteName);

            if (sportsman != null)
                return sportsman;

            // Если есть дата рождения в скобках: "Иванов Иван Иванович (15.03.2005)"
            var match = Regex.Match(athleteName, @"^(.+?)\s*\((\d{2}\.\d{2}\.\d{4})\)$");
            if (match.Success)
            {
                var fio = match.Groups[1].Value.Trim();
                var birthDateStr = match.Groups[2].Value;

                if (DateTime.TryParseExact(birthDateStr, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var birthDate))
                {
                    sportsman = await _context.Sportsmen
                        .FirstOrDefaultAsync(s => s.SportsmanGroups.Any(sg => sg.GroupId == groupId) && s.FIO == fio && s.BirthDate.Date == birthDate.Date);
                }
            }

            return sportsman;
        }

        private int ParseInt(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            if (int.TryParse(value, out var result))
                return result;

            return 0;
        }

        private double ParseDouble(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            if (double.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;

            return 0;
        }

        private class FileNameInfo
        {
            public string GroupName { get; set; }
            public string Type { get; set; }
            public DateTime DateTime { get; set; }
        }
    }
}
