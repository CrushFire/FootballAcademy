<template>
  <div class="bg-white rounded-2xl border border-neutral-200 p-5 flex flex-col gap-3">

    <!-- Период и тип активности над графиком -->
    <div class="flex items-center justify-between gap-3 flex-wrap">
      <div class="flex items-center gap-2 flex-wrap">
        <button
          v-for="p in periods" :key="p.value"
          @click="selectedPeriod = p.value"
          :class="['px-3 py-1 rounded-full text-xs font-semibold transition-colors', selectedPeriod === p.value ? 'bg-neutral-800 text-white' : 'bg-neutral-100 text-neutral-500 hover:bg-neutral-200']"
        >{{ p.label }}</button>
      </div>
      
      <div class="flex items-center gap-2 flex-wrap">
        <button
          v-for="t in activityTypes" :key="t.value"
          @click="selectedActivity = t.value"
          :class="['px-2.5 py-1 rounded-lg text-xs font-medium transition-colors border', selectedActivity === t.value ? 'bg-blue-50 border-blue-400 text-blue-600' : 'bg-white border-neutral-200 text-neutral-400 hover:border-neutral-300']"
        >{{ t.label }}</button>
      </div>
    </div>

    <!-- График -->
    <div class="relative" style="height: 280px;">
      <svg
        v-if="hasData"
        class="w-full h-full overflow-visible"
        :viewBox="`0 0 ${W} ${H}`"
        preserveAspectRatio="none"
        @mousemove="onMouseMove"
        @mouseleave="tooltip = null"
      >
        <g v-for="(tick, i) in yTicks" :key="i">
          <line :x1="PAD_L" :y1="tick.y" :x2="W" :y2="tick.y" stroke="#f1f5f9" stroke-width="1"/>
          <text :x="PAD_L - 4" :y="tick.y + 4" text-anchor="end" font-size="10" fill="#94a3b8">{{ tick.label }}</text>
        </g>
        <template v-for="(series, si) in activeSeries" :key="series.key">
          <defs>
            <linearGradient :id="`g-${series.key}`" x1="0" y1="0" x2="0" y2="1">
              <stop offset="0%" :stop-color="seriesColor(si)" stop-opacity="0.15"/>
              <stop offset="100%" :stop-color="seriesColor(si)" stop-opacity="0"/>
            </linearGradient>
          </defs>
          <polygon v-if="series.fill" :points="series.fill" :fill="`url(#g-${series.key})`"/>
          <polyline :points="series.points" fill="none" :stroke="seriesColor(si)" stroke-width="2" stroke-linejoin="round" stroke-linecap="round"/>
          <circle v-for="(pt, pi) in series.dots" :key="pi" :cx="pt.x" :cy="pt.y" r="3.5" :fill="seriesColor(si)"/>
        </template>
        <line v-if="tooltip" :x1="tooltip.x" y1="0" :x2="tooltip.x" :y2="H" stroke="#94a3b8" stroke-width="1" stroke-dasharray="3,3"/>
      </svg>
      <div v-else class="flex items-center justify-center h-full text-sm text-neutral-400">Нет данных за выбранный период</div>

      <div
        v-if="tooltip"
        class="absolute pointer-events-none bg-white border border-neutral-200 rounded-xl shadow-lg px-3 py-2 text-xs z-10 min-w-max"
        :style="{ left: tooltip.screenX + 'px', top: '0px', transform: tooltip.screenX > W * 0.6 ? 'translateX(-110%)' : 'translateX(8px)' }"
      >
        <div class="font-semibold text-neutral-700 mb-1">{{ tooltip.date }}</div>
        <div v-for="(v, i) in tooltip.values" :key="i" class="flex items-center gap-1.5">
          <span class="w-2 h-2 rounded-full" :style="{ backgroundColor: seriesColor(i) }"></span>
          <span class="text-neutral-500">{{ v.label }}:</span>
          <span class="font-semibold text-neutral-800">{{ v.value }}</span>
        </div>
      </div>
    </div>

    <!-- Легенда -->
    <div v-if="activeSeries.length > 1" class="flex gap-3 flex-wrap px-1">
      <div v-for="(s, i) in activeSeries" :key="s.key" class="flex items-center gap-1.5 text-xs text-neutral-500">
        <span class="w-4 h-0.5 rounded-full inline-block" :style="{ backgroundColor: seriesColor(i) }"></span>
        {{ currentGroup.params.find(p => p.key === s.key)?.label }}
      </div>
    </div>

    <!-- Контролы снизу -->
    <div class="border-t border-neutral-100 pt-3 flex flex-col gap-3">

      <div class="flex gap-1 flex-wrap">
        <button
          v-for="g in groups" :key="g.key"
          @click="onGroupClick(g.key)"
          class="px-3 py-1.5 rounded-xl text-xs font-semibold transition-all border flex items-center gap-1"
          :class="activeGroup === g.key
            ? ''
            : 'bg-white border-neutral-200 text-neutral-400'"
          :style="activeGroup === g.key
            ? { backgroundColor: g.bg, borderColor: g.color, color: g.textColor }
            : {}"
        >
          {{ g.label }}
          <AppTooltip :text="CATEGORY_TOOLTIPS[g.key]" />
        </button>
      </div>

      <div class="flex items-start justify-between gap-2">
        <div class="flex gap-2 flex-wrap">
          <button
            v-for="(p, pi) in currentGroup.params" :key="p.key"
            @click="toggleParam(p.key)"
            class="px-2.5 py-1 rounded-lg text-xs font-medium transition-all border"
            :class="activeParams.has(p.key)
              ? ''
              : 'bg-white border-neutral-200 text-neutral-400'"
            :style="activeParams.has(p.key)
              ? { backgroundColor: hexWithAlpha(currentGroup.colors[pi % currentGroup.colors.length], 0.12), borderColor: currentGroup.colors[pi % currentGroup.colors.length], color: currentGroup.colors[pi % currentGroup.colors.length] }
              : {}"
          >{{ p.label }}</button>
        </div>

        <div class="flex items-center gap-1 flex-shrink-0">
          <button
            @click="resetParams"
            class="px-2 py-0.5 rounded-md border text-xs transition-colors border-neutral-200 text-neutral-400 hover:border-neutral-400 hover:text-neutral-600"
          >Сброс</button>
        </div>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { formatDate } from '@/utils/formatDate'
