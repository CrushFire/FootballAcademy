<template>
  <div class="p-3 overflow-y-auto h-full">
    <AppCard no-border class="min-h-full">

      <!-- Шапка страницы -->
      <div class="flex items-start justify-between gap-2 -mx-4 -mt-4 px-4 md:px-5 py-4 mb-5 rounded-t-2xl border-b border-neutral-100">
        <div class="min-w-0">
          <h1 class="text-xl font-bold text-neutral-900">Матчи</h1>
          <p class="text-sm text-neutral-500 mt-0.5">Результаты игр</p>
        </div>
        <div class="flex items-center gap-2 shrink-0">
          <button @click="page > 1 && page--" :disabled="page === 1" class="p-1.5 rounded-lg hover:bg-neutral-100 disabled:opacity-30 transition-colors">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/></svg>
          </button>
          <span class="text-xs text-neutral-500">{{ page }} / {{ totalPages }}</span>
          <button @click="page < totalPages && page++" :disabled="page === totalPages" class="p-1.5 rounded-lg hover:bg-neutral-100 disabled:opacity-30 transition-colors">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/></svg>
          </button>
        </div>
      </div>

      <div v-if="loading" class="text-sm text-neutral-400">Загрузка...</div>
      <div v-else-if="!matches.length" class="text-sm text-neutral-400">Нет матчей</div>

      <!-- Карточка матча — единый вид с тренерским списком (frontend/src/pages/Trainer/MatchesPage.vue) -->
      <div v-else class="flex flex-col gap-2">
        <div
          v-for="m in paginated"
          :key="m.id"
          @click="router.push(`/matches/${m.id}`)"
          class="bg-white rounded-2xl border border-neutral-200 p-4 cursor-pointer hover:border-blue-200 hover:bg-blue-50/30 transition-all"
        >
          <div class="flex items-center justify-between mb-3 pb-3 border-b border-neutral-100">
            <span class="text-xs font-semibold px-2 py-0.5 rounded-full" :class="statusClass(m.status)">
              {{ statusLabel[m.status] ?? m.status }}
            </span>
            <span class="text-xs text-neutral-400">{{ formatDate(m.date) }}</span>
          </div>

          <div class="flex items-center justify-between gap-2 md:gap-4">
            <div class="flex flex-col items-center gap-2 flex-1 min-w-0">
              <img v-if="teamImage(m.homeTeamId)" :src="teamImage(m.homeTeamId)" class="w-12 h-12 md:w-14 md:h-14 object-contain flex-shrink-0" />
              <div v-else class="w-12 h-12 md:w-14 md:h-14 rounded-full border-2 border-neutral-200 bg-neutral-50 flex items-center justify-center flex-shrink-0">
                <span class="text-sm font-bold text-blue-600">{{ initials(m.homeTeamName) }}</span>
              </div>
              <span class="text-xs font-semibold text-neutral-800 text-center leading-tight w-full line-clamp-2">{{ m.homeTeamName }}</span>
            </div>

            <div class="flex flex-col items-center gap-1 shrink-0 min-w-[60px] md:min-w-[70px]">
              <div v-if="m.status !== 'Scheduled'" class="text-xl md:text-2xl font-extrabold text-neutral-900">
                {{ m.homeStats?.goals ?? 0 }} : {{ m.awayStats?.goals ?? 0 }}
              </div>
              <div v-else class="text-xl md:text-2xl font-extrabold text-neutral-400">VS</div>
              <span v-if="m.result" class="text-[10px] md:text-xs font-bold px-2 py-0.5 rounded-full mt-0.5 whitespace-nowrap" :class="resultClass(m.result)">
                {{ formatResult(m) }}
              </span>
              <span class="text-[10px] md:text-xs text-neutral-400 mt-1 whitespace-nowrap">{{ typeLabel[m.type] ?? m.type }}</span>
            </div>

            <div class="flex flex-col items-center gap-2 flex-1 min-w-0">
              <img v-if="m.opponentTeamId && teamImage(m.opponentTeamId)" :src="teamImage(m.opponentTeamId)" class="w-12 h-12 md:w-14 md:h-14 object-contain flex-shrink-0" />
              <div v-else class="w-12 h-12 md:w-14 md:h-14 rounded-full border-2 border-neutral-200 bg-neutral-50 flex items-center justify-center flex-shrink-0">
                <span class="text-sm font-bold text-neutral-500">{{ initials(m.opponentTeamName ?? '?') }}</span>
              </div>
              <span class="text-xs font-semibold text-neutral-800 text-center leading-tight w-full line-clamp-2">{{ m.opponentTeamName ?? 'Соперник' }}</span>
            </div>
          </div>
        </div>
      </div>

    </AppCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '@/services/api'
