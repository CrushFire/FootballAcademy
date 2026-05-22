# Frontend — UI зафиксированные решения

## Layout (AppLayout.vue)

Структура: `flex-col h-screen` → AppHeader → `flex flex-1 overflow-hidden` → AppNav + `main overflow-y-auto` → AppFooter

- Фон лейаута: `#f1f5f9`
- Хедер: `bg-blue-600`, высота 60px, `box-shadow: 0 2px 8px rgba(0,0,0,0.08)`
- Навигация: `background: #3b82f6`, иконки/текст `text-white/80`, hover `bg-blue-700`, active `bg-blue-800`
- Футер: `background: #f1f5f9`, `border-t border-neutral-200`, `shadow-[0_-2px_8px_rgba(0,0,0,0.06)]`
- Сайдбар раскрывается при hover: 64px → 220px

## AppCard (компонент)

```vue
<div class="bg-white rounded-2xl p-6 shadow-sm border border-neutral-200">
  <slot />
</div>
```

Путь: `src/components/ui/AppCard.vue`

## DashboardPage

Одна большая `AppCard` с отступом `p-3` от рамки.

Внутри карточки:
- Заголовок + дата (с `border-b border-neutral-100 mb-6 pb-4`)
- Сетка `grid-cols-3`:
  - Левая колонка (`col-span-1`): пятиугольник + легенда, отделён `border-r border-neutral-100`
  - Правая часть (`col-span-2`): сетка `grid-cols-2` — занятие, матч, нормативы, посещаемость
  - Каждый блок: `p-4 rounded-xl border border-neutral-100 hover:bg-neutral-50`

## Tailwind

- Версия: **v3**
- Конфиг: `tailwind.config.js` (работает)
- CSS переменные в `main.css` для кастомных утилит
- **НЕ использовать** `* { color: var(--color-foreground) }` — ломает цвета

## Dev мок авторизации

В `src/store/auth.ts` — при `import.meta.env.DEV` логин без запроса к API (любой email/пароль).
