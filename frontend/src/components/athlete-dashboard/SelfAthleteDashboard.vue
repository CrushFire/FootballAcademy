<template>
  <AppCard no-border class="flex-1">
    <div class="flex items-start justify-between mb-6 pb-4 border-b border-neutral-100">
      <p class="text-sm text-neutral-500">{{ auth.userLogin ?? 'Пользователь' }}</p>
      <span class="text-sm text-neutral-400 capitalize">{{ todayStr }}</span>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">

      <!-- Левая: пятиугольник + ближайшее занятие -->
      <div class="col-span-1 flex flex-col gap-4 border-r border-neutral-100 pr-6">
        <DashCard title="Профиль игрока">
          <div class="flex justify-center">
            <svg :viewBox="`0 0 ${SVG} ${SVG}`" class="w-full max-w-[240px] h-auto">
              <polygon v-for="lvl in [0.25,0.5,0.75,1]" :key="lvl" :points="gridPoints(lvl)" fill="none" stroke="#e2e8f0" stroke-width="1"/>
              <line v-for="(pt, i) in outerPoints" :key="'ax'+i" :x1="CX" :y1="CY" :x2="pt.x" :y2="pt.y" stroke="#e2e8f0" stroke-width="1"/>
              <polygon :points="dataPoints" fill="rgba(59,130,246,0.18)" stroke="#3b82f6" stroke-width="2"/>
              <circle v-for="(pt, i) in dataPointsArr" :key="'dp'+i" :cx="pt.x" :cy="pt.y" r="4" fill="#3b82f6"/>
              <text v-for="(item, i) in PENTAGON_LABELS" :key="'lb'+i" :x="labelPos(i).x" :y="labelPos(i).y" text-anchor="middle" dominant-baseline="middle" font-size="11" fill="#475569" font-weight="600">{{ item.label }}</text>
            </svg>
          </div>
          <div class="flex flex-col gap-2">
            <div v-for="(item, i) in PENTAGON_LABELS" :key="item.key" class="flex items-center gap-2 text-sm">
              <span class="w-2 h-2 rounded-full bg-blue-500 shrink-0" />
              <span class="flex-1 text-neutral-500 flex items-center gap-1">
                {{ item.label }}
                <AppTooltip v-if="PENTAGON_TOOLTIPS[item.key]" :text="PENTAGON_TOOLTIPS[item.key]" />
              </span>
              <span class="font-bold text-blue-900">{{ Math.round(pentagonValues[i] * 100) }}%</span>
            </div>
          </div>
        </DashCard>

        <DashCard title="Ближайшее занятие" :icon="true" icon-bg="bg-blue-50 text-blue-500" :clickable="true" @click="$emit('go-schedule')">
          <template #icon>
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5"><path stroke-linecap="round" stroke-linejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/></svg>
          </template>
          <template v-if="nextClass">
            <div class="text-sm font-semibold text-neutral-800 capitalize">{{ nextClass.nextDate.toLocaleDateString('ru-RU', { weekday: 'long', day: 'numeric', month: 'short' }) }}</div>
            <div class="text-xs text-neutral-500 mt-0.5">{{ ((nextClass.BeginTime ?? nextClass.beginTime) ?? '').slice(0, 5) }} · {{ nextClass.SportHall ?? nextClass.sportHall }}</div>
          </template>
          <div v-else class="text-sm text-neutral-400">Нет запланированных занятий</div>
          <div class="text-xs text-blue-500 font-semibold mt-2">Перейти к расписанию →</div>
        </DashCard>
      </div>

      <!-- Средняя: матч + посещаемость + нормативы -->
      <div class="col-span-1 flex flex-col gap-4">
        <DashCard title="Последний матч" :icon="true" icon-bg="bg-green-50 text-green-500" :clickable="true" @click="$emit('go-matches')">
          <template #icon>
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5"><circle cx="12" cy="12" r="10"/><path stroke-linecap="round" stroke-linejoin="round" d="M12 8v4m0 4h.01"/></svg>
          </template>
          <template v-if="lastMatch">
            <div class="text-base font-bold text-neutral-900">{{ lastMatch.homeTeamName }} vs {{ lastMatch.opponentTeamName }}</div>
            <div class="text-sm text-neutral-500">{{ matchType[lastMatch.type] ?? lastMatch.type }}</div>
            <template v-if="lastMatch.status === 'Scheduled'">
              <span class="inline-flex mt-1 px-2.5 py-0.5 rounded-full bg-green-100 text-green-700 text-xs font-bold w-fit">Запланирован</span>
              <div class="text-xs text-neutral-400 mt-0.5">{{ formatDate(lastMatch.date) }}</div>
            </template>
            <template v-else>
              <div class="text-xl font-extrabold mt-1" :class="resultClass(lastMatch.result)">{{ lastMatch.result ? matchResult[lastMatch.result] : matchStatus[lastMatch.status] }}</div>
              <div class="text-xs text-neutral-400">{{ formatDate(lastMatch.date) }}</div>
            </template>
          </template>
          <div v-else class="text-sm text-neutral-400">Нет данных о матчах</div>
          <div class="text-xs text-blue-500 font-semibold mt-2">Все матчи →</div>
        </DashCard>

        <DashCard title="Посещаемость" :clickable="true" @click="$emit('go-attendance')">
          <div class="text-xs text-neutral-400 mb-2">За последние 3 месяца</div>
          <div class="flex gap-4 mb-2">
            <div class="flex flex-col gap-0.5"><span class="text-xl font-semibold text-blue-500">{{ attendanceStats.present }}</span><span class="text-xs text-neutral-600">Присутствовал</span></div>
            <div class="flex flex-col gap-0.5"><span class="text-xl font-semibold text-sky-500">{{ attendanceStats.late }}</span><span class="text-xs text-neutral-600">Опоздал</span></div>
            <div class="flex flex-col gap-0.5"><span class="text-xl font-semibold text-neutral-600">{{ attendanceStats.absent }}</span><span class="text-xs text-neutral-600">Отсутствовал</span></div>
          </div>
          <div class="flex h-2 rounded-full overflow-hidden bg-neutral-100">
            <div class="bg-blue-500 h-full transition-all" :style="{ width: attendancePct.present + '%' }" />
            <div class="bg-sky-400 h-full transition-all" :style="{ width: attendancePct.late + '%' }" />
            <div class="bg-neutral-600 h-full transition-all" :style="{ width: attendancePct.absent + '%' }" />
          </div>
          <div class="text-xs text-blue-500 font-semibold mt-1">Вся история посещений →</div>
        </DashCard>


        <DashCard title="Нормативы" :icon="true" icon-bg="bg-teal-50 text-orange-500" :clickable="true" @click="$emit('go-normatives')">
          <template #icon>
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5"><path stroke-linecap="round" stroke-linejoin="round" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"/></svg>
          </template>
          <div v-if="normStats" class="flex items-center gap-3">
            <div class="flex items-center gap-1.5"><span class="text-xs font-bold px-2 py-0.5 rounded-full bg-green-100 text-green-700">Отлично</span><span class="text-lg font-extrabold text-neutral-900">{{ normStats.excellent }}</span></div>
            <div class="flex items-center gap-1.5"><span class="text-xs font-bold px-2 py-0.5 rounded-full bg-blue-100 text-blue-700">Хорошо</span><span class="text-lg font-extrabold text-neutral-900">{{ normStats.good }}</span></div>
            <div class="flex items-center gap-1.5"><span class="text-xs font-bold px-2 py-0.5 rounded-full bg-neutral-100 text-neutral-600">Удовл.</span><span class="text-lg font-extrabold text-neutral-900">{{ normStats.satisfactory }}</span></div>
          </div>
          <div v-else class="text-sm text-neutral-400">Нет данных о нормативах</div>
          <div class="text-xs text-blue-500 font-semibold mt-2">Все нормативы →</div>
        </DashCard>

        <!-- Игровой профиль -->
        <DashCard title="Игровой профиль" :icon="true" icon-bg="bg-slate-100 text-slate-500" class="relative">
          <template #icon>
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5"><path stroke-linecap="round" stroke-linejoin="round" d="M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.196-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118L2.05 10.1c-.783-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.673z"/></svg>
          </template>

          <!-- Легенда цветов плашек — на уровне иконки-звезды -->
          <div class="absolute top-7 right-4 flex items-center gap-3">
            <div class="flex items-center gap-1.5">
              <span class="w-3 h-3 rounded-full border border-sky-200 bg-sky-100 shrink-0"></span>
              <span class="text-[10px] text-neutral-500">Очень подходит</span>
            </div>
            <div class="flex items-center gap-1.5">
              <span class="w-3 h-3 rounded-full border border-dashed border-neutral-300 bg-neutral-100 shrink-0"></span>
              <span class="text-[10px] text-neutral-500">Возможно подойдёт</span>
            </div>
          </div>

          <div v-if="sportsmanPosition" class="flex items-center gap-2 pb-2 mb-2 border-b border-neutral-100">
            <span class="text-xs text-neutral-500">Позиция</span>
            <span class="text-xs font-semibold px-2 py-1 rounded-full border border-sky-100 bg-sky-50 text-sky-700">{{ positionLabel[sportsmanPosition] ?? sportsmanPosition }}</span>
          </div>

          <div v-if="(playerProfiles?.length || 0) + (playerPotentialProfiles?.length || 0) > 0" class="grid grid-cols-2 gap-3">
            <!-- Левая колонка: Подтверждено -->
            <div>
              <div class="text-xs font-bold text-emerald-700 uppercase tracking-wide mb-1.5">Подтверждено</div>
              <div v-if="confirmedQualities.length" class="flex flex-wrap gap-1.5">
                <span
                  v-for="p in confirmedQualities" :key="'a-'+p"
                  class="inline-flex items-center gap-1 text-xs font-semibold px-2 py-1 rounded-full border"
                  :class="profileColor(p)"
                >
                  {{ profileLabels[p] ?? p }}
                  <AppTooltip v-if="PROFILE_TOOLTIPS[p]" :text="PROFILE_TOOLTIPS[p]" />
                </span>
              </div>
              <div v-else class="text-xs text-neutral-400 italic">Нет</div>
            </div>
            <div class="flex flex-col gap-3">
              <div>
                <div class="text-xs font-bold text-neutral-500 uppercase tracking-wide mb-1.5">Возможные позиции</div>
                <div v-if="confirmedPositions.length + suggestedPositions.length" class="flex flex-wrap gap-1.5">
                  <span
                    v-for="pos in confirmedPositions" :key="'cpos-'+pos"
                    class="text-xs font-semibold px-2 py-1 rounded-full border border-sky-100 bg-sky-50 text-sky-700"
                  >{{ positionLabel[pos] ?? pos }}</span>
                  <span
                    v-for="pos in suggestedPositions" :key="'pos-'+pos"
                    class="text-xs font-semibold px-2 py-1 rounded-full border border-dashed bg-neutral-50 text-neutral-500"
                  >{{ positionLabel[pos] ?? pos }}</span>
                </div>
                <div v-else class="text-xs text-neutral-400 italic">Нет</div>
              </div>
              <div>
                <div class="text-xs font-bold text-neutral-500 uppercase tracking-wide mb-1.5">Есть задатки</div>
                <div v-if="nonPositionalPotential.length" class="flex flex-wrap gap-1.5">
                  <span
                    v-for="p in nonPositionalPotential" :key="'p-'+p"
                    class="inline-flex items-center gap-1 text-xs font-semibold px-2 py-1 rounded-full border border-dashed bg-neutral-50 text-neutral-500"
                  >
                    {{ profileLabels[p] ?? p }}
                    <AppTooltip v-if="PROFILE_TOOLTIPS[p]" :text="PROFILE_TOOLTIPS[p]" />
                  </span>
                </div>
                <div v-else class="text-xs text-neutral-400 italic">Нет</div>
              </div>
            </div>
          </div>
          <div v-else class="text-sm text-neutral-400">Недостаточно данных</div>
        </DashCard>
      </div>

      <!-- Правая: последняя тренировка -->
      <div class="col-span-1 flex flex-col gap-4 border-l border-neutral-100 pl-6">
        <DashCard title="Последняя тренировка" :icon="true" icon-bg="bg-purple-50 text-purple-500" :clickable="true" @click="$emit('go-trainings')">
          <template #icon>
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5"><path stroke-linecap="round" stroke-linejoin="round" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>
          </template>
          <template v-if="lastTraining">
            <div class="text-xs text-neutral-400 mt-0.5">{{ formatDate(lastTraining.date) }}</div>
            <template v-if="lastTrainingMetrics">
              <div class="mt-3 pt-3 border-t border-neutral-100 flex flex-col gap-1">
                <div v-for="row in metricRows" :key="row.label" class="flex justify-between text-sm">
                  <span class="text-neutral-500">{{ row.label }}</span>
                  <span class="font-bold text-neutral-900">{{ row.value }}</span>
                </div>
              </div>
            </template>
          </template>
          <div v-else class="text-sm text-neutral-400">Нет данных о тренировках</div>
          <div class="text-xs text-blue-500 font-semibold pt-2">Все тренировки →</div>
        </DashCard>

      </div>

    </div>
  </AppCard>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/store/auth'
