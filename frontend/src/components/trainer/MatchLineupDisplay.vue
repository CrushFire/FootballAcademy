<template>
  <div>
    <FootballField v-if="fieldPositions.length" :positions="fieldPositions">
      <!-- Выбор тактики справа под полем (на одной линии с подписью про замену).
           Сохраняется в localStorage по ключу 'match-formation:{matchId}'. -->
      <template #footer-right>
        <div class="relative" v-click-outside="() => formationOpen = false">
          <button
            type="button"
            @click="formationOpen = !formationOpen"
            class="flex items-center gap-1.5 px-3 py-1 rounded-lg border border-neutral-200 bg-white hover:bg-neutral-50 text-xs font-semibold text-neutral-700 transition-colors"
          >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3.5 h-3.5 text-neutral-500">
              <path stroke-linecap="round" stroke-linejoin="round" d="M3 6h18M3 12h18M3 18h12"/>
            </svg>
            Тактика: <span class="text-blue-600">{{ FORMATION_LABEL[formation] }}</span>
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3 h-3 text-neutral-400">
              <path stroke-linecap="round" stroke-linejoin="round" d="M19 9l-7 7-7-7"/>
            </svg>
          </button>

          <!-- Выпадашка -->
          <div
            v-if="formationOpen"
            class="absolute right-0 bottom-full mb-1.5 w-56 max-h-[60vh] overflow-y-auto bg-white rounded-xl border border-neutral-200 shadow-lg z-30"
          >
            <template v-for="(f, i) in FORMATIONS" :key="i">
              <!-- Группирующий разделитель (не кликабельный) -->
              <div
                v-if="f.kind === 'divider'"
                class="px-3 pt-2 pb-1 text-[10px] font-bold uppercase tracking-wide text-neutral-400 bg-neutral-50 border-t border-neutral-100 first:border-t-0"
              >{{ f.label }}</div>
              <!-- Кликабельная схема -->
              <button
                v-else
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
            </template>
          </div>
        </div>
      </template>
    </FootballField>
    <div v-else class="text-center text-xs text-neutral-400 py-6 border border-dashed border-neutral-200 rounded-xl">
      Состав не задан — добавьте игроков через «Редактировать состав»
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, watch, type DirectiveBinding } from 'vue'
import FootballField from '@/components/trainer/FootballField.vue'

const props = defineProps<{
  lineup: { sportsmanId: number; position: string; type?: string; substituteId?: number }[]
  sportsmenMap: Record<number, string>
  events: any[]
  // Если матч "обе наши команды" и просматриваем гостевую — игроки рисуются
  // на ПРАВОЙ половине поля (зеркало). По умолчанию — на левой.
  mirror?: boolean
  // ID матча — нужен для уникального ключа localStorage сохранения тактики.
  matchId?: number | string
}>()

// ── Тактические пресеты ─────────────────────────────────────────────
type FormationKey =
  | 'default'
  // 4 защитника
  | '442' | '442diamond' | '433' | '4231' | '451' | '4411' | '4222'
  // 3 защитника
  | '343' | '352' | '3421'
  // 5 защитников
  | '532' | '523'

const FORMATION_LABEL: Record<FormationKey, string> = {
  default:      'Универсальная (раз-ие всех игроков)',
  // 4 защитника
  '442':        '4-4-2 (классическая)',
  '442diamond': '4-4-2 (ромб)',
  '433':        '4-3-3 (атакующая)',
  '4231':       '4-2-3-1',
  '451':        '4-5-1 (4-1-4-1)',
  '4411':       '4-4-1-1 (второй форвард)',
  '4222':       '4-2-2-2 (двойной квадрат)',
  // 3 защитника
  '343':        '3-4-3',
  '352':        '3-5-2 (с латералями)',
  '3421':       '3-4-2-1',
  // 5 защитников
  '532':        '5-3-2',
  '523':        '5-2-3',
}

// Элемент выпадашки: либо схема (key), либо разделитель (divider) с подписью.
type FormationItem =
  | { kind: 'option'; key: FormationKey; label: string }
  | { kind: 'divider'; label: string }

