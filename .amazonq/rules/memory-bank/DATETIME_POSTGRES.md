# PostgreSQL + DateTime — важная заметка

## Проблема

PostgreSQL через Npgsql требует чтобы все `DateTime` имели `Kind = Utc`.
Если передать `DateTime` с `Kind = Unspecified` (например `new DateTime(2010, 3, 15)`) — получишь ошибку:

```
Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone',
only UTC is supported.
```

## Решение

В `Program.cs` добавлена одна строка **до** `builder.Build()`:

```csharp
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
```

Это включает legacy-режим Npgsql где `DateTime.Unspecified` трактуется как UTC.

## Где стоит в проекте

```csharp
// Program.cs
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// PostgreSQL: все DateTime трактуем как UTC
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
```

## Правило для кода

Везде где создаётся `DateTime` вручную — использовать `DateTime.UtcNow` или `DateTime.SpecifyKind(..., DateTimeKind.Utc)`.

```csharp
// ❌ плохо
new DateTime(2010, 3, 15)

// ✅ хорошо
DateTime.SpecifyKind(new DateTime(2010, 3, 15), DateTimeKind.Utc)

// ✅ хорошо
DateTime.UtcNow
DateTime.UtcNow.AddDays(-7)
```
