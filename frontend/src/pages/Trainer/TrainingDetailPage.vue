<template>
  <div class="p-6 h-full flex flex-col gap-4 overflow-hidden">
    <!-- Хедер как у других страниц персонала -->
    <div class="bg-white rounded-2xl border border-neutral-200 shadow-sm shrink-0">
      <div class="h-1 bg-gradient-to-r from-blue-500 to-blue-400" />
      <div class="px-5 py-4 flex items-center justify-between gap-4">
        <div class="flex items-center gap-4 flex-1 min-w-0">
          <button @click="$router.back()"
            class="flex items-center gap-1.5 px-3 py-2 rounded-xl border border-blue-200 bg-blue-50 text-blue-600 text-xs font-semibold hover:bg-blue-100 transition-colors shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
            </svg>
            Назад
          </button>
          <div class="flex-1 min-w-0">
            <div class="text-base font-bold text-neutral-900 truncate">{{ training.type || 'Тренировка' }}</div>
            <div class="flex flex-wrap items-center gap-x-2 gap-y-0.5 mt-0.5">
              <span class="text-xs text-neutral-500">{{ formatDate(training.date) }}</span>
              <template v-if="training.groupName">
                <span class="text-neutral-200 text-xs">·</span>
                <span class="text-xs text-neutral-500">{{ training.groupName }}</span>
              </template>
              <template v-if="training.planTrainingName">
                <span class="text-neutral-200 text-xs">·</span>
                <span class="text-xs font-semibold px-1.5 py-0.5 rounded-md bg-blue-50 text-blue-600">{{ training.planTrainingName }}</span>
              </template>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Основная карта с содержимым -->
    <div class="bg-white rounded-2xl p-6 border border-neutral-200 space-y-6 flex-1 min-h-0 overflow-y-auto">
      <!-- Описание -->
      <div>
        <h3 class="text-sm font-semibold text-neutral-900 mb-2">Описание</h3>
        <div v-if="training.otherInformation" class="p-3 rounded-lg bg-blue-50 border border-blue-100 text-xs text-neutral-700">
          {{ training.otherInformation }}
        </div>
        <div v-else class="p-3 rounded-lg bg-neutral-50 border border-neutral-200 text-xs text-neutral-400">
          Нет описания
        </div>
      </div>

      <!-- Топ показателей -->
      <div v-if="topMetrics.length">
        <h3 class="text-sm font-semibold text-neutral-900 mb-2">Топ показателей</h3>
        <div class="grid grid-cols-3 gap-3">
          <div v-for="m in topMetrics" :key="m.label" class="p-3 rounded-xl border border-neutral-200 bg-neutral-50 text-center">
            <div class="text-lg font-bold text-blue-600">{{ m.value }}</div>
            <div class="text-xs text-neutral-600 mt-1">{{ m.label }}</div>
            <div class="text-xs text-neutral-500 mt-0.5">{{ m.player }}</div>
          </div>
        </div>
      </div>

