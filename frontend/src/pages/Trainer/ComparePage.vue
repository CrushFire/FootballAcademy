<template>
  <div class="p-3 h-full">
    <TrainerPageCard color="blue" class="h-full flex flex-col overflow-hidden">
      <div class="flex flex-col h-full overflow-y-auto px-4 py-4 gap-6">

        <!-- Player slots -->
        <div class="grid grid-cols-2 gap-4 flex-shrink-0">
          <div v-for="slot in [0, 1]" :key="slot">
            <div v-if="!players[slot]"
              @click="openPicker(slot)"
              class="border-2 border-dashed border-neutral-300 rounded-2xl flex flex-col items-center justify-center gap-2 cursor-pointer hover:border-blue-400 hover:bg-blue-50 transition-colors min-h-[160px]">
              <div class="w-12 h-12 rounded-full bg-neutral-100 flex items-center justify-center">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5" class="w-6 h-6 text-neutral-400">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/>
                </svg>
              </div>
              <span class="text-xs text-neutral-400 font-medium">Выбрать игрока {{ slot + 1 }}</span>
            </div>
            <div v-else class="bg-white rounded-2xl border-2 p-4 flex flex-col items-center gap-2 relative"
              :class="isMine(players[slot]) ? 'border-blue-400' : 'border-neutral-200'">
              <button @click="players[slot] = null; metrics[slot] = null; matchStats[slot] = null"
                class="absolute top-2 right-2 w-6 h-6 rounded-full bg-neutral-100 hover:bg-red-50 flex items-center justify-center transition-colors">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3 h-3 text-neutral-500 hover:text-red-500">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
                </svg>
              </button>
              <div class="w-14 h-14 rounded-full flex items-center justify-center text-white font-bold text-lg flex-shrink-0"
                :class="isMine(players[slot]) ? 'bg-blue-500' : 'bg-neutral-400'">
                {{ initials(players[slot].fio) }}
              </div>
              <div class="text-center">
                <div class="text-sm font-semibold text-neutral-800">{{ players[slot].fio }}</div>
                <div class="text-xs text-neutral-500 mt-0.5">{{ positionLabel[players[slot].position] ?? players[slot].position ?? '—' }}</div>
                <div class="text-xs text-neutral-400 mt-1">
                  {{ players[slot].age }} л · {{ players[slot].height }} см · {{ players[slot].weight }} кг
                </div>
                <div class="text-xs text-neutral-400">{{ monthsLabel(players[slot].createdAt) }}</div>
              </div>
              <span v-if="isMine(players[slot])" class="text-[10px] font-semibold text-blue-600 bg-blue-50 px-2 py-0.5 rounded-full">Мой</span>
            </div>
          </div>
        </div>

        <!-- Comparison table -->
        <div v-if="players[0] && players[1]" class="flex flex-col gap-1 flex-shrink-0">

          <!-- Basic stats — no colors -->
          <SectionLabel label="Основные данные" />
          <CompareSection>
            <CompareRow label="Позиция"
              :a="positionLabel[players[0].position] ?? players[0].position ?? '—'"
              :b="positionLabel[players[1].position] ?? players[1].position ?? '—'"
              :winner="undefined" />
            <CompareRow label="Возраст (лет)"
              :a="players[0].age" :b="players[1].age"
              :winner="undefined" />
            <CompareRow label="Рост (см)"
              :a="players[0].height" :b="players[1].height"
              :winner="undefined" />
            <CompareRow label="Вес (кг)"
              :a="players[0].weight" :b="players[1].weight"
              :winner="undefined" />
            <CompareRow label="Занимается"
              :a="monthsLabel(players[0].createdAt)"
              :b="monthsLabel(players[1].createdAt)"
              :winner="undefined" />
            <CompareRow label="Команда"
              :a="playerTeam(players[0])"
              :b="playerTeam(players[1])"
              :winner="undefined" />
            <CompareRow label="Группы"
              :a="playerGroups(players[0])"
              :b="playerGroups(players[1])"
              :winner="undefined" />
          </CompareSection>

          <!-- GPS metrics -->
          <SectionLabel :label="gpsLabel" />
          <div v-if="metricsLoading" class="text-xs text-neutral-400 text-center py-4">Загрузка метрик...</div>
          <CompareSection v-else>
            <CompareRowTip label="Дистанция (км)" :tooltip="GPS_TIPS.totalDistance"
              :a="mvalKm(0, 'totalDistance')" :b="mvalKm(1, 'totalDistance')"
              :winner="compareNum(mnum(0, 'totalDistance'), mnum(1, 'totalDistance'), true)" />
            <CompareRowTip label="Спринт (км)" :tooltip="GPS_TIPS.sprintDistance"
              :a="mvalKm(0, 'sprintDistance')" :b="mvalKm(1, 'sprintDistance')"
              :winner="compareNum(mnum(0, 'sprintDistance'), mnum(1, 'sprintDistance'), true)" />
            <CompareRowTip label="Макс. скорость (км/ч)" :tooltip="GPS_TIPS.maximumSpeed"
              :a="mval(0, 'maximumSpeed')" :b="mval(1, 'maximumSpeed')"
              :winner="compareNum(mnum(0, 'maximumSpeed'), mnum(1, 'maximumSpeed'), true)" />
            <CompareRowTip label="Средняя скорость (км/ч)" :tooltip="GPS_TIPS.averageSpeed"
              :a="mval(0, 'averageSpeed')" :b="mval(1, 'averageSpeed')"
              :winner="compareNum(mnum(0, 'averageSpeed'), mnum(1, 'averageSpeed'), true)" />
            <CompareRowTip label="Ускорения" :tooltip="GPS_TIPS.accelerationCount"
              :a="mval(0, 'accelerationCount')" :b="mval(1, 'accelerationCount')"
              :winner="compareNum(mnum(0, 'accelerationCount'), mnum(1, 'accelerationCount'), true)" />
            <CompareRowTip label="Замедления" :tooltip="GPS_TIPS.decelerationCount"
              :a="mval(0, 'decelerationCount')" :b="mval(1, 'decelerationCount')"
              :winner="compareNum(mnum(0, 'decelerationCount'), mnum(1, 'decelerationCount'), true)" />
            <CompareRowTip label="Нагрузка игрока" :tooltip="GPS_TIPS.playerLoad"
              :a="mval(0, 'playerLoad')" :b="mval(1, 'playerLoad')"
              :winner="compareNum(mnum(0, 'playerLoad'), mnum(1, 'playerLoad'), true)" />
            <CompareRowTip label="Пульс средний" :tooltip="GPS_TIPS.averageHeartRate"
              :a="mval(0, 'averageHeartRate')" :b="mval(1, 'averageHeartRate')"
              :winner="undefined" />
            <CompareRowTip label="Пульс макс." :tooltip="GPS_TIPS.maxHeartRate"
              :a="mval(0, 'maxHeartRate')" :b="mval(1, 'maxHeartRate')"
              :winner="compareNum(mnum(0, 'maxHeartRate'), mnum(1, 'maxHeartRate'), true)" />
          </CompareSection>

          <!-- Mini summary -->
          <SectionLabel label="Итог по метрикам" />
          <div class="grid grid-cols-2 gap-3">
            <MiniStat v-for="slot in [0, 1]" :key="slot"
              :wins="gpsWins[slot]"
              :is-mine="isMine(players[slot])" />
          </div>

          <!-- Match stats -->
          <SectionLabel label="Статистика матчей" />
          <div v-if="matchWinsLoading" class="text-xs text-neutral-400 text-center py-4">Загрузка матчей...</div>
          <div v-else class="grid grid-cols-2 gap-3">
            <div v-for="slot in [0, 1]" :key="slot"
              class="rounded-2xl border-2 p-3 text-center"
              :class="isMine(players[slot]) ? 'border-blue-400 bg-blue-50' : 'border-neutral-200 bg-white'">
              <template v-if="matchStats[slot] === null">
                <div class="text-xs text-neutral-400">Данный игрок пока не играл матчей</div>
              </template>
              <template v-else>
                <div class="flex items-end justify-center gap-3">
                  <div class="flex flex-col items-center">
                    <span class="text-2xl font-bold text-green-600">{{ matchStats[slot]!.wins }}</span>
                    <span class="text-[10px] text-neutral-400 mt-0.5">победы</span>
                  </div>
                  <div class="flex flex-col items-center">
                    <span class="text-2xl font-bold text-red-500">{{ matchStats[slot]!.losses }}</span>
                    <span class="text-[10px] text-neutral-400 mt-0.5">поражения</span>
                  </div>
                  <div class="flex flex-col items-center">
                    <span class="text-2xl font-bold text-neutral-500">{{ matchStats[slot]!.draws }}</span>
                    <span class="text-[10px] text-neutral-400 mt-0.5">ничьи</span>
                  </div>
                </div>
              </template>
            </div>
          </div>

        </div>

        <div v-else-if="players[0] || players[1]" class="text-xs text-neutral-400 text-center py-8">
          Выберите второго игрока для сравнения
        </div>
        <div v-else class="text-xs text-neutral-400 text-center py-8">
          Выберите двух игроков для сравнения
        </div>

      </div>
    </TrainerPageCard>
  </div>

  <!-- Picker modal -->
  <div v-if="pickerOpen" class="fixed inset-0 z-50 flex items-center justify-center bg-black/30 backdrop-blur-sm p-4">
    <div class="bg-white rounded-2xl shadow-xl w-full max-w-md flex flex-col max-h-[80vh]">
      <div class="flex items-center justify-between px-4 pt-4 pb-3 border-b border-neutral-100">
        <span class="text-sm font-semibold text-neutral-800">Выбор игрока</span>
        <button @click="pickerOpen = false" class="text-neutral-400 hover:text-neutral-600">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
          </svg>
        </button>
      </div>

      <!-- Фильтр группа/команда + поиск -->
      <div class="px-4 pt-3 pb-2 space-y-2 border-b border-neutral-100">
        <select v-model="pickerGroupKey" @change="pickerSearch = ''"
          class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 bg-neutral-50">
          <option value="mine">Все мои спортсмены</option>
          <option value="all">Все спортсмены академии</option>
          <optgroup v-if="myGroups.length" label="Группы">
            <option v-for="g in myGroups" :key="'g-' + g.id" :value="'g-' + g.id">{{ g.name }}</option>
          </optgroup>
          <optgroup v-if="myTeams.length" label="Команды">
            <option v-for="t in myTeams" :key="'t-' + t.id" :value="'t-' + t.id">{{ t.name }}</option>
          </optgroup>
        </select>
        <input v-model="pickerSearch" placeholder="Поиск по имени..." class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 bg-neutral-50" />
      </div>

      <div class="overflow-y-auto flex-1 py-1">
        <div v-for="s in pickerList" :key="s.id"
          @click="selectSportsman(s)"
          class="flex items-center gap-3 px-4 py-2.5 cursor-pointer hover:bg-neutral-50 transition-colors"
          :class="isMine(s) ? 'border-l-2 border-blue-400' : 'border-l-2 border-transparent'">
          <div class="w-8 h-8 rounded-full flex items-center justify-center text-white text-xs font-bold flex-shrink-0"
            :class="isMine(s) ? 'bg-blue-500' : 'bg-neutral-400'">
            {{ initials(s.fio) }}
          </div>
          <div class="flex-1 min-w-0">
            <div class="text-sm font-medium text-neutral-800 truncate">{{ s.fio }}</div>
            <div class="text-xs text-neutral-400">{{ positionLabel[s.position] ?? s.position ?? '—' }} · {{ s.age }} л</div>
          </div>
          <span v-if="isMine(s)" class="text-[10px] text-blue-500 font-semibold shrink-0">Мой</span>
        </div>
        <div v-if="pickerList.length === 0" class="text-xs text-neutral-400 text-center py-8">Нет игроков</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, defineComponent, h } from 'vue'
