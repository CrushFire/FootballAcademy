<template>
  <!-- Самостоятельный мобильный компонент: одна половина в вертикальной
       ориентации 3:4. Наши ворота снизу, атакуем вверх.
       Включает слотовую модель + кнопку выбора тактики + сохранение в localStorage.
       Аналог MatchLineupDisplay, но для телефона. -->
  <div>
    <div v-if="!lineup.length" class="text-center text-xs text-neutral-400 py-6 border border-dashed border-neutral-200 rounded-xl">
      Состав не задан — добавьте игроков через «Редактировать состав»
    </div>
    <template v-else>
      <div class="field-container">
        <svg class="field-lines" viewBox="0 0 75 100" preserveAspectRatio="xMidYMid slice">
          <!-- Полосы газона -->
          <rect x="0" y="0"  width="75" height="10" fill="#1a4fa3" opacity="0.6"/>
          <rect x="0" y="10" width="75" height="10" fill="#1b57b8" opacity="0.6"/>
          <rect x="0" y="20" width="75" height="10" fill="#1a4fa3" opacity="0.6"/>
          <rect x="0" y="30" width="75" height="10" fill="#1b57b8" opacity="0.6"/>
          <rect x="0" y="40" width="75" height="10" fill="#1a4fa3" opacity="0.6"/>
          <rect x="0" y="50" width="75" height="10" fill="#1b57b8" opacity="0.6"/>
          <rect x="0" y="60" width="75" height="10" fill="#1a4fa3" opacity="0.6"/>
          <rect x="0" y="70" width="75" height="10" fill="#1b57b8" opacity="0.6"/>
          <rect x="0" y="80" width="75" height="10" fill="#1a4fa3" opacity="0.6"/>
          <rect x="0" y="90" width="75" height="10" fill="#1b57b8" opacity="0.6"/>

          <!-- Внешняя рамка -->
          <rect x="3" y="3" width="69" height="94" fill="none" stroke="rgba(255,255,255,0.6)" stroke-width="0.5"/>
          <!-- Центральная линия (сверху — граница с чужой половиной) -->
          <line x1="3" y1="3" x2="72" y2="3" stroke="rgba(255,255,255,0.6)" stroke-width="0.7"/>
          <!-- Полукруг от центра поля -->
          <path d="M 20,3 A 16,16 0 0,0 55,3" fill="none" stroke="rgba(255,255,255,0.6)" stroke-width="0.5"/>
          <circle cx="37.5" cy="3" r="0.8" fill="rgba(255,255,255,0.7)"/>
          <!-- Штрафная (снизу — наши ворота) -->
          <rect x="17" y="77" width="40" height="20" fill="none" stroke="rgba(255,255,255,0.6)" stroke-width="0.5"/>
          <rect x="27" y="89" width="20" height="8" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
          <rect x="32" y="96.5" width="11" height="2.5" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
          <circle cx="37.5" cy="83" r="0.7" fill="rgba(255,255,255,0.7)"/>
          <path d="M 30,77 A 6,6 0 0,0 45,77" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
          <path d="M3,94 A3,3 0 0,1 6,97" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
          <path d="M69,97 A3,3 0 0,1 72,94" fill="none" stroke="rgba(255,255,255,0.5)" stroke-width="0.4"/>
        </svg>

        <!-- Игроки и пустые слоты -->
        <div class="players-layer">
          <div
            v-for="(pos, idx) in fieldPositions" :key="idx"
            class="player-wrapper"
            :style="{ left: pos.x + '%', top: pos.y + '%' }"
          >
            <!-- Пустой слот -->
            <template v-if="pos.empty">
              <div class="player-main">
                <div class="player-circle player-empty">
                  <span class="empty-mark">{{ pos.position || '?' }}</span>
                </div>
                <div class="empty-tooltip">
                  <div class="empty-tooltip-title">Слот не занят</div>
                  <div v-if="pos.preferred && pos.preferred.length" class="empty-tooltip-sub">Подходящие позиции:</div>
                  <div v-if="pos.preferred && pos.preferred.length" class="empty-tooltip-list">
                    <span
                      v-for="(code, i) in pos.preferred" :key="code"
                      class="empty-tooltip-chip"
                      :class="i === 0 ? 'empty-tooltip-chip-primary' : ''"
                    >{{ code }} <span class="empty-tooltip-chip-label">({{ POSITION_LABEL[code] ?? code }})</span></span>
                  </div>
                </div>
              </div>
            </template>
            <!-- Обычный игрок -->
            <template v-else>
              <div class="player-main">
                <div
                  class="player-circle"
                  :class="{ 'has-substitute': pos.substitute, 'is-reserve': pos.type === 'Reserve' }"
                  @click="pos.substitute && toggle(idx)"
                >
                  {{ initials(pos.name) }}
                </div>
                <div class="player-name">{{ shortName(pos.name) }}</div>
              </div>
              <Transition name="sub-fade">
                <div v-if="pos.substitute && activeIdx === idx" class="player-substitute">
                  <div class="substitute-arrow">↓</div>
                  <div class="substitute-circle" @click="toggle(idx)">
                    {{ initials(pos.substitute.name) }}
                  </div>
                  <div class="substitute-name">{{ shortName(pos.substitute.name) }}</div>
                </div>
              </Transition>
            </template>
          </div>
        </div>
      </div>

      <!-- Подпись + кнопка выбора тактики -->
      <div class="text-xs text-neutral-500 mt-2 flex items-center justify-between gap-3">
        <div>
          <span class="text-[#10b981] font-semibold">Зелёная рамка</span> — есть замена
        </div>
        <div class="relative" v-click-outside="() => formationOpen = false">
          <button
            type="button"
            @click="formationOpen = !formationOpen"
            class="flex items-center gap-1.5 px-3 py-1 rounded-lg border border-neutral-200 bg-white hover:bg-neutral-50 text-xs font-semibold text-neutral-700 transition-colors"
          >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3.5 h-3.5 text-neutral-500">
              <path stroke-linecap="round" stroke-linejoin="round" d="M3 6h18M3 12h18M3 18h12"/>
            </svg>
            <span class="text-blue-600">{{ FORMATION_LABEL[formation] }}</span>
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3 h-3 text-neutral-400">
              <path stroke-linecap="round" stroke-linejoin="round" d="M19 9l-7 7-7-7"/>
            </svg>
          </button>
          <div
            v-if="formationOpen"
            class="absolute right-0 bottom-full mb-1.5 w-60 bg-white rounded-xl border border-neutral-200 shadow-lg overflow-hidden z-30"
          >
            <button
              v-for="f in FORMATIONS" :key="f.key"
              type="button"
              @click="setFormation(f.key)"
              class="w-full text-left px-3 py-2 text-xs hover:bg-neutral-50 transition-colors flex items-center justify-between"
              :class="formation === f.key ? 'bg-blue-50 text-blue-700 font-semibold' : 'text-neutral-700'"
            >
              <span>{{ f.label }}</span>
              <svg v-if="formation === f.key" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M5 13l4 4L19 7"/>
              </svg>
            </button>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch, type DirectiveBinding } from 'vue'
