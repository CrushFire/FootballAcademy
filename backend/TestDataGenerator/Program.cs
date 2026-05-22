using System.Globalization;
using Npgsql;
using OfficeOpenXml;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// === CLI args ===
string connectionString = GetArg(args, "--connection")
    ?? "Host=localhost;Port=5432;Database=FootballAcademy_DB;Username=postgres;Password=14041404";
string outDir = GetArg(args, "--out") ?? "output";
int limit = int.TryParse(GetArg(args, "--limit"), out var l) ? l : int.MaxValue;
bool onlyEmpty = !args.Contains("--all"); // по умолчанию только тренировки без метрик

Directory.CreateDirectory(outDir);

await using var conn = new NpgsqlConnection(connectionString);
await conn.OpenAsync();

// === Берём тренировки + группу + тренера, опционально только те, у кого ещё нет метрик ===
var trainings = new List<TrainingRow>();
var sql = @"
    SELECT t.""Id"", t.""GroupId"", t.""Type"", t.""Date"", g.""Name""
    FROM ""Trainings"" t
    JOIN ""Groups"" g ON g.""Id"" = t.""GroupId""
    WHERE (@onlyEmpty = false OR NOT EXISTS (
        SELECT 1 FROM ""Metrics"" m WHERE m.""TrainingId"" = t.""Id""
    ))
    ORDER BY t.""Date"" DESC
    LIMIT @lim;";

await using (var cmd = new NpgsqlCommand(sql, conn))
{
    cmd.Parameters.AddWithValue("onlyEmpty", onlyEmpty);
    cmd.Parameters.AddWithValue("lim", limit);
    await using var rd = await cmd.ExecuteReaderAsync();
    while (await rd.ReadAsync())
    {
        trainings.Add(new TrainingRow
        {
            Id = rd.GetInt64(0),
            GroupId = rd.GetInt64(1),
            Type = rd.GetString(2),
            Date = rd.GetDateTime(3),
            GroupName = rd.GetString(4)
        });
    }
}

if (trainings.Count == 0)
{
    Console.WriteLine("Нет подходящих тренировок (попробуй --all для всех или --limit N).");
    return;
}

Console.WriteLine($"Найдено тренировок: {trainings.Count}. Папка вывода: {Path.GetFullPath(outDir)}");

var random = new Random();
int created = 0, skipped = 0;