import api from '@/services/api'
import { useAuthStore } from '@/store/auth'
import { POSITION_AND_GROUP_LABEL } from '@/constants'
import TrainerPageCard from '@/components/trainer/TrainerPageCard.vue'
import AppTooltip from '@/components/ui/AppTooltip.vue'
import { GPS_METRIC_TOOLTIPS } from '@/constants/metricTooltips'

const auth = useAuthStore()
const GPS_TIPS = GPS_METRIC_TOOLTIPS

// Inline sub-components
const SectionLabel = defineComponent({
  props: { label: String },
  setup(props) {
    return () => h('div', { class: 'text-[11px] font-semibold text-neutral-400 uppercase tracking-wide pt-2 pb-1 px-1' }, props.label)
  }
})

const CompareSection = defineComponent({
  setup(_, { slots }) {
    return () => h('div', { class: 'bg-white rounded-2xl border border-neutral-100 overflow-hidden divide-y divide-neutral-50' }, slots.default?.())
  }
})

const CompareRow = defineComponent({
  props: { label: String, a: [String, Number], b: [String, Number] },
  setup(props, { attrs }) {
    return () => {
      const w = attrs.winner as number | null | undefined
      const aClass = w === 0 ? 'text-green-600 font-bold' : w === 1 ? 'text-red-400' : 'text-neutral-700 font-medium'
      const bClass = w === 1 ? 'text-green-600 font-bold' : w === 0 ? 'text-red-400' : 'text-neutral-700 font-medium'
      return h('div', { class: 'grid grid-cols-3 items-center px-3 py-2' }, [
        h('div', { class: aClass + ' text-sm text-right' }, String(props.a ?? '—')),
        h('div', { class: 'text-[10px] text-neutral-400 text-center px-2' }, props.label),
        h('div', { class: bClass + ' text-sm text-left' }, String(props.b ?? '—')),
      ])
    }
  }
})

