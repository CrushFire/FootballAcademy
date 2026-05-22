# CoachAthleteHeader — UI паттерн

## Компонент

`src/components/athlete-dashboard/CoachAthleteHeader.vue`

## Что делает

Шапка страницы спортсмена для тренера:
- Синяя градиентная полоска сверху (`h-1 bg-gradient-to-r from-blue-500 to-blue-400`)
- Кнопка "К списку" — светло-синяя (`border-blue-200 bg-blue-50 text-blue-600`)
- Аватар с инициалами (`bg-blue-100 text-blue-600`)
- ФИО, позиция (бейдж), специализация, возраст, рост, вес, группа
- Кнопка "Редактировать" — зелёная (`border-green-200 bg-green-50 text-green-600`)

## Паттерн синей полоски

Используется для визуального акцента на карточках тренера:

```html
<div class="bg-white rounded-2xl border border-neutral-200 shadow-sm overflow-hidden">
  <div class="h-1 bg-gradient-to-r from-blue-500 to-blue-400" />
  <!-- контент -->
</div>
```

Можно переиспользовать на других важных карточках.
