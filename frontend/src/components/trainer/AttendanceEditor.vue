<template>
  <div>
    <div v-if="!sportsmen.length" class="text-sm text-neutral-400 italic py-4 text-center">
      {{ groupId ? 'В группе нет спортсменов' : 'Сначала выберите группу' }}
    </div>
    <div v-else>
      <div class="flex items-center justify-between mb-3">
        <div class="text-xs text-neutral-500">
          Всего: <span class="font-semibold text-neutral-700">{{ sportsmen.length }}</span> ·
          Присут.: <span class="font-semibold text-blue-600">{{ counts.Present }}</span> ·
          Опоз.: <span class="font-semibold text-sky-600">{{ counts.Late }}</span> ·
          Отсут.: <span class="font-semibold text-neutral-700">{{ counts.Absent }}</span> ·
          По уваж.: <span class="font-semibold text-green-600">{{ counts.ExcusedAbsent }}</span>
        </div>
        <div class="flex gap-1 text-xs">
          <button type="button" @click="bulkSet('Present')" class="px-2 py-1 rounded-lg border border-blue-200 bg-blue-50 text-blue-700 hover:bg-blue-100 transition-colors">Все присут.</button>
          <button type="button" @click="bulkSet(null)" class="px-2 py-1 rounded-lg border border-neutral-200 bg-white text-neutral-600 hover:bg-neutral-50 transition-colors">Сбросить</button>
        </div>
      </div>

      <div class="rounded-xl border border-neutral-200 divide-y divide-neutral-100 max-h-[420px] overflow-y-auto bg-white">
        <div v-for="s in sportsmen" :key="s.id" class="flex items-center gap-3 px-3 py-2">
          <div class="w-8 h-8 rounded-full bg-neutral-100 text-neutral-600 inline-flex items-center justify-center text-xs font-bold shrink-0">
            {{ initials(s.fio) }}
          </div>
          <div class="flex-1 min-w-0">
            <div class="text-sm font-semibold text-neutral-800 truncate">{{ s.fio }}</div>
            <div class="text-[11px] text-neutral-400">{{ s.position || '—' }}<span v-if="s.age"> · {{ s.age }} лет</span></div>
          </div>
          <div class="flex gap-1 shrink-0">
            <button
              v-for="opt in OPTIONS"
              :key="opt.value"
              type="button"
              @click="setStatus(s.id, opt.value)"
              class="px-2.5 py-1 rounded-lg text-xs font-semibold border transition-colors"
              :class="statusOf(s.id) === opt.value ? opt.activeCls : 'border-neutral-200 bg-white text-neutral-500 hover:bg-neutral-50'"
              :title="opt.label"
            >
              {{ opt.short }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

type Status = 'Present' | 'Late' | 'Absent' | 'ExcusedAbsent'

interface AttendanceItem {
  sportsmanId: number
  status: Status
}
interface Sportsman {
  id: number
  fio: string
  position?: string
  age?: number
}

const props = defineProps<{
  modelValue: AttendanceItem[]
  sportsmen: Sportsman[]
  groupId?: number | null
}>()

const emit = defineEmits<{ 'update:modelValue': [value: AttendanceItem[]] }>()

const OPTIONS = [
  { value: 'Present' as Status,        label: 'Присутствовал',  short: 'П',  activeCls: 'border-blue-300 bg-blue-100 text-blue-700' },
  { value: 'Late' as Status,           label: 'Опоздал',         short: 'О',  activeCls: 'border-sky-300 bg-sky-100 text-sky-700' },
  { value: 'Absent' as Status,         label: 'Не был',          short: 'Н',  activeCls: 'border-neutral-300 bg-neutral-200 text-neutral-700' },
  { value: 'ExcusedAbsent' as Status,  label: 'По уваж.',        short: 'У',  activeCls: 'border-green-300 bg-green-100 text-green-700' },
]

function statusOf(id: number): Status | null {
  return props.modelValue.find(x => x.sportsmanId === id)?.status ?? null
}

function setStatus(id: number, status: Status | null) {
  const rest = props.modelValue.filter(x => x.sportsmanId !== id)
  if (status == null) {
    emit('update:modelValue', rest)
  } else {
    emit('update:modelValue', [...rest, { sportsmanId: id, status }])
  }
}

function bulkSet(status: Status | null) {
  if (status == null) {
    emit('update:modelValue', [])
  } else {
    emit('update:modelValue', props.sportsmen.map(s => ({ sportsmanId: s.id, status })))
  }
}

function initials(fio: string): string {
  if (!fio) return '—'
  const parts = fio.trim().split(/\s+/)
  return ((parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '')).toUpperCase()
}

const counts = computed(() => {
  const c = { Present: 0, Late: 0, Absent: 0, ExcusedAbsent: 0 }
  for (const a of props.modelValue) c[a.status]++
  return c
})
</script>