import AppTooltip from '@/components/ui/AppTooltip.vue'
import { CATEGORY_TOOLTIPS } from '@/constants/categoryTooltips'
// @ts-ignore
import tailwindConfig from '../../../tailwind.config.js'

const tw = tailwindConfig.theme.extend.colors.chart

interface GraphPoint {
  date: string
  metrics: Record<string, number>
  type?: string
  matchType?: string
}

const props = defineProps<{ points: GraphPoint[] }>()

const W = 560, H = 240, PAD_L = 38

const periods = [
  { label: '1 мес', value: 1 },
  { label: '3 мес', value: 3 },
  { label: '6 мес', value: 6 },
  { label: '1 год', value: 12 },
  { label: 'Всё',   value: 0 },
]
const selectedPeriod = ref(3)
const scaleAuto = true // всегда авто

const activityTypes = [
  { label: 'Все', value: 'all' },
  { label: 'Тренировки', value: 'training' },
  { label: 'Матчи', value: 'match' },
  { label: 'Товарищеские', value: 'friendly' },
  { label: 'Официальные', value: 'official' },
]
const selectedActivity = ref('all')

const groups = [
  {
    key: 'speed', label: 'Скорость',
    color: tw.blue.DEFAULT, colors: [tw.blue.DEFAULT, tw.blue.light, tw.blue.dark, tw.blue.muted, tw.blue.dark],
    bg: tw.blue.bg, textColor: tw.blue.text,
    params: [
      { key: 'avgSpeed',            label: 'Ср. скорость',     unit: 'км/ч', min: 3,    max: 10 },
      { key: 'maxSpeed',            label: 'Макс. скорость',   unit: 'км/ч', min: 18,   max: 36 },
      { key: 'sprintRatio',         label: 'Доля спринта',     unit: '',     min: 0,    max: 0.15 },
      { key: 'highSpeedRatio',      label: 'Высокая скорость', unit: '',     min: 0,    max: 0.30 },
      { key: 'sprintEffortsPerMin', label: 'Спринтов/мин',     unit: '',     min: 0,    max: 0.35 },
    ]
  },
  {
    key: 'power', label: 'Мощность',
    color: tw.amber.DEFAULT, colors: [tw.amber.DEFAULT, tw.amber.dark, tw.amber.light, tw.amber.text, tw.amber.muted],
    bg: tw.amber.bg, textColor: tw.amber.text,
    params: [
      { key: 'playerLoad',     label: 'Нагрузка',        unit: '',     min: 100, max: 450 },
      { key: 'explosiveIndex', label: 'Взрывной индекс', unit: '',     min: 0,   max: 25 },
      { key: 'metabolicPower', label: 'Метаб. мощность', unit: 'W/kg', min: 3,   max: 12 },
    ]
  },
  {
    key: 'cardio', label: 'Кардио',
    color: tw.red.DEFAULT, colors: [tw.red.DEFAULT, tw.red.dark, tw.red.light, tw.red.text, tw.red.muted],
    bg: tw.red.bg, textColor: tw.red.text,
    params: [
      { key: 'hrRedPercent',     label: 'Красная зона ЧСС',   unit: '', min: 0,    max: 0.12 },
      { key: 'cardioEfficiency', label: 'Кардио эфф.',        unit: '', min: 20,   max: 70 },
      { key: 'hrStability',      label: 'Стаб. ЧСС',          unit: '', min: 0.65, max: 0.95 },
      { key: 'recoveryIndex',    label: 'Инд. восстановления', unit: '', min: 0,    max: 15 },
    ]
  },
  {
    key: 'fatigue', label: 'Нагрузка',
    color: tw.violet.DEFAULT, colors: [tw.violet.DEFAULT, tw.violet.dark, tw.violet.light, tw.violet.text, tw.violet.muted],
    bg: tw.violet.bg, textColor: tw.violet.text,
    params: [
      { key: 'fatigueIndex',   label: 'Усталость',         unit: '', min: 0.7, max: 1.6 },
      { key: 'aerobicLoad',    label: 'Аэробная нагрузка', unit: '', min: 0,   max: 0.85 },
      { key: 'anaerobicRatio', label: 'Анаэробное соотн.', unit: '', min: 0,   max: 1.5 },
      { key: 'workRatio',      label: 'Рабочий коэф.',     unit: '', min: 0.8, max: 3.2 },
      { key: 'hi_LO_Ratio',    label: 'Высок./низк.',      unit: '', min: 0,   max: 4 },
    ]
  },
  {
    key: 'efficiency', label: 'Эффективность',
    color: tw.green.DEFAULT, colors: [tw.green.DEFAULT, tw.green.dark, tw.green.light, tw.green.text, tw.green.muted],
    bg: tw.green.bg, textColor: tw.green.text,
    params: [
      { key: 'energyEfficiency', label: 'Энергоэфф.',   unit: '', min: 10, max: 40 },
      { key: 'consistency',      label: 'Стабильность', unit: '', min: 0,  max: 25 },
      { key: 'rsa',              label: 'RSA',           unit: '', min: 0,  max: 12 },
    ]
  },
]

