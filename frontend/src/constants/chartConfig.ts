export const CHART_PALETTES = {
  blue: {
    color:     '#3b82f6',
    colors:    ['#3b82f6', '#60a5fa', '#1d4ed8', '#93c5fd', '#2563eb'],
    bg:        '#eff6ff',
    textColor: '#1d4ed8',
  },
  amber: {
    color:     '#f59e0b',
    colors:    ['#f59e0b', '#d97706', '#fbbf24', '#b45309', '#fcd34d'],
    bg:        '#fffbeb',
    textColor: '#b45309',
  },
  red: {
    color:     '#ef4444',
    colors:    ['#ef4444', '#dc2626', '#f87171', '#b91c1c', '#fca5a5'],
    bg:        '#fef2f2',
    textColor: '#b91c1c',
  },
  violet: {
    color:     '#8b5cf6',
    colors:    ['#8b5cf6', '#7c3aed', '#a78bfa', '#6d28d9', '#c4b5fd'],
    bg:        '#f5f3ff',
    textColor: '#6d28d9',
  },
  green: {
    color:     '#10b981',
    colors:    ['#10b981', '#059669', '#34d399', '#047857', '#6ee7b7'],
    bg:        '#ecfdf5',
    textColor: '#047857',
  },
} as const

export type PaletteKey = keyof typeof CHART_PALETTES

export interface MetricParam {
  key:      string
  label:    string
  unit:     string
  min:      number
  max:      number
  scale?:   number   // множитель перед отображением (например ×100 для ratio → %)
}

export interface MetricGroup {
  key:       string
  label:     string
  palette:   PaletteKey
  allowAuto: boolean
  params:    MetricParam[]
}

export const METRIC_GROUPS: MetricGroup[] = [
  {
    key: 'speed', label: 'Скорость', palette: 'blue', allowAuto: true,
    params: [
      { key: 'avgSpeed',            label: 'Ср. скорость',     unit: 'км/ч', min: 0, max: 12 },
      { key: 'maxSpeed',            label: 'Макс. скорость',   unit: 'км/ч', min: 0, max: 35 },
      { key: 'sprintRatio',         label: 'Доля спринта',     unit: '%',    min: 0, max: 15,  scale: 100 },
      { key: 'highSpeedRatio',      label: 'Высокая скорость', unit: '%',    min: 0, max: 30,  scale: 100 },
      { key: 'sprintEffortsPerMin', label: 'Спринтов/мин',     unit: '',     min: 0, max: 0.5 },
    ]
  },
  {
    key: 'power', label: 'Мощность', palette: 'amber', allowAuto: true,
    params: [
      { key: 'playerLoad',     label: 'Нагрузка',        unit: '', min: 0, max: 450 },
      { key: 'explosiveIndex', label: 'Взрывной индекс', unit: '', min: 0, max: 0.06 },
      { key: 'metabolicPower', label: 'Метаб. мощность', unit: 'W/kg', min: 0, max: 12 },
    ]
  },
  {
    key: 'cardio', label: 'Кардио', palette: 'red', allowAuto: true,
    params: [
      { key: 'hrRedPercent',     label: 'Красная зона ЧСС',    unit: '',  min: 0, max: 0.06 },
      { key: 'cardioEfficiency', label: 'Кардио эфф.',         unit: '',  min: 0, max: 65 },
      { key: 'hrStability',      label: 'Стаб. ЧСС',           unit: '',  min: 0.6, max: 1.0 },
      { key: 'recoveryIndex',    label: 'Инд. восстановления',  unit: '',  min: 0, max: 180 },
    ]
  },
  {
    key: 'fatigue', label: 'Нагрузка', palette: 'violet', allowAuto: true,
    params: [
      { key: 'fatigueIndex',   label: 'Усталость',         unit: '', min: 0,   max: 2.5 },
      { key: 'aerobicLoad',    label: 'Аэробная нагрузка', unit: '', min: 0.4, max: 1.0 },
      { key: 'anaerobicRatio', label: 'Анаэробное соотн.', unit: '', min: 0,   max: 1.8 },
      { key: 'workRatio',      label: 'Рабочий коэф.',     unit: '', min: 0,   max: 3.5 },
      { key: 'hi_LO_Ratio',    label: 'Высок./низк.',      unit: '', min: 0,   max: 1.2 },
    ]
  },
  {
    key: 'efficiency', label: 'Эффективность', palette: 'green', allowAuto: true,
    params: [
      { key: 'energyEfficiency', label: 'Энергоэфф.',   unit: '', min: 0, max: 45 },
      { key: 'consistency',      label: 'Стабильность', unit: '', min: 0, max: 60 },
      { key: 'rsa',              label: 'RSA',           unit: '', min: 0, max: 3.5 },
    ]
  },
]
