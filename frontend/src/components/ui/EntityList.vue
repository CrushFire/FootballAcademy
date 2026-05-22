<template>
  <div class="flex flex-col gap-3">

    <!-- Поиск + фильтр -->
    <div class="flex items-center gap-2">
      <div class="flex-1 relative">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"
          class="w-4 h-4 text-neutral-400 absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none">
          <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-4.35-4.35M17 11A6 6 0 115 11a6 6 0 0112 0z"/>
        </svg>
        <input
          v-model="search"
          type="text"
          :placeholder="searchPlaceholder"
          class="w-full pl-9 pr-3 py-2 text-sm rounded-xl border border-neutral-200 bg-neutral-50 focus:outline-none focus:border-blue-400 focus:bg-white transition-colors"
        />
      </div>
      <slot name="filters" />
    </div>

    <!-- Список -->
    <div v-if="!filtered.length" class="text-sm text-neutral-400">Нет записей</div>
    <div class="flex flex-col gap-1.5">
      <slot v-for="item in paged" :key="item.id" name="item" :item="item" />
    </div>

    <!-- Пагинация -->
    <div class="flex items-center justify-between pt-1">
      <button @click="page > 1 && page--" :disabled="page === 1" class="p-1.5 rounded-lg hover:bg-neutral-100 disabled:opacity-30 transition-colors">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/></svg>
      </button>
      <span class="text-xs text-neutral-500">{{ page }} / {{ totalPages }} · {{ filtered.length }} записей</span>
      <button @click="page < totalPages && page++" :disabled="page === totalPages" class="p-1.5 rounded-lg hover:bg-neutral-100 disabled:opacity-30 transition-colors">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/></svg>
      </button>
    </div>

  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue'

const props = defineProps<{
  items: any[]
  searchKeys: string[]
  searchPlaceholder?: string
  perPage?: number
}>()

const search = ref('')
const page = ref(1)
const PER_PAGE = props.perPage ?? 10

const filtered = computed(() => {
  if (!search.value.trim()) return props.items
  const q = search.value.toLowerCase()
  return props.items.filter(item =>
    props.searchKeys.some(k => String(item[k] ?? '').toLowerCase().includes(q))
  )
})

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / PER_PAGE)))
const paged = computed(() => filtered.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

watch(search, () => { page.value = 1 })
</script>