<!-- Посещаемость (компактная) -->
      <div>
        <h3 class="text-sm font-semibold text-neutral-900 mb-2">Посещаемость</h3>
        <div class="grid grid-cols-4 gap-2">
          <div class="p-2 rounded-lg border border-neutral-200 bg-neutral-50 text-center">
            <div class="text-base font-bold text-blue-500">{{ presentCount }}</div>
            <div class="text-xs text-neutral-600 mt-0.5">Присутствовали</div>
          </div>
          <div class="p-2 rounded-lg border border-neutral-200 bg-neutral-50 text-center">
            <div class="text-base font-bold text-sky-400">{{ lateCount }}</div>
            <div class="text-xs text-neutral-600 mt-0.5">Опоздали</div>
          </div>
          <div class="p-2 rounded-lg border border-neutral-200 bg-neutral-50 text-center">
            <div class="text-base font-bold text-neutral-600">{{ absentCount }}</div>
            <div class="text-xs text-neutral-600 mt-0.5">Отсутствовали</div>
          </div>
          <div class="p-2 rounded-lg border border-neutral-200 bg-neutral-50 text-center">
            <div class="text-base font-bold text-neutral-900">{{ totalCount }}</div>
            <div class="text-xs text-neutral-600 mt-0.5">Всего</div>
          </div>
        </div>
      </div>

      <!-- Список спортсменов -->
      <div>
        <button
          @click="expandedSportsmen = !expandedSportsmen"
          class="w-full flex items-center justify-between p-3 rounded-xl border border-neutral-200 bg-neutral-50 hover:border-blue-300 hover:shadow-md transition-all text-sm font-semibold text-neutral-900"
        >
          <span>Спортсмены ({{ sportsmen.length }})</span>
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" 
            class="w-4 h-4 text-neutral-600 transition-transform" :class="expandedSportsmen ? 'rotate-180' : ''">
            <path stroke-linecap="round" stroke-linejoin="round" d="M19 14l-7-7m0 0l-7 7m7-7v12"/>
          </svg>
        </button>

        <div v-if="expandedSportsmen" class="mt-3 space-y-2">
          <!-- Присутствовали -->
          <div v-if="presentSportsmen.length">
            <div class="text-xs font-semibold text-blue-600 mb-1 px-2">✓ Присутствовали ({{ presentSportsmen.length }})</div>
            <div class="space-y-1">
              <router-link
                v-for="s in presentSportsmen"
                :key="s.id"
                :to="`/trainer/sportsman/${s.id}`"
                class="flex items-center justify-between p-2 rounded-lg border border-blue-200 bg-blue-50 hover:border-blue-300 hover:shadow-md transition-all text-xs"
              >
                <div>
                  <div class="font-semibold text-neutral-800">{{ s.fio }}</div>
                  <div class="text-neutral-500 text-xs">{{ s.position ? `${s.position} · ${POSITION_LABELS[s.position] ?? s.position}` : '—' }}</div>
                </div>
              </router-link>
            </div>
          </div>

          <!-- Опоздали -->
          <div v-if="lateSportsmen.length">
            <div class="text-xs font-semibold text-sky-600 mb-1 px-2">Опоздали ({{ lateSportsmen.length }})</div>
            <div class="space-y-1">
              <router-link
                v-for="s in lateSportsmen"
                :key="s.id"
                :to="`/trainer/sportsman/${s.id}`"
                class="flex items-center justify-between p-2 rounded-lg border border-sky-200 bg-sky-50 hover:border-sky-300 hover:shadow-md transition-all text-xs"
              >
                <div>
                  <div class="font-semibold text-neutral-800">{{ s.fio }}</div>
                  <div class="text-neutral-500 text-xs">{{ s.position ? `${s.position} · ${POSITION_LABELS[s.position] ?? s.position}` : '—' }}</div>
                </div>
              </router-link>
            </div>
          </div>

          <!-- Отсутствовали -->
          <div v-if="absentSportsmen.length">
            <div class="text-xs font-semibold text-neutral-600 mb-1 px-2">✗ Отсутствовали ({{ absentSportsmen.length }})</div>
            <div class="space-y-1">
              <router-link
                v-for="s in absentSportsmen"
                :key="s.id"
                :to="`/trainer/sportsman/${s.id}`"
                class="flex items-center justify-between p-2 rounded-lg border border-neutral-300 bg-neutral-100 hover:border-neutral-400 hover:shadow-md transition-all text-xs"
              >
                <div>
                  <div class="font-semibold text-neutral-800">{{ s.fio }}</div>
                  <div class="text-neutral-500 text-xs">{{ s.position ? `${s.position} · ${POSITION_LABELS[s.position] ?? s.position}` : '—' }}</div>
                </div>
              </router-link>
            </div>
          </div>

          <div v-if="!presentSportsmen.length && !lateSportsmen.length && !absentSportsmen.length" class="p-3 text-xs text-neutral-400 text-center">
            Нет данных о спортсменах
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
import api from '@/services/api'

const POSITION_LABELS: Record<string, string> = {
  GK: 'Вратарь',
  CB: 'Центр. защитник', LB: 'Левый защитник', RB: 'Правый защитник',
  LWB: 'Левый латераль', RWB: 'Правый латераль',
  CM: 'Центр. полузащитник', CDM: 'Опорный полузащитник', CAM: 'Атакующий полузащитник',
  LW: 'Левый вингер', RW: 'Правый вингер',
  ST: 'Центр. нападающий', CF: 'Второй нападающий', SS: 'Оттянутый нападающий',
}