import { POSITION_LABEL } from '@/constants'

const props = defineProps<{
  lineup: { sportsmanId: number; position: string; type?: string; substituteId?: number }[]
  sportsmenMap: Record<number, string>
  events: any[]
  // Если матч "обе наши команды" и просматриваем гостевую — игроки рисуются
  // на верхней половине поля (зеркало по Y). По умолчанию — внизу (наши ворота снизу).
  mirror?: boolean
  matchId?: number | string
}>()

// ── Тактические пресеты ─────────────────────────────────────────────
type FormationKey = 'default' | '442' | '442diamond' | '4231' | '433' | '352'

const FORMATION_LABEL: Record<FormationKey, string> = {
  default:    'Универсальная (раз-ие всех игроков)',
  '442':      '4-4-2 (классическая)',
  '442diamond': '4-4-2 (ромб)',
  '433':      '4-3-3 (атакующая)',
  '4231':     '4-2-3-1',
  '352':      '3-5-2 (с латералями)',
}

const FORMATIONS: { key: FormationKey; label: string }[] = [
  { key: 'default',     label: 'Универсальная (раз-ие всех игроков)' },
  { key: '442',         label: '4-4-2 (классическая)' },
  { key: '442diamond',  label: '4-4-2 (ромб)' },
  { key: '433',         label: '4-3-3 (атакующая)' },
  { key: '4231',        label: '4-2-3-1' },
  { key: '352',         label: '3-5-2 (с латералями)' },
]

