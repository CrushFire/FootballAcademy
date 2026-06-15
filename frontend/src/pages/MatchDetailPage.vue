<template>
  <div class="p-3 md:p-6 space-y-4 h-full overflow-y-auto">
    <!-- Хедер с кнопкой Назад -->
    <div class="bg-white rounded-2xl border border-neutral-200 shadow-sm">
      <div class="h-1 bg-gradient-to-r from-blue-500 to-blue-400" />
      <div class="px-3 md:px-5 py-3 md:py-4 flex items-center gap-2 md:gap-4">
        <button @click="router.back()"
          class="flex items-center gap-1.5 px-2 md:px-3 py-2 rounded-xl border border-blue-200 bg-blue-50 text-blue-600 text-xs font-semibold hover:bg-blue-100 transition-colors shrink-0">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
          </svg>
          <span class="hidden md:inline">Назад</span>
        </button>
        <div v-if="match" class="flex-1 min-w-0">
          <div class="text-base font-bold text-neutral-900 truncate">
            {{ match.homeTeamName }} vs {{ match.opponentTeamName ?? 'Соперник' }}
          </div>
          <div class="flex flex-wrap items-center gap-x-2 gap-y-0.5 mt-0.5">
            <span class="text-xs text-neutral-500">{{ formatDate(match.date) }}</span>
            <span class="text-neutral-200 text-xs">·</span>
            <span class="text-xs font-semibold px-1.5 py-0.5 rounded-md bg-blue-50 text-blue-600">{{ typeLabel[match.type] ?? match.type }}</span>
          </div>
        </div>
      </div>
    </div>

    <div v-if="loading" class="text-sm text-neutral-400">Загрузка...</div>

    <!-- Основная карта (как у тренера, но БЕЗ блока поля) -->
    <div v-else-if="match" class="bg-white rounded-2xl border border-neutral-200 space-y-6">
      <div class="h-1 bg-gradient-to-r from-blue-500 to-blue-400 rounded-t-2xl" />

      <!-- Эмблемы + счёт -->
      <div class="p-3 md:p-6 pb-4 pt-5">
        <div class="flex items-start gap-2 md:gap-3">
          <!-- Эмблема домашней -->
          <div class="flex-1 min-w-0 flex items-center justify-center">
            <img v-if="homeImage" :src="homeImage" class="w-24 h-24 md:w-48 md:h-48 object-contain" />
            <div v-else class="w-24 h-24 md:w-48 md:h-48 rounded-full border-2 border-neutral-200 bg-neutral-50 flex items-center justify-center">
              <span class="text-2xl md:text-4xl font-bold text-blue-500">{{ initials(match.homeTeamName) }}</span>
            </div>
          </div>
          <!-- Счёт по центру -->
          <div class="flex flex-col items-center gap-1 flex-shrink-0 min-w-[96px] md:min-w-[200px]">
            <div v-if="match.status !== 'Scheduled'" class="text-3xl md:text-4xl font-extrabold text-neutral-900">
              {{ match.homeStats?.goals ?? 0 }} : {{ match.awayStats?.goals ?? 0 }}
            </div>
            <div v-else class="text-3xl md:text-4xl font-extrabold text-neutral-400">VS</div>
            <span v-if="match.result" class="text-xs font-bold px-2 py-0.5 rounded-full mt-1" :class="resultClass(match.result)">
              {{ resultLabel[match.result] }}
            </span>
          </div>
          <!-- Эмблема гостевой -->
          <div class="flex-1 min-w-0 flex items-center justify-center">
            <img v-if="awayImage" :src="awayImage" class="w-24 h-24 md:w-48 md:h-48 object-contain" />
            <div v-else class="w-24 h-24 md:w-48 md:h-48 rounded-full border-2 border-neutral-200 bg-neutral-50 flex items-center justify-center">
              <span class="text-2xl md:text-4xl font-bold text-neutral-400">{{ initials(match.opponentTeamName ?? '?') }}</span>
            </div>
          </div>
        </div>
      </div>

      <div class="px-3 md:px-6 pb-6 space-y-6">
        <!-- Статистика (без поля — спортсмену не показываем расстановку) -->
        <div v-if="match.status === 'Finished' && stats.length">
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

        <hr v-if="match.events?.length" class="border-neutral-900" />

        <!-- События -->
        <div v-if="match.events?.length">
          <div class="text-base font-bold text-neutral-500 uppercase tracking-wide mb-4 text-center">События</div>

          <!-- ДЕСКТОП: симметричная разводка (наша команда слева, соперник справа), время по центру -->
          <div class="hidden md:flex flex-col">
            <div v-for="ev in sortedEvents" :key="'d' + ev.id"
              class="flex items-center gap-2 py-3 border-b border-neutral-100 last:border-b-0"
              :class="ev.isHomeTeam ? 'flex-row' : 'flex-row-reverse'"
            >
              <div class="flex items-center gap-2 flex-1 min-w-0" :class="ev.isHomeTeam ? 'justify-start' : 'justify-end'">
                <span class="text-sm font-semibold text-neutral-700 truncate">{{ ev.comment || eventLabel[ev.type] }}</span>
              </div>
              <div class="w-14 text-center text-base font-bold text-neutral-400 flex-shrink-0">{{ ev.minute }}'</div>
              <div class="flex-1"></div>
            </div>
          </div>

          <!-- МОБИЛА: события в одну колонку, время всегда справа -->
          <div class="md:hidden flex flex-col">
            <div v-for="ev in sortedEvents" :key="'m' + ev.id"
              class="flex items-center gap-2 py-2.5 border-b border-neutral-100 last:border-b-0"
            >
              <!-- Цветная точка-маркер чьё событие -->
              <span class="w-1.5 h-1.5 rounded-full shrink-0" :class="ev.isHomeTeam ? 'bg-blue-500' : 'bg-neutral-400'" />
              <span class="text-sm font-semibold text-neutral-700 flex-1 min-w-0 truncate">{{ ev.comment || eventLabel[ev.type] }}</span>
              <span class="text-sm font-bold text-neutral-400 shrink-0 tabular-nums">{{ ev.minute }}'</span>
            </div>
          </div>
        </div>

        <hr v-if="match.trainerComment" class="border-neutral-900" />

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
import { useRoute, useRouter } from 'vue-router'
import api from '@/services/api'
import { formatDate } from '@/utils/formatDate'
import { imageUrl } from '@/utils/imageUrl'
import { MATCH_TYPE, MATCH_RESULT } from '@/constants'

const route = useRoute()
const router = useRouter()
const loading = ref(true)
const match = ref<any>(null)
const homeImage = ref<string | null>(null)
const awayImage = ref<string | null>(null)

const resultLabel = MATCH_RESULT
const typeLabel = MATCH_TYPE
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
    if (hRes.status === 'fulfilled') homeImage.value = imageUrl(hRes.value?.data?.data?.images)
    if (aRes.status === 'fulfilled') awayImage.value = imageUrl((aRes.value as any)?.data?.data?.images)
  }
  loading.value = false
})
</script>
