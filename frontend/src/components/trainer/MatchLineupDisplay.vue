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
  lineup: { sportsmanId: number; position: string; type?: string }[]
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

const fieldPositions = computed(() => {
  if (!props.lineup.length) return []

  // Применяем замены из событий (уже отфильтрованы по команде снаружи)
  const substitutions = props.events.filter(e => e.type === 'Substitution')
  const currentLineup = props.lineup.map(e => ({ ...e }))
  substitutions.forEach(sub => {
    if (!sub.sportsmanId || !sub.substituteSportsmanId) return
    const idx = currentLineup.findIndex(e => e.sportsmanId === sub.sportsmanId && e.type !== 'Reserve')
    if (idx !== -1) currentLineup[idx] = { ...currentLineup[idx], sportsmanId: sub.substituteSportsmanId }
  })

  // Группируем по позиции: main + reserve (запасной = зелёная рамка)
  const posMap = new Map<string, { main: any; reserve: any | null }>()
  currentLineup.forEach(entry => {
    const pos = entry.position || 'ST'
    if (!posMap.has(pos)) posMap.set(pos, { main: null, reserve: null })
    const g = posMap.get(pos)!
    if (entry.type === 'Reserve') { g.reserve = entry }
    else if (!g.main) { g.main = entry }
  })

  const result: any[] = []
  posMap.forEach((g, pos) => {
    if (!g.main) return
    const fp = FIELD_POSITIONS[pos] || FIELD_POSITIONS['ST']
    result.push({
      x: fp.x, y: fp.y,
      number: result.length + 1,
      name: props.sportsmenMap[g.main.sportsmanId] ?? `#${g.main.sportsmanId}`,
      position: g.main.position || 'ST',
      type: g.main.type || 'Main',
      late: false,
      substitute: g.reserve ? {
        number: 0,
        name: props.sportsmenMap[g.reserve.sportsmanId] ?? `#${g.reserve.sportsmanId}`,
        position: g.reserve.position || 'ST',
        type: g.reserve.type || 'Reserve',
      } : null,
    })
  })
  return result
})
</script>
