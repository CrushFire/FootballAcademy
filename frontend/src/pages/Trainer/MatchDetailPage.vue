<template>
  <div class="p-3 md:p-6 space-y-4 h-full overflow-y-auto">
    <!-- Хедер -->
    <div class="bg-white rounded-2xl border border-neutral-200 shadow-sm">
      <div class="h-1 bg-gradient-to-r from-blue-500 to-blue-400" />
      <div class="px-3 md:px-5 py-3 md:py-4 flex items-center gap-2 md:gap-4">
        <button @click="$router.back()"
          class="flex items-center gap-1.5 px-2 md:px-3 py-2 rounded-xl border border-blue-200 bg-blue-50 text-blue-600 text-xs font-semibold hover:bg-blue-100 transition-colors shrink-0">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
          </svg>
          <span class="hidden md:inline">Назад</span>
        </button>
        <div class="flex-1 min-w-0">
          <div class="text-base font-bold text-neutral-900 truncate">
            {{ match.homeTeamName }} vs {{ match.opponentTeamName ?? 'Соперник' }}
          </div>
          <div class="flex flex-wrap items-center gap-x-2 gap-y-0.5 mt-0.5">
            <span class="text-xs text-neutral-500">{{ formatDate(match.date) }}</span>
            <span class="text-neutral-200 text-xs">·</span>
            <span class="text-xs font-semibold px-1.5 py-0.5 rounded-md bg-blue-50 text-blue-600">{{ typeLabel[match.type] ?? match.type }}</span>
          </div>
        </div>
        <span class="text-xs font-semibold px-2 py-0.5 rounded-full shrink-0" :class="statusClass(match.status)">
          {{ statusLabel[match.status] ?? match.status }}
        </span>
      </div>
    </div>

    <!-- Основная карта -->
    <div class="bg-white rounded-2xl border border-neutral-200 space-y-6">
      <div class="h-1 bg-gradient-to-r from-blue-500 to-blue-400 rounded-t-2xl" />

      <!-- Иконки + счёт -->
      <div class="p-3 md:p-6 pb-4 pt-5">
        <div class="flex items-start gap-2 md:gap-3">
          <!-- Эмблема домашней -->
          <div class="flex-1 min-w-0 flex items-center justify-center">
            <img v-if="teamImages[match.homeTeamId]" :src="teamImages[match.homeTeamId]" class="w-24 h-24 md:w-48 md:h-48 object-contain" />
            <div v-else class="w-24 h-24 md:w-48 md:h-48 rounded-full border-2 border-neutral-200 bg-neutral-50 flex items-center justify-center">
              <span class="text-2xl md:text-4xl font-bold text-blue-500">{{ initials(match.homeTeamName ?? '') }}</span>
            </div>
          </div>
          <!-- Счёт по центру -->
          <div class="flex flex-col items-center gap-1 flex-shrink-0 min-w-[96px] md:min-w-[200px]">
            <div v-if="match.status !== 'Scheduled'" class="text-3xl md:text-4xl font-extrabold text-neutral-900">
              {{ match.homeStats?.goals ?? 0 }} : {{ match.awayStats?.goals ?? 0 }}
            </div>
            <div v-else class="text-3xl md:text-4xl font-extrabold text-neutral-400">VS</div>
            <span v-if="match.result" class="text-xs font-bold px-2 py-0.5 rounded-full mt-1" :class="resultClass(match.result)">
              {{ internalResultLabel }}
            </span>
          </div>
          <!-- Эмблема гостевой -->
          <div class="flex-1 min-w-0 flex items-center justify-center">
            <img v-if="match.opponentTeamId && teamImages[match.opponentTeamId]" :src="teamImages[match.opponentTeamId]" class="w-24 h-24 md:w-48 md:h-48 object-contain" />
            <div v-else class="w-24 h-24 md:w-48 md:h-48 rounded-full border-2 border-neutral-200 bg-neutral-50 flex items-center justify-center">
              <span class="text-2xl md:text-4xl font-bold text-neutral-400">{{ initials(match.opponentTeamName ?? '?') }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="px-3 md:px-6 pb-6 space-y-6">
        <!-- Поле с расстановкой -->
        <div v-if="fieldPositions.length">
          <div class="text-base font-bold text-neutral-500 uppercase tracking-wide mb-4 text-center">Расстановка на поле</div>
          <!-- Десктоп: горизонтальное поле -->
          <div class="hidden md:block">
            <FootballField :positions="fieldPositions" />
          </div>
          <!-- Мобила: вертикальное поле (полное).
               Координаты пересчитываем из горизонтальных в вертикальные:
               у горизонтального наши ворота слева (x≈0), у вертикального — снизу (y≈100). -->
          <div class="md:hidden">
            <FootballFieldVertical :positions="fieldPositionsVertical" />
          </div>
        </div>

        <!-- Статистика -->
        <div v-if="match.homeStats || match.awayStats">
          <div class="text-base font-bold text-neutral-500 uppercase tracking-wide mb-4 text-center">Статистика</div>
          <div class="flex flex-col gap-2">
            <div v-for="stat in stats" :key="stat.label"
              class="flex items-center justify-between px-4 py-2 rounded-xl border border-neutral-100 bg-neutral-50"
            >
              <span class="text-sm font-extrabold w-8 text-center text-neutral-700">{{ stat.home }}</span>
              <span class="text-xs font-semibold flex-1 text-center text-neutral-400">{{ stat.label }}</span>
              <span class="text-sm font-extrabold w-8 text-center text-neutral-700">{{ stat.away }}</span>
            </div>
          </div>
        </div>

        <hr class="border-neutral-900" />

        <!-- События -->
        <div v-if="match.events?.length">
          <div class="text-base font-bold text-neutral-500 uppercase tracking-wide mb-4 text-center">События</div>

          <!-- ДЕСКТОП: симметричная разводка, время по центру -->
          <div class="hidden md:flex flex-col">
            <div v-for="ev in sortedEvents" :key="'d' + ev.id"
              class="flex items-center gap-2 py-3 border-b border-neutral-100 last:border-b-0"
              :class="ev.isHomeTeam ? 'flex-row' : 'flex-row-reverse'"
            >
              <div class="flex items-center gap-2 flex-1" :class="ev.isHomeTeam ? 'justify-start' : 'justify-end'">
                <template v-if="ev.type === 'Substitution'">
                  <div :class="ev.isHomeTeam ? 'text-left' : 'text-right'">
                    <span class="text-sm font-semibold text-neutral-800">Замена</span>
                    <div class="text-sm font-bold text-neutral-400">
                      {{ getSubstitutionPosition(ev) }} ({{ positionLabel[getSubstitutionPosition(ev)] ?? getSubstitutionPosition(ev) }}) {{ sportsmenMap[ev.substituteSportsmanId] ?? ev.substituteSportsmanId }}
                    </div>
                  </div>
                </template>
                <template v-else>
                  <span class="text-sm font-semibold text-neutral-700">{{ eventTypeLabel[ev.type] ?? ev.type }}</span>
                </template>
              </div>
              <div class="w-14 text-center text-base font-bold text-neutral-400 flex-shrink-0">{{ ev.minute }}'</div>
              <div class="flex-1"></div>
            </div>
          </div>

          <!-- МОБИЛА: в одну колонку, время всегда справа -->
          <div class="md:hidden flex flex-col">
            <div v-for="ev in sortedEvents" :key="'m' + ev.id"
              class="flex items-center gap-2 py-2.5 border-b border-neutral-100 last:border-b-0"
            >
              <span class="w-1.5 h-1.5 rounded-full shrink-0" :class="ev.isHomeTeam ? 'bg-blue-500' : 'bg-neutral-400'" />
              <div class="flex-1 min-w-0">
                <template v-if="ev.type === 'Substitution'">
                  <span class="text-sm font-semibold text-neutral-800">Замена</span>
                  <div class="text-xs text-neutral-400 truncate">
                    {{ getSubstitutionPosition(ev) }} · {{ sportsmenMap[ev.substituteSportsmanId] ?? ev.substituteSportsmanId }}
                  </div>
                </template>
                <template v-else>
                  <span class="text-sm font-semibold text-neutral-700">{{ eventTypeLabel[ev.type] ?? ev.type }}</span>
                </template>
              </div>
              <span class="text-sm font-bold text-neutral-400 shrink-0 tabular-nums">{{ ev.minute }}'</span>
            </div>
          </div>
        </div>

        <hr class="border-neutral-900" />

        <!-- Комментарий тренера -->
        <div v-if="match.trainerComment">
          <div class="text-base font-bold text-neutral-500 uppercase tracking-wide mb-4 text-center">Комментарий тренера</div>
          <div class="p-3 rounded-lg bg-neutral-50 border border-neutral-100 text-sm font-medium text-neutral-700">
            {{ match.trainerComment }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { formatDate } from '@/utils/formatDate'
import { imageUrl } from '@/utils/imageUrl'
import api from '@/services/api'
import { POSITION_AND_GROUP_LABEL, MATCH_TYPE, MATCH_STATUS, MATCH_RESULT } from '@/constants'
import FootballField from '@/components/trainer/FootballField.vue'
import FootballFieldVertical from '@/components/trainer/FootballFieldVertical.vue'

// Универсальная раскладка на ВСЁ горизонтальное поле (база — 4-3-3).
// Поле ГОРИЗОНТАЛЬНОЕ: X = 0 (левые ворота, НАШИ) → 100 (правые ворота, СОПЕРНИКА).
// Y = вертикальная позиция игрока (0 = верх трибуны, 100 = низ).
// Центр поля X=50. Защитники прижаты к нашим воротам (X малое), нападающие — к чужим (X большое).
const FIELD_POSITIONS: Record<string, { x: number; y: number }> = {
  GK:  { x: 8,  y: 50 },
  // Защита: 4 игрока линией, крайние — у боковых, центральные — ближе к центру по Y
  LB:  { x: 18, y: 12 },
  CB:  { x: 18, y: 36 },
  CB2: { x: 18, y: 64 },
  RB:  { x: 18, y: 88 },
  // Латерали — чуть впереди защитников, по бокам
  LWB: { x: 28, y: 8  },
  RWB: { x: 28, y: 92 },
  // Полузащита: CDM ближе к защите, CM/CM2 по бокам, CAM ближе к атаке
  CDM: { x: 32, y: 50 },
  CM:  { x: 42, y: 30 },
  CM2: { x: 42, y: 70 },
  CM3: { x: 44, y: 50 },
  CAM: { x: 56, y: 50 },
  // Атака: вингеры по краям, ST в центре у чужих ворот, CF/SS чуть позади
  LW:  { x: 72, y: 15 },
  RW:  { x: 72, y: 85 },
  CF:  { x: 80, y: 35 },
  ST:  { x: 86, y: 50 },
  SS:  { x: 80, y: 65 },
}

const route = useRoute()
const match = ref<any>({})
const teamImages = ref<Record<number, string>>({})
const sportsmenMap = ref<Record<number, string>>({})

const statusLabel = MATCH_STATUS
const resultLabel = MATCH_RESULT
const typeLabel = MATCH_TYPE
const eventTypeLabel: Record<string, string> = {
  Goal: 'Гол', YellowCard: 'Жёлтая карточка', RedCard: 'Красная карточка',
  Corner: 'Угловой', Penalty: 'Пенальти', Foul: 'Фол', Substitution: 'Замена'
}
const positionLabel = POSITION_AND_GROUP_LABEL

const stats = computed(() => [
  { label: 'Голы',              home: match.value.homeStats?.goals ?? 0,        away: match.value.awayStats?.goals ?? 0 },
  { label: 'Жёлтые карточки',  home: match.value.homeStats?.yellowCards ?? 0,   away: match.value.awayStats?.yellowCards ?? 0 },
  { label: 'Красные карточки', home: match.value.homeStats?.redCards ?? 0,      away: match.value.awayStats?.redCards ?? 0 },
  { label: 'Угловые',          home: match.value.homeStats?.corners ?? 0,       away: match.value.awayStats?.corners ?? 0 },
  { label: 'Фолы',             home: match.value.homeStats?.fouls ?? 0,         away: match.value.awayStats?.fouls ?? 0 },
  { label: 'Пенальти',         home: match.value.homeStats?.penalties ?? 0,     away: match.value.awayStats?.penalties ?? 0 },
])

const sortedEvents = computed(() => [...(match.value.events ?? [])].sort((a: any, b: any) => a.minute - b.minute))

// Если матч между двумя нашими командами (Home + opponentTeamId есть) — показываем
// «Победа {имя_победителя}» / «Ничья» вместо абстрактных Победа/Поражение.
const internalResultLabel = computed(() => {
  const m = match.value
  if (!m?.result) return ''
  const isInternal = m.type === 'Home' && !!m.opponentTeamId
  if (!isInternal) return resultLabel[m.result]
  if (m.result === 'Draw') return 'Ничья'
  return m.result === 'Win'
    ? `Победа ${m.homeTeamName ?? ''}`.trim()
    : `Победа ${m.opponentTeamName ?? ''}`.trim()
})

function getSubstitutionPosition(ev: any): string {
  const lineup: { sportsmanId: number; position: string }[] = match.value.lineup ?? []
  const entry = lineup.find(l => l.sportsmanId === ev.sportsmanId)
  return entry?.position ?? ''
}

const fieldPositions = computed(() => {
  const lineup: { sportsmanId: number; position: string; type?: 'Main' | 'Reserve' }[] = match.value.lineup ?? []
  const events: any[] = match.value.events ?? []
  if (!lineup.length) return []

  // Находим все замены из событий
  const substitutions = events.filter(e => e.type === 'Substitution' && e.isHomeTeam)
  
  // Создаём карту: sportsmanId (кто вышел с поля) -> substituteSportsmanId (кто вышел на поле)
  const subMap = new Map<number, number>()
  substitutions.forEach(sub => {
    if (sub.sportsmanId && sub.substituteSportsmanId) {
      subMap.set(sub.sportsmanId, sub.substituteSportsmanId)
    }
  })

  console.log('🔍 Substitutions map:', Array.from(subMap.entries()))

  // Группируем по позициям: основные и запасные
  const positionGroups = new Map<string, { main: any | null; reserve: any | null }>()
  
  lineup.forEach((entry) => {
    const pos = entry.position || 'ST'
    if (!positionGroups.has(pos)) {
      positionGroups.set(pos, { main: null, reserve: null })
    }
    const group = positionGroups.get(pos)!
    
    // Определяем тип: если type === 'Reserve' или это второй игрок на позиции
    if (entry.type === 'Reserve' || group.main !== null) {
      group.reserve = entry
    } else {
      group.main = entry
    }
  })

  // Добавляем запасных из событий замены
  subMap.forEach((substituteId, mainId) => {
    // Находим позицию основного игрока
    const mainEntry = lineup.find(e => e.sportsmanId === mainId)
    if (mainEntry) {
      const pos = mainEntry.position || 'ST'
      const group = positionGroups.get(pos)
      if (group && !group.reserve) {
        group.reserve = { sportsmanId: substituteId, position: pos }
      }
    }
  })

  console.log('🔍 Position groups:', Array.from(positionGroups.entries()))

  // Формируем позиции для поля.
  // Если в группе (один ключ pos) больше одного игрока — разносим их
  // вокруг базовой точки с лёгкой асимметрией: разводим по X и слегка по Y,
  // чтобы не накладывались. Например 3 CB → расходятся влево-вправо от центра.
  const result: any[] = []
  const usedCount: Record<string, number> = {}
  // Сколько раз встречается каждая позиция всего (для расчёта смещения).
  const totalByPos: Record<string, number> = {}
  positionGroups.forEach((group, pos) => {
    if (!group.main) return
    totalByPos[pos] = (totalByPos[pos] ?? 0) + 1
  })

  positionGroups.forEach((group, pos) => {
    if (!group.main) return

    const fp = FIELD_POSITIONS[pos] || FIELD_POSITIONS['ST']
    let x = fp.x, y = fp.y

    // Если на одной позиции (одинаковый код, например 2 раза CB) >1 игрока,
    // разводим их симметрично по X относительно базовой точки + чередуем Y
    // (один чуть глубже, другой чуть выше) для естественной асимметрии.
    const total = totalByPos[pos] ?? 1
    if (total > 1) {
      const idx = usedCount[pos] ?? 0
      const half = (total - 1) / 2
      const dx = (idx - half) * 12
      const dy = (idx % 2 === 0 ? 1 : -1) * 3
      x = Math.max(6, Math.min(94, x + dx))
      y = Math.max(6, Math.min(94, y + dy))
    }
    usedCount[pos] = (usedCount[pos] ?? 0) + 1

    result.push({
      x, y,
      number: result.length + 1,
      name: sportsmenMap.value[group.main.sportsmanId] ?? `#${group.main.sportsmanId}`,
      position: pos,
      late: false,
      substitute: group.reserve ? {
        number: 0,
        name: sportsmenMap.value[group.reserve.sportsmanId] ?? `#${group.reserve.sportsmanId}`,
        position: group.reserve.position || pos,
      } : null,
    })
  })
  
  console.log('🔍 Field positions result:', result)
  return result
})

// Для вертикального поля (мобила): переворачиваем координаты.
// Горизонтальное поле: наши ворота слева (x=0..20), чужие справа (x=80..100), Y — поперёк поля.
// Вертикальное поле: наши ворота снизу (y=80..100), чужие сверху (y=0..20), X — поперёк.
// Формула: vertical.x = horizontal.y, vertical.y = 100 - horizontal.x
const fieldPositionsVertical = computed(() =>
  fieldPositions.value.map((p: any) => ({ ...p, x: p.y, y: 100 - p.x }))
)

function statusClass(s: string) {
  if (s === 'Finished')   return 'bg-neutral-100 text-neutral-600'
  if (s === 'InProgress') return 'bg-green-100 text-green-700'
  return 'bg-blue-50 text-blue-600'
}

function resultClass(r: string) {
  if (r === 'Win')  return 'bg-green-100 text-green-700'
  if (r === 'Loss') return 'bg-red-100 text-red-600'
  return 'bg-neutral-100 text-neutral-600'
}

function initials(name: string) {
  return name.split(' ').map(w => w[0]).join('').slice(0, 2).toUpperCase()
}

onMounted(async () => {
  const id = route.params.id
  if (!id) return

  const res = await api.get(`/match/${id}`).catch(() => null)
  if (res?.data?.data) match.value = res.data.data

  const ids = [match.value.homeTeamId, match.value.opponentTeamId].filter(Boolean)
  await Promise.allSettled(ids.map(async (tid: number) => {
    const t = await api.get(`/team/${tid}`).catch(() => null)
    const url = imageUrl(t?.data?.data?.images)
    if (url) teamImages.value[tid] = url
  }))

  const lineup: { sportsmanId: number }[] = match.value.lineup ?? []
  const subIds = (match.value.events ?? [])
    .filter((e: any) => e.type === 'Substitution' && e.substituteSportsmanId)
    .map((e: any) => e.substituteSportsmanId)
  const allIds = [...new Set([...lineup.map(e => e.sportsmanId), ...subIds])]
  await Promise.allSettled(allIds.map(async (sid: number) => {
    const s = await api.get(`/sportsman/${sid}`).catch(() => null)
    const fio = s?.data?.data?.fio
    if (fio) sportsmenMap.value[sid] = fio
  }))
})
</script>
