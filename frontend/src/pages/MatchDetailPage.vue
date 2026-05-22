<template>
  <div class="p-3 overflow-y-auto h-full">
    <AppCard no-border>

      <!-- Назад -->
      <div class="flex items-center gap-3 mb-6 pb-4 border-b border-neutral-100">
        <button @click="router.back()" class="p-1.5 rounded-lg hover:bg-neutral-100 transition-colors">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/></svg>
        </button>
        <div class="text-base font-bold text-neutral-900">Матч</div>
      </div>

      <div v-if="loading" class="text-sm text-neutral-400">Загрузка...</div>

      <template v-else-if="match">

        <!-- Шапка: команды + счёт -->
        <div class="flex items-center justify-between gap-2 mb-2">
          <!-- Домашняя -->
          <div class="flex flex-col items-center gap-2 flex-1">
            <span class="text-base font-bold text-neutral-600 text-center">{{ match.homeTeamName }}</span>
          </div>
          <!-- Счёт -->
          <div class="flex flex-col items-center gap-1">
            <div class="text-4xl font-extrabold text-neutral-900 tracking-tight">
              {{ match.homeStats?.goals ?? 0 }} : {{ match.awayStats?.goals ?? 0 }}
            </div>
            <span v-if="match.result" class="text-xs font-bold px-2.5 py-0.5 rounded-full mt-0.5" :class="resultClass(match.result)">
              {{ resultLabel[match.result] }}
            </span>
            <span class="text-xs text-neutral-400 mt-1">{{ formatDate(match.date) }}</span>
            <span class="text-xs text-neutral-400">{{ typeLabel[match.type] ?? match.type }}</span>
          </div>
          <!-- Гостевая -->
          <div class="flex flex-col items-center gap-2 flex-1">
            <span class="text-base font-bold text-neutral-600 text-center">{{ match.opponentTeamName ?? 'Соперник' }}</span>
          </div>
        </div>

        <!-- Статистика -->
        <div v-if="match.status === 'Finished'" class="flex items-start gap-3 mb-6 mt-4">
          <!-- Эмблема домашней -->
          <div class="flex-1 flex items-center justify-center">
            <div class="w-48 h-48 rounded-full overflow-hidden border-2 border-neutral-200 flex items-center justify-center bg-blue-50">
              <img v-if="homeImage" :src="homeImage" class="w-full h-full object-cover" />
              <span v-else class="text-4xl font-bold text-blue-500">{{ initials(match.homeTeamName) }}</span>
            </div>
          </div>
          <!-- Плашки -->
          <div class="flex flex-col items-center gap-2 flex-shrink-0" style="min-width: 200px">
            <div v-for="stat in stats" :key="stat.label" class="w-full">
              <div
                class="flex items-center justify-between px-4 py-2 rounded-xl border"
                :class="stat.danger && (stat.home > 0 || stat.away > 0) ? 'border-red-200 bg-red-50' : 'border-neutral-100 bg-neutral-50'"
              >
                <span class="text-sm font-extrabold w-8 text-center" :class="stat.home === 0 ? 'text-neutral-300' : stat.danger ? 'text-red-500' : 'text-neutral-500'">{{ stat.home }}</span>
                <span class="text-xs font-semibold flex-1 text-center" :class="stat.danger ? 'text-red-400' : 'text-neutral-400'">{{ stat.label }}</span>
                <span class="text-sm font-extrabold w-8 text-center" :class="stat.away === 0 ? 'text-neutral-300' : stat.danger ? 'text-red-500' : 'text-neutral-500'">{{ stat.away }}</span>
              </div>
            </div>
          </div>
          <!-- Эмблема гостевой -->
          <div class="flex-1 flex items-center justify-center">
            <div class="w-48 h-48 rounded-full overflow-hidden border-2 border-neutral-200 flex items-center justify-center bg-neutral-50">
              <img v-if="awayImage" :src="awayImage" class="w-full h-full object-cover" />
              <span v-else class="text-4xl font-bold text-neutral-400">{{ initials(match.opponentTeamName ?? '?') }}</span>
            </div>
          </div>
        </div>

        <!-- События -->
        <div v-if="match.events?.length" class="mt-4">
          <div class="text-base font-bold text-neutral-500 uppercase tracking-wide mb-4 text-center">События матча</div>
          <div class="flex flex-col">
            <div
              v-for="ev in sortedEvents"
              :key="ev.id"
              class="flex items-center gap-2 py-3 border-b border-neutral-100 last:border-b-0"
              :class="ev.isHomeTeam ? 'flex-row' : 'flex-row-reverse'"
            >
              <div class="flex items-center gap-2 flex-1" :class="ev.isHomeTeam ? 'justify-start' : 'justify-end'">
                <span class="text-base font-semibold text-neutral-500">{{ ev.comment || eventLabel[ev.type] }}</span>
              </div>
              <div class="w-14 text-center text-base font-bold text-neutral-400 flex-shrink-0">{{ ev.minute }}'</div>
              <div class="flex-1"></div>
            </div>
          </div>
        </div>
        <div v-else class="text-sm text-neutral-400 mt-2">Нет событий</div>

        <!-- Комментарий тренера -->
        <div v-if="match.trainerComment" class="mt-4 pt-4 border-t border-neutral-200">
          <div class="text-xs font-bold text-neutral-400 uppercase tracking-wide mb-2">Комментарий тренера</div>
          <div class="text-sm font-medium text-neutral-700">{{ match.trainerComment }}</div>
        </div>

      </template>

    </AppCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api from '@/services/api'
