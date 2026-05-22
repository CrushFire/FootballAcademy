<template>
  <div>
    <!-- Навигация -->
    <div class="flex items-center justify-between mb-4 pb-4 border-b border-neutral-100">
      <slot name="title">
        <div class="text-base font-bold text-neutral-900">Расписание</div>
      </slot>
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

    <!-- Заголовки дней -->
    <div class="grid grid-cols-7 gap-1 mb-1">
      <div v-for="d in DAYS" :key="d" class="text-center text-xs font-semibold text-neutral-400 uppercase tracking-wide py-1">{{ d }}</div>
    </div>

    <!-- Сетка -->
    <div class="grid grid-cols-7 gap-1">
      <div
        v-for="day in calendarDays"
        :key="day.iso"
        class="min-h-[140px] rounded-lg p-1.5 flex flex-col gap-1"
        :class="[
          day.currentMonth ? 'bg-white border border-neutral-200' : 'bg-neutral-100 border border-transparent',
          day.isToday ? 'ring-2 ring-blue-400' : ''
        ]"
      >
        <!-- Число -->
        <div class="mb-0.5">
          <span
            class="text-xs font-semibold w-5 h-5 flex items-center justify-center rounded-full"
            :class="day.isToday ? 'bg-blue-600 text-white' : day.currentMonth ? 'text-neutral-700' : 'text-neutral-300'"
          >{{ day.dayNum }}</span>
        </div>

        <!-- Ивенты через slot -->
        <slot name="events" :day="day" />
      </div>
    </div>

    <!-- Легенда -->
    <slot name="legend">
      <div class="flex items-center gap-4 mt-3 pt-3 border-t border-neutral-100">
        <div class="flex items-center gap-1.5"><div class="w-3 h-3 rounded bg-blue-100 border border-blue-200 flex-shrink-0"></div><span class="text-xs text-neutral-500">Занятие</span></div>
        <div class="flex items-center gap-1.5"><div class="w-3 h-3 rounded bg-green-100 border border-green-200 flex-shrink-0"></div><span class="text-xs text-neutral-500">Матч</span></div>
        <div class="flex items-center gap-1.5"><div class="w-3 h-3 rounded ring-2 ring-blue-400 bg-white flex-shrink-0"></div><span class="text-xs text-neutral-500">Сегодня</span></div>
      </div>
    </slot>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'

const DAYS = ['Пн', 'Вт', 'Ср', 'Чт', 'Пт', 'Сб', 'Вс']

const monthOffset = ref(0)

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
  const days: ReturnType<typeof makeDay>[] = []
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
    isPast: d < today,
  }
}

function prevMonth() { monthOffset.value-- }
function nextMonth() { monthOffset.value++ }
function goToday() { monthOffset.value = 0 }
</script>