const FORMATIONS: FormationItem[] = [
  { kind: 'option', key: 'default',     label: 'Универсальная (раз-ие всех игроков)' },
  { kind: 'divider', label: '4 защитника' },
  { kind: 'option', key: '442',         label: '4-4-2 (классическая)' },
  { kind: 'option', key: '442diamond',  label: '4-4-2 (ромб)' },
  { kind: 'option', key: '433',         label: '4-3-3 (атакующая)' },
  { kind: 'option', key: '4231',        label: '4-2-3-1' },
  { kind: 'option', key: '451',         label: '4-5-1 (4-1-4-1)' },
  { kind: 'option', key: '4411',        label: '4-4-1-1 (второй форвард)' },
  { kind: 'option', key: '4222',        label: '4-2-2-2 (двойной квадрат)' },
  { kind: 'divider', label: '3 защитника' },
  { kind: 'option', key: '343',         label: '3-4-3' },
  { kind: 'option', key: '352',         label: '3-5-2 (с латералями)' },
  { kind: 'option', key: '3421',        label: '3-4-2-1' },
  { kind: 'divider', label: '5 защитников' },
  { kind: 'option', key: '532',         label: '5-3-2' },
  { kind: 'option', key: '523',         label: '5-2-3' },
]

// Универсальная схема — каждая позиция в логичном месте (default, не привязана к схеме).
// Используется когда тренер выбрал "Универсальная" — игроки встают по своим позициям.
const DEFAULT_POSITIONS: Record<string, { x: number; y: number }> = {
  GK:  { x: 6,  y: 50 },
  LB:  { x: 16, y: 12 }, CB:  { x: 16, y: 38 }, CB2: { x: 16, y: 62 }, RB:  { x: 16, y: 88 },
  LWB: { x: 22, y: 8  }, RWB: { x: 22, y: 92 },
  CDM: { x: 26, y: 50 },
  CM:  { x: 30, y: 30 }, CM2: { x: 30, y: 70 }, CM3: { x: 32, y: 50 },
  CAM: { x: 36, y: 50 },
  LW:  { x: 40, y: 15 }, RW:  { x: 40, y: 85 },
  CF:  { x: 44, y: 35 }, ST:  { x: 48, y: 50 }, SS:  { x: 44, y: 65 },
}

// СЛОТОВАЯ МОДЕЛЬ для конкретных тактических схем.
// Каждый слот — фиксированная точка на поле с подписью (label) и списком приоритетных
// позиций из БД (preferred). Алгоритм раскладки:
//   1. Идём по слотам по порядку
//   2. Для каждого слота ищем первого свободного игрока с позицией из preferred
//   3. Если не нашли — берём любого подходящего по группе (GK/DEF/MID/FWD)
//   4. Если не нашли вообще — слот остаётся ПУСТЫМ (empty: true)
//   5. Лишние игроки (которые не попали ни в один слот) — отображаются как
//      "не вошли в состав" предупреждением (выводим в консоль для отладки).
type Slot = {
  label: string                    // подпись внутри пустого слота (GK/LB/CM/ST...)
  x: number; y: number             // координаты на левой половине
  preferred: string[]              // приоритетные позиции из БД (первая — самая подходящая)
}

