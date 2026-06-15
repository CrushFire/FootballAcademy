<template>
  <div class="p-3 h-full overflow-y-auto">
    <AppCard no-border class="min-h-full flex flex-col">

      <!-- Шапка -->
      <div class="flex items-start justify-between -mx-4 -mt-4 px-5 py-4 mb-5 rounded-t-2xl border-b border-neutral-100">
        <div>
          <h1 class="text-xl font-bold text-neutral-900">Индивидуальные задания</h1>
          <p class="text-sm text-neutral-500 mt-0.5">Рекомендации и упражнения от тренеров</p>
        </div>
        <button @click="$router.back()"
          class="flex items-center gap-1.5 px-3 py-2 rounded-xl border border-blue-200 bg-blue-50 text-blue-600 text-xs font-semibold hover:bg-blue-100 transition-colors shrink-0">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
          </svg>
          Назад
        </button>
      </div>

      <!-- Сводка -->
      <div class="grid grid-cols-2 md:grid-cols-4 gap-3 mb-5">
        <div class="rounded-2xl border border-neutral-200 p-4">
          <div class="text-xs font-bold text-neutral-500 uppercase tracking-wide">Всего заданий</div>
          <div class="text-2xl font-extrabold text-neutral-900 mt-1">{{ workouts.length }}</div>
        </div>
        <div class="rounded-2xl border border-green-100 bg-green-50 p-4">
          <div class="text-xs font-bold text-green-700 uppercase tracking-wide">За месяц</div>
          <div class="text-2xl font-extrabold text-green-700 mt-1">{{ recentCount }}</div>
        </div>
        <div class="rounded-2xl border border-blue-100 bg-blue-50 p-4">
          <div class="text-xs font-bold text-blue-600 uppercase tracking-wide">Тренеров</div>
          <div class="text-2xl font-extrabold text-blue-700 mt-1">{{ trainersCount }}</div>
        </div>
        <div class="rounded-2xl border border-violet-100 bg-violet-50 p-4">
          <div class="text-xs font-bold text-violet-600 uppercase tracking-wide">Последнее</div>
          <div class="text-sm font-bold text-violet-700 mt-1.5 truncate">{{ lastDateLabel }}</div>
        </div>
      </div>

      <!-- Поиск + фильтры -->
      <div class="flex flex-wrap items-center gap-2 mb-4">
        <div class="relative flex-1 min-w-[220px]">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"
               class="w-4 h-4 text-neutral-400 absolute left-3 top-1/2 -translate-y-1/2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/>
          </svg>
          <input
            v-model="search"
            type="text"
            placeholder="Поиск по тексту задания или тренеру..."
            class="w-full text-sm rounded-xl border border-neutral-200 bg-white pl-9 pr-3 py-2 focus:outline-none focus:border-blue-400"
          />
        </div>

        <select v-model="trainerFilter"
                class="text-xs rounded-xl border border-neutral-200 bg-white px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-700">
          <option value="">Все тренеры</option>
          <option v-for="t in trainersList" :key="t.id" :value="t.id">{{ t.fio }}</option>
        </select>

        <select v-model="periodFilter"
                class="text-xs rounded-xl border border-neutral-200 bg-white px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-700">
          <option value="all">Всё время</option>
          <option value="month">За месяц</option>
          <option value="quarter">За 3 мес.</option>
          <option value="year">За год</option>
        </select>

        <div class="flex items-center rounded-xl border border-neutral-200 bg-white overflow-hidden">
          <button v-for="o in sortOptions" :key="o.value"
                  @click="sortBy = o.value"
                  class="text-xs px-3 py-2 font-semibold transition-colors"
                  :class="sortBy === o.value ? 'bg-blue-500 text-white' : 'text-neutral-600 hover:bg-neutral-50'">
            {{ o.label }}
          </button>
        </div>

        <button @click="sortDir = sortDir === 'asc' ? 'desc' : 'asc'"
                class="w-9 h-9 rounded-xl border border-neutral-200 bg-white flex items-center justify-center text-neutral-500 hover:bg-neutral-50 transition-colors"
                :title="sortDir === 'desc' ? 'Сначала новые / Я→А' : 'Сначала старые / А→Я'">
          <svg v-if="sortDir === 'desc'" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
            <path stroke-linecap="round" stroke-linejoin="round" d="M19 14l-7 7m0 0l-7-7m7 7V3"/>
          </svg>
          <svg v-else xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
            <path stroke-linecap="round" stroke-linejoin="round" d="M5 10l7-7m0 0l7 7m-7-7v18"/>
          </svg>
        </button>
      </div>

      <!-- Список -->
      <div v-if="loading" class="flex-1 flex items-center justify-center text-sm text-neutral-400">Загрузка...</div>
      <div v-else-if="!filtered.length" class="flex-1 flex flex-col items-center justify-center text-center py-10">
        <div class="w-16 h-16 rounded-2xl bg-neutral-100 flex items-center justify-center mb-3">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5" class="w-8 h-8 text-neutral-400">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 12h6m-6 4h6m2 5H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z"/>
          </svg>
        </div>
        <div class="text-sm font-semibold text-neutral-700">
          {{ workouts.length ? 'Ничего не найдено' : 'Заданий пока нет' }}
        </div>
        <div class="text-xs text-neutral-400 mt-1">
          {{ workouts.length ? 'Попробуйте сбросить фильтры' : 'Тренер пока не назначил вам персональных рекомендаций' }}
        </div>
      </div>

      <div v-else class="flex flex-col gap-2.5 flex-1">
        <div
          v-for="w in paged" :key="w.id"
          @click="open = w"
          class="group rounded-2xl border border-neutral-200 bg-white p-4 cursor-pointer hover:border-blue-300 hover:shadow-sm transition-all"
        >
          <div class="flex items-start gap-3">
            <div class="w-10 h-10 rounded-xl bg-violet-100 flex items-center justify-center text-violet-600 shrink-0">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
              </svg>
            </div>
            <div class="flex-1 min-w-0">
              <div class="flex items-center justify-between gap-2 flex-wrap">
                <div class="flex items-center gap-2 min-w-0">
                  <span class="text-sm font-bold text-neutral-900 truncate">{{ w.personalFIO }}</span>
                  <span v-if="isRecent(w.createdAt)" class="text-[10px] font-bold uppercase tracking-wider px-1.5 py-0.5 rounded-full bg-green-100 text-green-700 shrink-0">Новое</span>
                </div>
                <span class="text-xs text-neutral-400 shrink-0">{{ formatDate(w.createdAt) }} · {{ relTime(w.createdAt) }}</span>
              </div>
              <div class="text-sm text-neutral-600 mt-1.5 line-clamp-2 leading-relaxed whitespace-pre-wrap">{{ w.workout }}</div>
              <div class="mt-2 flex items-center justify-between">
                <div class="text-[11px] text-neutral-400">{{ wordCount(w.workout) }} слов</div>
                <div class="text-[11px] font-semibold text-blue-500 opacity-0 group-hover:opacity-100 transition-opacity">Подробнее →</div>
              </div>
            </div>
          </div>
        </div>

        <!-- Пагинация -->
        <div v-if="totalPages > 1" class="flex items-center justify-center gap-2 mt-4 pt-3 border-t border-neutral-100">
          <button @click="page--" :disabled="page === 1"
            class="w-8 h-8 rounded-lg border border-neutral-200 flex items-center justify-center text-neutral-500 hover:bg-neutral-50 transition-colors disabled:opacity-30">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
            </svg>
          </button>
          <span class="text-xs text-neutral-500">{{ page }} / {{ totalPages }}</span>
          <button @click="page++" :disabled="page === totalPages"
            class="w-8 h-8 rounded-lg border border-neutral-200 flex items-center justify-center text-neutral-500 hover:bg-neutral-50 transition-colors disabled:opacity-30">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/>
            </svg>
          </button>
        </div>
      </div>
    </AppCard>

    <!-- Модалка просмотра -->
    <Teleport to="body">
      <div v-if="open" class="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4" @click.self="open = null">
        <div class="bg-white rounded-2xl border border-neutral-200 shadow-xl w-full max-w-2xl flex flex-col max-h-[85vh]">
          <div class="flex items-center gap-3 p-5 border-b border-neutral-100">
            <div class="w-11 h-11 rounded-xl bg-violet-100 flex items-center justify-center text-violet-600 shrink-0">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
              </svg>
            </div>
            <div class="flex-1 min-w-0">
              <div class="text-sm font-bold text-neutral-900 truncate">Задание от {{ open.personalFIO }}</div>
              <div class="text-xs text-neutral-400 mt-0.5">{{ formatDate(open.createdAt) }} · {{ relTime(open.createdAt) }}</div>
            </div>
            <button @click="open = null"
              class="w-8 h-8 rounded-lg flex items-center justify-center text-neutral-400 hover:bg-neutral-100 transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
                <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
              </svg>
            </button>
          </div>
          <div class="flex-1 overflow-y-auto p-5">
            <div class="text-[10px] font-bold text-neutral-400 uppercase tracking-wide mb-3">Содержание задания</div>
            <div class="text-sm text-neutral-800 whitespace-pre-wrap leading-relaxed">{{ open.workout }}</div>
          </div>
          <div class="p-4 border-t border-neutral-100 flex items-center justify-between gap-2">
            <div class="text-xs text-neutral-400">{{ wordCount(open.workout) }} слов · {{ open.workout.length }} симв.</div>
            <button @click="open = null"
              class="px-4 py-2 rounded-xl bg-blue-600 text-white text-xs font-semibold hover:bg-blue-700 transition-colors">
              Понятно
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import AppCard from '@/components/ui/AppCard.vue'
import { formatDate } from '@/utils/formatDate'