import { formatDate } from '@/utils/formatDate'
import AppCard from '@/components/ui/AppCard.vue'
import DashCard from '@/components/ui/DashCard.vue'
import AppTooltip from '@/components/ui/AppTooltip.vue'
import { PROFILE_TOOLTIPS, PENTAGON_LABELS, PENTAGON_TOOLTIPS } from '@/constants/metricTooltips'
import { POSITION_AND_GROUP_LABEL, MATCH_TYPE, MATCH_STATUS, MATCH_RESULT } from '@/constants'
import type { Training, Match } from '@/types'

const props = defineProps<{
  pentagonValues: number[]
  playerProfiles?: string[]
  playerPotentialProfiles?: string[]
  sportsmanPosition?: string | null
  nextClass: any
  lastTraining: Training | null
  lastTrainingMetrics: any
  lastMatch: Match | null
  attendanceStats: { present: number; absent: number; late: number }
  attendancePct: { present: number; absent: number; late: number }
  normStats: { excellent: number; good: number; satisfactory: number } | null
}>()

defineEmits<{
  'go-schedule': []
  'go-matches': []
  'go-normatives': []
  'go-trainings': []
  'go-attendance': []
}>()

const auth = useAuthStore()
const todayStr = new Date().toLocaleDateString('ru-RU', { weekday: 'long', day: 'numeric', month: 'long' })