const FORMATION_SLOTS: Record<Exclude<FormationKey, 'default'>, Slot[]> = {
  // 4-4-2 классическая: GK + 4 защ (плоско) + 4 пз (плоско, LW/RW по краям) + 2 нап
  '442': [
    { label: 'GK', x: 6,  y: 50, preferred: ['GK'] },
    { label: 'LB', x: 18, y: 14, preferred: ['LB', 'LWB'] },
    { label: 'CB', x: 18, y: 38, preferred: ['CB'] },
    { label: 'CB', x: 18, y: 62, preferred: ['CB'] },
    { label: 'RB', x: 18, y: 86, preferred: ['RB', 'RWB'] },
    { label: 'LM', x: 32, y: 14, preferred: ['LW', 'LWB', 'CM'] },
    { label: 'CM', x: 32, y: 38, preferred: ['CM', 'CDM', 'CAM'] },
    { label: 'CM', x: 32, y: 62, preferred: ['CM', 'CDM', 'CAM'] },
    { label: 'RM', x: 32, y: 86, preferred: ['RW', 'RWB', 'CM'] },
    { label: 'ST', x: 44, y: 38, preferred: ['ST', 'CF', 'SS'] },
    { label: 'ST', x: 44, y: 62, preferred: ['ST', 'CF', 'SS'] },
  ],
  // 4-4-2 ромб: GK + 4 защ + CDM/CM/CM/CAM (ромб) + 2 нап
  '442diamond': [
    { label: 'GK',  x: 6,  y: 50, preferred: ['GK'] },
    { label: 'LB',  x: 18, y: 14, preferred: ['LB', 'LWB'] },
    { label: 'CB',  x: 18, y: 38, preferred: ['CB'] },
    { label: 'CB',  x: 18, y: 62, preferred: ['CB'] },
    { label: 'RB',  x: 18, y: 86, preferred: ['RB', 'RWB'] },
    { label: 'CDM', x: 26, y: 50, preferred: ['CDM', 'CM'] },
    { label: 'CM',  x: 32, y: 25, preferred: ['CM', 'LW', 'CAM'] },
    { label: 'CM',  x: 32, y: 75, preferred: ['CM', 'RW', 'CAM'] },
    { label: 'CAM', x: 38, y: 50, preferred: ['CAM', 'SS', 'CM'] },
    { label: 'ST',  x: 44, y: 38, preferred: ['ST', 'CF'] },
    { label: 'ST',  x: 44, y: 62, preferred: ['ST', 'CF'] },
  ],
  // 4-3-3 атакующая: GK + 4 защ + CDM/CM/CM + LW/ST/RW
  '433': [
    { label: 'GK', x: 6,  y: 50, preferred: ['GK'] },
    { label: 'LB', x: 18, y: 14, preferred: ['LB', 'LWB'] },
    { label: 'CB', x: 18, y: 38, preferred: ['CB'] },
    { label: 'CB', x: 18, y: 62, preferred: ['CB'] },
    { label: 'RB', x: 18, y: 86, preferred: ['RB', 'RWB'] },
    { label: 'CDM', x: 28, y: 50, preferred: ['CDM', 'CM'] },
    { label: 'CM',  x: 32, y: 30, preferred: ['CM', 'CAM'] },
    { label: 'CM',  x: 32, y: 70, preferred: ['CM', 'CAM'] },
    { label: 'LW',  x: 44, y: 14, preferred: ['LW', 'CAM', 'CF'] },
    { label: 'ST',  x: 48, y: 50, preferred: ['ST', 'CF', 'SS'] },
    { label: 'RW',  x: 44, y: 86, preferred: ['RW', 'CAM', 'CF'] },
  ],
  // 4-2-3-1: GK + 4 защ + 2 опор. + LW/CAM/RW + 1 нап
  '4231': [
    { label: 'GK',  x: 6,  y: 50, preferred: ['GK'] },
    { label: 'LB',  x: 18, y: 14, preferred: ['LB', 'LWB'] },
    { label: 'CB',  x: 18, y: 38, preferred: ['CB'] },
    { label: 'CB',  x: 18, y: 62, preferred: ['CB'] },
    { label: 'RB',  x: 18, y: 86, preferred: ['RB', 'RWB'] },
    { label: 'CDM', x: 28, y: 38, preferred: ['CDM', 'CM'] },
    { label: 'CDM', x: 28, y: 62, preferred: ['CDM', 'CM'] },
    { label: 'LW',  x: 38, y: 14, preferred: ['LW', 'CAM'] },
    { label: 'CAM', x: 38, y: 50, preferred: ['CAM', 'SS', 'CM'] },
    { label: 'RW',  x: 38, y: 86, preferred: ['RW', 'CAM'] },
    { label: 'ST',  x: 46, y: 50, preferred: ['ST', 'CF'] },
  ],
  // 3-5-2 с латералями: GK + 3 ЦБ + LWB/CM/CM/CM/RWB + 2 нап
  '352': [
    { label: 'GK',  x: 6,  y: 50, preferred: ['GK'] },
    { label: 'CB',  x: 16, y: 25, preferred: ['CB', 'LB'] },
    { label: 'CB',  x: 14, y: 50, preferred: ['CB'] },
    { label: 'CB',  x: 16, y: 75, preferred: ['CB', 'RB'] },
    { label: 'LWB', x: 26, y: 10, preferred: ['LWB', 'LB', 'LW'] },
    { label: 'CM',  x: 32, y: 32, preferred: ['CM', 'CAM'] },
    { label: 'CDM', x: 28, y: 50, preferred: ['CDM', 'CM'] },
    { label: 'CM',  x: 32, y: 68, preferred: ['CM', 'CAM'] },
    { label: 'RWB', x: 26, y: 90, preferred: ['RWB', 'RB', 'RW'] },
    { label: 'ST',  x: 44, y: 38, preferred: ['ST', 'CF'] },
    { label: 'ST',  x: 44, y: 62, preferred: ['ST', 'CF'] },
  ],
  // 3-4-3: GK + 3 ЦБ + LWB/CM/CM/RWB (4 пз) + LW/ST/RW (3 нап)
  '343': [
    { label: 'GK',  x: 6,  y: 50, preferred: ['GK'] },
    { label: 'CB',  x: 16, y: 25, preferred: ['CB', 'LB'] },
    { label: 'CB',  x: 14, y: 50, preferred: ['CB'] },
    { label: 'CB',  x: 16, y: 75, preferred: ['CB', 'RB'] },
    { label: 'LWB', x: 26, y: 12, preferred: ['LWB', 'LB', 'LW'] },
    { label: 'CM',  x: 30, y: 38, preferred: ['CM', 'CDM', 'CAM'] },
    { label: 'CM',  x: 30, y: 62, preferred: ['CM', 'CDM', 'CAM'] },
    { label: 'RWB', x: 26, y: 88, preferred: ['RWB', 'RB', 'RW'] },
    { label: 'LW',  x: 42, y: 14, preferred: ['LW', 'CF', 'CAM'] },
    { label: 'ST',  x: 46, y: 50, preferred: ['ST', 'CF', 'SS'] },
    { label: 'RW',  x: 42, y: 86, preferred: ['RW', 'CF', 'CAM'] },
  ],
  // 4-5-1 (4-1-4-1): GK + 4 защ + CDM + LM/CM/CM/RM + 1 нап
  '451': [
    { label: 'GK',  x: 6,  y: 50, preferred: ['GK'] },
    { label: 'LB',  x: 18, y: 14, preferred: ['LB', 'LWB'] },
    { label: 'CB',  x: 18, y: 38, preferred: ['CB'] },
    { label: 'CB',  x: 18, y: 62, preferred: ['CB'] },
    { label: 'RB',  x: 18, y: 86, preferred: ['RB', 'RWB'] },
    { label: 'CDM', x: 26, y: 50, preferred: ['CDM', 'CM'] },
    { label: 'LM',  x: 34, y: 14, preferred: ['LW', 'LWB', 'CM'] },
    { label: 'CM',  x: 34, y: 38, preferred: ['CM', 'CAM'] },
    { label: 'CM',  x: 34, y: 62, preferred: ['CM', 'CAM'] },
    { label: 'RM',  x: 34, y: 86, preferred: ['RW', 'RWB', 'CM'] },
    { label: 'ST',  x: 46, y: 50, preferred: ['ST', 'CF', 'SS'] },
  ],
  // 4-4-1-1 (второй форвард): GK + 4 защ + LM/CM/CM/RM + CF + ST
  '4411': [
    { label: 'GK',  x: 6,  y: 50, preferred: ['GK'] },
    { label: 'LB',  x: 18, y: 14, preferred: ['LB', 'LWB'] },
    { label: 'CB',  x: 18, y: 38, preferred: ['CB'] },
    { label: 'CB',  x: 18, y: 62, preferred: ['CB'] },
    { label: 'RB',  x: 18, y: 86, preferred: ['RB', 'RWB'] },
    { label: 'LM',  x: 30, y: 14, preferred: ['LW', 'LWB', 'CM'] },
    { label: 'CM',  x: 30, y: 38, preferred: ['CM', 'CDM'] },
    { label: 'CM',  x: 30, y: 62, preferred: ['CM', 'CAM'] },
    { label: 'RM',  x: 30, y: 86, preferred: ['RW', 'RWB', 'CM'] },
    { label: 'CF',  x: 38, y: 50, preferred: ['CF', 'SS', 'CAM'] },
    { label: 'ST',  x: 46, y: 50, preferred: ['ST', 'CF'] },
  ],
  // 4-2-2-2 (двойной квадрат): GK + 4 защ + 2 опор. + 2 атак.пз (LAM/RAM) + 2 нап
  '4222': [
    { label: 'GK',  x: 6,  y: 50, preferred: ['GK'] },
    { label: 'LB',  x: 18, y: 14, preferred: ['LB', 'LWB'] },
    { label: 'CB',  x: 18, y: 38, preferred: ['CB'] },
    { label: 'CB',  x: 18, y: 62, preferred: ['CB'] },
    { label: 'RB',  x: 18, y: 86, preferred: ['RB', 'RWB'] },
    { label: 'CDM', x: 28, y: 38, preferred: ['CDM', 'CM'] },
    { label: 'CDM', x: 28, y: 62, preferred: ['CDM', 'CM'] },
    { label: 'LAM', x: 38, y: 22, preferred: ['LW', 'CAM', 'CM'] },
    { label: 'RAM', x: 38, y: 78, preferred: ['RW', 'CAM', 'CM'] },
    { label: 'ST',  x: 46, y: 38, preferred: ['ST', 'CF'] },
    { label: 'ST',  x: 46, y: 62, preferred: ['ST', 'CF'] },
  ],
  // 3-4-2-1: GK + 3 ЦБ + LWB/CM/CM/RWB + 2 атак.пз (LAM/RAM) + 1 нап
  '3421': [
    { label: 'GK',  x: 6,  y: 50, preferred: ['GK'] },
    { label: 'CB',  x: 16, y: 25, preferred: ['CB', 'LB'] },
    { label: 'CB',  x: 14, y: 50, preferred: ['CB'] },
    { label: 'CB',  x: 16, y: 75, preferred: ['CB', 'RB'] },
    { label: 'LWB', x: 26, y: 12, preferred: ['LWB', 'LB', 'LW'] },
    { label: 'CM',  x: 30, y: 38, preferred: ['CM', 'CDM', 'CAM'] },
    { label: 'CM',  x: 30, y: 62, preferred: ['CM', 'CDM', 'CAM'] },
    { label: 'RWB', x: 26, y: 88, preferred: ['RWB', 'RB', 'RW'] },
    { label: 'LAM', x: 40, y: 28, preferred: ['CAM', 'LW', 'SS'] },
    { label: 'RAM', x: 40, y: 72, preferred: ['CAM', 'RW', 'SS'] },
    { label: 'ST',  x: 48, y: 50, preferred: ['ST', 'CF'] },
  ],
  // 5-3-2: GK + 5 защ (LWB/LCB/CB/RCB/RWB) + 3 пз (CM/CDM/CM) + 2 нап
  '532': [
    { label: 'GK',  x: 6,  y: 50, preferred: ['GK'] },
    { label: 'LWB', x: 18, y: 10, preferred: ['LWB', 'LB'] },
    { label: 'CB',  x: 16, y: 30, preferred: ['CB', 'LB'] },
    { label: 'CB',  x: 14, y: 50, preferred: ['CB'] },
    { label: 'CB',  x: 16, y: 70, preferred: ['CB', 'RB'] },
    { label: 'RWB', x: 18, y: 90, preferred: ['RWB', 'RB'] },
    { label: 'CM',  x: 32, y: 30, preferred: ['CM', 'CAM'] },
    { label: 'CDM', x: 30, y: 50, preferred: ['CDM', 'CM'] },
    { label: 'CM',  x: 32, y: 70, preferred: ['CM', 'CAM'] },
    { label: 'ST',  x: 46, y: 38, preferred: ['ST', 'CF'] },
    { label: 'ST',  x: 46, y: 62, preferred: ['ST', 'CF'] },
  ],
  // 5-2-3: GK + 5 защ (LWB/LCB/CB/RCB/RWB) + 2 пз (CM/CM) + LW/ST/RW
  '523': [
    { label: 'GK',  x: 6,  y: 50, preferred: ['GK'] },
    { label: 'LWB', x: 18, y: 10, preferred: ['LWB', 'LB'] },
    { label: 'CB',  x: 16, y: 30, preferred: ['CB', 'LB'] },
    { label: 'CB',  x: 14, y: 50, preferred: ['CB'] },
    { label: 'CB',  x: 16, y: 70, preferred: ['CB', 'RB'] },
    { label: 'RWB', x: 18, y: 90, preferred: ['RWB', 'RB'] },
    { label: 'CM',  x: 30, y: 38, preferred: ['CM', 'CDM', 'CAM'] },
    { label: 'CM',  x: 30, y: 62, preferred: ['CM', 'CDM', 'CAM'] },
    { label: 'LW',  x: 44, y: 14, preferred: ['LW', 'CF', 'CAM'] },
    { label: 'ST',  x: 48, y: 50, preferred: ['ST', 'CF', 'SS'] },
    { label: 'RW',  x: 44, y: 86, preferred: ['RW', 'CF', 'CAM'] },
  ],
}