function hexWithAlpha(hex: string, alpha: number): string {
  const r = parseInt(hex.slice(1, 3), 16)
  const g = parseInt(hex.slice(3, 5), 16)
  const b = parseInt(hex.slice(5, 7), 16)
  return `rgba(${r},${g},${b},${alpha})`
}

const activeGroup = ref('speed')
const currentGroup = computed(() => groups.find(g => g.key === activeGroup.value)!)
const activeParams = ref(new Set<string>(['avgSpeed']))

function toggleParam(key: string) {
  if (activeParams.value.has(key)) {
    if (activeParams.value.size > 1) activeParams.value.delete(key)
  } else {
    activeParams.value.add(key)
  }
}

function resetParams() {
  activeGroup.value = 'speed'
  activeParams.value = new Set(['avgSpeed'])
  selectedPeriod.value = 3
  selectedActivity.value = 'all'
}

function onGroupClick(key: string) {
  if (activeGroup.value !== key) {
    activeGroup.value = key
    activeParams.value = new Set([groups.find(g => g.key === key)!.params[0].key])
  }
}

const filteredPoints = computed(() => {
  let sorted = [...props.points].sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime())
  
  // Фильтр по типу активности
  if (selectedActivity.value === 'training') {
    // Только тренировки (без матчей)
    sorted = sorted.filter(p => !p.matchType)
  } else if (selectedActivity.value === 'match') {
    // Только матчи (любые)
    sorted = sorted.filter(p => p.matchType)
  } else if (selectedActivity.value === 'friendly') {
    // Только товарищеские матчи
    sorted = sorted.filter(p => p.matchType === 'Friendly')
  } else if (selectedActivity.value === 'official') {
    // Только официальные матчи (League, Cup, Tournament)
    sorted = sorted.filter(p => p.matchType && p.matchType !== 'Friendly')
  }
  // 'all' — без фильтрации
  
  // Фильтр по периоду
  if (selectedPeriod.value === 0) return sorted.map(p => ({ ...p, dateLabel: formatDate(p.date) }))
  const from = new Date()
  from.setMonth(from.getMonth() - selectedPeriod.value)
  return sorted.filter(p => new Date(p.date) >= from).map(p => ({ ...p, dateLabel: formatDate(p.date) }))
})

