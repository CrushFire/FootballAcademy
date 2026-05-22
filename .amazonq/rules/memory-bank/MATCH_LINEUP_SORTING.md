# Состав матча — логика сортировки Main/Reserve

## Проблема

Нужно хранить информацию о том, кто основной игрок на позиции, а кто запасной (для замены), без дублирования статуса в событиях.

## Решение

В `LineupEntry` добавлено поле `Type`:
```csharp
public enum PlayerType { Main, Reserve }

public class LineupEntry
{
    public long SportsmanId { get; set; }
    public string Position { get; set; }
    public PlayerType Type { get; set; } = PlayerType.Main;
}
```

## Логика на бэкенде

При обновлении состава (`UpdateLineupAsync`) список автоматически сортируется:
1. Сначала все `Main` игроки
2. Потом все `Reserve` игроки
3. Внутри каждой группы — по позиции

```csharp
var sortedLineup = lineup
    .OrderBy(l => l.Type == PlayerType.Main ? 0 : 1)
    .ThenBy(l => l.Position)
    .ToList();
```

## Логика на фронте

Компонент `MatchDetailPage.vue` группирует игроков по позициям:
- Для каждой позиции первый в списке — основной игрок
- Второй (если есть) — запасной, отображается под основным с зелёной рамкой

```ts
// Группируем по позициям
const positionGroups = new Map<string, { main: any | null; reserve: any | null }>()

lineup.forEach((entry) => {
  const group = positionGroups.get(entry.position)!
  if (entry.type === 'Reserve' || group.main !== null) {
    group.reserve = entry
  } else {
    group.main = entry
  }
})
```

## Компонент поля

`FootballField.vue` отображает:
- Основного игрока — белый круг с синей рамкой
- Запасного (если есть) — зелёный круг под основным, появляется при клике

## Миграция

```bash
dotnet ef migrations add AddPlayerTypeToLineup --project DataAccess --startup-project FootballAcademy
```

## Правило

- При создании состава всегда указывать `type: "Main"` или `type: "Reserve"`
- Бэкенд автоматически отсортирует список при сохранении
- Фронт определяет замены по порядку: второй игрок на позиции = запасной