interface Workout {
  id: number
  sportsmanId: number
  sportsmanFIO: string
  personalId: number
  personalFIO: string
  workout: string
  createdAt: string
}

const loading = ref(true)
const workouts = ref<Workout[]>([])

const search = ref('')
const trainerFilter = ref<number | ''>('')
const periodFilter = ref<'all' | 'month' | 'quarter' | 'year'>('all')
const sortBy = ref<'date' | 'trainer'>('date')
const sortDir = ref<'asc' | 'desc'>('desc')
const page = ref(1)
const PER_PAGE = 10
const open = ref<Workout | null>(null)

const sortOptions = [
  { value: 'date', label: 'По дате' },
  { value: 'trainer', label: 'По тренеру' },
] as const

const trainersList = computed(() => {
  const m = new Map<number, { id: number; fio: string }>()
  for (const w of workouts.value) m.set(w.personalId, { id: w.personalId, fio: w.personalFIO })
  return Array.from(m.values()).sort((a, b) => a.fio.localeCompare(b.fio, 'ru'))
})

const trainersCount = computed(() => trainersList.value.length)

const recentCount = computed(() => {
  const cutoff = Date.now() - 30 * 24 * 60 * 60 * 1000
  return workouts.value.filter(w => new Date(w.createdAt).getTime() >= cutoff).length
})

