<template>
  <div class="p-6 h-full overflow-y-auto">
    <AppCard no-border class="min-h-full flex flex-col">
      <!-- Шапка карточки белая -->
      <div class="flex items-start justify-between bg-white -mx-4 -mt-4 px-5 py-4 mb-5 rounded-t-2xl border-b border-neutral-100">
        <div>
          <h1 class="text-xl font-bold text-neutral-900">История посещений</h1>
          <p class="text-sm text-neutral-500 mt-0.5">Все тренировки и статус посещения</p>
        </div>
        <button @click="$router.back()"
          class="flex items-center gap-1.5 px-3 py-2 rounded-xl border border-blue-200 bg-blue-50 text-blue-600 text-xs font-semibold hover:bg-blue-100 transition-colors shrink-0">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
          </svg>
          Назад
        </button>
      </div>

      <!-- Сводка за всё время -->
      <div v-if="totalStats" class="grid grid-cols-5 gap-3 mb-6">
        <div class="rounded-2xl border border-neutral-200 p-4">
          <div class="text-xs font-bold text-neutral-500 uppercase tracking-wide">Всего</div>
          <div class="text-2xl font-extrabold text-neutral-900 mt-1">{{ totalStats.total }}</div>
        </div>
        <div class="rounded-2xl border border-blue-100 bg-blue-50 p-4">
          <div class="text-xs font-bold text-blue-600 uppercase tracking-wide">Присутствовал</div>
          <div class="text-2xl font-extrabold text-blue-700 mt-1">{{ totalStats.present }}</div>
        </div>
        <div class="rounded-2xl border border-sky-100 bg-sky-50 p-4">
          <div class="text-xs font-bold text-sky-600 uppercase tracking-wide">Опоздал</div>
          <div class="text-2xl font-extrabold text-sky-700 mt-1">{{ totalStats.late }}</div>
        </div>
        <div class="rounded-2xl border border-neutral-200 bg-neutral-100 p-4">
          <div class="text-xs font-bold text-neutral-600 uppercase tracking-wide">Отсутствовал</div>
          <div class="text-2xl font-extrabold text-neutral-700 mt-1">{{ totalStats.absent }}</div>
        </div>
        <div class="rounded-2xl border border-green-100 bg-green-50 p-4">
          <div class="text-xs font-bold text-green-700 uppercase tracking-wide">Уважительная</div>
          <div class="text-2xl font-extrabold text-green-700 mt-1">{{ totalStats.excused }}</div>
        </div>
      </div>

      <!-- Пагинация по кварталам со стрелками (адаптивная — сколько влезет) -->
      <div v-if="quarters.length" ref="paginationRef" class="mb-4 flex items-center gap-2 justify-center w-full">
        <button
          @click="shiftLeft"
          :disabled="!canShiftLeft"
          class="w-7 h-7 rounded-full flex items-center justify-center text-neutral-400 hover:text-neutral-700 hover:bg-neutral-100 transition-colors disabled:opacity-30 disabled:hover:bg-transparent disabled:cursor-not-allowed shrink-0"
          aria-label="Назад"
        >
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-4 h-4">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
          </svg>
        </button>

        <div class="flex items-center gap-2 justify-center">
          <button
            v-for="q in visibleQuarters" :key="q.key"
            @click="activeIdx = q.idx"
            class="text-xs font-semibold px-3 py-1.5 rounded-full transition-colors whitespace-nowrap"
            :class="q.idx === activeIdx ? 'bg-blue-500 text-white' : 'bg-neutral-100 text-neutral-600 hover:bg-neutral-200'"
          >{{ q.label }}</button>
        </div>

        <button
          @click="shiftRight"
          :disabled="!canShiftRight"
          class="w-7 h-7 rounded-full flex items-center justify-center text-neutral-400 hover:text-neutral-700 hover:bg-neutral-100 transition-colors disabled:opacity-30 disabled:hover:bg-transparent disabled:cursor-not-allowed shrink-0"
          aria-label="Вперёд"
        >
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-4 h-4">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/>
          </svg>
        </button>
      </div>

      <!-- Сводка по выбранному кварталу (компактнее и приглушённее) -->
      <div v-if="activeQuarter && activeQuarter.items.length" class="grid grid-cols-4 gap-2 mb-4">
        <div class="rounded-xl border border-neutral-100 px-3 py-2 flex items-center justify-between">
          <span class="text-xs font-medium text-neutral-400">Присутствовал</span>
          <span class="text-base font-bold text-neutral-500">{{ activeQuarter.present }}</span>
        </div>
        <div class="rounded-xl border border-neutral-100 px-3 py-2 flex items-center justify-between">
          <span class="text-xs font-medium text-neutral-400">Опоздал</span>
          <span class="text-base font-bold text-neutral-500">{{ activeQuarter.late }}</span>
        </div>
        <div class="rounded-xl border border-neutral-100 px-3 py-2 flex items-center justify-between">
          <span class="text-xs font-medium text-neutral-400">Отсутствовал</span>
          <span class="text-base font-bold text-neutral-500">{{ activeQuarter.absent }}</span>
        </div>
        <div class="rounded-xl border border-neutral-100 px-3 py-2 flex items-center justify-between">
          <span class="text-xs font-medium text-neutral-400">Уважительная</span>
          <span class="text-base font-bold text-neutral-500">{{ activeQuarter.excused }}</span>
        </div>
      </div>

      <!-- Таблица занятий -->
      <div v-if="activeQuarter && activeQuarter.items.length" class="rounded-2xl border border-neutral-200 overflow-hidden">
        <table class="w-full">
          <thead class="bg-neutral-50 text-xs font-bold text-neutral-500 uppercase tracking-wide">
            <tr>
              <th class="text-left px-4 py-3">Дата</th>
              <th class="text-left px-4 py-3">Тип тренировки</th>
              <th class="text-left px-4 py-3">Статус</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in activeQuarter.items" :key="item.id" class="border-t border-neutral-100">
              <td class="px-4 py-3 text-sm text-neutral-700">{{ formatDate(item.trainingDate) }}</td>
              <td class="px-4 py-3 text-sm text-neutral-700">{{ item.trainingType ?? '—' }}</td>
              <td class="px-4 py-3">
                <span class="text-xs font-bold px-2 py-0.5 rounded-full" :class="statusClass(item.status)">
                  {{ statusLabel[item.status] ?? item.status }}
                </span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
      <div v-else-if="!loading" class="text-sm text-neutral-400 text-center py-8">
        Нет записей в выбранном периоде
      </div>
    </AppCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { useRoute } from 'vue-router'
