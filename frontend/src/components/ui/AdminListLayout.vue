<template>
  <div class="p-3 h-full relative" @click="sortOpen = false">
    <AppCard no-border class="h-full flex flex-col">
      <div v-if="stripeColor" class="h-1 bg-gradient-to-r -mx-6 -mt-6 mb-2" :class="stripeClass" />

      <!-- Заголовок -->
      <div class="flex items-center justify-between mb-4 pb-4 border-b border-neutral-100 dark:border-theme-border flex-shrink-0">
        <div class="text-xl font-bold text-neutral-900 dark:text-theme-text">{{ title }}</div>
        <slot name="actions" />
      </div>

      <!-- Поиск + фильтр + сортировка -->
      <div class="flex flex-wrap md:flex-nowrap items-center gap-2 mb-3 flex-shrink-0">
        <div class="flex-1 min-w-[180px] relative">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"
            class="w-4 h-4 text-neutral-400 dark:text-theme-muted absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none">
            <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-4.35-4.35M17 11A6 6 0 115 11a6 6 0 0112 0z"/>
          </svg>
          <input
            :value="search"
            @input="$emit('update:search', ($event.target as HTMLInputElement).value)"
            type="text"
            placeholder="Поиск..."
            class="w-full pl-9 pr-3 py-2 text-sm rounded-xl border border-neutral-200 dark:border-theme-border bg-neutral-50 dark:bg-theme-surface-2 dark:text-theme-text focus:outline-none focus:border-blue-400 focus:bg-white dark:focus:bg-theme-surface transition-colors placeholder-neutral-300 dark:placeholder-theme-muted"
          />
        </div>
        <slot name="filter" />
        <div v-if="!noSort" class="flex items-center gap-1">
          <!-- Сортировка: три полоски = дропдаун (скрыть если noSortMenu) -->
          <div v-if="!noSortMenu" class="relative" @click.stop>
            <button
              @click.stop="sortOpen = !sortOpen"
              class="p-2 rounded-xl border border-neutral-200 dark:border-theme-border bg-neutral-50 dark:bg-theme-surface-2 hover:bg-neutral-100 dark:hover:bg-theme-surface transition-colors flex items-center gap-1.5"
            >
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500 dark:text-theme-text-2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M4 6h16M4 12h16M4 18h16"/>
              </svg>
              <span v-if="sortLabel" class="text-xs text-neutral-600 dark:text-theme-text-2">{{ sortLabel }}</span>
            </button>
            <div v-if="sortOpen" class="absolute right-0 top-full mt-1 bg-white dark:bg-theme-surface-2 border border-neutral-200 dark:border-theme-border rounded-xl shadow-lg z-20 min-w-[140px] py-1" @click.stop>
              <slot name="sort" :close="() => sortOpen = false" />
            </div>
          </div>
          <!-- Лейбл сортировки (только если noSortMenu — без дропдауна) -->
          <span
            v-if="noSortMenu && sortLabel"
            class="px-2.5 py-2 rounded-xl border border-neutral-200 dark:border-theme-border bg-neutral-50 dark:bg-theme-surface-2 text-xs text-neutral-600 dark:text-theme-text-2"
          >{{ sortLabel }}</span>
          <!-- Стрелка направления -->
          <button
            @click="$emit('toggleDir')"
            class="p-2 rounded-xl border border-neutral-200 dark:border-theme-border bg-neutral-50 dark:bg-theme-surface-2 hover:bg-neutral-100 dark:hover:bg-theme-surface transition-colors"
            title="Направление сортировки"
          >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500 dark:text-theme-text-2 transition-transform" :class="sortDir === 'desc' ? 'rotate-180' : ''">
              <path stroke-linecap="round" stroke-linejoin="round" d="M5 15l7-7 7 7"/>
            </svg>
          </button>
        </div>
      </div>

      <!-- Список -->
      <div class="flex-1 overflow-y-auto flex flex-col gap-1.5">
        <div v-if="loading" class="text-sm text-neutral-400 dark:text-theme-muted py-4 text-center">Загрузка...</div>
        <div v-else-if="!items.length" class="text-sm text-neutral-400 dark:text-theme-muted py-4 text-center">Нет записей</div>
        <slot v-else name="items" />
      </div>

      <!-- Пагинация -->
      <div class="flex items-center justify-between pt-3 mt-2 border-t border-neutral-100 dark:border-theme-border flex-shrink-0">
        <button @click="$emit('prev')" :disabled="page === 1" class="p-1.5 rounded-lg hover:bg-neutral-100 dark:hover:bg-theme-surface-2 disabled:opacity-30 transition-colors">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500 dark:text-theme-text-2"><path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/></svg>
        </button>
        <span class="text-xs text-neutral-500 dark:text-theme-text-2">{{ page }} / {{ totalPages }}</span>
        <button @click="$emit('next')" :disabled="page === totalPages" class="p-1.5 rounded-lg hover:bg-neutral-100 dark:hover:bg-theme-surface-2 disabled:opacity-30 transition-colors">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500 dark:text-theme-text-2"><path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/></svg>
        </button>
      </div>

    </AppCard>

    <!-- Toast -->
    <div
      v-if="toast"
      class="absolute top-6 left-1/2 -translate-x-1/2 text-white text-xs font-semibold px-4 py-2.5 rounded-2xl shadow-lg z-10 pointer-events-none whitespace-nowrap"
      :class="toast === 'saved' ? 'bg-green-600' : 'bg-neutral-600'"
    >
      {{ toast === 'saved' ? 'Изменения сохранены' : 'Запись удалена' }}
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import AppCard from '@/components/ui/AppCard.vue'

const sortOpen = ref(false)

const props = defineProps<{
  title: string
  search: string
  loading: boolean
  items: any[]
  page: number
  totalPages: number
  total: number
  sortDir?: 'asc' | 'desc'
  sortLabel?: string
  stripeColor?: 'blue' | 'green' | 'purple' | 'orange'
  toast?: 'saved' | 'deleted' | null
  noSort?: boolean
  noSortMenu?: boolean
}>()

const stripeClass = computed(() => {
  switch (props.stripeColor) {
    case 'green':
      return 'from-green-500 to-green-400'
    case 'purple':
      return 'from-purple-500 to-purple-400'
    case 'orange':
      return 'from-orange-500 to-orange-400'
    case 'blue':
    default:
      return 'from-blue-500 to-blue-400'
  }
})

defineEmits<{
  'update:search': [value: string]
  prev: []
  next: []
  toggleDir: []
}>()
</script>
