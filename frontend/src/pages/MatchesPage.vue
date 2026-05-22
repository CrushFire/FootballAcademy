<template>
  <div class="p-3 overflow-y-auto h-full">
    <AppCard no-border>

      <div class="flex items-center justify-between mb-4 pb-4 border-b border-neutral-100">
        <div class="text-base font-bold text-neutral-900">Матчи</div>

        <div class="flex items-center gap-2">
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

      <div v-else class="flex flex-col gap-2">
        <div
          v-for="match in paginated"
          :key="match.id"
          @click="router.push(`/matches/${match.id}`)"
          class="border rounded-xl px-4 py-3 cursor-pointer transition-all group"
          :class="borderClass(match)"
        >
          <div class="flex items-center justify-between mb-2">
            <span class="text-xs font-semibold px-2 py-0.5 rounded-full" :class="statusClass(match.status)">
              {{ statusLabel[match.status] ?? match.status }}
            </span>
            <span class="text-xs text-neutral-400">{{ formatDate(match.date) }}</span>
          </div>

          <div class="flex items-center justify-between gap-3">
            <div class="flex flex-col items-center gap-1 flex-1">
              <div class="w-16 h-16 rounded-full overflow-hidden border-2 border-neutral-200 flex items-center justify-center bg-blue-50 flex-shrink-0">
                <img v-if="teamImage(match.homeTeamId)" :src="teamImage(match.homeTeamId)" class="w-full h-full object-cover" />
                <span v-else class="text-lg font-bold text-blue-600">{{ initials(match.homeTeamName) }}</span>
              </div>
              <span class="text-xs text-neutral-600 text-center leading-tight max-w-[120px] line-clamp-2">{{ match.homeTeamName }}</span>
            </div>

            <div class="flex flex-col items-center gap-0.5 min-w-[64px]">
              <div v-if="match.status !== 'Scheduled'" class="text-xl font-extrabold text-neutral-900 leading-none">
                {{ match.homeStats?.goals ?? 0 }} : {{ match.awayStats?.goals ?? 0 }}
              </div>
              <div v-else class="text-xl font-extrabold text-neutral-400 leading-none">VS</div>
              <span v-if="match.result" class="text-xs font-bold px-2 py-0.5 rounded-full mt-0.5" :class="resultClass(match.result)">
                {{ resultLabel[match.result] }}
              </span>
              <span class="text-xs text-neutral-400 mt-0.5">{{ typeLabel[match.type] ?? match.type }}</span>
            </div>

            <div class="flex flex-col items-center gap-1 flex-1">
              <div class="w-16 h-16 rounded-full overflow-hidden border-2 border-neutral-200 flex items-center justify-center bg-neutral-50 flex-shrink-0">
                <img v-if="match.opponentTeamId && teamImage(match.opponentTeamId)" :src="teamImage(match.opponentTeamId)" class="w-full h-full object-cover" />
                <span v-else class="text-lg font-bold text-neutral-500">{{ initials(match.opponentTeamName ?? '?') }}</span>
              </div>
              <span class="text-xs text-neutral-600 text-center leading-tight max-w-[120px] line-clamp-2">{{ match.opponentTeamName ?? 'Соперник' }}</span>
            </div>
          </div>
        </div>
      </div>

    </AppCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/services/api'
import { formatDate } from '@/utils/formatDate'
import AppCard from '@/components/ui/AppCard.vue'
import { MATCH_TYPE, MATCH_STATUS, MATCH_RESULT } from '@/constants'

const router = useRouter()
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

function borderClass(_match: any) {
  return 'border-neutral-200 bg-white hover:bg-neutral-100 hover:border-neutral-300'
}

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

function initials(name: string) {
  return name.split(' ').map(w => w[0]).join('').slice(0, 2).toUpperCase()
}

function teamImage(id: number) {
  return teamImages.value[id] ?? null
}

onMounted(async () => {
  const res = await api.get('/match').catch(() => null)
  if (res) {
    matches.value = (res.data.data ?? []).sort((a: any, b: any) => new Date(b.date).getTime() - new Date(a.date).getTime())
    const ids = [...new Set(matches.value.flatMap((m: any) => [m.homeTeamId, m.opponentTeamId].filter(Boolean)))] as number[]
    await Promise.allSettled(ids.map(async id => {
      const t = await api.get(`/team/${id}`).catch(() => null)
      const img = t?.data?.data?.images?.[0]?.path ?? null
      if (img) teamImages.value[id] = img
    }))
  }
  loading.value = false
})
</script>