// Группы позиций для fallback'а если в preferred нет подходящего игрока.
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

// ── Выбор тактики + сохранение в localStorage ──────────────────────
const formationOpen = ref(false)
const formation = ref<FormationKey>('default')

const storageKey = computed(() => `match-formation:${props.matchId ?? 'unknown'}`)

onMounted(() => {
  try {
    const saved = localStorage.getItem(storageKey.value) as FormationKey | null
    if (saved && (saved === 'default' || saved in FORMATION_SLOTS)) formation.value = saved
  } catch { /* localStorage недоступен — игнорируем */ }
})

watch(formation, (v) => {
  try { localStorage.setItem(storageKey.value, v) } catch { /* ignore */ }
})

function setFormation(k: FormationKey) {
  formation.value = k
  formationOpen.value = false
}

// Закрытие выпадашки по клику снаружи
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

// Хелпер: зеркалит координату x если включён mirror.
function mx(x: number): number {
  return props.mirror ? 100 - x : x
}

// Координаты для default-схемы (универсальной).
const DEFAULT_POSITIONS_MIRRORED = computed(() => {
  if (!props.mirror) return DEFAULT_POSITIONS
  return Object.fromEntries(
    Object.entries(DEFAULT_POSITIONS).map(([k, v]) => [k, { x: 100 - v.x, y: v.y }])
  )
})