function seriesColor(i: number) {
  return currentGroup.value.colors[i % currentGroup.value.colors.length]
}

const yRange = computed(() => {
  const defs = currentGroup.value.params.filter(p => activeParams.value.has(p.key))
  if (!defs.length) return { min: 0, max: 1 }
  if (scaleAuto) {
    const allVals: number[] = []
    for (const p of defs)
      for (const pt of filteredPoints.value)
        allVals.push(pt.metrics[p.key] ?? 0)
    if (!allVals.length) return { min: 0, max: 1 }
    const mn = Math.min(...allVals), mx = Math.max(...allVals)
    const pad = (mx - mn) * 0.1 || 0.1
    return { min: mn - pad, max: mx + pad }
  }
  return { min: Math.min(...defs.map(p => p.min)), max: Math.max(...defs.map(p => p.max)) }
})

const yTicks = computed(() => {
  const { min, max } = yRange.value
  return Array.from({ length: 5 }, (_, i) => {
    const val = min + (max - min) * (i / 4)
    return { y: H - (i / 4) * H, label: val >= 10 ? val.toFixed(0) : val.toFixed(2) }
  })
})

function toY(raw: number, _p: any): number {
  const { min, max } = yRange.value
  return H - ((raw - min) / (max - min || 1)) * (H - 12) - 6
}

function toX(i: number): number {
  const n = filteredPoints.value.length
  return n < 2 ? PAD_L + (W - PAD_L) / 2 : PAD_L + (i / (n - 1)) * (W - PAD_L)
}

const activeSeries = computed(() =>
  currentGroup.value.params
    .filter(p => activeParams.value.has(p.key))
    .map(p => {
      const dots = filteredPoints.value
        .map((pt, i) => pt.metrics[p.key] == null ? null : { x: toX(i), y: toY(pt.metrics[p.key], p) })
        .filter(Boolean) as { x: number; y: number }[]
      if (dots.length < 2) return { key: p.key, dots: [], points: '', fill: '' }
      const pts = dots.map(d => `${d.x},${d.y}`).join(' ')
      return { key: p.key, dots, points: pts, fill: `${pts} ${dots[dots.length - 1].x},${H} ${dots[0].x},${H}` }
    })
)

const hasData = computed(() => activeSeries.value.some(s => s.dots.length >= 2))

const tooltip = ref<{ x: number; screenX: number; date: string; values: { label: string; value: string }[] } | null>(null)

function onMouseMove(e: MouseEvent) {
  const rect = (e.currentTarget as SVGElement).getBoundingClientRect()
  const relX = ((e.clientX - rect.left) / rect.width) * W
  const n = filteredPoints.value.length
  if (n < 2) return
  let closest = 0, minDist = Infinity
  for (let i = 0; i < n; i++) {
    const d = Math.abs(toX(i) - relX)
    if (d < minDist) { minDist = d; closest = i }
  }
  const pt = filteredPoints.value[closest]
  tooltip.value = {
    x: toX(closest),
    screenX: (toX(closest) / W) * rect.width,
    date: pt.dateLabel,
    values: currentGroup.value.params
      .filter(p => activeParams.value.has(p.key))
      .map(p => {
        const v = pt.metrics[p.key] ?? 0
        return { label: p.label, value: `${v >= 10 ? v.toFixed(1) : v.toFixed(3)}${p.unit ? ' ' + p.unit : ''}` }
      })
  }
}
</script>
