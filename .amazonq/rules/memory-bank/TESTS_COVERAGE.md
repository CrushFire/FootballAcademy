# Автотесты — покрытие и структура

## Итог

**689 тестов — все зелёные**

| Категория | Тестов |
|---|---|
| Ручные тесты (UserService, GroupService, TeamService и т.д.) | ~111 |
| Аналитика (MainMetric, Medical, Profile, Graph, Pentagon) | ~48 |
| FakerTests (10 seed × 13 сервисов) | 130 |
| FakerExtendedTests (20 seed × 9 сервисов × 2 типа) | 360 |
| **Итого** | **689** |

## Структура файлов

| Файл | Что тестирует | Тестов |
|---|---|---|
| `UserServiceTests.cs` | User CRUD + граничные случаи | ~14 |
| `GroupServiceTests.cs` | Group CRUD + дубли + тренер | ~13 |
| `TeamServiceTests.cs` | Team CRUD + спортсмены | ~12 |
| `ClassServiceTests.cs` | Class CRUD + дубли + время | ~12 |
| `NormativeServiceTests.cs` | Normative + LocalNormative + результаты | ~15 |
| `MatchServiceTests.cs` | Match CRUD + события + статусы | ~16 |
| `MessageServiceTests.cs` | Message + Broadcast + диалог | ~16 |
| `SchedualeServiceTests.cs` | Attendance + перезапись | ~9 |
| `MainMetricServiceTests.cs` | Средние метрики + конкретная тренировка | ~10 |
| `MedicalMetricServiceTests.cs` | Медицинские проверки + пороги | ~11 |
| `ProfileServiceTests.cs` | Профили игрока (Sprinter, Forward и т.д.) | ~7 |
| `GraphServiceTests.cs` | Временные точки для графиков | ~7 |
| `PentagonServiceTests.cs` | Пятиугольник + позиции + clamp [0,1] | ~9 |
| `FakerTests.cs` | Автогенерация Bogus, 10 seed × 13 сервисов | 130 |
| `FakerExtendedTests.cs` | Валид + невалид, 20 seed × 9 сервисов × 2 | 360 |

## Типы тестов

### Ручные тесты (~151)
- Конкретные сценарии с заранее известными данными
- Граничные значения (0, null, максимум, минимум)
- Проверка конкретных бизнес-правил (порог 500, диапазон минут 0-130 и т.д.)
- Ожидаемые коды ошибок (404, 409, 400, 403)

### Faker-тесты (490)
- Библиотека **Bogus** для генерации случайных данных
- Каждый тест запускается с фиксированным seed → воспроизводимо
- `FakerTests` — только валидные данные, ожидание Success
- `FakerExtendedTests` — валидные (Success) + невалидные (404/409)

## Покрытые сервисы

| Сервис | Ручные | Faker валид | Faker невалид |
|---|---|---|---|
| UserService | ✅ | ✅ | ✅ |
| GroupService | ✅ | ✅ | ✅ |
| TeamService | ✅ | ✅ | ✅ |
| TrainingService | ✅ | ✅ | ✅ |
| ClassService | ✅ | ✅ | ✅ |
| NormativeService | ✅ | ✅ | ✅ |
| MatchService | ✅ | ✅ | ✅ |
| MessageService | ✅ | ✅ | ✅ |
| SchedualeService | ✅ | ✅ | ✅ |
| BroadcastService | ✅ | ✅ | ✅ |
| MainMetricService | ✅ | ✅ | — |
| MedicalMetricService | ✅ | ✅ | — |
| ProfileService | ✅ | — | — |
| GraphService | ✅ | — | — |
| PentagonService | ✅ | ✅ | — |

## Как запустить

```bash
cd backend/Tests
dotnet test
```

С подробным выводом:
```bash
dotnet test --verbosity normal
```

## Зависимости тестов

- `xUnit` — фреймворк тестирования
- `Microsoft.EntityFrameworkCore.InMemory` — InMemory БД вместо PostgreSQL
- `AutoMapper 12.0.1` — маппинг как в проде
- `Bogus 35.6.1` — генерация случайных данных