import { formatDate } from '@/utils/formatDate'
import { imageUrl } from '@/utils/imageUrl'
import AppCard from '@/components/ui/AppCard.vue'
import { MATCH_TYPE, MATCH_STATUS, MATCH_RESULT } from '@/constants'

const router = useRouter()
const route = useRoute()
const loading = ref(true)
const matches = ref<any[]>([])
const teamImages = ref<Record<number, string>>({})
const page = ref(1)
const PER_PAGE = 10

const paginated = computed(() => matches.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))
const totalPages = computed(() => Math.ceil(matches.value.length / PER_PAGE) || 1)

const statusLabel = MATCH_STATUS
const resultLabel = MATCH_RESULT
const typeLabel = MATCH_TYPE

function statusClass(s: string) {
  if (s === 'Finished')  return 'bg-neutral-100 text-neutral-600'
  if (s === 'InProgress') return 'bg-green-100 text-green-700'
  return 'bg-blue-50 text-blue-600'
}

function resultClass(r: string) {
  if (r === 'Win')  return 'bg-green-100 text-green-700'
  if (r === 'Loss') return 'bg-red-100 text-red-600'
  return 'bg-neutral-100 text-neutral-600'
}

// Если обе команды наши (Home + opponentTeamId) — подписываем «Победа {имя}» / «Ничья».
function formatResult(m: any): string {
  const isInternal = m.type === 'Home' && !!m.opponentTeamId
  if (!isInternal) return resultLabel[m.result] ?? m.result
  if (m.result === 'Draw') return 'Ничья'
  return m.result === 'Win'
    ? `Победа ${m.homeTeamName ?? ''}`.trim()
    : `Победа ${m.opponentTeamName ?? ''}`.trim()
}

function initials(name: string) {
  return name.split(' ').map(w => w[0]).join('').slice(0, 2).toUpperCase()
}

function teamImage(id: number) {
  return teamImages.value[id] ?? null
}

onMounted(async () => {
  // Определяем команды спортсмена, чтобы показать только релевантные матчи.
  // Если в роуте есть :id (тренер смотрит чужого спортсмена) — тянем по нему.
  // Иначе (сам спортсмен на /matches) — тянем sportsman/me.
  let sportsmanTeamIds = new Set<number>()
  const routeId = route.params.id ? Number(route.params.id) : null
  const sportsmanRes = routeId
    ? await api.get(`/sportsman/${routeId}`).catch(() => null)
    : await api.get('/sportsman/me').catch(() => null)
  const sportsman = sportsmanRes?.data?.data
  if (sportsman?.teamId) sportsmanTeamIds.add(Number(sportsman.teamId))

  const res = await api.get('/match').catch(() => null)
  if (res) {
    const all = (res.data.data ?? [])
    // Фильтруем только матчи команд спортсмена (либо home, либо opponent).
    // Если команд у спортсмена нет — не показываем ничего.
    const filtered = sportsmanTeamIds.size > 0
      ? all.filter((m: any) =>
          sportsmanTeamIds.has(Number(m.homeTeamId)) ||
          (m.opponentTeamId && sportsmanTeamIds.has(Number(m.opponentTeamId)))
        )
      : []
    matches.value = filtered.sort((a: any, b: any) => new Date(b.date).getTime() - new Date(a.date).getTime())
    const ids = [...new Set(matches.value.flatMap((m: any) => [m.homeTeamId, m.opponentTeamId].filter(Boolean)))] as number[]
    await Promise.allSettled(ids.map(async id => {
      const t = await api.get(`/team/${id}`).catch(() => null)
      const url = imageUrl(t?.data?.data?.images)
      if (url) teamImages.value[id] = url
    }))
  }
  loading.value = false
})
</script>
