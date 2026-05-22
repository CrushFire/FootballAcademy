# Адаптивность — основы

## Стратегия: Mobile First

Пишем базовые стили под мобилку, потом расширяем через брейкпоинты.

```html
<!-- сначала 1 колонка, на md — 2, на lg — 3 -->
<div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3">
```

## Брейкпоинты Tailwind

| Префикс | Ширина | Устройство |
|---|---|---|
| (без) | 0px+ | мобилка |
| `sm:` | 640px+ | большой телефон |
| `md:` | 768px+ | планшет |
| `lg:` | 1024px+ | десктоп |
| `xl:` | 1280px+ | широкий экран |

## Сайдбар

- **мобилка** — скрыт, открывается по бургеру (`hidden md:flex`)
- **планшет** — иконки `w-14`
- **десктоп** — hover раскрытие до `w-64`

```html
<aside class="hidden md:flex ...">
```

Бургер в хедере — только на мобилке:
```html
<button class="md:hidden ...">☰</button>
```

## Таблицы

На мобилке таблицы — боль. Два варианта:

**1. Горизонтальный скролл:**
```html
<div class="overflow-x-auto">
  <table class="min-w-full ...">
```

**2. Карточки на мобилке (лучше для спортсменов):**
```html
<!-- таблица только на md+ -->
<table class="hidden md:table ...">
<!-- карточки только на мобилке -->
<div class="md:hidden space-y-3 ...">
```

## Формы

На мобилке — одна колонка, на десктопе — две:
```html
<div class="grid grid-cols-1 md:grid-cols-2 gap-4">
```

## Хедер на мобилке

```
[ ☰ ]  Название страницы  [ ID ]
```

```html
<header class="flex items-center justify-between px-4 md:px-6">
  <button class="md:hidden">☰</button>
  <h2>{{ title }}</h2>
  <span class="text-sm text-muted">{{ userId }}</span>
</header>
```

## Типичные паттерны

```html
<!-- скрыть на мобилке -->
<div class="hidden md:block">

<!-- показать только на мобилке -->
<div class="md:hidden">

<!-- отступы адаптивные -->
<div class="p-4 md:p-6 lg:p-8">

<!-- текст адаптивный -->
<h1 class="text-lg md:text-xl lg:text-2xl">
```

## Что делать сразу

- [ ] Сайдбар скрывать на мобилке (`hidden md:flex`)
- [ ] Бургер в хедере (`md:hidden`)
- [ ] Таблицы оборачивать в `overflow-x-auto`
- [ ] Сетки через `grid-cols-1 md:grid-cols-2 lg:grid-cols-3`
- [ ] Формы в одну колонку на мобилке