const route = useRoute()
const training = ref<any>({})
const sportsmen = ref<any[]>([])
const metrics = ref<any[]>([])
const attendance = ref<Record<number, string>>({})
const expandedSportsmen = ref(false)

const presentCount = computed(() => Object.values(attendance.value).filter(s => s === 'Present').length)
const lateCount = computed(() => Object.values(attendance.value).filter(s => s === 'Late').length)
const absentCount = computed(() => Object.values(attendance.value).filter(s => s === 'Absent' || s === 'ExcusedAbsent').length)
const totalCount = computed(() => Object.keys(attendance.value).length)

const presentSportsmen = computed(() => 
  sportsmen.value.filter(s => attendance.value[s.id] === 'Present')
)

const lateSportsmen = computed(() => 
  sportsmen.value.filter(s => attendance.value[s.id] === 'Late')
)

const absentSportsmen = computed(() => 
  sportsmen.value.filter(s => attendance.value[s.id] === 'Absent' || attendance.value[s.id] === 'ExcusedAbsent')
)

// Топ показателей
const topMetrics = computed(() => {
  if (!metrics.value.length) return []
  
  const sorted = [...metrics.value].sort((a, b) => (b.totalDistance || 0) - (a.totalDistance || 0))
  
  return [
    { label: 'Макс. расстояние', value: (sorted[0]?.totalDistance || 0) + ' м', player: sorted[0]?.sportsman?.fio || '—' },
    { label: 'Макс. скорость', value: (Math.max(...metrics.value.map(m => m.maximumSpeed || 0))).toFixed(1) + ' км/ч', player: metrics.value.find(m => m.maximumSpeed === Math.max(...metrics.value.map(m => m.maximumSpeed || 0)))?.sportsman?.fio || '—' },
    { label: 'Макс. нагрузка', value: (Math.max(...metrics.value.map(m => m.playerLoad || 0))).toFixed(1), player: metrics.value.find(m => m.playerLoad === Math.max(...metrics.value.map(m => m.playerLoad || 0)))?.sportsman?.fio || '—' },
  ]
})

onMounted(async () => {
  try {
    const trainingId = route.params.id
    console.log('=== Loading training', trainingId)
    if (!trainingId) return

    // Загружаем данные тренировки
    const trainingRes = await api.get(`/training/${trainingId}`).catch((e) => {
      console.error('Training error:', e)
      return null
    })
    if (trainingRes?.data?.data) {
      training.value = trainingRes.data.data
      console.log('Training loaded:', training.value)
    }

    // Загружаем спортсменов группы
    if (training.value.groupId) {
      console.log('Loading sportsmen for group:', training.value.groupId)
      const res = await api.get(`/sportsman/group/${training.value.groupId}`).catch((e) => {
        console.error('Sportsmen error:', e)
        return null
      })
      console.log('Sportsmen response:', res?.data)
      sportsmen.value = res?.data?.data ?? []
      console.log('Sportsmen loaded:', sportsmen.value.length, sportsmen.value)
    }

    // Загружаем посещаемость
    console.log('Loading attendance for training:', trainingId)
    const attendanceRes = await api.get(`/schedule/attendance/${trainingId}`).catch((e) => {
      console.error('Attendance error:', e)
      return null
    })
    console.log('Attendance response:', attendanceRes?.data)
    const attendanceList = attendanceRes?.data?.data ?? []
    attendance.value = {}
    attendanceList.forEach((a: any) => {
      attendance.value[a.sportsmanId] = a.status
    })
    console.log('Attendance loaded:', attendance.value)

    // Загружаем метрики тренировки
    if (trainingRes?.data?.data?.metrics) {
      metrics.value = trainingRes.data.data.metrics
      console.log('Metrics loaded:', metrics.value.length)
    }
  } catch (e) {
    console.error('Fatal error:', e)
  }
})
</script>

<style scoped>
</style>
