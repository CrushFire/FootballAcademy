# Enum сериализация в JSON — важная заметка

## Проблема

По умолчанию .NET сериализует enum как числа:

```json
"profiles": [2, 8, 9]
"status": 1
"position": 3
```

Фронт получает числа вместо читаемых строк — непонятно что означает `2` или `9`.

## Решение

В `Program.cs` добавлен глобальный конвертер:

```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()
        );
    });
```

Теперь все enum сериализуются как строки:

```json
"profiles": ["PowerPlayer", "CentralMidfielder", "DefensiveMidfielder"]
"status": "Finished"
"position": "CM"
```

## Где стоит в проекте

`backend/FootballAcademy/Program.cs` — сразу после `builder.Services.AddControllers()`.

## Затрагивает все enum проекта

- `PlayerProfile` — профили игрока
- `MatchStatus` — статус матча (Scheduled, InProgress, Finished)
- `GameType` — тип матча (Friendly, League, Cup, Tournament)
- `MatchResult` — результат (Win, Draw, Loss)
- `MatchEventType` — тип события (Goal, YellowCard, RedCard...)
- `Position` — позиция спортсмена (GK, CM, ST...)
- `SenderRole` — роль отправителя (Trainer, Admin, Medical)
- `BroadcastTargetType` — тип рассылки (All, Team, Group, Individual)
- `AttendanceStatus` — посещаемость (Present, Absent, Late)
- `AgeGroup` — возрастная группа (U10, U12, U14...)
- `Specialization` — специализация (Football, Minifootball)

## Правило для фронта

Фронт всегда получает и отправляет строки для enum:

```typescript
// ✅ правильно
status: "Finished"
position: "CM"

// ❌ неправильно (старое поведение)
status: 2
position: 3
```