const CompareRowTip = defineComponent({
  props: { label: String, a: [String, Number], b: [String, Number], tooltip: String },
  setup(props, { attrs }) {
    return () => {
      const w = attrs.winner as number | null | undefined
      const aClass = w === 0 ? 'text-green-600 font-bold' : w === 1 ? 'text-red-400' : 'text-neutral-700 font-medium'
      const bClass = w === 1 ? 'text-green-600 font-bold' : w === 0 ? 'text-red-400' : 'text-neutral-700 font-medium'
      return h('div', { class: 'grid grid-cols-3 items-center px-3 py-2' }, [
        h('div', { class: aClass + ' text-sm text-right' }, String(props.a ?? '—')),
        h('div', { class: 'text-[10px] text-neutral-400 text-center px-2 flex items-center justify-center gap-1' }, [
          props.label,
          props.tooltip ? h(AppTooltip, { text: props.tooltip }) : null,
        ]),
        h('div', { class: bClass + ' text-sm text-left' }, String(props.b ?? '—')),
      ])
    }
  }
})

const MiniStat = defineComponent({
  props: { wins: Number, isMine: Boolean },
  setup(props) {
    return () => h('div', {
      class: 'rounded-2xl border-2 p-3 text-center ' + (props.isMine ? 'border-blue-400 bg-blue-50' : 'border-neutral-200 bg-white')
    }, [
      h('div', { class: 'text-2xl font-bold ' + (props.isMine ? 'text-blue-600' : 'text-neutral-600') }, String(props.wins ?? 0)),
      h('div', { class: 'text-[10px] text-neutral-400 mt-0.5' }, 'выигрывает по параметрам'),
    ])
  }
})

