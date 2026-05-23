<template>
  <div>
    <FootballField v-if="fieldPositions.length" :positions="fieldPositions" />
    <div v-else class="text-center text-xs text-neutral-400 py-6 border border-dashed border-neutral-200 rounded-xl">
      Состав не задан — добавьте игроков через «Редактировать состав»
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import FootballField from '@/components/trainer/FootballField.vue'

const props = defineProps<{
  lineup: { sportsmanId: number; position: string; type?: string; substituteId?: number }[]
  sportsmenMap: Record<number, string>
  events: any[]
}>()

const FIELD_POSITIONS: Record<string, { x: number; y: number }> = {
  GK: { x: 50, y: 10 }, CB: { x: 30, y: 25 }, CB2: { x: 70, y: 25 },
  LB: { x: 15, y: 35 }, RB: { x: 85, y: 35 }, LWB: { x: 15, y: 45 },
  RWB: { x: 85, y: 45 }, CM: { x: 50, y: 50 }, CM2: { x: 35, y: 50 },
  CM3: { x: 65, y: 50 }, CDM: { x: 50, y: 40 }, CAM: { x: 50, y: 60 },
  LW: { x: 20, y: 70 }, RW: { x: 80, y: 70 }, ST: { x: 50, y: 85 },
  CF: { x: 35, y: 80 }, SS: { x: 65, y: 80 },
}

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

  const result: any[] = []
  posMap.forEach((p, pos) => {
    const fp = FIELD_POSITIONS[pos] || FIELD_POSITIONS['ST']

    // Если этот игрок вышел заменой — в зелёный кружок-парный кладём ушедшего (того кого он заменил)
    let pair: { sportsmanId: number; position: string; type: string } | null = null
    if (p.cameOnFromBench) {
      const replaced = offFieldByReplacement.get(p.sportsmanId)
      if (replaced) {
        pair = { sportsmanId: replaced.sportsmanId, position: replaced.position, type: 'OffField' }
      }
    }

    // Иначе — обычная логика: показываем потенциального запасного (явный substituteId или fallback по позиции)
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

    // Имя ушедшего (если игрок вышел заменой) — для тултипа
    const replacedName = p.replacedSportsmanId
      ? (props.sportsmenMap[p.replacedSportsmanId] ?? `#${p.replacedSportsmanId}`)
      : null

    result.push({
      x: fp.x, y: fp.y,
      number: result.length + 1,
      name: props.sportsmenMap[p.sportsmanId] ?? `#${p.sportsmanId}`,
      position: p.position || 'ST',
      type: p.type || 'Main',
      cameOnFromBench: p.cameOnFromBench,
      subInMinute: p.subInMinute,
      replacedName,
      late: false,
      substitute: pair ? {
        number: 0,
        name: props.sportsmenMap[pair.sportsmanId] ?? `#${pair.sportsmanId}`,
        position: pair.position || 'ST',
        type: pair.type,
      } : null,
    })
  })
  return result
})
</script>