foreach (var t in trainings)
{
    // Парсер ожидает: имя группы и тип — одно слово без '_'
    if (t.GroupName.Contains('_') || t.Type.Contains('_'))
    {
        Console.WriteLine($"  SKIP id={t.Id}: '_' в имени группы/типе ({t.GroupName} / {t.Type})");
        skipped++;
        continue;
    }

    // Список спортсменов этой группы
    var sportsmen = new List<string>();
    await using (var cmd = new NpgsqlCommand(
        @"SELECT s.""FIO"" FROM ""SportsmanGroups"" sg
          JOIN ""Sportsmen"" s ON s.""Id"" = sg.""SportsmanId""
          WHERE sg.""GroupId"" = @gid
          ORDER BY s.""FIO"";", conn))
    {
        cmd.Parameters.AddWithValue("gid", t.GroupId);
        await using var rd = await cmd.ExecuteReaderAsync();
        while (await rd.ReadAsync()) sportsmen.Add(rd.GetString(0));
    }

    if (sportsmen.Count == 0)
    {
        Console.WriteLine($"  SKIP id={t.Id} ({t.GroupName} {t.Type} {t.Date:yyyy-MM-dd HH:mm}): нет спортсменов в группе");
        skipped++;
        continue;
    }

    var fileName = $"Группа_{t.GroupName}_{t.Type}_{t.Date:yyyy-MM-dd}_{t.Date:HH-mm}_001.xlsx";
    var filePath = Path.Combine(outDir, fileName);

    WriteExcel(filePath, sportsmen, random);
    Console.WriteLine($"  OK   {fileName} — {sportsmen.Count} строк");
    created++;
}

Console.WriteLine($"\nГотово. Создано: {created}, пропущено: {skipped}");
Console.WriteLine($"Чтобы импортировать — скопируй файлы в backend/FootballAcademy/uploads/metrics/ и вызови автоимпорт.");

// === helpers ===

static string? GetArg(string[] args, string name)
{
    var i = Array.IndexOf(args, name);
    return (i >= 0 && i + 1 < args.Length) ? args[i + 1] : null;
}

static void WriteExcel(string path, List<string> sportsmen, Random random)
{
    var headers = new[]
    {
        "Athlete Name","Duration (s)","Total Distance (m)","Low Speed Distance (m)","Moderate Speed Distance (m)",
        "High Speed Running Distance (m)","Sprint Distance (m)",
        "Time In Speed Zone 1 (s)","Time In Speed Zone 2 (s)","Time In Speed Zone 3 (s)",
        "Time In Speed Zone 4 (s)","Time In Speed Zone 5 (s)","Time In Speed Zone 6 (s)","Time In Speed Zone 7 (s)",
        "Distance In Speed Zone 1 (m)","Distance In Speed Zone 2 (m)","Distance In Speed Zone 3 (m)","Distance In Speed Zone 4 (m)",
        "Distance In Speed Zone 5 (m)","Distance In Speed Zone 6 (m)","Distance In Speed Zone 7 (m)",
        "Average Speed (km/h)","Maximum Speed (km/h)","Accelerations","Decelerations",
        "Max Acceleration (m/s²)","Max Deceleration (m/s²)",
        "Sprint Efforts","High Speed Efforts","PlayerLoad","Energy (kJ)","Work Ratio","Metabolic Power Average (W/kg)","Impacts",
        "Average Satellites","Average HDOP","Position","Position Group","Explosive Efforts",
        "Average Heart Rate (bpm)","Max Heart Rate (bpm)","Heart Rate Exertion",
        "Time In HR Zone 1 (s)","Time In HR Zone 2 (s)","Time In HR Zone 3 (s)",
        "Time In HR Zone 4 (s)","Time In HR Zone 5 (s)","Time In HR Zone 6 (s)","Time In HR Red Zone (s)"
    };

    using var package = new ExcelPackage();
    var ws = package.Workbook.Worksheets.Add("Metrics");

    for (int i = 0; i < headers.Length; i++)
        ws.Cells[1, i + 1].Value = headers[i];

    string[] positions = { "Goalkeeper", "Defender", "Midfield", "Forward" };
    string[] positionGroups = { "Goal", "Back", "Center", "Attack" };

    for (int i = 0; i < sportsmen.Count; i++)
    {
        int posIdx = i % positions.Length;
        int col = 1;

        ws.Cells[i + 2, col++].Value = sportsmen[i];
        ws.Cells[i + 2, col++].Value = random.Next(3000, 5400);           // Duration (s) ≈ 50–90 мин
        ws.Cells[i + 2, col++].Value = random.Next(4000, 9000);           // Total Distance
        ws.Cells[i + 2, col++].Value = random.Next(800, 2000);            // Low
        ws.Cells[i + 2, col++].Value = random.Next(1500, 3000);           // Moderate
        ws.Cells[i + 2, col++].Value = random.Next(800, 2000);            // High
        ws.Cells[i + 2, col++].Value = random.Next(100, 500);             // Sprint

        for (int z = 0; z < 7; z++)
            ws.Cells[i + 2, col++].Value = random.Next(50, 600);          // Time In Speed Zones 1-7

        for (int z = 0; z < 7; z++)
            ws.Cells[i + 2, col++].Value = random.Next(100, 1500);        // Distance In Speed Zones 1-7

        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 4 + 4, 2);    // AvgSpeed km/h
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 12 + 22, 2);  // MaxSpeed km/h
        ws.Cells[i + 2, col++].Value = random.Next(20, 60);               // Accelerations
        ws.Cells[i + 2, col++].Value = random.Next(20, 60);               // Decelerations
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 2 + 2, 2);    // Max Acc
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 2 + 2, 2);    // Max Dec
        ws.Cells[i + 2, col++].Value = random.Next(3, 15);                // Sprint Efforts
        ws.Cells[i + 2, col++].Value = random.Next(10, 40);               // High Speed Efforts
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 200 + 200, 2);  // PlayerLoad
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 800 + 400, 2);  // Energy kJ
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 1.5 + 1.5, 2);  // Work Ratio
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 4 + 7, 2);      // Metabolic Power
        ws.Cells[i + 2, col++].Value = random.Next(0, 20);                // Impacts
        ws.Cells[i + 2, col++].Value = random.Next(8, 14);                // Avg Satellites
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 1.5 + 0.6, 2); // HDOP
        ws.Cells[i + 2, col++].Value = positions[posIdx];
        ws.Cells[i + 2, col++].Value = positionGroups[posIdx];
        ws.Cells[i + 2, col++].Value = random.Next(1, 10);                // Explosive Efforts

        ws.Cells[i + 2, col++].Value = random.Next(130, 170);             // Avg HR
        ws.Cells[i + 2, col++].Value = random.Next(180, 200);             // Max HR
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 200 + 100, 2); // HR Exertion
        for (int z = 0; z < 6; z++)
            ws.Cells[i + 2, col++].Value = random.Next(50, 400);          // HR Zones 1-6
        ws.Cells[i + 2, col++].Value = random.Next(10, 100);              // HR Red Zone
    }

    package.SaveAs(new FileInfo(path));
}

record TrainingRow
{
    public long Id { get; set; }
    public long GroupId { get; set; }
    public string Type { get; set; } = "";
    public DateTime Date { get; set; }
    public string GroupName { get; set; } = "";
}