// Координаты для вертикального поля 3:4 (75×100).
// Ворота СНИЗУ (y=90+), атака УВЕРХУ (y=10-20). x=0..75.
// Универсальная: каждая позиция в своём логичном месте.
const DEFAULT_POSITIONS: Record<string, { x: number; y: number }> = {
  GK:  { x: 50, y: 92 },
  // Защита — линия y≈75
  LB:  { x: 12, y: 80 }, CB:  { x: 38, y: 78 }, CB2: { x: 62, y: 78 }, RB:  { x: 88, y: 80 },
  LWB: { x: 8,  y: 70 }, RWB: { x: 92, y: 70 },
  // Опорный
  CDM: { x: 50, y: 60 },
  // Центральные ПЗ
  CM:  { x: 30, y: 55 }, CM2: { x: 70, y: 55 }, CM3: { x: 50, y: 50 },
  // Атакующий ПЗ
  CAM: { x: 50, y: 40 },
  // Вингеры
  LW:  { x: 15, y: 30 }, RW:  { x: 85, y: 30 },
  // Нападение
  CF:  { x: 35, y: 18 }, ST:  { x: 50, y: 10 }, SS:  { x: 65, y: 18 },
}

type Slot = { label: string; x: number; y: number; preferred: string[] }

const FORMATION_SLOTS: Record<Exclude<FormationKey, 'default'>, Slot[]> = {
  // 4-4-2 классическая
  '442': [
    { label: 'GK', x: 50, y: 92, preferred: ['GK'] },
    { label: 'LB', x: 12, y: 78, preferred: ['LB', 'LWB'] },
    { label: 'CB', x: 38, y: 78, preferred: ['CB'] },
    { label: 'CB', x: 62, y: 78, preferred: ['CB'] },
    { label: 'RB', x: 88, y: 78, preferred: ['RB', 'RWB'] },
    { label: 'LM', x: 12, y: 50, preferred: ['LW', 'LWB', 'CM'] },
    { label: 'CM', x: 38, y: 50, preferred: ['CM', 'CDM', 'CAM'] },
    { label: 'CM', x: 62, y: 50, preferred: ['CM', 'CDM', 'CAM'] },
    { label: 'RM', x: 88, y: 50, preferred: ['RW', 'RWB', 'CM'] },
    { label: 'ST', x: 35, y: 18, preferred: ['ST', 'CF', 'SS'] },
    { label: 'ST', x: 65, y: 18, preferred: ['ST', 'CF', 'SS'] },
  ],
  // 4-4-2 ромб
  '442diamond': [
    { label: 'GK',  x: 50, y: 92, preferred: ['GK'] },
    { label: 'LB',  x: 12, y: 78, preferred: ['LB', 'LWB'] },
    { label: 'CB',  x: 38, y: 78, preferred: ['CB'] },
    { label: 'CB',  x: 62, y: 78, preferred: ['CB'] },
    { label: 'RB',  x: 88, y: 78, preferred: ['RB', 'RWB'] },
    { label: 'CDM', x: 50, y: 62, preferred: ['CDM', 'CM'] },
    { label: 'CM',  x: 22, y: 48, preferred: ['CM', 'LW', 'CAM'] },
    { label: 'CM',  x: 78, y: 48, preferred: ['CM', 'RW', 'CAM'] },
    { label: 'CAM', x: 50, y: 35, preferred: ['CAM', 'SS', 'CM'] },
    { label: 'ST',  x: 35, y: 18, preferred: ['ST', 'CF'] },
    { label: 'ST',  x: 65, y: 18, preferred: ['ST', 'CF'] },
  ],
  // 4-3-3 атакующая
  '433': [
    { label: 'GK', x: 50, y: 92, preferred: ['GK'] },
    { label: 'LB', x: 12, y: 78, preferred: ['LB', 'LWB'] },
    { label: 'CB', x: 38, y: 78, preferred: ['CB'] },
    { label: 'CB', x: 62, y: 78, preferred: ['CB'] },
    { label: 'RB', x: 88, y: 78, preferred: ['RB', 'RWB'] },
    { label: 'CDM', x: 50, y: 55, preferred: ['CDM', 'CM'] },
    { label: 'CM',  x: 25, y: 50, preferred: ['CM', 'CAM'] },
    { label: 'CM',  x: 75, y: 50, preferred: ['CM', 'CAM'] },
    { label: 'LW',  x: 12, y: 20, preferred: ['LW', 'CAM', 'CF'] },
    { label: 'ST',  x: 50, y: 12, preferred: ['ST', 'CF', 'SS'] },
    { label: 'RW',  x: 88, y: 20, preferred: ['RW', 'CAM', 'CF'] },
  ],
  // 4-2-3-1
  '4231': [
    { label: 'GK',  x: 50, y: 92, preferred: ['GK'] },
    { label: 'LB',  x: 12, y: 78, preferred: ['LB', 'LWB'] },
    { label: 'CB',  x: 38, y: 78, preferred: ['CB'] },
    { label: 'CB',  x: 62, y: 78, preferred: ['CB'] },
    { label: 'RB',  x: 88, y: 78, preferred: ['RB', 'RWB'] },
    { label: 'CDM', x: 35, y: 58, preferred: ['CDM', 'CM'] },
    { label: 'CDM', x: 65, y: 58, preferred: ['CDM', 'CM'] },
    { label: 'LW',  x: 15, y: 38, preferred: ['LW', 'CAM'] },
    { label: 'CAM', x: 50, y: 38, preferred: ['CAM', 'SS', 'CM'] },
    { label: 'RW',  x: 85, y: 38, preferred: ['RW', 'CAM'] },
    { label: 'ST',  x: 50, y: 14, preferred: ['ST', 'CF'] },
  ],
  // 3-5-2
  '352': [
    { label: 'GK',  x: 50, y: 92, preferred: ['GK'] },
    { label: 'CB',  x: 25, y: 78, preferred: ['CB', 'LB'] },
    { label: 'CB',  x: 50, y: 80, preferred: ['CB'] },
    { label: 'CB',  x: 75, y: 78, preferred: ['CB', 'RB'] },
    { label: 'LWB', x: 8,  y: 55, preferred: ['LWB', 'LB', 'LW'] },
    { label: 'CM',  x: 30, y: 50, preferred: ['CM', 'CAM'] },
    { label: 'CDM', x: 50, y: 58, preferred: ['CDM', 'CM'] },
    { label: 'CM',  x: 70, y: 50, preferred: ['CM', 'CAM'] },
    { label: 'RWB', x: 92, y: 55, preferred: ['RWB', 'RB', 'RW'] },
    { label: 'ST',  x: 35, y: 18, preferred: ['ST', 'CF'] },
    { label: 'ST',  x: 65, y: 18, preferred: ['ST', 'CF'] },
  ],
}

