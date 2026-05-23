<template>
  <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
    <div class="bg-white rounded-2xl shadow-lg max-w-2xl w-full max-h-[90vh] flex flex-col overflow-hidden">
      
      <!-- Синяя полоска сверху -->
      <div class="h-1 bg-gradient-to-r from-blue-500 to-blue-400" />

      <!-- Заголовок с результатом -->
      <div class="p-6 border-b-2 border-neutral-300 flex-shrink-0 bg-blue-50">
        <div class="flex items-center justify-between mb-4">
          <span class="text-xs font-semibold px-2 py-0.5 rounded-full" :class="statusClass(match.status)">
            {{ statusLabel[match.status] ?? match.status }}
          </span>
          <button @click="$emit('close')" class="p-2 rounded-lg hover:bg-neutral-100 transition-colors">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5 text-neutral-500">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>

        <!-- Счёт и команды -->
        <div class="flex items-center justify-between gap-4">
          <!-- Домашняя команда -->
          <div class="flex flex-col items-center gap-2 flex-1">
            <img v-if="teamImages[match.homeTeamId]" :src="teamImages[match.homeTeamId]" class="w-16 h-16 object-contain flex-shrink-0" />
            <div v-else class="w-16 h-16 rounded-full border-2 border-neutral-200 bg-blue-50 flex items-center justify-center flex-shrink-0">
              <span class="text-lg font-bold text-blue-600">{{ initials(match.homeTeamName) }}</span>
            </div>
            <span class="text-sm font-semibold text-neutral-800 text-center leading-tight">{{ match.homeTeamName }}</span>
          </div>

          <!-- Счёт -->
          <div class="flex flex-col items-center gap-2">
            <div v-if="match.status !== 'Scheduled'" class="text-4xl font-extrabold text-neutral-900">
              {{ match.homeStats?.goals ?? 0 }} : {{ match.awayStats?.goals ?? 0 }}
            </div>
            <div v-else class="text-4xl font-extrabold text-neutral-400">VS</div>
            <span v-if="match.result" class="text-xs font-bold px-2 py-0.5 rounded-full" :class="resultClass(match.result)">
              {{ resultLabel[match.result] }}
            </span>
            <span class="text-xs text-neutral-400 mt-1">{{ typeLabel[match.type] ?? match.type }}</span>
            <span class="text-xs text-neutral-400">{{ formatDate(match.date) }}</span>
          </div>

          <!-- Гостевая команда -->
          <div class="flex flex-col items-center gap-2 flex-1">
            <img v-if="match.opponentTeamId && teamImages[match.opponentTeamId]" :src="teamImages[match.opponentTeamId]" class="w-16 h-16 object-contain flex-shrink-0" />
            <div v-else class="w-16 h-16 rounded-full border-2 border-neutral-200 bg-neutral-50 flex items-center justify-center flex-shrink-0">
              <span class="text-lg font-bold text-neutral-500">{{ initials(match.opponentTeamName ?? '?') }}</span>
            </div>
            <span class="text-sm font-semibold text-neutral-800 text-center leading-tight">{{ match.opponentTeamName ?? 'Соперник' }}</span>
          </div>
        </div>
      </div>

      <!-- Содержимое -->
      <div class="flex-1 overflow-y-auto p-6 space-y-4">
        <!-- События матча -->
        <div v-if="match.events?.length">
          <h3 class="text-sm font-semibold text-neutral-900 mb-3">События</h3>
          <div class="space-y-1 border-b-2 border-neutral-300 pb-4">
            <ModalCard v-for="ev in match.events" :key="ev.id" class="flex items-center gap-3 text-xs">
              <span class="font-bold text-blue-600 min-w-[35px] text-center">{{ ev.minute }}'</span>
              <span class="text-neutral-700 flex-1">{{ eventTypeLabel[ev.type] ?? ev.type }}</span>
              <span class="text-neutral-500">{{ ev.isHomeTeam ? 'Домашняя команда' : 'Команда соперника' }}</span>
            </ModalCard>
          </div>
        </div>

        <!-- Статистика -->
        <div v-if="match.homeStats || match.awayStats">
          <h3 class="text-sm font-semibold text-neutral-900 mb-3">Статистика</h3>
          <div class="space-y-1 border-b-2 border-neutral-300 pb-4">
            <ModalCard class="flex items-center justify-between text-xs">
              <span class="text-neutral-600">Голы</span>
              <span class="font-semibold text-neutral-800">{{ match.homeStats?.goals ?? 0 }} - {{ match.awayStats?.goals ?? 0 }}</span>
            </ModalCard>
            <ModalCard class="flex items-center justify-between text-xs">
              <span class="text-neutral-600">Жёлтые карточки</span>
              <span class="font-semibold text-neutral-800">{{ match.homeStats?.yellowCards ?? 0 }} - {{ match.awayStats?.yellowCards ?? 0 }}</span>
            </ModalCard>
            <ModalCard class="flex items-center justify-between text-xs">
              <span class="text-neutral-600">Красные карточки</span>
              <span class="font-semibold text-neutral-800">{{ match.homeStats?.redCards ?? 0 }} - {{ match.awayStats?.redCards ?? 0 }}</span>
            </ModalCard>
            <ModalCard class="flex items-center justify-between text-xs">
              <span class="text-neutral-600">Угловые</span>
              <span class="font-semibold text-neutral-800">{{ match.homeStats?.corners ?? 0 }} - {{ match.awayStats?.corners ?? 0 }}</span>
            </ModalCard>
            <ModalCard class="flex items-center justify-between text-xs">
              <span class="text-neutral-600">Фолы</span>
              <span class="font-semibold text-neutral-800">{{ match.homeStats?.fouls ?? 0 }} - {{ match.awayStats?.fouls ?? 0 }}</span>
            </ModalCard>
          </div>
        </div>

        <!-- Комментарий тренера -->
        <div v-if="match.trainerComment">
          <h3 class="text-sm font-semibold text-neutral-900 mb-2">Комментарий</h3>
          <div class="p-3 rounded-lg bg-blue-50 border border-blue-100 text-xs text-neutral-700">
            {{ match.trainerComment }}
          </div>
        </div>
      </div>

      <!-- Кнопка закрытия -->
      <div class="p-4 border-t-2 border-neutral-300 flex-shrink-0">
        <button @click="$emit('close')" class="w-full px-4 py-2 rounded-xl border border-neutral-300 bg-neutral-100 text-neutral-600 hover:bg-neutral-200 transition-colors text-sm font-medium">
          Закрыть
        </button>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { formatDate } from '@/utils/formatDate'
import { ref, onMounted, onUnmounted } from 'vue'
import ModalCard from './ModalCard.vue'
import { MATCH_TYPE, MATCH_STATUS, MATCH_RESULT } from '@/constants'

defineProps<{
  match: any
  teamImages: Record<number, string>
}>()

defineEmits<{
  close: []
}>()

const sortOpen = ref(false)

function onClickOutside(e: MouseEvent) {
  const target = e.target as HTMLElement
  if (!target.closest('.sort-dropdown-match')) sortOpen.value = false
}

onMounted(() => {
  document.addEventListener('click', onClickOutside)
})

onUnmounted(() => {
  document.removeEventListener('click', onClickOutside)
})

const statusLabel = MATCH_STATUS
const resultLabel = MATCH_RESULT
const typeLabel = MATCH_TYPE
const eventTypeLabel: Record<string, string> = {
  Goal: 'Гол',
  YellowCard: 'Жёлтая карточка',
  RedCard: 'Красная карточка',
  Corner: 'Угловой',
  Penalty: 'Пенальти',
  Substitution: 'Замена'
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
</script>