const lastDateLabel = computed(() => {
  if (!workouts.value.length) return '—'
  const latest = workouts.value.reduce((max, w) =>
    new Date(w.createdAt).getTime() > new Date(max.createdAt).getTime() ? w : max
  )
  return formatDate(latest.createdAt)
})

function isRecent(iso: string): boolean {
  return Date.now() - new Date(iso).getTime() < 7 * 24 * 60 * 60 * 1000
}

function wordCount(s: string): number {
  return (s ?? '').trim().split(/\s+/).filter(Boolean).length
}

function relTime(iso: string): string {
  const ms = Date.now() - new Date(iso).getTime()
  const day = 24 * 60 * 60 * 1000
  if (ms < day) return 'сегодня'
  if (ms < 2 * day) return 'вчера'
  const days = Math.floor(ms / day)
  if (days < 7) return `${days} дн. назад`
  if (days < 30) return `${Math.floor(days / 7)} нед. назад`
  if (days < 365) return `${Math.floor(days / 30)} мес. назад`
  return `${Math.floor(days / 365)} г. назад`
}

const filtered = computed(() => {
  let list = workouts.value

  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(w =>
      w.workout?.toLowerCase().includes(q) ||
      w.personalFIO?.toLowerCase().includes(q)
    )
  }

  if (trainerFilter.value) {
    list = list.filter(w => w.personalId === trainerFilter.value)
  }

  if (periodFilter.value !== 'all') {
    const ranges = { month: 30, quarter: 90, year: 365 }
    const cutoff = Date.now() - ranges[periodFilter.value] * 24 * 60 * 60 * 1000
    list = list.filter(w => new Date(w.createdAt).getTime() >= cutoff)
  }

  return [...list].sort((a, b) => {
    let res = 0
    if (sortBy.value === 'date') {
      res = new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    } else if (sortBy.value === 'trainer') {
      res = (a.personalFIO ?? '').localeCompare(b.personalFIO ?? '', 'ru')
    }
    return sortDir.value === 'asc' ? -res : res
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / PER_PAGE)))
const paged = computed(() => filtered.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

onMounted(async () => {
  loading.value = true
  try {
    const res = await api.get('/sportsman/me/workouts').catch(() => null)
    workouts.value = res?.data?.data ?? []
  } finally {
    loading.value = false
  }
})
</script>