// State
const allSportsmen = ref<any[]>([])
const myGroups = ref<any[]>([])
const myTeams = ref<any[]>([])
const sportsmenByGroup = ref<Record<number, any[]>>({})
const sportsmenByTeam = ref<Record<number, any[]>>({})
const mySportsmenIds = ref<Set<number>>(new Set())
const teamNameById = ref<Record<number, string>>({})   // teamId -> name

const players = ref<any[]>([null, null])
const metrics = ref<any[]>([null, null])
const metricsLoading = ref(false)
interface MatchStats { wins: number; losses: number; draws: number }
const matchStats = ref<(MatchStats | null)[]>([null, null])
const matchWinsLoading = ref(false)

const pickerOpen = ref(false)
const pickerSlot = ref(0)
const pickerSearch = ref('')
const pickerGroupKey = ref('mine')   // 'mine' | 'all' | 'g-{id}' | 't-{id}'

const positionLabel = POSITION_AND_GROUP_LABEL

const gpsLabel = computed(() => 'Метрики тренировок (среднее за 6 мес.)')

// Список в пикере зависит от выбранного фильтра
const pickerPool = computed((): any[] => {
  if (pickerGroupKey.value === 'all') return allSportsmen.value
  if (pickerGroupKey.value.startsWith('g-')) {
    const id = Number(pickerGroupKey.value.slice(2))
    return sportsmenByGroup.value[id] ?? []
  }
  if (pickerGroupKey.value.startsWith('t-')) {
    const id = Number(pickerGroupKey.value.slice(2))
    return sportsmenByTeam.value[id] ?? []
  }
  // 'mine' — спортсмены из групп/команд тренера; если данных нет — все
  const mine = allSportsmen.value.filter(s => mySportsmenIds.value.has(s.id))
  return mine.length > 0 ? mine : allSportsmen.value
})

