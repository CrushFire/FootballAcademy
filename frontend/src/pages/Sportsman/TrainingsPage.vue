<template>
  <div class="p-3 h-full">
    <AppCard no-border class="h-full flex flex-col">
      <!-- Шапка страницы -->
      <div class="-mx-4 -mt-4 px-5 py-4 mb-5 rounded-t-2xl border-b border-neutral-100">
        <h1 class="text-xl font-bold text-neutral-900">Тренировки</h1>
        <p class="text-sm text-neutral-500 mt-0.5">Графики тренировок и список занятий</p>
      </div>
      <!-- Мобила: вертикальный стек (график сверху, тренировки снизу).
           Десктоп: 5-колоночная сетка (график 3 + тренировки 2). -->
      <div class="flex flex-col md:grid md:grid-cols-5 gap-4 flex-1 min-h-0 overflow-y-auto md:overflow-visible">

        <!-- График (3/5 на десктопе, 100% на мобиле) -->
        <div class="md:col-span-3 flex flex-col flex-shrink-0">
          <!-- Заголовок -->
          <div class="mb-3 px-1 flex flex-col md:flex-row md:items-end md:justify-between gap-2">
            <div>
              <div class="text-base font-bold text-neutral-400">График тренировок</div>
              <div class="text-xs text-neutral-500 mt-0.5">Выбери категорию и параметры для анализа динамики роста</div>
            </div>
            <div class="flex items-center gap-2 md:gap-3 text-xs text-neutral-400 flex-wrap md:justify-end">
              <span class="flex items-center gap-1"><span class="w-2 h-2 rounded-full bg-blue-400"></span>Скорость</span>
              <span class="flex items-center gap-1"><span class="w-2 h-2 rounded-full bg-amber-400"></span>Нагрузка</span>
              <span class="flex items-center gap-1"><span class="w-2 h-2 rounded-full bg-red-400"></span>Кардио</span>
              <span class="flex items-center gap-1"><span class="w-2 h-2 rounded-full bg-violet-400"></span>Усталость</span>
              <span class="flex items-center gap-1"><span class="w-2 h-2 rounded-full bg-green-400"></span>Эффективность</span>
            </div>
          </div>
          <!-- График прибит к низу на десктопе; на мобиле фиксированная высота. -->
          <div class="flex-1 flex items-end h-56 md:h-auto">
            <div class="w-full">
              <TrainingMetricsChart :points="graphPoints" />
            </div>
          </div>
        </div>

        <!-- Список тренировок (2/5 на десктопе, 100% на мобиле) -->
       <div class="md:col-span-2 flex flex-col gap-1.5 min-h-0 flex-shrink-0" ref="listRef">
          <div class="text-xs font-bold text-neutral-400 uppercase tracking-wide mb-1 flex-shrink-0">Тренировки</div>

          <div v-if="loading" class="text-sm text-neutral-400">Загрузка...</div>

          <div class="flex flex-col gap-1.5 flex-1 overflow-y-auto">
            <div
              v-for="t in pagedTrainings" :key="t.id"
              @click="goDetail(t.id)"
              class="flex flex-col gap-0.5 p-2.5 rounded-xl border border-neutral-200 cursor-pointer hover:bg-neutral-50 transition-colors flex-shrink-0"
            >
              <div class="text-xs font-semibold text-neutral-800">{{ t.type }}</div>
              <div class="text-xs text-neutral-400">{{ formatDate(t.date) }}</div>
              <div class="text-xs text-neutral-500 truncate">{{ t.groupName }}</div>
            </div>
          </div>

          <div v-if="!loading && pagedTrainings.length === 0" class="text-sm text-neutral-400">Нет тренировок</div>

          <!-- Пагинация — всегда видна -->
          <div class="flex items-center justify-between pt-2 flex-shrink-0">
            <button @click="page > 1 && page--" :disabled="page === 1" class="p-1.5 rounded-lg hover:bg-neutral-100 disabled:opacity-30 transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-600"><path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/></svg>
            </button>
            <span class="text-xs text-neutral-500">{{ page }} / {{ totalPages }}</span>
            <button @click="page < totalPages && page++" :disabled="page === totalPages" class="p-1.5 rounded-lg hover:bg-neutral-100 disabled:opacity-30 transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-600"><path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/></svg>
            </button>
          </div>
        </div>

      </div>
    </AppCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/store/auth'
import api from '@/services/api'
import { formatDate } from '@/utils/formatDate'
import type { Training } from '@/types'
import AppCard from '@/components/ui/AppCard.vue'
import TrainingMetricsChart from '@/components/ui/TrainingMetricsChart.vue'

const auth = useAuthStore()
const router = useRouter()
const route = useRoute()
const listRef = ref<HTMLElement | null>(null)

const trainings = ref<Training[]>([])
const graphPoints = ref<{ date: string; type?: string; matchType?: string; metrics: Record<string, number> }[]>([])
const loading = ref(true)
const page = ref(1)
const pageSize = ref(7)

function calcPageSize() {
  if (!listRef.value) return
  const h = listRef.value.clientHeight
  pageSize.value = Math.max(3, Math.floor((h - 60) / 62))
}

const sorted = computed(() =>
  [...trainings.value].sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
)
const pagedTrainings = computed(() =>
  sorted.value.slice((page.value - 1) * pageSize.value, page.value * pageSize.value)
)
const totalPages = computed(() => Math.ceil(trainings.value.length / pageSize.value) || 1)

function goDetail(id: number) {
  router.push(`/trainings/${id}`)
}

const ro = new ResizeObserver(() => calcPageSize())

onMounted(async () => {
  const routeId = route.params.id ? Number(route.params.id) : null
  let sportsmanId: number
  if (routeId) {
    sportsmanId = routeId
  } else {
    const meRes = await api.get('/sportsman/me').catch(() => null)
    sportsmanId = meRes?.data?.data?.id ?? auth.userId!
  }

  const [trainRes, graphRes] = await Promise.allSettled([
    api.get(`/training/sportsman/${sportsmanId}`),
    api.get(`/analytic/${sportsmanId}/graph`)
  ])
  if (trainRes.status === 'fulfilled') trainings.value = trainRes.value.data.data ?? []
  if (graphRes.status === 'fulfilled') {
    const raw = graphRes.value.data?.data ?? graphRes.value.data ?? []
    graphPoints.value = Array.isArray(raw) ? raw.map((p: any) => ({ date: p.date, type: p.type, matchType: p.matchType, metrics: p.metrics ?? {} })) : []
  }
  loading.value = false
  await nextTick()
  calcPageSize()
  if (listRef.value) ro.observe(listRef.value)
})

onUnmounted(() => ro.disconnect())
</script>