function positionGroup(p: string): 'GK' | 'DEF' | 'MID' | 'FWD' {
  if (p === 'GK') return 'GK'
  if (['CB','LB','RB','LWB','RWB','CB2'].includes(p)) return 'DEF'
  if (['CDM','CM','CAM','CM2','CM3'].includes(p)) return 'MID'
  if (['LW','RW','ST','CF','SS'].includes(p)) return 'FWD'
  return 'MID'
}
function slotGroup(label: string): 'GK' | 'DEF' | 'MID' | 'FWD' {
  if (label === 'GK') return 'GK'
  if (['LB','CB','RB','LWB','RWB'].includes(label)) return 'DEF'
  if (['CM','CDM','CAM','LM','RM'].includes(label)) return 'MID'
  if (['ST','CF','LW','RW','SS'].includes(label)) return 'FWD'
  return 'MID'
}

// ── Выбор тактики + localStorage ────────────────────────────────────
const formationOpen = ref(false)
const formation = ref<FormationKey>('default')
const storageKey = computed(() => `match-formation-mobile:${props.matchId ?? 'unknown'}`)

onMounted(() => {
  try {
    const saved = localStorage.getItem(storageKey.value) as FormationKey | null
    if (saved && (saved === 'default' || saved in FORMATION_SLOTS)) formation.value = saved
  } catch { /* ignore */ }
})
watch(formation, (v) => { try { localStorage.setItem(storageKey.value, v) } catch {} })
function setFormation(k: FormationKey) { formation.value = k; formationOpen.value = false }