import api from '@/services/api'
import { formatDate } from '@/utils/formatDate'
import AppCard from '@/components/ui/AppCard.vue'

const props = defineProps<{ athleteId?: number }>()
const route = useRoute()

const attendances    = ref<any[]>([])
const activeIdx      = ref(0)
const startIdx       = ref(0)
const loading        = ref(true)
const paginationRef  = ref<HTMLElement | null>(null)
const visibleCount   = ref(6)

// Адаптивно считаем сколько плашек влезает в строку
function recalcVisible() {
  const el = paginationRef.value
  if (!el) return
  const total = el.clientWidth
  // 2 стрелки по 28px + 2 gap-2 (8px) = 72px резерв
  const reserved = 72
  // Одна плашка ~115px (с padding и текстом «Янв–Мар 2024»), gap 8
  const itemW = 123
  const fits = Math.max(1, Math.floor((total - reserved) / itemW))
  visibleCount.value = fits
}

let resizeObs: ResizeObserver | null = null

onUnmounted(() => {
  resizeObs?.disconnect()
  window.removeEventListener('resize', recalcVisible)
})

const statusLabel: Record<string, string> = {
  Present:        'Присутствовал',
  Late:           'Опоздал',
  Absent:         'Отсутствовал',
  ExcusedAbsent:  'Уважительная',
}