import { formatDate } from '@/utils/formatDate'
import AppCard from '@/components/ui/AppCard.vue'

const route = useRoute()
const router = useRouter()
const loading = ref(true)
const match = ref<any>(null)
const homeImage = ref<string | null>(null)
const awayImage = ref<string | null>(null)

const resultLabel: Record<string, string> = { Win: 'Победа', Draw: 'Ничья', Loss: 'Поражение' }
const typeLabel: Record<string, string> = { Friendly: 'Товарищеский', League: 'Лига', Cup: 'Кубок', Tournament: 'Турнир' }
const eventLabel: Record<string, string> = { Goal: 'Гол', YellowCard: 'Жёлтая карточка', RedCard: 'Красная карточка', Corner: 'Угловой', Foul: 'Фол', Penalty: 'Пенальти' }

function resultClass(r: string) {
  if (r === 'Win') return 'bg-green-100 text-green-700'
  if (r === 'Loss') return 'bg-red-100 text-red-600'
  return 'bg-neutral-100 text-neutral-600'
}

function initials(name: string) {
  return name.split(' ').map((w: string) => w[0]).join('').slice(0, 2).toUpperCase()
}

const sortedEvents = computed(() => [...(match.value?.events ?? [])].sort((a: any, b: any) => a.minute - b.minute))

const stats = computed(() => {
  const h = match.value?.homeStats
  const a = match.value?.awayStats
  if (!h || !a) return []
  return [
    { label: 'Голы',      home: h.goals,       away: a.goals,       danger: false },
    { label: 'Угловые',   home: h.corners,     away: a.corners,     danger: false },
    { label: 'Фолы',      home: h.fouls,       away: a.fouls,       danger: false },
    { label: 'Жёлтые карточки', home: h.yellowCards, away: a.yellowCards, danger: false },
    { label: 'Красные карточки', home: h.redCards,    away: a.redCards,    danger: false },
    { label: 'Пенальти',  home: h.penalties,   away: a.penalties,   danger: false },
  ]
})

onMounted(async () => {
  const id = route.params.id
  const res = await api.get(`/match/${id}`).catch(() => null)
  if (res) {
    match.value = res.data.data
    // картинки команд
    const [hRes, aRes] = await Promise.allSettled([
      api.get(`/team/${match.value.homeTeamId}`),
      match.value.opponentTeamId ? api.get(`/team/${match.value.opponentTeamId}`) : Promise.resolve(null),
    ])
    if (hRes.status === 'fulfilled') homeImage.value = hRes.value?.data?.data?.images?.[0]?.path ?? null
    if (aRes.status === 'fulfilled') awayImage.value = (aRes.value as any)?.data?.data?.images?.[0]?.path ?? null
  }
  loading.value = false
})
</script>
