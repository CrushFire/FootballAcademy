# Маршруты тренера/медика на страницы спортсмена

## Правило

Когда тренер или медик открывает страницу конкретного спортсмена — маршрут должен вести на **реальную страницу спортсмена**, а не на заглушку `Trainer/*.vue` или `Medical/*.vue`.

## ❌ Неправильно

```ts
{
  path: 'trainer/sportsman/:id/schedule',
  component: () => import('@/pages/Trainer/SchedulePage.vue') // заглушка!
}
```

## ✅ Правильно

```ts
{
  path: 'trainer/sportsman/:id/schedule',
  component: () => import('@/pages/SchedulePage.vue') // реальная страница
}
{
  path: 'trainer/sportsman/:id/trainings',
  component: () => import('@/pages/Sportsman/TrainingsPage.vue')
}
{
  path: 'trainer/sportsman/:id/matches',
  component: () => import('@/pages/MatchesPage.vue')
}
{
  path: 'trainer/sportsman/:id/normatives',
  component: () => import('@/pages/NormativesPage.vue')
}
```

## Как страницы узнают чьи данные грузить

Все реальные страницы спортсмена читают `route.params.id`:

```ts
const route = useRoute()
const routeId = route.params.id ? Number(route.params.id) : null
// если есть id в route — грузим по нему
// если нет — грузим через /sportsman/me (self-режим)
const sportsmanId = routeId ?? meRes?.data?.data?.id
```

## Текущие маршруты тренера на страницы спортсмена

| Маршрут | Компонент |
|---|---|
| `trainer/sportsman/:id` | `Trainer/SportsmanProfilePage.vue` (обёртка AthleteDashboard) |
| `trainer/sportsman/:id/schedule` | `SchedulePage.vue` |
| `trainer/sportsman/:id/trainings` | `Sportsman/TrainingsPage.vue` |
| `trainer/sportsman/:id/matches` | `MatchesPage.vue` |
| `trainer/sportsman/:id/normatives` | `NormativesPage.vue` |

## Для медика — то же самое

Когда медик открывает спортсмена:
```ts
{
  path: 'medical/sportsman/:id/...',
  component: () => import('@/pages/SchedulePage.vue') // не Medical/SchedulePage.vue!
}
```

## Итог

Заглушки `Trainer/*.vue` и `Medical/*.vue` — только для собственных страниц тренера/медика (их список, их расписание и т.д.).
Для просмотра данных конкретного спортсмена — всегда реальные страницы из `pages/` или `pages/Sportsman/`.
