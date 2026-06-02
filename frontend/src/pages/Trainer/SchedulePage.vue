<template>
  <div class="p-3 h-full">
    <TrainerPageCard color="blue" class="h-full flex flex-col">
      <div class="flex items-center justify-between mb-4 pb-4 border-b border-neutral-100">
        <h1 class="text-xl font-bold text-neutral-900">Расписание</h1>
        <div class="flex items-center gap-2">
          <button @click="prevMonth" class="p-1.5 rounded-lg hover:bg-neutral-100 transition-colors">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/></svg>
          </button>
          <span @click="goToday" class="text-sm font-semibold text-blue-700 capitalize min-w-[150px] text-center px-3 py-1 rounded-full bg-blue-50 border border-blue-100 cursor-pointer select-none">{{ monthLabel }}</span>
          <button @click="nextMonth" class="p-1.5 rounded-lg hover:bg-neutral-100 transition-colors">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/></svg>
          </button>
        </div>
      </div>

      <div v-if="loading" class="text-sm text-neutral-400">Загрузка...</div>

      <div v-else>
        <div class="grid grid-cols-7 gap-1 mb-1">
          <div v-for="d in DAYS" :key="d" class="text-center text-xs font-semibold text-neutral-400 uppercase tracking-wide py-1">{{ d }}</div>
        </div>

        <div class="grid grid-cols-7 gap-1">
          <div
            v-for="day in calendarDays"
            :key="day.iso"
            class="min-h-[90px] rounded-lg p-1.5 flex flex-col gap-1"
            :class="[
              day.currentMonth ? 'bg-white border border-neutral-200' : 'bg-neutral-100 border border-transparent',
              day.isToday ? 'ring-2 ring-blue-400' : ''
            ]"
          >
            <div class="mb-0.5">
              <span
                class="text-xs font-semibold w-5 h-5 flex items-center justify-center rounded-full"
                :class="day.isToday ? 'bg-blue-600 text-white' : day.currentMonth ? 'text-neutral-700' : 'text-neutral-300'"
              >{{ day.dayNum }}</span>
            </div>

            <template v-for="ev in eventsForDay(day.iso)" :key="ev.key">
              <div v-if="ev.type === 'match'" class="rounded-lg px-2 py-1.5 bg-green-50 border border-green-200">
                <div class="flex items-center gap-1 mb-0.5">
                  <span class="w-2 h-2 rounded-full bg-green-500 flex-shrink-0"></span>
                  <span class="text-xs font-semibold text-green-700">Матч</span>
                </div>
                <div class="text-xs text-green-600 leading-tight truncate">{{ ev.homeTeamName }} vs {{ ev.opponentTeamName || 'Соперник' }}</div>
                <div class="text-xs text-green-400 mt-0.5">{{ formatTime(ev.date) }}</div>
              </div>
              <div v-else class="rounded-lg px-2 py-1.5 bg-blue-50 border border-blue-100">
                <div class="text-xs font-semibold text-blue-700 mb-0.5">{{ (ev.beginTime ?? '').slice(0, 5) }}</div>
                <div class="text-xs text-blue-400 mt-0.5 truncate">{{ ev.groupName }}</div>
              </div>
            </template>
          </div>
        </div>

        <div class="flex items-center gap-4 mt-3 pt-3 border-t border-neutral-100">
          <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded bg-blue-100 border border-blue-200"></span><span class="text-xs text-neutral-500">Занятие группы</span></div>
          <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded bg-green-100 border border-green-200"></span><span class="text-xs text-neutral-500">Матч команды</span></div>
          <div class="flex items-center gap-1.5"><span class="w-3 h-3 rounded ring-2 ring-blue-400 bg-white"></span><span class="text-xs text-neutral-500">Сегодня</span></div>
        </div>
      </div>

    </TrainerPageCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import TrainerPageCard from '@/components/trainer/TrainerPageCard.vue'
import { useAuthStore } from '@/store/auth'

const auth = useAuthStore()