const vClickOutside = {
  mounted(el: HTMLElement, binding: DirectiveBinding<() => void>) {
    (el as any).__clickOutside = (ev: MouseEvent) => {
      if (!el.contains(ev.target as Node)) binding.value?.()
    }
    document.addEventListener('click', (el as any).__clickOutside)
  },
  unmounted(el: HTMLElement) {
    document.removeEventListener('click', (el as any).__clickOutside)
  },
}

// ── Зеркалирование по Y (если обе наши команды и смотрим гостевую) ──
// На мобиле зеркало = поворот по вертикали: ворота вверху, атака внизу.
function my(y: number): number { return props.mirror ? 100 - y : y }

const DEFAULT_POSITIONS_MIRRORED = computed(() => {
  if (!props.mirror) return DEFAULT_POSITIONS
  return Object.fromEntries(
    Object.entries(DEFAULT_POSITIONS).map(([k, v]) => [k, { x: v.x, y: 100 - v.y }])
  )
})

// ── Раскладка игроков по слотам ────────────────────────────────────
const activeIdx = ref<number | null>(null)
function toggle(idx: number) { activeIdx.value = activeIdx.value === idx ? null : idx }
function initials(name: string) {
  const parts = (name ?? '').trim().split(' ')
  return ((parts[0]?.[0] ?? '') + (parts[1]?.[0] ?? '')).toUpperCase()
}
function shortName(name: string) {
  const parts = (name ?? '').trim().split(' ')
  if (parts.length >= 2) return `${parts[0]} ${parts[1][0]}.`
  return name
}