function statusClass(s: string) {
  if (s === 'Present')        return 'bg-blue-100 text-blue-700'
  if (s === 'Late')           return 'bg-sky-100 text-sky-700'
  if (s === 'Absent')         return 'bg-neutral-200 text-neutral-700'
  if (s === 'ExcusedAbsent')  return 'bg-green-100 text-green-700'
  return 'bg-neutral-100 text-neutral-600'
}

const totalStats = computed(() => {
  if (!attendances.value.length) return null
  return {
    total:    attendances.value.length,
    present:  attendances.value.filter(a => a.status === 'Present').length,
    late:     attendances.value.filter(a => a.status === 'Late').length,
    absent:   attendances.value.filter(a => a.status === 'Absent').length,
    excused:  attendances.value.filter(a => a.status === 'ExcusedAbsent').length,
  }
})

const quarters = computed(() => {
  if (!attendances.value.length) return []
  const sorted = [...attendances.value].sort((a, b) => new Date(b.trainingDate).getTime() - new Date(a.trainingDate).getTime())
  const groups = new Map<string, any[]>()
  for (const a of sorted) {
    const d = new Date(a.trainingDate)
    const y = d.getFullYear()
    const m = d.getMonth()
    const qStart = Math.floor(m / 3) * 3
    const key = `${y}-${qStart}`
    if (!groups.has(key)) groups.set(key, [])
    groups.get(key)!.push(a)
  }
  const months = ['Янв','Фев','Мар','Апр','Май','Июн','Июл','Авг','Сен','Окт','Ноя','Дек']
  return Array.from(groups.entries()).map(([key, items]) => {
    const [y, qStart] = key.split('-').map(Number)
    const qEnd = qStart + 2
    const label = `${months[qStart]}–${months[qEnd]} ${y}`
    return {
      key, label, items,
      present: items.filter(a => a.status === 'Present').length,
      late:    items.filter(a => a.status === 'Late').length,
      absent:  items.filter(a => a.status === 'Absent').length,
      excused: items.filter(a => a.status === 'ExcusedAbsent').length,
    }
  })
})

const visibleQuarters = computed(() =>
  quarters.value
    .slice(startIdx.value, startIdx.value + visibleCount.value)
    .map((q, i) => ({ ...q, idx: startIdx.value + i }))
)

const canShiftLeft  = computed(() => startIdx.value > 0)
const canShiftRight = computed(() => startIdx.value + visibleCount.value < quarters.value.length)

function shiftLeft() {
  if (canShiftLeft.value) startIdx.value = Math.max(0, startIdx.value - 1)
}

function shiftRight() {
  if (canShiftRight.value) startIdx.value++
}

const activeQuarter = computed(() => quarters.value[activeIdx.value] ?? null)

onMounted(async () => {
  let id = props.athleteId ?? Number(route.params.id) ?? 0
  if (!id) {
    const me = await api.get('/sportsman/me').catch(() => null)
    id = me?.data?.data?.id ?? 0
  }
  if (!id) { loading.value = false; return }

  const res = await api.get(`/schedule/attendance/sportsman/${id}`).catch(() => null)
  const list: any[] = res?.data?.data ?? res?.data ?? []
  attendances.value = list.map(a => ({
    id: a.id ?? a.Id,
    trainingId: a.trainingId ?? a.TrainingId,
    status: a.status ?? a.Status,
    trainingDate: a.trainingDate ?? a.TrainingDate ?? a.createdAt ?? a.CreatedAt,
    trainingType: a.trainingType ?? a.TrainingType,
  }))
  loading.value = false

  nextTick(() => {
    recalcVisible()
    if (paginationRef.value && typeof ResizeObserver !== 'undefined') {
      resizeObs = new ResizeObserver(recalcVisible)
      resizeObs.observe(paginationRef.value)
    }
    window.addEventListener('resize', recalcVisible)
  })
})
</script>