const loading = ref(true)
const classes = ref<any[]>([])
const matches = ref<any[]>([])
const monthOffset = ref(0)

const DAYS = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс']

const DOW_MAP: Record<string, number> = {
  Sunday: 0, Monday: 1, Tuesday: 2, Wednesday: 3,
  Thursday: 4, Friday: 5, Saturday: 6
}

const today = new Date()
today.setHours(0, 0, 0, 0)

const currentYear = computed(() => new Date(today.getFullYear(), today.getMonth() + monthOffset.value, 1).getFullYear())
const currentMonth = computed(() => new Date(today.getFullYear(), today.getMonth() + monthOffset.value, 1).getMonth())

const monthLabel = computed(() =>
  new Date(currentYear.value, currentMonth.value, 1).toLocaleDateString('ru-RU', { month: 'long', year: 'numeric' })
)

const calendarDays = computed(() => {
  const year = currentYear.value
  const month = currentMonth.value
  const firstDay = new Date(year, month, 1)
  const lastDay = new Date(year, month + 1, 0)
  const startDow = (firstDay.getDay() + 6) % 7
  const endDow = (lastDay.getDay() + 6) % 7
  const days = []
  for (let i = startDow - 1; i >= 0; i--) days.push(makeDay(new Date(year, month, -i), false))
  for (let i = 1; i <= lastDay.getDate(); i++) days.push(makeDay(new Date(year, month, i), true))
  const remaining = endDow === 6 ? 0 : 6 - endDow
  for (let i = 1; i <= remaining; i++) days.push(makeDay(new Date(year, month + 1, i), false))
  return days
})

function makeDay(d: Date, isCurrent: boolean) {
  return {
    iso: d.toISOString().slice(0, 10),
    dayNum: d.getDate(),
    currentMonth: isCurrent,
    isToday: d.getTime() === today.getTime(),
  }
}

function getWeekNumber(iso: string): number {
  const d = new Date(iso)
  const startOfYear = new Date(d.getFullYear(), 0, 1)
  const diff = d.getTime() - startOfYear.getTime()
  return Math.ceil((diff / 86400000 + startOfYear.getDay() + 1) / 7)
}

function eventsForDay(iso: string) {
  const date = new Date(iso)
  const dow = date.getDay()
  const dayMatches = matches.value.filter(m => m.date?.slice(0, 10) === iso)
  if (dayMatches.length) return dayMatches.map(m => ({ ...m, type: 'match', key: 'm' + m.id }))
  if (date < today) return []
  const weekNum = getWeekNumber(iso)
  const isEven = weekNum % 2 === 0
  return classes.value
    .filter(c => {
      const classDow = typeof c.dayOfWeek === 'number' ? c.dayOfWeek : (DOW_MAP[c.dayOfWeek] ?? -1)
      if (classDow !== dow) return false
      const wt = c.weekType ?? 'Any'
      if (wt === 'Even' || wt === 2) return isEven
      if (wt === 'Odd'  || wt === 3) return !isEven
      return true // Any
    })
    .map(c => ({ ...c, type: 'class', key: 'c' + c.id }))
}

function formatTime(value: string) {
  if (!value) return ''
  if (value.includes('T')) return new Date(value).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })
  return value.slice(0, 5)
}

function prevMonth() { monthOffset.value-- }
function nextMonth() { monthOffset.value++ }
function goToday() { monthOffset.value = 0 }

onMounted(async () => {
  const tid = auth.personalId
  const [cRes, mRes] = await Promise.allSettled([
    api.get('/schedule', { params: tid ? { filters: { trainerId: [tid] } } : undefined }),
    api.get('/match',    { params: tid ? { filters: { trainerId: [tid] } } : undefined }),
  ])
  
  if (cRes.status === 'fulfilled') {
    classes.value = cRes.value.data.data ?? []
  }
  
  if (mRes.status === 'fulfilled') {
    matches.value = (mRes.value.data.data ?? []).filter((m: any) => m.status === 'Scheduled')
  }
  
  loading.value = false
})
</script>