const pickerList = computed(() => {
  const otherSlot = pickerSlot.value === 0 ? 1 : 0
  const taken = players.value[otherSlot]?.id
  let list = pickerPool.value
  if (taken) list = list.filter(s => s.id !== taken)
  if (pickerSearch.value.trim()) {
    const q = pickerSearch.value.toLowerCase()
    list = list.filter(s => s.fio?.toLowerCase().includes(q))
  }
  return list
})

function initials(fio: string): string {
  if (!fio) return '?'
  const parts = fio.trim().split(/\s+/)
  return parts.slice(0, 2).map(p => p[0]?.toUpperCase() ?? '').join('')
}

function isMine(s: any): boolean {
  return s && mySportsmenIds.value.has(s.id)
}

function monthsRaw(createdAt: string): number {
  if (!createdAt) return 0
  const from = new Date(createdAt)
  const now = new Date()
  return (now.getFullYear() - from.getFullYear()) * 12 + (now.getMonth() - from.getMonth())
}

function monthsLabel(createdAt: string): string {
  const m = monthsRaw(createdAt)
  if (!createdAt || m < 0) return '—'
  if (m < 1) return 'меньше месяца'  // будет редко — createdAt = дата первой тренировки
  if (m < 12) return `${m} мес.`
  const y = Math.floor(m / 12)
  const rem = m % 12
  return rem > 0 ? `${y} г. ${rem} мес.` : `${y} г.`
}

function compareNum(a: number | null | undefined, b: number | null | undefined, moreIsBetter: boolean): number | undefined {
  if (a == null || b == null) return undefined
  if (a === b) return undefined
  if (moreIsBetter) return a > b ? 0 : 1
  return a < b ? 0 : 1
}

function mnum(slot: number, key: string): number | null {
  const m = metrics.value[slot]
  if (!m) return null
  const v = m[key]
  return v != null ? Number(v) : null
}

function mval(slot: number, key: string): string {
  const v = mnum(slot, key)
  if (v == null) return '—'
  return Number.isInteger(v) ? String(v) : v.toFixed(1)
}

function mvalKm(slot: number, key: string): string {
  const v = mnum(slot, key)
  if (v == null) return '—'
  return (v / 1000).toFixed(2)
}

const gpsWins = computed(() => {
  const w = [0, 0]
  if (!players.value[0] || !players.value[1]) return w
  const keys: Array<[string, boolean]> = [
    ['totalDistance', true],
    ['sprintDistance', true],
    ['maximumSpeed', true],
    ['averageSpeed', true],
    ['accelerationCount', true],
    ['decelerationCount', true],
    ['playerLoad', true],
    ['maxHeartRate', true],
  ]
  for (const [key, mib] of keys) {
    const winner = compareNum(mnum(0, key), mnum(1, key), mib)
    if (winner === 0) w[0]++
    else if (winner === 1) w[1]++
  }
  return w
})

function openPicker(slot: number) {
  pickerSlot.value = slot
  pickerSearch.value = ''
  pickerGroupKey.value = 'mine'
  pickerOpen.value = true
}

async function selectSportsman(s: any) {
  players.value[pickerSlot.value] = s
  pickerOpen.value = false
  await Promise.all([
    loadMetrics(s.id, pickerSlot.value),
    loadMatchWins(s.id, pickerSlot.value),
  ])
}

async function loadMetrics(sportsmanId: number, slot: number) {
  metricsLoading.value = true
  try {
    const from = new Date()
    from.setMonth(from.getMonth() - 6)
    const fromStr = from.toISOString().slice(0, 10)

    const res = await api.get('/metric', {
      params: {
        filters: { sportsmanId: [sportsmanId], date: { from: fromStr } },
        pagination: { page: 1, pageSize: 200 },
      }
    }).catch(() => null)

    const data: any[] = res?.data?.data ?? res?.data ?? []
    const list = Array.isArray(data) ? data : []

    if (list.length === 0) {
      metrics.value[slot] = null
      return
    }

    const numericKeys = [
      'totalDistance', 'sprintDistance', 'maximumSpeed', 'averageSpeed',
      'accelerationCount', 'decelerationCount', 'playerLoad',
      'averageHeartRate', 'maxHeartRate',
    ]
    const avg: Record<string, number | null> = {}
    for (const key of numericKeys) {
      const vals = list.map((m: any) => m[key]).filter((v: any) => v != null && !isNaN(Number(v))).map(Number)
      avg[key] = vals.length > 0 ? vals.reduce((a: number, b: number) => a + b, 0) / vals.length : null
    }
    metrics.value[slot] = avg
  } finally {
    metricsLoading.value = false
  }
}

