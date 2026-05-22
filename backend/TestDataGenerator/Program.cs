using OfficeOpenXml;

ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

var teams = new List<(string FileName, int Players)>
{
    ("Группа_Дошкольники5_Тренировка_2026-03-01_10-00_001.xlsx", 10),
    ("Группа_Подростки14_Тренировка_2026-03-01_12-30_001.xlsx", 16),
    ("Группа_Юниоры16_Тренировка_2026-03-01_15-00_001.xlsx", 22)
};

var headers = new string[]
{
    "Athlete Name","Duration (s)","Total Distance (m)","Low Speed Distance (m)","Moderate Speed Distance (m)",
    "High Speed Running Distance (m)","Sprint Distance (m)","Time In Speed Zone 1 (s)","Time In Speed Zone 2 (s)","Time In Speed Zone 3 (s)",
    "Time In Speed Zone 4 (s)","Time In Speed Zone 5 (s)","Time In Speed Zone 6 (s)","Time In Speed Zone 7 (s)",
    "Distance In Speed Zone 1 (m)","Distance In Speed Zone 2 (m)","Distance In Speed Zone 3 (m)","Distance In Speed Zone 4 (m)",
    "Distance In Speed Zone 5 (m)","Distance In Speed Zone 6 (m)","Distance In Speed Zone 7 (m)",
    "Average Speed (km/h)","Maximum Speed (km/h)","Accelerations","Decelerations","Max Acceleration (m/s²)","Max Deceleration (m/s²)",
    "Sprint Efforts","High Speed Efforts","PlayerLoad","Energy (kJ)","Work Ratio","Metabolic Power Average (W/kg)","Impacts",
    "Average Satellites","Average HDOP","Position","Position Group","Explosive Efforts",
    "Average Heart Rate (bpm)","Max Heart Rate (bpm)","Heart Rate Exertion","Time In HR Zone 1 (s)","Time In HR Zone 2 (s)",
    "Time In HR Zone 3 (s)","Time In HR Zone 4 (s)","Time In HR Zone 5 (s)","Time In HR Zone 6 (s)","Time In HR Red Zone (s)"
};

var random = new Random();

foreach (var team in teams)
{
    using var package = new ExcelPackage();
    var ws = package.Workbook.Worksheets.Add("Metrics");

    for (int i = 0; i < headers.Length; i++)
        ws.Cells[1, i + 1].Value = headers[i];

    for (int i = 0; i < team.Players; i++)
    {
        int col = 1;
        ws.Cells[i + 2, col++].Value = $"Фамилия{i + 1} Имя{i + 1} Отчество{i + 1}";
        ws.Cells[i + 2, col++].Value = random.Next(500, 800);
        ws.Cells[i + 2, col++].Value = random.Next(1500, 3000);
        ws.Cells[i + 2, col++].Value = random.Next(300, 700);
        ws.Cells[i + 2, col++].Value = random.Next(500, 1000);
        ws.Cells[i + 2, col++].Value = random.Next(500, 1000);
        ws.Cells[i + 2, col++].Value = random.Next(50, 150);

        for (int z = 0; z < 7; z++)
            ws.Cells[i + 2, col++].Value = random.Next(50, 200);

        for (int z = 0; z < 7; z++)
            ws.Cells[i + 2, col++].Value = random.Next(100, 500);

        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 15 + 5, 2);
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 25 + 10, 2);
        ws.Cells[i + 2, col++].Value = random.Next(1, 10);
        ws.Cells[i + 2, col++].Value = random.Next(1, 10);
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 3 + 1, 2);
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 3 + 1, 2);
        ws.Cells[i + 2, col++].Value = random.Next(1, 10);
        ws.Cells[i + 2, col++].Value = random.Next(5, 20);
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 50 + 50, 2);
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 400 + 100, 2);
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 3 + 1, 2);
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 10 + 5, 2);
        ws.Cells[i + 2, col++].Value = random.Next(0, 20);
        ws.Cells[i + 2, col++].Value = random.Next(5, 15);
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 2 + 1, 2);

        string[] positions = { "Goalkeeper", "Defender", "Midfield", "Forward" };
        string[] positionGroups = { "Goal", "Back", "Center", "Attack" };
        int posIndex = i % positions.Length;
        ws.Cells[i + 2, col++].Value = positions[posIndex];
        ws.Cells[i + 2, col++].Value = positionGroups[posIndex];
        ws.Cells[i + 2, col++].Value = random.Next(1, 10);

        ws.Cells[i + 2, col++].Value = random.Next(120, 180);
        ws.Cells[i + 2, col++].Value = random.Next(170, 200);
        ws.Cells[i + 2, col++].Value = Math.Round(random.NextDouble() * 200 + 100, 2);
        ws.Cells[i + 2, col++].Value = random.Next(50, 200);
        ws.Cells[i + 2, col++].Value = random.Next(50, 200);
        ws.Cells[i + 2, col++].Value = random.Next(50, 200);
        ws.Cells[i + 2, col++].Value = random.Next(50, 200);
        ws.Cells[i + 2, col++].Value = random.Next(50, 200);
        ws.Cells[i + 2, col++].Value = random.Next(50, 200);
        ws.Cells[i + 2, col++].Value = random.Next(10, 100);
    }

    package.SaveAs(new FileInfo(team.FileName));
    Console.WriteLine($"✅ Создан файл: {team.FileName}");
}

Console.WriteLine("\n🎉 Все файлы созданы!");