const fieldPositions = computed(() => {
  if (!props.lineup.length) return []

  type FieldPlayer = {
    sportsmanId: number
    position: string
    type: string
    onField: boolean
    cameOnFromBench?: boolean
    subInMinute?: number
    replacedSportsmanId?: number
    subOutMinute?: number
    replacedBySportsmanId?: number
  }

  const players: FieldPlayer[] = props.lineup.map(e => ({
    sportsmanId: e.sportsmanId,
    position: e.position || 'ST',
    type: e.type || 'Main',
    onField: e.type !== 'Reserve',
  }))

  const substitutions = [...props.events]
    .filter(e => e.type === 'Substitution' && e.sportsmanId && e.substituteSportsmanId)
    .sort((a, b) => (a.minute ?? 0) - (b.minute ?? 0))

  substitutions.forEach(sub => {
    const out = players.find(p => p.sportsmanId === sub.sportsmanId && p.onField)
    const inP = players.find(p => p.sportsmanId === sub.substituteSportsmanId && !p.onField)
    if (!out || !inP) return
    out.onField = false
    out.subOutMinute = sub.minute
    out.replacedBySportsmanId = sub.substituteSportsmanId
    inP.onField = true
    inP.position = out.position
    inP.cameOnFromBench = true
    inP.subInMinute = sub.minute
    inP.replacedSportsmanId = sub.sportsmanId
  })

  const posMap = new Map<string, FieldPlayer>()
  players.forEach(p => {
    if (!p.onField) return
    if (!posMap.has(p.position)) posMap.set(p.position, p)
  })

  const remainingReserves = players.filter(p => !p.onField && p.type === 'Reserve' && !p.replacedBySportsmanId)
  const lineupBySportsmanId = new Map<number, any>(props.lineup.map(e => [e.sportsmanId, e]))
  const offFieldByReplacement = new Map<number, FieldPlayer>()
  players.forEach(p => {
    if (!p.onField && p.replacedBySportsmanId) {
      offFieldByReplacement.set(p.replacedBySportsmanId, p)
    }
  })

  function buildPairFor(p: FieldPlayer, pos: string): { sportsmanId: number; position: string; type: string } | null {
    let pair: { sportsmanId: number; position: string; type: string } | null = null
    if (p.cameOnFromBench) {
      const replaced = offFieldByReplacement.get(p.sportsmanId)
      if (replaced) pair = { sportsmanId: replaced.sportsmanId, position: replaced.position, type: 'OffField' }
    }
    if (!pair) {
      const explicitId = lineupBySportsmanId.get(p.sportsmanId)?.substituteId
      let reserve: FieldPlayer | null = null
      if (explicitId) {
        const explIdx = remainingReserves.findIndex(r => r.sportsmanId === Number(explicitId))
        if (explIdx !== -1) reserve = remainingReserves.splice(explIdx, 1)[0]
      }
      if (!reserve) {
        const reserveIdx = remainingReserves.findIndex(r => r.position === pos)
        if (reserveIdx !== -1) reserve = remainingReserves.splice(reserveIdx, 1)[0]
      }
      if (reserve) pair = { sportsmanId: reserve.sportsmanId, position: reserve.position, type: reserve.type }
    }
    return pair
  }

  function toFieldEntry(p: FieldPlayer, x: number, y: number, pos: string, index: number) {
    const pair = buildPairFor(p, pos)
    return {
      x, y,
      number: index + 1,
      name: props.sportsmenMap[p.sportsmanId] ?? `#${p.sportsmanId}`,
      position: p.position || 'ST',
      type: p.type || 'Main',
      cameOnFromBench: p.cameOnFromBench,
      subInMinute: p.subInMinute,
      late: false,
      empty: false,
      substitute: pair ? {
        number: 0,
        name: props.sportsmenMap[pair.sportsmanId] ?? `#${pair.sportsmanId}`,
        position: pair.position || 'ST',
        type: pair.type,
      } : null,
    }
  }

  // Ветка 1: Универсальная
  if (formation.value === 'default') {
    const result: any[] = []
    const positions = DEFAULT_POSITIONS_MIRRORED.value
    posMap.forEach((p, pos) => {
      const fp = positions[pos] || positions['ST']
      result.push(toFieldEntry(p, fp.x, fp.y, pos, result.length))
    })
    return result
  }

  // Ветка 2: слотовая раскладка
  const slots = FORMATION_SLOTS[formation.value as Exclude<FormationKey, 'default'>]
  const playersOnField: FieldPlayer[] = []
  posMap.forEach(p => playersOnField.push(p))

  const result: any[] = []
  for (const slot of slots) {
    let chosenIdx = -1
    for (const pref of slot.preferred) {
      chosenIdx = playersOnField.findIndex(p => p.position === pref)
      if (chosenIdx !== -1) break
    }
    if (chosenIdx === -1) {
      const sg = slotGroup(slot.label)
      chosenIdx = playersOnField.findIndex(p => positionGroup(p.position) === sg)
    }

    if (chosenIdx !== -1) {
      const p = playersOnField.splice(chosenIdx, 1)[0]
      result.push(toFieldEntry(p, slot.x, my(slot.y), slot.label, result.length))
    } else {
      const sg = slotGroup(slot.label)
      const groupExtras = (['GK','LB','CB','RB','LWB','RWB','CDM','CM','CAM','LW','RW','CF','ST','SS'] as const)
        .filter(p => positionGroup(p) === sg && !slot.preferred.includes(p))
      const preferred = [...slot.preferred, ...groupExtras]
      result.push({
        x: slot.x, y: my(slot.y),
        number: result.length + 1,
        name: '',
        position: slot.label,
        type: 'Empty',
        empty: true,
        late: false,
        preferred,
        substitute: null,
      })
    }
  }

  if (playersOnField.length > 0 && import.meta.env?.DEV) {
    console.debug(
      `[FootballFieldMobile] Схема ${formation.value}: не вошли ${playersOnField.length} игроков:`,
      playersOnField.map(p => `${p.position} ${props.sportsmenMap[p.sportsmanId] ?? p.sportsmanId}`).join(', ')
    )
  }

  return result
})
</script>

