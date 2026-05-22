<template>
  <div class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
    <div class="bg-white rounded-2xl shadow-lg max-w-2xl w-full max-h-[90vh] flex flex-col overflow-hidden">
      
      <!-- Синяя полоска сверху -->
      <div class="h-1 bg-gradient-to-r from-blue-500 to-blue-400" />

      <!-- Заголовок -->
      <ModalHeader
        :title="training.type || 'Тренировка'"
        :subtitle="[training.groupName, training.planTrainingName].filter(Boolean).join(' · ')"
        :info="`${formatDate(training.date)}`"
        @close="$emit('close')"
      />

      <!-- Содержимое -->
      <div class="flex-1 overflow-y-auto p-6 space-y-4">
        <!-- Описание -->
        <div v-if="training.otherInformation">
          <h3 class="text-sm font-semibold text-neutral-900 mb-2">Описание</h3>
          <div class="p-3 rounded-lg bg-blue-50 border border-blue-100 text-xs text-neutral-700">
            {{ training.otherInformation }}
          </div>
        </div>

        <!-- Статистика посещаемости -->
        <div>
          <h3 class="text-sm font-semibold text-neutral-900 mb-3">Посещаемость</h3>
          <div class="grid grid-cols-4 gap-2">
            <ModalCard class="text-center">
              <div class="text-lg font-bold text-green-600">{{ presentCount }}</div>
              <div class="text-xs text-neutral-600 mt-1">Присутствовали</div>
            </ModalCard>
            <ModalCard class="text-center">
              <div class="text-lg font-bold text-yellow-600">{{ lateCount }}</div>
              <div class="text-xs text-neutral-600 mt-1">Опоздали</div>
            </ModalCard>
            <ModalCard class="text-center">
              <div class="text-lg font-bold text-red-600">{{ absentCount }}</div>
              <div class="text-xs text-neutral-600 mt-1">Отсутствовали</div>
            </ModalCard>
            <ModalCard class="text-center">
              <div class="text-lg font-bold text-neutral-600">{{ totalCount }}</div>
              <div class="text-xs text-neutral-600 mt-1">Всего</div>
            </ModalCard>
          </div>
        </div>

        <!-- Список спортсменов -->
        <div>
          <button
            @click="expandedSportsmen = !expandedSportsmen"
            class="w-full flex items-center justify-between p-3 rounded-xl border border-neutral-200 bg-neutral-50 hover:bg-neutral-100 transition-colors text-sm font-semibold text-neutral-900"
          >
            <span>Спортсмены ({{ sportsmen.length }})</span>
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" 
              class="w-4 h-4 text-neutral-600 transition-transform" :class="expandedSportsmen ? 'rotate-180' : ''">
              <path stroke-linecap="round" stroke-linejoin="round" d="M19 14l-7-7m0 0l-7 7m7-7v12"/>
            </svg>
          </button>

          <div v-if="expandedSportsmen" class="mt-2 space-y-3">
            <!-- Присутствовали -->
            <div v-if="presentSportsmen.length">
              <div class="text-xs font-semibold text-green-600 mb-2 px-2">✓ Присутствовали</div>
              <div class="space-y-1">
                <router-link
                  v-for="s in presentSportsmen"
                  :key="s.id"
                  :to="`/trainer/sportsman/${s.id}`"
                  class="flex items-center justify-between p-2 rounded-lg border border-green-200 bg-green-50 hover:bg-green-100 transition-colors text-xs"
                >
                  <div>
                    <div class="font-semibold text-neutral-800">{{ s.fio }}</div>
                    <div class="text-neutral-500 text-xs">{{ s.position ?? '—' }}</div>
                  </div>
                  <span class="px-2 py-0.5 rounded-full bg-green-100 text-green-700 text-xs font-semibold">Присутствовал</span>
                </router-link>
              </div>
            </div>

            <!-- Опоздали -->
            <div v-if="lateSportsmen.length">
              <div class="text-xs font-semibold text-yellow-600 mb-2 px-2">⚠ Опоздали</div>
              <div class="space-y-1">
                <router-link
                  v-for="s in lateSportsmen"
                  :key="s.id"
                  :to="`/trainer/sportsman/${s.id}`"
                  class="flex items-center justify-between p-2 rounded-lg border border-yellow-200 bg-yellow-50 hover:bg-yellow-100 transition-colors text-xs"
                >
                  <div>
                    <div class="font-semibold text-neutral-800">{{ s.fio }}</div>
                    <div class="text-neutral-500 text-xs">{{ s.position ?? '—' }}</div>
                  </div>
                  <span class="px-2 py-0.5 rounded-full bg-yellow-100 text-yellow-700 text-xs font-semibold">Опоздал</span>
                </router-link>
              </div>
            </div>

            <!-- Отсутствовали -->
            <div v-if="absentSportsmen.length">
              <div class="text-xs font-semibold text-red-600 mb-2 px-2">✗ Отсутствовали</div>
              <div class="space-y-1">
                <router-link
                  v-for="s in absentSportsmen"
                  :key="s.id"
                  :to="`/trainer/sportsman/${s.id}`"
                  class="flex items-center justify-between p-2 rounded-lg border border-red-200 bg-red-50 hover:bg-red-100 transition-colors text-xs"
                >
                  <div>
                    <div class="font-semibold text-neutral-800">{{ s.fio }}</div>
                    <div class="text-neutral-500 text-xs">{{ s.position ?? '—' }}</div>
                  </div>
                  <span class="px-2 py-0.5 rounded-full bg-red-100 text-red-700 text-xs font-semibold">Отсутствовал</span>
                </router-link>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Кнопка закрытия -->
      <div class="p-4 border-t border-neutral-100 flex-shrink-0">
        <button @click="$emit('close')" class="w-full px-4 py-2 rounded-xl border border-neutral-300 bg-neutral-100 text-neutral-600 hover:bg-neutral-200 transition-colors text-sm font-medium">
          Закрыть
        </button>
      </div>

    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { formatDate } from '@/utils/formatDate'
import api from '@/services/api'
import ModalHeader from './ModalHeader.vue'
import ModalCard from './ModalCard.vue'

const props = defineProps<{
  training: any
}>()

defineEmits<{
  close: []
}>()

const expandedSportsmen = ref(false)
const sportsmen = ref<any[]>([])
const attendance = ref<Record<number, string>>({})

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

onMounted(async () => {
  try {
    // Загружаем спортсменов группы
    if (props.training.groupId) {
      const res = await api.get(`/sportsman/group/${props.training.groupId}`).catch(() => null)
      sportsmen.value = res?.data?.data ?? []
    }

    // Загружаем посещаемость
    if (props.training.id) {
      const res = await api.get(`/schedule/training/${props.training.id}`).catch(() => null)
      const attendanceList = res?.data?.data ?? []
      attendance.value = {}
      attendanceList.forEach((a: any) => {
        attendance.value[a.sportsmanId] = a.status
      })
    }
  } catch (e) {
    console.error(e)
  }
})
</script>