async function loadMatchWins(sportsmanId: number, slot: number) {
  matchWinsLoading.value = true
  try {
    // Найти команду спортсмена
    const player = players.value[slot]
    const teamId = player?.teamId
    if (!teamId) {
      matchStats.value[slot] = null
      return
    }

    // Берём матчи где команда спортсмена была хозяином ИЛИ соперником (для матчей между нашими)
    const [homeRes, awayRes] = await Promise.allSettled([
      api.get('/match', { params: { filters: { homeTeamId: [teamId] }, pagination: { page: 1, pageSize: 500 } } }),
      api.get('/match', { params: { filters: { opponentTeamId: [teamId] }, pagination: { page: 1, pageSize: 500 } } }),
    ])
    const res = {
      data: {
        data: [
          ...(homeRes.status === 'fulfilled' ? (homeRes.value.data?.data ?? []) : []),
          ...(awayRes.status === 'fulfilled' ? (awayRes.value.data?.data ?? []) : []),
        ],
      },
    }

    const list: any[] = res?.data?.data ?? res?.data ?? []
    if (!Array.isArray(list) || list.length === 0) {
      matchStats.value[slot] = null
      return
    }

    // Фильтруем матчи где спортсмен есть в lineup
    const participated = list.filter((m: any) =>
      Array.isArray(m.lineup) && m.lineup.some((e: any) => Number(e.sportsmanId) === sportsmanId)
    )

    if (participated.length === 0) {
      matchStats.value[slot] = null
      return
    }

    let wins = 0, losses = 0, draws = 0
    for (const m of participated) {
      // Определяем «свои голы» по тому, на какой стороне команда спортсмена
      const isHome = m.homeTeamId === teamId
      const myGoals  = Number((isHome ? m.homeStats : m.awayStats)?.goals ?? 0)
      const oppGoals = Number((isHome ? m.awayStats : m.homeStats)?.goals ?? 0)
      if (myGoals > oppGoals) wins++
      else if (myGoals < oppGoals) losses++
      else draws++
    }
    matchStats.value[slot] = { wins, losses, draws }
  } finally {
    matchWinsLoading.value = false
  }
}

async function load() {
  const tid = auth.personalId

  const [sportsmenRes, groupsRes, teamsRes] = await Promise.allSettled([
    api.get('/sportsman', { params: { pagination: { page: 1, pageSize: 500 } } }),
    api.get('/group', { params: tid ? { filters: { trainerId: [tid] } } : undefined }),
    api.get('/team',  { params: tid ? { filters: { trainerId: [tid] } } : undefined }),
  ])

  allSportsmen.value = (sportsmenRes as any).value?.data?.data ?? []
  myGroups.value = (groupsRes as any).value?.data?.data ?? []
  myTeams.value  = (teamsRes as any).value?.data?.data ?? []

  // Кеш имён команд
  const nameMap: Record<number, string> = {}
  myTeams.value.forEach((t: any) => { nameMap[t.id] = t.name })
  teamNameById.value = nameMap

  const myIds = new Set<number>()

  await Promise.allSettled([
    ...myGroups.value.map((g: any) =>
      api.get(`/sportsman/group/${g.id}`).then((r: any) => {
        const list = r.data?.data ?? []
        sportsmenByGroup.value[g.id] = list
        list.forEach((s: any) => myIds.add(s.id))
      }).catch(() => {})
    ),
    ...myTeams.value.map((t: any) =>
      api.get('/sportsman', { params: { filters: { teamId: [t.id] }, pagination: { page: 1, pageSize: 500 } } }).then((r: any) => {
        const list = r.data?.data ?? []
        sportsmenByTeam.value[t.id] = list
        list.forEach((s: any) => myIds.add(s.id))
      }).catch(() => {})
    ),
  ])

  mySportsmenIds.value = myIds
}

function playerTeam(s: any): string {
  if (!s?.teamId) return '—'
  return teamNameById.value[s.teamId] ?? `Команда ${s.teamId}`
}

function playerGroups(s: any): string {
  if (!s?.id) return '—'
  const names = myGroups.value
    .filter((g: any) => sportsmenByGroup.value[g.id]?.some((sp: any) => sp.id === s.id))
    .map((g: any) => g.name)
  return names.length ? names.join(', ') : '—'
}

onMounted(load)
</script>
