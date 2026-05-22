<template>
  <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
    <div class="bg-white rounded-2xl shadow-lg max-w-2xl w-full max-h-[90vh] flex flex-col overflow-hidden">
      
      <!-- Синяя полоска сверху -->
      <div class="h-1 bg-gradient-to-r from-blue-500 to-blue-400" />

      <!-- Заголовок -->
      <ModalHeader 
        :title="group.name"
        :subtitle="group.description"
        :info="`Число спортсменов: ${sportsmen.length}`"
        @close="$emit('close')"
      />

      <!-- Поиск + сортировка -->
      <div class="flex items-center gap-2 p-4 border-b border-neutral-100 flex-shrink-0">
        <div class="flex-1 relative">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"
            class="w-4 h-4 text-neutral-500 absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none">
            <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-4.35-4.35M17 11A6 6 0 115 11a6 6 0 0112 0z"/>
          </svg>
          <input
            v-model="search"
            type="text"
            placeholder="Поиск спортсмена..."
            class="w-full pl-9 pr-3 py-2 text-sm rounded-xl border border-neutral-300 bg-slate-200 focus:outline-none focus:border-neutral-400 focus:bg-slate-100 transition-colors placeholder-neutral-400 text-neutral-800"
          />
        </div>
        <div class="flex items-center gap-1">
          <div class="relative sort-dropdown-modal">
            <button
              @click="sortOpen = !sortOpen"
              class="p-2 rounded-xl border border-neutral-300 bg-slate-200 hover:bg-slate-300 transition-colors flex items-center gap-1.5"
            >
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-600">
                <path stroke-linecap="round" stroke-linejoin="round" d="M4 6h16M4 12h16M4 18h16"/>
              </svg>
              <span class="text-xs text-neutral-700 font-medium">{{ currentSortLabel }}</span>
            </button>
            <div v-if="sortOpen" class="absolute right-0 top-full mt-1 bg-white border border-neutral-300 rounded-xl shadow-lg z-20 min-w-[140px] py-1">
              <button v-for="o in sortOptions" :key="o.value" @click="sortBy = o.value; sortOpen = false"
                class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-100 transition-colors"
                :class="sortBy === o.value ? 'text-neutral-800 font-semibold bg-neutral-100' : 'text-neutral-600'">{{ o.label }}</button>
            </div>
          </div>
          <button
            @click="sortDir = sortDir === 'asc' ? 'desc' : 'asc'"
            class="p-2 rounded-xl border border-neutral-300 bg-slate-200 hover:bg-slate-300 transition-colors"
            title="Направление сортировки"
          >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-600 transition-transform" :class="sortDir === 'desc' ? 'rotate-180' : ''">
              <path stroke-linecap="round" stroke-linejoin="round" d="M5 15l7-7 7 7"/>
            </svg>
          </button>
        </div>
      </div>

      <!-- Список спортсменов -->
      <div class="flex-1 overflow-y-auto p-4 space-y-2">
        <div v-if="loading" class="text-sm text-neutral-400 text-center py-4">Загрузка...</div>
        <div v-else-if="!filtered.length" class="text-sm text-neutral-400 text-center py-4">Нет спортсменов</div>
        <template v-else>
          <router-link
            v-for="s in filtered" :key="s.id"
            :to="`/trainer/sportsman/${s.id}`"
            class="block"
          >
            <ModalCard>
              <div class="text-sm font-semibold text-neutral-800">{{ s.fio }}</div>
              <div class="text-xs text-neutral-500 mt-0.5">
                {{ s.position ?? '—' }} · {{ s.age }} лет · {{ s.height }} см · {{ s.weight }} кг
              </div>
            </ModalCard>
          </router-link>
        </template>
      </div>

      <!-- Кнопка закрытия -->
      <div class="p-4 border-t border-neutral-100 flex-shrink-0">
        <button @click="$emit('close')" class="w-full px-4 py-2 rounded-xl border border-neutral-300 bg-neutral-100 text-neutral-600 hover:bg-neutral-200 transition-colors text-sm font-medium">
          Закрыть
        </button>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import api from '@/services/api'
import ModalHeader from './ModalHeader.vue'
import ModalCard from './ModalCard.vue'

const props = defineProps<{
  group: any
}>()

defineEmits<{
  close: []
}>()

const sortOptions = [
  { value: 'fio', label: 'По имени' },
  { value: 'age', label: 'По возрасту' },
  { value: 'height', label: 'По росту' },
  { value: 'weight', label: 'По весу' },
]

const loading = ref(true)
const sportsmen = ref<any[]>([])
const search = ref('')
const sortBy = ref('fio')
const sortDir = ref<'asc'|'desc'>('asc')
const sortOpen = ref(false)

const currentSortLabel = computed(() => sortOptions.find(o => o.value === sortBy.value)?.label ?? '')

function onClickOutside(e: MouseEvent) {
  const target = e.target as HTMLElement
  if (!target.closest('.sort-dropdown-modal')) sortOpen.value = false
}

onMounted(() => {
  document.addEventListener('click', onClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', onClickOutside)
})

const filtered = computed(() => {
  let list = sportsmen.value
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(s => s.fio?.toLowerCase().includes(q) || s.position?.toLowerCase().includes(q))
  }
  return [...list].sort((a, b) => {
    let res = 0
    if (sortBy.value === 'fio')     res = (a.fio ?? '').localeCompare(b.fio ?? '')
    else if (sortBy.value === 'age')    res = (a.age ?? 0) - (b.age ?? 0)
    else if (sortBy.value === 'height') res = (a.height ?? 0) - (b.height ?? 0)
    else if (sortBy.value === 'weight') res = (a.weight ?? 0) - (b.weight ?? 0)
    if (res === 0) res = (a.fio ?? '').localeCompare(b.fio ?? '')
    return sortDir.value === 'asc' ? res : -res
  })
})

onMounted(async () => {
  loading.value = true
  const res = await api.get(`/sportsman/group/${props.group.id}`).catch(() => null)
  sportsmen.value = res?.data?.data ?? []
  loading.value = false
})
</script>
