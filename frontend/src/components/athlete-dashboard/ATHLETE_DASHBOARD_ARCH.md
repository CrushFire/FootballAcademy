# Архитектура дашборда спортсмена

## Структура

```
components/athlete-dashboard/AthleteDashboard.vue  ← общий компонент
composables/useAthleteDashboardData.ts             ← вся логика загрузки
pages/Sportsman/DashboardPage.vue                  ← обёртка mode="self"
pages/Trainer/SportsmanProfilePage.vue             ← обёртка mode="coach-view"
```

## Два режима

| mode | athleteId | Кто использует |
|---|---|---|
| `self` | из `/sportsman/me` | Спортсмен смотрит себя |
| `coach-view` | из `route.params.id` | Тренер смотрит спортсмена |

## Маршруты

```
/                              → Sportsman/DashboardPage (mode=self)
/trainer/sportsman/:id         → Trainer/SportsmanProfilePage (mode=coach-view)
```

## Режим coach-view

Сверху показывается "Карточка спортсмена":
- кнопка ← Назад к спортсменам
- ФИО, позиция, специализация, возраст, рост, вес, группа
- кнопка "Редактировать" — только height, weight, position

## Правило навигации внутри дашборда

Все переходы строятся от `effectiveId`:
```ts
// coach-view
router.push(`/trainer/sportsman/${effectiveId}/schedule`)

// self
router.push('/schedule')
```

## Роли

Тренер в системе хранится как `userRole === 'personal'` + `personalType === 'trainer'`.
Не проверять `userRole === 'trainer'` — такой роли нет.
