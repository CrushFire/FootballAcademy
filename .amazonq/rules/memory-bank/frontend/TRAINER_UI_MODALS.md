# Trainer UI — модалки и карточки

## Шапка модалок

Все модалки тренера используют компонент `ModalHeader.vue`:
- Фон: `bg-blue-50` (светло-голубой)
- Граница снизу: `border-b-2 border-neutral-300` (темная)
- Содержит: title, subtitle (опционально), info (опционально)

```vue
<ModalHeader 
  :title="group.name"
  :subtitle="group.description"
  :info="`Число спортсменов: ${sportsmen.length}`"
  @close="$emit('close')"
/>
```

## Карточки в модалках

Все карточки (спортсмены, матчи) имеют единый стиль:
- Граница: `border-2 border-neutral-300` (темная)
- Фон: `bg-gradient-to-r from-neutral-50 to-blue-50` (градиент серый → голубой)
- Hover: `hover:from-blue-50 hover:to-blue-100` (более голубой)
- Текст: `text-neutral-800` (основной), `text-neutral-500` (вторичный)

```vue
<router-link
  :to="`/trainer/sportsman/${s.id}`"
  class="block p-3 rounded-xl border-2 border-neutral-300 bg-gradient-to-r from-neutral-50 to-blue-50 hover:from-blue-50 hover:to-blue-100 transition-colors"
>
  <div class="text-sm font-semibold text-neutral-800">{{ s.fio }}</div>
  <div class="text-xs text-neutral-500 mt-0.5">{{ info }}</div>
</router-link>
```

## Где применяется

- `GroupDetailModal.vue` — карточки спортсменов
- `MatchDetailModal.vue` — информация о матче
- Все будущие модалки тренера