const matchType   = MATCH_TYPE
const matchStatus = MATCH_STATUS
const matchResult = MATCH_RESULT

function resultClass(r: string | null) {
  if (r === 'Win')  return 'text-green-600'
  if (r === 'Loss') return 'text-red-600'
  return 'text-yellow-600'
}

// Лейблы позиций — единый источник из @/constants (POSITION_AND_GROUP_LABEL включает
// и коды позиций GK/CB/..., и группы Goalkeeper/Defender/...).
const positionLabel = POSITION_AND_GROUP_LABEL

const profileLabels: Record<string, string> = {
  Sprinter:            'Спринтер',
  EnduranceRunner:     'Выносливый',
  PowerPlayer:         'Силовой',
  ExplosivePlayer:     'Взрывной',
  FlankPlayer:         'Фланговый',
  DefenderType:        'Защитный тип',
  Universal:           'Универсал',
  CentralMidfielder:   'Центр. полузащитник',
  DefensiveMidfielder: 'Опорник',
  AttackingMidfielder: 'Атак. полузащитник',
  Forward:             'Нападающий',
  Goalkeeper:          'Вратарь',
  DynamicPlayer:       'Динамичный',
  StaticPlayer:        'Статичный',
  Offensive:           'Атакующий',
  Defensive:           'Оборонительный',
}