<style scoped>
.field-container {
  position: relative;
  aspect-ratio: 3 / 4;
  border-radius: 16px;
  overflow: visible;
  background: #1d4ed8;
  box-shadow: 0 4px 12px rgba(29, 78, 216, 0.25);
}
.field-lines {
  position: absolute; inset: 0;
  width: 100%; height: 100%;
  pointer-events: none;
  border-radius: 16px;
  overflow: hidden;
}
.players-layer { position: absolute; inset: 0; }
.player-wrapper { position: absolute; transform: translate(-50%, -50%); }
.player-main {
  position: relative;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
}
.player-circle {
  width: 32px; height: 32px;
  border-radius: 50%;
  background: #fff;
  border: 2px solid #3b82f6;
  display: flex; align-items: center; justify-content: center;
  font-size: 10px; font-weight: 700; color: #1d4ed8;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
}
.player-circle.is-reserve,
.player-circle.has-substitute {
  border-color: #10b981;
  color: #047857;
  background: #d1fae5;
}
.player-circle.has-substitute { cursor: pointer; }

/* Пустой слот — пунктирный кружок с белой жирной подписью */
.player-circle.player-empty,
:global(.dark) .player-circle.player-empty {
  background: transparent !important;
  border: 2px dashed rgba(255, 255, 255, 0.55) !important;
  color: #ffffff !important;
  font-size: 10px;
  font-weight: 900;
  letter-spacing: 0.3px;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.6);
  box-shadow: none;
}
.player-circle.player-empty .empty-mark { pointer-events: none; color: #ffffff; }

/* Имя под кружком (на мобиле всегда видно) */
.player-name {
  font-size: 9px; font-weight: 700;
  color: #fff;
  text-shadow: 0 1px 2px rgba(0,0,0,0.7);
  white-space: nowrap;
  pointer-events: none;
}

/* Плашка-подсказка под пустым слотом — светлая в любой теме */
.empty-tooltip {
  position: absolute;
  top: calc(100% + 6px);
  left: 50%;
  transform: translateX(-50%);
  min-width: 200px; max-width: 260px;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 8px 10px;
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.18);
  opacity: 0; pointer-events: none;
  transition: opacity 0.15s;
  z-index: 25;
}
.player-wrapper:hover .empty-tooltip { opacity: 1; }
.empty-tooltip-title {
  font-size: 11px; font-weight: 700; color: #0f172a;
  margin-bottom: 4px;
}
.empty-tooltip-sub {
  font-size: 9px; font-weight: 700; text-transform: uppercase;
  letter-spacing: 0.4px; color: #94a3b8; margin-bottom: 4px;
}
.empty-tooltip-list { display: flex; flex-wrap: wrap; gap: 3px; }
.empty-tooltip-chip {
  display: inline-flex; align-items: center; gap: 3px;
  padding: 2px 6px;
  border-radius: 6px;
  background: #f1f5f9; color: #475569;
  font-size: 10px; font-weight: 500;
  border: 1px solid #e2e8f0;
}
.empty-tooltip-chip-primary {
  background: #dbeafe; color: #1e40af; border-color: #93c5fd;
}
.empty-tooltip-chip-label {
  font-weight: 500; font-size: 9px;
  color: inherit; opacity: 0.85;
}

/* Замена */
.player-substitute {
  position: absolute; top: calc(100% + 4px); left: 50%;
  transform: translateX(-50%);
  display: flex; flex-direction: column; align-items: center; gap: 2px;
  z-index: 20;
}
.substitute-arrow { font-size: 10px; color: #10b981; font-weight: 700; line-height: 1; }
.substitute-circle {
  width: 28px; height: 28px;
  border-radius: 50%;
  background: #d1fae5;
  border: 2px solid #10b981;
  display: flex; align-items: center; justify-content: center;
  font-size: 10px; font-weight: 700; color: #047857;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.2);
  cursor: pointer;
}
.substitute-name {
  padding: 2px 5px;
  background: rgba(15, 23, 42, 0.9);
  color: #6ee7b7;
  font-size: 9px; font-weight: 600;
  border-radius: 4px;
  white-space: nowrap;
}
.sub-fade-enter-active, .sub-fade-leave-active { transition: opacity 0.2s; }
.sub-fade-enter-from, .sub-fade-leave-to { opacity: 0; }
</style>