// Хронологически применяем события замены к стартовому lineup.
// Алгоритм:
//   1. effectiveLineup = копия стартовых Main + Reserve (по позициям)
//   2. Для каждой Substitution в порядке минут:
//      - находим стартового Main по sportsmanId → помечаем как "вышедший с поля" (off-field)
//      - находим Reserve по substituteSportsmanId → переводим его на позицию ушедшего,
//        помечаем флагом cameOnFromBench и сохраняем минуту + кого заменил
//   3. На поле остаются все Main + те Reserve которые вошли заменой.
//      Reserve без выхода — не показываем на поле (они на скамейке).
const fieldPositions = computed(() => {
  if (!props.lineup.length) return []

  type FieldPlayer = {
    sportsmanId: number
    position: string
    type: string
    onField: boolean         // присутствует ли сейчас на поле
    cameOnFromBench?: boolean
    subInMinute?: number     // минута выхода (для тех кто вышел заменой)
    replacedSportsmanId?: number  // кого заменил (для тултипа)
    subOutMinute?: number    // минута ухода (для ушедших)
    replacedBySportsmanId?: number  // кем заменён
  }

  // Стартовые: Main → на поле, Reserve → на скамейке
  const players: FieldPlayer[] = props.lineup.map(e => ({
    sportsmanId: e.sportsmanId,
    position: e.position || 'ST',
    type: e.type || 'Main',
    onField: e.type !== 'Reserve',
  }))

  // События замены — в хронологическом порядке
  const substitutions = [...props.events]
    .filter(e => e.type === 'Substitution' && e.sportsmanId && e.substituteSportsmanId)
    .sort((a, b) => (a.minute ?? 0) - (b.minute ?? 0))

  substitutions.forEach(sub => {
    const out = players.find(p => p.sportsmanId === sub.sportsmanId && p.onField)
    const inP = players.find(p => p.sportsmanId === sub.substituteSportsmanId && !p.onField)
    if (!out || !inP) return
    // Ушедший — снимаем с поля
    out.onField = false
    out.subOutMinute = sub.minute
    out.replacedBySportsmanId = sub.substituteSportsmanId
    // Пришедший — на позицию ушедшего
    inP.onField = true
    inP.position = out.position
    inP.cameOnFromBench = true
    inP.subInMinute = sub.minute
    inP.replacedSportsmanId = sub.sportsmanId
  })

  // На поле — только onField. По одному на каждую позицию (первого нашли — берём).
  const posMap = new Map<string, FieldPlayer>()
  players.forEach(p => {
    if (!p.onField) return
    if (!posMap.has(p.position)) posMap.set(p.position, p)
  })

  // Запасные доступные для отображения парного кружка (ещё не использованы как Substitute).
  // Приоритет: substituteId (тренер выбрал явно на шаге утверждения), иначе fallback по совпадению позиции.
  const remainingReserves = players.filter(p => !p.onField && p.type === 'Reserve' && !p.replacedBySportsmanId)
  const lineupBySportsmanId = new Map<number, any>(props.lineup.map(e => [e.sportsmanId, e]))

  // Ушедшие с поля игроки — для отображения как «бывший основной» рядом с тем кто вышел заменой
  const offFieldByReplacement = new Map<number, FieldPlayer>()
  players.forEach(p => {
    if (!p.onField && p.replacedBySportsmanId) {
      offFieldByReplacement.set(p.replacedBySportsmanId, p)
    }
  })

  // Хелпер: для конкретного игрока на поле — собираем подходящую пару (запасного/ушедшего)
  function buildPairFor(p: FieldPlayer, pos: string): { sportsmanId: number; position: string; type: string } | null {
    let pair: { sportsmanId: number; position: string; type: string } | null = null
    if (p.cameOnFromBench) {
      const replaced = offFieldByReplacement.get(p.sportsmanId)
      if (replaced) {
        pair = { sportsmanId: replaced.sportsmanId, position: replaced.position, type: 'OffField' }
      }
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
      if (reserve) {
        pair = { sportsmanId: reserve.sportsmanId, position: reserve.position, type: reserve.type }
      }
    }
    return pair
  }

  // Хелпер: формат игрока для FootballField.
  function toFieldEntry(p: FieldPlayer, x: number, y: number, pos: string, index: number) {
    const pair = buildPairFor(p, pos)
    const replacedName = p.replacedSportsmanId
      ? (props.sportsmenMap[p.replacedSportsmanId] ?? `#${p.replacedSportsmanId}`)
      : null
    return {
      x, y,
      number: index + 1,
      name: props.sportsmenMap[p.sportsmanId] ?? `#${p.sportsmanId}`,
      position: p.position || 'ST',
      type: p.type || 'Main',
      cameOnFromBench: p.cameOnFromBench,
      subInMinute: p.subInMinute,
      replacedName,
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

  // ── Ветка 1: УНИВЕРСАЛЬНАЯ — игроки встают строго по своей позиции из БД ──
  if (formation.value === 'default') {
    const result: any[] = []
    const positions = DEFAULT_POSITIONS_MIRRORED.value
    posMap.forEach((p, pos) => {
      const fp = positions[pos] || positions['ST']
      result.push(toFieldEntry(p, fp.x, fp.y, pos, result.length))
    })
    return result
  }

  // ── Ветка 2: СЛОТОВАЯ модель для конкретных схем ──
  // Жадно идём по слотам и подбираем игрока: сначала по preferred, потом по группе.
  const slots = FORMATION_SLOTS[formation.value as Exclude<FormationKey, 'default'>]
  const playersOnField: FieldPlayer[] = []
  posMap.forEach(p => playersOnField.push(p))

  const result: any[] = []
  for (const slot of slots) {
    // 1. Ищем по preferred — позиция игрока в списке предпочтений слота
    let chosenIdx = -1
    for (const pref of slot.preferred) {
      chosenIdx = playersOnField.findIndex(p => p.position === pref)
      if (chosenIdx !== -1) break
    }
    // 2. Если не нашли — по группе (GK→GK, DEF→DEF, MID→MID, FWD→FWD)
    if (chosenIdx === -1) {
      const sg = slotGroup(slot.label)
      chosenIdx = playersOnField.findIndex(p => positionGroup(p.position) === sg)
    }

    if (chosenIdx !== -1) {
      const p = playersOnField.splice(chosenIdx, 1)[0]
      result.push(toFieldEntry(p, mx(slot.x), slot.y, slot.label, result.length))
    } else {
      // Пустой слот — пунктирный кружок с подписью позиции.
      // preferred: сначала прямые предпочтения слота, затем остальные позиции из той же
      // группы (DEF/MID/FWD) для информативности — показываем кто ещё мог бы подойти.
      const sg = slotGroup(slot.label)
      const groupExtras = (['GK','LB','CB','RB','LWB','RWB','CDM','CM','CAM','LW','RW','CF','ST','SS'] as const)
        .filter(p => positionGroup(p) === sg && !slot.preferred.includes(p))
      const preferred = [...slot.preferred, ...groupExtras]
      result.push({
        x: mx(slot.x), y: slot.y,
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

  // Лишние игроки (не попали ни в один слот) — для отладки в консоль
  if (playersOnField.length > 0 && import.meta.env?.DEV) {
    console.debug(
      `[MatchLineupDisplay] Схема ${formation.value}: не попали в состав ${playersOnField.length} игроков:`,
      playersOnField.map(p => `${p.position} ${props.sportsmenMap[p.sportsmanId] ?? p.sportsmanId}`).join(', ')
    )
  }

  return result
})
</script>