// Позиционные профили → конкретные позиции (амплуа)
const positionalProfileToPositions: Record<string, string[]> = {
  FlankPlayer:         ['LW', 'RW', 'LWB', 'RWB', 'LB', 'RB'],
  DefenderType:        ['CB', 'LB', 'RB'],
  CentralMidfielder:   ['CM', 'CAM', 'CDM'],
  DefensiveMidfielder: ['CDM', 'CM'],
  AttackingMidfielder: ['CAM', 'SS', 'CM'],
  Forward:             ['ST', 'CF'],
  Goalkeeper:          ['GK'],
}

// Список позиционных профилей (их перенаправляем в «Подходящие позиции»)
const positionalProfileKeys = new Set(Object.keys(positionalProfileToPositions))

// Подходящие позиции — из потенциальных позиционных профилей, БЕЗ дублирующих active и текущей
const suggestedPositions = computed(() => {
  const confirmedSet = new Set(confirmedPositions.value)
  const set = new Set<string>()
  for (const p of props.playerPotentialProfiles ?? []) {
    if (!positionalProfileKeys.has(p)) continue
    for (const pos of positionalProfileToPositions[p] ?? []) {
      if (pos === props.sportsmanPosition) continue
      if (confirmedSet.has(pos)) continue
      set.add(pos)
    }
  }
  return Array.from(set)
})

