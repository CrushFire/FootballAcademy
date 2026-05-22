<template>
  <div class="p-3 overflow-y-auto h-full">
    <AppCard no-border>

      <!-- Заголовок -->
      <div class="flex items-center gap-3 mb-6 pb-4 border-b border-neutral-100">
        <button @click="router.back()" class="p-1.5 rounded-lg hover:bg-neutral-100 transition-colors">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/></svg>
        </button>
        <div>
          <div class="text-base font-bold text-neutral-900">{{ training?.type ?? 'Тренировка' }}</div>
          <div class="text-xs text-neutral-400">{{ formatDate(training?.date ?? '') }} · {{ training?.groupName }}</div>
        </div>
      </div>

      <div v-if="loading" class="text-sm text-neutral-400">Загрузка...</div>

      <div v-else-if="metrics" class="grid grid-cols-2 gap-4">
        <div v-for="group in metricGroups" :key="group.title" class="bg-white rounded-xl border border-neutral-200 p-4">
          <div class="text-xs font-bold text-neutral-400 uppercase tracking-wide mb-3">{{ group.title }}</div>
          <div class="flex flex-col gap-2">
            <div v-for="item in group.items" :key="item.label" class="flex justify-between text-sm">
              <span class="text-neutral-500 flex items-center gap-1">
                {{ item.label }}
                <AppTooltip :text="item.tip" />
              </span>
              <span class="font-semibold text-neutral-900">{{ item.value }}</span>
            </div>
          </div>
        </div>
      </div>

      <div v-else class="text-sm text-neutral-400">Нет данных о метриках</div>

    </AppCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/store/auth'
import api from '@/services/api'
import { formatDate } from '@/utils/formatDate'
import type { Training } from '@/types'
import AppCard from '@/components/ui/AppCard.vue'
import AppTooltip from '@/components/ui/AppTooltip.vue'
import { METRIC_TOOLTIPS as T } from '@/constants/metricTooltips'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const training = ref<Training | null>(null)
const metrics = ref<any>(null)
const loading = ref(true)

const metricGroups = computed(() => {
  const m = metrics.value
  if (!m) return []
  return [
    {
      title: 'Скорость',
      items: [
        { label: 'Средняя скорость',  value: `${(m.avgSpeed ?? 0).toFixed(1)} км/ч`,          tip: T.avgSpeed },
        { label: 'Макс. скорость',    value: `${(m.maxSpeed ?? 0).toFixed(1)} км/ч`,          tip: T.maxSpeed },
        { label: 'Доля спринта',      value: `${((m.sprintRatio ?? 0) * 100).toFixed(1)}%`,  tip: T.sprintRatio },
        { label: 'Доля выс. скорости',value: `${((m.highSpeedRatio ?? 0) * 100).toFixed(1)}%`,tip: T.highSpeedRatio },
        { label: 'Спринт / мин',      value: (m.sprintEffortsPerMin ?? 0).toFixed(2),        tip: T.sprintEffortsPerMin },
      ]
    },
    {
      title: 'Нагрузка',
      items: [
        { label: 'Нагрузка игрока', value: (m.playerLoad ?? 0).toFixed(1),     tip: T.playerLoad },
        { label: 'Метаб. мощность', value: (m.metabolicPower ?? 0).toFixed(2), tip: T.metabolicPower },
        { label: 'Взрывной индекс', value: (m.explosiveIndex ?? 0).toFixed(2), tip: T.explosiveIndex },
      ]
    },
    {
      title: 'Кардио',
      items: [
        { label: 'Эфф. кардио',           value: (m.cardioEfficiency ?? 0).toFixed(2),                              tip: T.cardioEfficiency },
        { label: 'Стаб. ЧСС',             value: (m.hRStability ?? m.hrStability ?? 0).toFixed(2),                 tip: T.hrStability },
        { label: 'Красная зона',          value: `${((m.hRRedPercent ?? m.hrRedPercent ?? 0) * 100).toFixed(1)}%`, tip: T.hrRedPercent },
        { label: 'Индекс восстановления', value: (m.recoveryIndex ?? 0).toFixed(2),                                tip: T.recoveryIndex },
      ]
    },
    {
      title: 'Усталость',
      items: [
        { label: 'Индекс усталости',  value: (m.fatigueIndex ?? 0).toFixed(2),                         tip: T.fatigueIndex },
        { label: 'Аэробная нагрузка', value: `${((m.aerobicLoad ?? 0) * 100).toFixed(1)}%`,            tip: T.aerobicLoad },
        { label: 'Анаэробная доля',   value: `${((m.anaerobicRatio ?? 0) * 100).toFixed(1)}%`,         tip: T.anaerobicRatio },
        { label: 'Рабочий коэф.',     value: (m.workRatio ?? 0).toFixed(2),                            tip: T.workRatio },
        { label: 'HI/LO',             value: (m.hI_LO_Ratio ?? m.hiLoRatio ?? 0).toFixed(2),           tip: T.hiLoRatio },
      ]
    },
    {
      title: 'Эффективность',
      items: [
        { label: 'Энерго эфф.',  value: (m.energyEfficiency ?? 0).toFixed(2), tip: T.energyEfficiency },
        { label: 'Стабильность', value: (m.consistency ?? 0).toFixed(2),      tip: T.consistency },
        { label: 'RSA',          value: (m.rSA ?? m.RSA ?? 0).toFixed(2),     tip: T.rsa },
      ]
    },
  ]
})

onMounted(async () => {
  const trainingId = Number(route.params.id)

  const meRes = await api.get('/sportsman/me').catch(() => null)
  const sportsmanId = meRes?.data?.data?.id ?? auth.userId!

  const [tRes, mRes] = await Promise.allSettled([
    api.get(`/training/${trainingId}`),
    api.get(`/analytic/${sportsmanId}/metrics/${trainingId}`)
  ])

  if (tRes.status === 'fulfilled') training.value = tRes.value.data.data
  if (mRes.status === 'fulfilled') {
    const d = mRes.value.data.data
    metrics.value = d?.metrics ?? d?.Metrics ?? d ?? null
  }

  loading.value = false
})
</script>