// Возможные — только непозиционные потенциальные профили (физические / поведенческие)
const nonPositionalPotential = computed(() => {
  return (props.playerPotentialProfiles ?? []).filter(p => !positionalProfileKeys.has(p))
})

// Подтверждённые качества (непозиционные active профили)
const confirmedQualities = computed(() => {
  return (props.playerProfiles ?? []).filter(p => !positionalProfileKeys.has(p))
})

// Подтверждённые позиции (раскрытые позиционные active профили), БЕЗ текущей позиции
const confirmedPositions = computed(() => {
  const set = new Set<string>()
  for (const p of props.playerProfiles ?? []) {
    if (!positionalProfileKeys.has(p)) continue
    for (const pos of positionalProfileToPositions[p] ?? []) {
      if (pos !== props.sportsmanPosition) set.add(pos)
    }
  }
  return Array.from(set)
})

// Все «Подтверждено» плашки одного зелёного цвета (как заголовок). Работает в светлой и тёмной теме.
function profileColor(_key: string) {
  return 'bg-emerald-50 text-emerald-700 border-emerald-100'
}

const SVG = 260, CX = 130, CY = 120, R = 90
function angleFor(i: number) { return (Math.PI * 2 * i) / 5 - Math.PI / 2 }
const outerPoints = computed(() => PENTAGON_LABELS.map((_, i) => ({ x: CX + R * Math.cos(angleFor(i)), y: CY + R * Math.sin(angleFor(i)) })))
function gridPoints(lvl: number) { return PENTAGON_LABELS.map((_, i) => `${CX + R * lvl * Math.cos(angleFor(i))},${CY + R * lvl * Math.sin(angleFor(i))}`).join(' ') }
const dataPointsArr = computed(() => props.pentagonValues.map((v, i) => ({ x: CX + R * v * Math.cos(angleFor(i)), y: CY + R * v * Math.sin(angleFor(i)) })))
const dataPoints = computed(() => dataPointsArr.value.map(p => `${p.x},${p.y}`).join(' '))
function labelPos(i: number) { return { x: CX + (R + 22) * Math.cos(angleFor(i)), y: CY + (R + 22) * Math.sin(angleFor(i)) } }

const metricRows = computed(() => {
  const m = props.lastTrainingMetrics
  if (!m) return []
  return [
    { label: 'Макс. скорость',   value: m.maxSpeed.toFixed(1) + ' км/ч' },
    { label: 'Ср. скорость',     value: m.avgSpeed.toFixed(1) + ' км/ч' },
    { label: 'Нагрузка',         value: m.playerLoad.toFixed(1) },
    { label: 'Метаб. мощность',  value: m.metabolicPower.toFixed(1) },
    { label: 'Аэробная нагрузка',value: (m.aerobicLoad * 100).toFixed(1) + '%' },
    { label: 'Анаэробная доля',  value: (m.anaerobicRatio * 100).toFixed(1) + '%' },
    { label: 'Спринт / мин',     value: m.sprintEffortsPerMin.toFixed(2) },
    { label: 'Доля спринта',     value: (m.sprintRatio * 100).toFixed(1) + '%' },
    { label: 'Высокоскор. доля', value: (m.highSpeedRatio * 100).toFixed(1) + '%' },
    { label: 'Взрывной индекс',  value: m.explosiveIndex.toFixed(2) },
    { label: 'RSA',              value: (m.rsa ?? 0).toFixed(2) },
    { label: 'ЧСС красная зона', value: ((m.hrRedPct ?? 0) * 100).toFixed(1) + '%' },
    { label: 'Кардио эфф.',      value: (m.cardioEfficiency ?? 0).toFixed(2) },
    { label: 'Стабильность ЧСС', value: (m.hrStability ?? 0).toFixed(2) },
    { label: 'Индекс восст.',    value: (m.recoveryIndex ?? 0).toFixed(2) },
    { label: 'Hi/Lo',            value: (m.hiLoRatio ?? 0).toFixed(2) },
    { label: 'Рабочий коэф.',    value: m.workRatio.toFixed(2) },
    { label: 'Энерго эфф.',      value: m.energyEfficiency.toFixed(2) },
    { label: 'Индекс усталости', value: m.fatigueIndex.toFixed(2) },
    { label: 'Стабильность',     value: m.consistency.toFixed(2) },
  ]
})
</script>
