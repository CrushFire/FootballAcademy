<template>
  <div class="p-3 h-full">
    <MedicalPageCard color="emerald" class="h-full flex flex-col overflow-hidden">

      <!-- Topbar: только заголовок -->
      <div class="flex items-center justify-between mb-4 flex-shrink-0">
        <div>
          <h2 class="text-base font-bold text-neutral-800">Медицинский мониторинг</h2>
          <p class="text-xs text-neutral-400 mt-0.5">Анализ состояния игроков</p>
        </div>
      </div>

      <!-- Loading -->
      <div v-if="loading" class="flex-1 flex items-center justify-center">
        <div class="w-6 h-6 border-2 border-emerald-400 border-t-transparent rounded-full animate-spin" />
      </div>

      <template v-else>
        <!-- Alert banner -->
        <div v-if="problemItems.length > 0"
          class="flex items-center gap-3 px-4 py-3 rounded-2xl bg-amber-50 border border-amber-200 mb-4 flex-shrink-0">
          <div class="w-8 h-8 rounded-xl bg-amber-100 flex items-center justify-center flex-shrink-0">
            <svg class="w-4 h-4 text-amber-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m-9.303 3.376c-.866 1.5.217 3.374 1.948 3.374h14.71c1.73 0 2.813-1.874 1.948-3.374L13.949 3.378c-.866-1.5-3.032-1.5-3.898 0L2.697 16.126ZM12 15.75h.007v.008H12v-.008Z"/>
            </svg>
          </div>
          <div class="text-sm font-medium text-amber-800">
            Обнаружены отклонения от нормы у <span class="font-bold">{{ problemItems.length }}</span> спортсменов
          </div>
        </div>

        <!-- Alert cards slider: 3 видны, стрелки -->
        <div v-if="problemItems.length > 0" class="mb-4 flex-shrink-0">
          <div class="flex items-center gap-3">
            <button
              :disabled="cardOffset === 0"
              @click="cardOffset = Math.max(0, cardOffset - 3)"
              class="w-7 h-7 rounded-xl border border-neutral-200 bg-white flex items-center justify-center text-neutral-400 hover:border-emerald-400 hover:text-emerald-500 disabled:opacity-20 transition-colors flex-shrink-0"
            >
              <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15.75 19.5 8.25 12l7.5-7.5" />
              </svg>
            </button>

            <div class="grid grid-cols-3 gap-3 flex-1">
              <div
                v-for="item in problemItems.slice(cardOffset, cardOffset + 3)"
                :key="item.sportsmanId"
                class="rounded-xl border border-neutral-200 bg-white p-3.5 cursor-pointer hover:shadow-sm transition-all"
                :class="isCriticalItem(item) ? 'hover:border-red-200' : 'hover:border-amber-200'"
                @click="openModal(item)"
              >
                <div class="flex items-center gap-2 mb-2.5">
                  <div class="w-7 h-7 rounded-lg flex items-center justify-center text-xs font-bold flex-shrink-0"
                    :class="isCriticalItem(item) ? 'bg-red-100 text-red-600' : 'bg-amber-100 text-amber-600'">
                    {{ initials(item.sportsmanName) }}
                  </div>
                  <div class="min-w-0">
                    <div class="text-xs font-semibold text-neutral-800 truncate">{{ item.sportsmanName }}</div>
                  </div>
                </div>
                <span class="text-[10px] font-medium px-2 py-0.5 rounded-full mb-2.5 inline-block" :class="overallStatusTag(item)">
                  {{ overallStatusLabel(item) }}
                </span>
                <div v-if="item.checkResult.issues?.length" class="mt-1">
                  <div class="text-[10px] text-neutral-400 mb-1">Основная проблема</div>
                  <div class="text-[10px] text-neutral-700 font-medium leading-snug line-clamp-2">{{ item.checkResult.issues[0] }}</div>
                </div>
                <button
                  class="mt-3 w-full border-none rounded-lg py-1.5 text-[10px] font-medium transition-colors cursor-pointer"
                  :class="isCriticalItem(item) ? 'bg-red-50 text-red-600 hover:bg-red-100' : 'bg-amber-50 text-amber-700 hover:bg-amber-100'"
                >Подробнее →</button>
              </div>
              <!-- Плейсхолдеры -->
              <div
                v-for="n in (3 - problemItems.slice(cardOffset, cardOffset + 3).length)"
                :key="'ph' + n"
                class="rounded-xl border border-dashed border-neutral-200 bg-neutral-50/50"
              />
            </div>

            <button
              :disabled="cardOffset + 3 >= problemItems.length"
              @click="cardOffset = Math.min(problemItems.length - 1, cardOffset + 3)"
              class="w-7 h-7 rounded-xl border border-neutral-200 bg-white flex items-center justify-center text-neutral-400 hover:border-emerald-400 hover:text-emerald-500 disabled:opacity-20 transition-colors flex-shrink-0"
            >
              <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                <path stroke-linecap="round" stroke-linejoin="round" d="m8.25 4.5 7.5 7.5-7.5 7.5" />
              </svg>
            </button>
          </div>

          <!-- Кружочки под слайдером -->
          <div v-if="problemItems.length > 3" class="flex justify-center gap-1.5 mt-2">
            <button
              v-for="(_, i) in Math.ceil(problemItems.length / 3)"
              :key="i"
              @click="cardOffset = i * 3"
              class="rounded-full transition-all"
              :class="Math.floor(cardOffset / 3) === i ? 'w-4 h-2 bg-emerald-500' : 'w-2 h-2 bg-neutral-300 hover:bg-neutral-400'"
            />
          </div>
        </div>

        <!-- Table -->
        <div class="flex-1 overflow-auto min-h-0 flex flex-col">
          <div class="rounded-xl border border-neutral-200 bg-white overflow-hidden flex flex-col flex-1 min-h-0">

            <!-- Поиск и фильтр внутри таблицы -->
            <div class="px-4 py-2.5 border-b border-neutral-100 flex items-center justify-between gap-3 flex-shrink-0">
              <div class="text-xs font-semibold text-neutral-700">Все спортсмены</div>
              <div class="flex items-center gap-2">
                <select v-model="filterStatus" @change="page = 1" class="text-xs rounded-lg border border-neutral-200 bg-neutral-50 px-2.5 py-1.5 focus:outline-none focus:border-emerald-400 text-neutral-600">
                  <option value="">Все</option>
                  <option value="issues">Есть проблемы</option>
                  <option value="risks">Есть риски</option>
                  <option value="ok">Норма</option>
                </select>
                <div class="relative">
                  <input
                    v-model="search"
                    @input="page = 1"
                    type="text"
                    placeholder="Поиск по имени..."
                    class="text-xs rounded-lg border border-neutral-200 bg-neutral-50 pl-7 pr-3 py-1.5 focus:outline-none focus:border-emerald-400 text-neutral-600 w-44"
                  />
                  <svg class="w-3.5 h-3.5 text-neutral-400 absolute left-2 top-1/2 -translate-y-1/2" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                    <path stroke-linecap="round" stroke-linejoin="round" d="m21 21-5.197-5.197m0 0A7.5 7.5 0 1 0 5.196 5.196a7.5 7.5 0 0 0 10.607 10.607Z" />
                  </svg>
                </div>
              </div>
            </div>

            <div class="overflow-auto flex-1">
              <table class="w-full text-xs border-collapse">
                <thead>
                  <tr class="bg-neutral-50 border-b border-neutral-100">
                    <th class="text-left px-3 py-2.5 text-[11px] font-medium text-neutral-500 whitespace-nowrap">ФИО</th>
                    <th class="text-left px-3 py-2.5 text-[11px] font-medium text-neutral-500 whitespace-nowrap">Позиция</th>
                    <th class="text-center px-3 py-2.5 text-[11px] font-medium text-neutral-500 whitespace-nowrap">
                      <div class="inline-flex items-center gap-1 justify-center">
                        ЧСС макс.
                        <AppTooltip :text="MEDICAL_CHECK_TOOLTIPS.maxHeartRate" />
                      </div>
                      <span class="block text-[10px] font-normal text-neutral-400">уд/мин</span>
                    </th>
                    <th class="text-center px-3 py-2.5 text-[11px] font-medium text-neutral-500 whitespace-nowrap">
                      <div class="inline-flex items-center gap-1 justify-center">
                        Нагрузка
                        <AppTooltip :text="MEDICAL_CHECK_TOOLTIPS.playerLoad" />
                      </div>
                      <span class="block text-[10px] font-normal text-neutral-400">PlayerLoad</span>
                    </th>
                    <th class="text-center px-3 py-2.5 text-[11px] font-medium text-neutral-500 whitespace-nowrap">
                      <div class="inline-flex items-center gap-1 justify-center">
                        Восстановление
                        <AppTooltip :text="MEDICAL_CHECK_TOOLTIPS.timeInLowIntensity" />
                      </div>
                      <span class="block text-[10px] font-normal text-neutral-400">мин в зоне 1</span>
                    </th>
                    <th class="text-center px-3 py-2.5 text-[11px] font-medium text-neutral-500 whitespace-nowrap">
                      <div class="inline-flex items-center gap-1 justify-center">
                        Риск травм
                        <AppTooltip :text="MEDICAL_CHECK_TOOLTIPS.acuteChronicRatio" />
                      </div>
                      <span class="block text-[10px] font-normal text-neutral-400">О/Х нагрузка</span>
                    </th>
                    <th class="text-center px-3 py-2.5 text-[11px] font-medium text-neutral-500 whitespace-nowrap">
                      <div class="inline-flex items-center gap-1 justify-center">
                        Усталость
                        <AppTooltip :text="MEDICAL_CHECK_TOOLTIPS.fatigueIndex" />
                      </div>
                      <span class="block text-[10px] font-normal text-neutral-400">FatigueIndex</span>
                    </th>
                    <th class="text-center px-3 py-2.5 text-[11px] font-medium text-neutral-500 whitespace-nowrap">
                      <div class="inline-flex items-center gap-1 justify-center">
                        Стабильность
                        <AppTooltip :text="MEDICAL_CHECK_TOOLTIPS.consistency" />
                      </div>
                      <span class="block text-[10px] font-normal text-neutral-400">std нагрузки</span>
                    </th>
                    <th class="text-center px-3 py-2.5 text-[11px] font-medium text-neutral-500 whitespace-nowrap">Статус</th>
                    <th class="px-3 py-2.5" />
                  </tr>
                </thead>
                <tbody>
                  <tr v-if="paged.length === 0">
                    <td colspan="10" class="text-center py-10 text-neutral-400">Нет данных</td>
                  </tr>
                  <tr
                    v-for="item in paged"
                    :key="item.sportsmanId"
                    class="border-b border-neutral-50 hover:bg-neutral-50 transition-colors cursor-pointer"
                    @click="openModal(item)"
                  >
                    <td class="px-3 py-2.5">
                      <div class="flex items-center gap-2">
                        <div class="w-6 h-6 rounded-lg flex items-center justify-center text-[10px] font-bold flex-shrink-0"
                          :class="item.isHealthy ? 'bg-green-100 text-green-600' : 'bg-red-100 text-red-600'">
                          {{ initials(item.sportsmanName) }}
                        </div>
                        <span class="font-semibold text-neutral-800 whitespace-nowrap text-xs">{{ item.sportsmanName }}</span>
                      </div>
                    </td>
                    <td class="px-3 py-2.5 text-neutral-500 text-[11px] whitespace-nowrap">{{ positionLabel(item.position) }}</td>

                    <!-- ЧСС макс. (CardiovascularOk: MaxHeartRate < threshold) -->
                    <td class="px-3 py-2.5 text-center">
                      <MetricValue
                        :value="item.metrics?.maxHeartRate"
                        :status="item.checkResult?.cardiovascularOk === false ? 'critical' : 'ok'"
                        suffix=""
                      />
                    </td>

                    <!-- Нагрузка (LoadOk: PlayerLoad < 500) -->
                    <td class="px-3 py-2.5 text-center">
                      <MetricValue
                        :value="item.metrics?.playerLoad != null ? +item.metrics.playerLoad.toFixed(1) : undefined"
                        :status="playerLoadStatus(item.metrics?.playerLoad, item.checkResult?.loadOk)"
                        suffix=""
                      />
                    </td>

                    <!-- Восстановление (RecoveryOk: TimeInSpeedZone1 > 300) -->
                    <td class="px-3 py-2.5 text-center">
                      <MetricValue
                        :value="item.metrics?.timeInLowIntensity != null ? +(item.metrics.timeInLowIntensity / 60).toFixed(1) : undefined"
                        :status="recoveryStatus(item.metrics?.timeInLowIntensity, item.checkResult?.recoveryOk)"
                        suffix=" мин"
                      />
                    </td>

                    <!-- Риск травм (InjuryRiskOk: acuteChronicRatio < 1.3) -->
                    <td class="px-3 py-2.5 text-center">
                      <MetricValue
                        :value="item.metrics?.acuteChronicRatio != null ? +item.metrics.acuteChronicRatio.toFixed(2) : undefined"
                        :status="acuteChronicStatus(item.metrics?.acuteChronicRatio, item.checkResult?.injuryRiskOk)"
                        suffix=""
                      />
                    </td>

                    <!-- Усталость (FatigueOk: fatigueIndex < 1.2) -->
                    <td class="px-3 py-2.5 text-center">
                      <MetricValue
                        :value="item.metrics?.fatigueIndex != null ? +item.metrics.fatigueIndex.toFixed(2) : undefined"
                        :status="fatigueStatus(item.metrics?.fatigueIndex, item.checkResult?.fatigueOk)"
                        suffix=""
                      />
                    </td>

                    <!-- Стабильность (ConsistencyOk: consistency <= 20) -->
                    <td class="px-3 py-2.5 text-center">
                      <MetricValue
                        :value="item.metrics?.consistency != null ? +item.metrics.consistency.toFixed(1) : undefined"
                        :status="consistencyStatus(item.metrics?.consistency, item.checkResult?.consistencyOk)"
                        suffix=""
                      />
                    </td>

                    <td class="px-3 py-2.5 text-center">
                      <span class="inline-flex items-center gap-1.5">
                        <span class="w-2.5 h-2.5 rounded-full flex-shrink-0" :class="overallDotClass(item)" />
                        <span class="text-[10px] font-medium" :class="overallTextClass(item)">
                          {{ item.isHealthy ? 'Норма' : 'Проблемы' }}
                        </span>
                      </span>
                    </td>
                    <td class="px-3 py-2.5 text-center">
                      <button class="bg-emerald-50 text-emerald-600 border-none rounded-lg px-2.5 py-1 text-[11px] font-medium hover:bg-emerald-100 transition-colors cursor-pointer whitespace-nowrap">
                        Детали →
                      </button>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- Нижняя панель: легенда + пагинация -->
            <div class="flex items-center justify-between px-4 py-2.5 bg-neutral-50 border-t border-neutral-100 flex-shrink-0 gap-4">
              <!-- Легенда -->
              <div class="flex items-center gap-4 flex-wrap">
                <div v-for="l in LEGEND" :key="l.label" class="flex items-center gap-1.5">
                  <span class="w-2.5 h-2.5 rounded-full flex-shrink-0" :class="l.dot" />
                  <span class="text-[11px] text-neutral-500">{{ l.label }}</span>
                </div>
              </div>

              <!-- Пагинация: стрелки + кружки -->
              <div class="flex items-center gap-2 flex-shrink-0">
                <span class="text-[11px] text-neutral-400 mr-1">{{ filtered.length }} записей</span>
                <button
                  :disabled="page <= 1"
                  @click="page--"
                  class="p-1.5 rounded-lg hover:bg-neutral-100 disabled:opacity-30 transition-colors"
                >
                  <svg class="w-4 h-4 text-neutral-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7" />
                  </svg>
                </button>
                <div class="flex items-center gap-1">
                  <button
                    v-for="p in pageNumbers"
                    :key="p"
                    @click="typeof p === 'number' && (page = p)"
                    class="w-6 h-6 rounded-full flex items-center justify-center transition-all text-[11px] font-medium"
                    :class="p === page
                      ? 'bg-emerald-500 text-white font-semibold'
                      : p === '…'
                        ? 'text-neutral-400 cursor-default'
                        : 'bg-neutral-100 hover:bg-neutral-200 text-neutral-600'"
                  >{{ p }}</button>
                </div>
                <button
                  :disabled="page >= totalPages"
                  @click="page++"
                  class="p-1.5 rounded-lg hover:bg-neutral-100 disabled:opacity-30 transition-colors"
                >
                  <svg class="w-4 h-4 text-neutral-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </div>
      </template>
    </MedicalPageCard>

    <!-- Detail Modal -->
    <Teleport to="body">
      <Transition name="modal">
        <div v-if="modalItem" class="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div class="absolute inset-0 bg-neutral-900/35" @click="modalItem = null" />
          <div class="relative bg-white rounded-2xl shadow-2xl w-full max-w-md max-h-[85vh] overflow-y-auto z-10 border border-neutral-200">
            <div class="flex items-start justify-between p-5 border-b border-neutral-100">
              <div>
                <div class="text-base font-semibold text-neutral-900">{{ modalItem.sportsmanName }}</div>
                <div class="text-xs text-neutral-400 mt-0.5">{{ positionLabel(modalItem.position) }}</div>
              </div>
              <button @click="modalItem = null" class="w-8 h-8 rounded-lg bg-neutral-100 flex items-center justify-center hover:bg-neutral-200 transition-colors flex-shrink-0">
                <svg class="w-4 h-4 text-neutral-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M6 18 18 6M6 6l12 12" />
                </svg>
              </button>
            </div>

            <div class="p-5">
              <!-- 5 metric cards matching the 5 check categories -->
              <div class="grid grid-cols-2 gap-2.5 mb-4">
                <div v-for="m in modalMetricCards" :key="m.label" class="bg-neutral-50 rounded-xl p-3 border border-neutral-100">
                  <div class="text-[11px] text-neutral-400 mb-1.5">{{ m.label }}</div>
                  <div class="text-lg font-bold" :class="m.textClass">{{ m.displayValue }}</div>
                  <div class="text-[10px] text-neutral-400 mt-0.5">{{ m.hint }}</div>
                </div>
              </div>

              <div v-if="modalItem.checkResult.issues?.length || modalWarnings.length">
                <div class="text-xs font-semibold text-neutral-600 mb-2">Выявленные проблемы</div>
                <div class="flex flex-col gap-1.5">
                  <div v-for="(issue, i) in modalItem.checkResult.issues" :key="'i'+i"
                    class="flex items-start gap-2 p-2.5 rounded-xl bg-red-50 border border-red-100">
                    <div class="w-1.5 h-1.5 rounded-full bg-red-500 mt-1.5 flex-shrink-0" />
                    <span class="text-xs text-neutral-800 font-medium">{{ issue }}</span>
                  </div>
                  <div v-for="(warn, i) in modalWarnings" :key="'w'+i"
                    class="flex items-start gap-2 p-2.5 rounded-xl bg-yellow-50 border border-yellow-100">
                    <div class="w-1.5 h-1.5 rounded-full bg-yellow-400 mt-1.5 flex-shrink-0" />
                    <span class="text-xs text-neutral-700 font-medium">{{ warn }}</span>
                  </div>
                </div>
              </div>
              <div v-else class="text-xs text-green-600 font-medium text-center py-3">Все показатели в норме</div>

              <div class="flex items-center gap-2 mt-4 pt-4 border-t border-neutral-100">
                <span class="text-xs text-neutral-500">Статус:</span>
                <span class="w-2.5 h-2.5 rounded-full flex-shrink-0" :class="overallDotClass(modalItem)" />
                <span class="text-xs font-medium" :class="overallTextClass(modalItem)">
                  {{ modalItem.isHealthy ? 'Норма' : 'Требует внимания' }}
                </span>
              </div>
            </div>

            <div class="px-5 pb-5">
              <RouterLink
                :to="`/medical/sportsman/${modalItem.sportsmanId}`"
                class="flex items-center justify-center gap-2 w-full py-2.5 rounded-xl bg-emerald-50 border border-emerald-100 text-emerald-600 text-xs font-semibold hover:bg-emerald-100 transition-colors no-underline"
                @click="modalItem = null"
              >
                Перейти к профилю игрока
                <svg class="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5">
                  <path stroke-linecap="round" stroke-linejoin="round" d="m8.25 4.5 7.5 7.5-7.5 7.5" />
                </svg>
              </RouterLink>
            </div>
          </div>
        </div>
      </Transition>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, defineComponent, h } from 'vue'
import { RouterLink } from 'vue-router'
import api from '@/services/api'
import MedicalPageCard from '@/components/medical/MedicalPageCard.vue'
import AppTooltip from '@/components/ui/AppTooltip.vue'
import { MEDICAL_CHECK_TOOLTIPS } from '@/constants/metricTooltips'

// ok — норма, warning — близко к порогу (<10% запаса), caution — нарушено, critical — нарушено >+25%
type MetricStatus = 'ok' | 'warning' | 'caution' | 'critical' | 'unknown'

const LEGEND = [
  { label: 'Критично',              dot: 'bg-red-500'    },
  { label: 'Нарушено',              dot: 'bg-orange-500' },
  { label: 'Близко к критическому', dot: 'bg-yellow-400' },
  { label: 'Норма',                 dot: 'bg-green-500'  },
]

// ─── Статусы по категориям checkResult (бэкенд уже всё проверил) ─────────────
// ok=false → caution/critical в зависимости от степени превышения

// Для "выше — хуже": caution = нарушено, critical = нарушено >25% сверх порога
function aboveThreshold(val: number, limit: number): MetricStatus {
  if (val >= limit * 1.25) return 'critical'
  if (val >= limit)        return 'caution'
  if (val >= limit * 0.9)  return 'warning'
  return 'ok'
}

// Для "ниже — хуже": caution = нарушено, critical = ниже 75% от порога
function belowThreshold(val: number, limit: number): MetricStatus {
  if (val <= limit * 0.75) return 'critical'
  if (val <= limit)        return 'caution'
  if (val <= limit * 1.1)  return 'warning'
  return 'ok'
}

// Нагрузка — бэкенд: LoadOk = PlayerLoad < 500 && acuteChronicRatio < 1.5
function playerLoadStatus(val: number | null | undefined, loadOk: boolean | undefined): MetricStatus {
  if (val == null) return 'unknown'
  if (loadOk === false) return aboveThreshold(val, 500)
  // В норме — предупреждение если близко к 450 (injuryRisk порог)
  if (val != null && val >= 405) return 'warning'  // 90% от 450
  return 'ok'
}

// Восстановление — бэкенд: RecoveryOk = TimeInSpeedZone1 > 300 (секунды)
function recoveryStatus(val: number | null | undefined, recoveryOk: boolean | undefined): MetricStatus {
  if (val == null) return 'unknown'
  if (recoveryOk === false) return belowThreshold(val, 300)
  if (val <= 330) return 'warning'  // 110% от 300
  return 'ok'
}

// Риск травм — бэкенд: InjuryRiskOk = PlayerLoad < 450 && accels < 200 && acuteChronicRatio < 1.3
function acuteChronicStatus(val: number | null | undefined, injuryRiskOk: boolean | undefined): MetricStatus {
  if (val == null) return 'unknown'
  if (injuryRiskOk === false) return aboveThreshold(val, 1.3)
  if (val >= 1.17) return 'warning'  // 90% от 1.3
  return 'ok'
}

// Усталость — бэкенд: FatigueOk = TimeInHRRedZone < 600 && fatigueIndex < 1.2
function fatigueStatus(val: number | null | undefined, fatigueOk: boolean | undefined): MetricStatus {
  if (val == null) return 'unknown'
  if (fatigueOk === false) return aboveThreshold(val, 1.2)
  if (val >= 1.08) return 'warning'  // 90% от 1.2
  return 'ok'
}

// Стабильность — бэкенд: ConsistencyOk = consistency <= 20
function consistencyStatus(val: number | null | undefined, consistencyOk: boolean | undefined): MetricStatus {
  if (val == null) return 'unknown'
  if (consistencyOk === false) return aboveThreshold(val, 20)
  if (val >= 18) return 'warning'  // 90% от 20
  return 'ok'
}

function statusTextClass(s: MetricStatus): string {
  return s === 'critical' ? 'text-red-600 font-bold'
       : s === 'caution'  ? 'text-orange-500 font-semibold'
       : s === 'warning'  ? 'text-yellow-500 font-medium'
       : s === 'ok'       ? 'text-green-700 font-medium'
       :                    'text-neutral-400'
}

// ─── Состояние ────────────────────────────────────────────────────────────────
const PER_PAGE = 6

const loading      = ref(true)
const allItems     = ref<any[]>([])
const search       = ref('')
const filterStatus = ref('')
const page         = ref(1)
const cardOffset   = ref(0)
const modalItem    = ref<any>(null)

const POSITION_LABEL: Record<string, string> = {
  GK: 'GK (Вратарь)', CB: 'CB (Центральный защитник)', LB: 'LB (Левый защитник)',
  RB: 'RB (Правый защитник)', LWB: 'LWB (Левый латераль)', RWB: 'RWB (Правый латераль)',
  CDM: 'CDM (Опорный полузащитник)', CM: 'CM (Центральный полузащитник)',
  CAM: 'CAM (Атакующий полузащитник)', LW: 'LW (Левый вингер)', RW: 'RW (Правый вингер)',
  ST: 'ST (Нападающий)', CF: 'CF (Центральный нападающий)', SS: 'SS (Второй нападающий)'
}

function initials(fio: string) {
  return (fio ?? '').split(' ').slice(0, 2).map((w: string) => w[0]).join('').toUpperCase()
}
function positionLabel(pos: string) { return POSITION_LABEL[pos] ?? pos ?? '—' }

function overallDotClass(item: any): string {
  if (item.isHealthy) return 'bg-green-500'
  return isCriticalItem(item) ? 'bg-red-500' : 'bg-orange-400'
}
function overallTextClass(item: any): string {
  if (item.isHealthy) return 'text-green-700'
  return isCriticalItem(item) ? 'text-red-600' : 'text-orange-500'
}
// Критично если хотя бы один показатель превышает порог более чем на 25%
function isCriticalItem(item: any): boolean {
  if (item.isHealthy) return false
  const m  = item.metrics
  const cr = item.checkResult ?? {}
  // ЧСС: нарушена категория — всегда критично
  if (!cr.cardiovascularOk) return true
  // Нагрузка: LoadOk нарушен (>= 500) — всегда критично
  if (!cr.loadOk) return true
  if (m) {
    // PlayerLoad > 562 (450 * 1.25)
    if (m.playerLoad != null && m.playerLoad >= 562) return true
    // AcuteChronicRatio > 1.625 (1.3 * 1.25)
    if (m.acuteChronicRatio != null && m.acuteChronicRatio >= 1.625) return true
    // FatigueIndex > 1.5 (1.2 * 1.25)
    if (m.fatigueIndex != null && m.fatigueIndex >= 1.5) return true
    // Восстановление < 225с (300 * 0.75)
    if (m.timeInLowIntensity != null && m.timeInLowIntensity <= 225) return true
    // Consistency > 25 (20 * 1.25) — бэкенд теперь даёт consistencyOk
    if (m.consistency != null && m.consistency > 25) return true
  }
  // 2+ нарушения — красный
  if ((cr.issues?.length ?? 0) >= 2) return true
  // ConsistencyOk = false — проверяем степень: если нарушено сильно (> 25) уже выше
  // Если нарушено умеренно (20–25) — оранжевый, не красный
  return false
}
function overallStatusTag(item: any): string {
  if (item.isHealthy) return 'bg-green-100 text-green-700'
  return isCriticalItem(item) ? 'bg-red-100 text-red-700' : 'bg-amber-100 text-amber-700'
}
function overallStatusLabel(item: any): string {
  if (item.isHealthy) return 'Норма'
  return isCriticalItem(item) ? 'Критично' : 'Предупреждение'
}

const problemItems = computed(() => allItems.value.filter(i => !i.isHealthy))

const filtered = computed(() => {
  let list = allItems.value
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(i => i.sportsmanName?.toLowerCase().includes(q) || i.position?.toLowerCase().includes(q))
  }
  if (filterStatus.value === 'issues') list = list.filter(i => !i.isHealthy)
  if (filterStatus.value === 'risks')  list = list.filter(i => i.hasWarnings)
  if (filterStatus.value === 'ok')     list = list.filter(i => i.isHealthy && !i.hasWarnings)
  return [...list].sort((a, b) => (b.checkResult?.issues?.length ?? 0) - (a.checkResult?.issues?.length ?? 0))
})

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / PER_PAGE)))
const paged      = computed(() => filtered.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

const pageNumbers = computed(() => {
  const total = totalPages.value
  const cur   = page.value
  if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1)
  const pages: (number | '…')[] = [1]
  if (cur > 3) pages.push('…')
  for (let p = Math.max(2, cur - 1); p <= Math.min(total - 1, cur + 1); p++) pages.push(p)
  if (cur < total - 2) pages.push('…')
  pages.push(total)
  return pages
})

// Модальное окно: 5 метрик соответствующих 5 категориям
const modalMetricCards = computed(() => {
  if (!modalItem.value) return []
  const m  = modalItem.value.metrics
  const cr = modalItem.value.checkResult
  return [
    {
      label: 'ЧСС макс.',
      displayValue: m?.maxHeartRate != null ? `${m.maxHeartRate} уд/мин` : '—',
      textClass: statusTextClass(cr?.cardiovascularOk === false ? 'critical' : 'ok'),
      hint: 'порог: 220 − возраст',
    },
    {
      label: 'Нагрузка',
      displayValue: m?.playerLoad != null ? `${m.playerLoad.toFixed(1)}` : '—',
      textClass: statusTextClass(playerLoadStatus(m?.playerLoad, cr?.loadOk)),
      hint: 'норма < 450 / критично ≥ 500',
    },
    {
      label: 'Восстановление',
      displayValue: m?.timeInLowIntensity != null ? `${(m.timeInLowIntensity / 60).toFixed(1)} мин` : '—',
      textClass: statusTextClass(recoveryStatus(m?.timeInLowIntensity, cr?.recoveryOk)),
      hint: 'минимум 5 мин в зоне 1',
    },
    {
      label: 'Риск травм (О/Х)',
      displayValue: m?.acuteChronicRatio != null ? `${m.acuteChronicRatio.toFixed(2)}` : '—',
      textClass: statusTextClass(acuteChronicStatus(m?.acuteChronicRatio, cr?.injuryRiskOk)),
      hint: 'норма < 1.3 / критично ≥ 1.5',
    },
    {
      label: 'Усталость',
      displayValue: m?.fatigueIndex != null ? `${m.fatigueIndex.toFixed(2)}` : '—',
      textClass: statusTextClass(fatigueStatus(m?.fatigueIndex, cr?.fatigueOk)),
      hint: 'норма < 1.2',
    },
    {
      label: 'Стабильность',
      displayValue: m?.consistency != null ? `${m.consistency.toFixed(1)}` : '—',
      textClass: statusTextClass(consistencyStatus(m?.consistency, cr?.consistencyOk)),
      hint: 'норма ≤ 20 у.е.',
    },
  ]
})


const modalWarnings = computed(() => {
  if (!modalItem.value) return []
  const m  = modalItem.value.metrics
  const cr = modalItem.value.checkResult
  if (!m) return []
  const warns: string[] = []
  if (playerLoadStatus(m.playerLoad, cr?.loadOk) === 'warning')
    warns.push(`Риск нагрузки: ${m.playerLoad?.toFixed(1)} у.е.`)
  if (recoveryStatus(m.timeInLowIntensity, cr?.recoveryOk) === 'warning')
    warns.push(`Риск недовосстановления: ${(m.timeInLowIntensity / 60).toFixed(1)} мин`)
  if (acuteChronicStatus(m.acuteChronicRatio, cr?.injuryRiskOk) === 'warning')
    warns.push(`Риск травм: острая/хроническая нагрузка ${m.acuteChronicRatio?.toFixed(2)}`)
  if (fatigueStatus(m.fatigueIndex, cr?.fatigueOk) === 'warning')
    warns.push(`Риск усталости: индекс усталости ${m.fatigueIndex?.toFixed(2)}`)
  if (consistencyStatus(m.consistency, cr?.consistencyOk) === 'warning')
    warns.push(`Риск нестабильной нагрузки: разброс ${m.consistency?.toFixed(1)} у.е.`)
  return warns
})

function openModal(item: any) { modalItem.value = item }

onMounted(async () => {
  loading.value = true
  const [sportsmenRes] = await Promise.allSettled([api.get('/sportsman').catch(() => null)])
  const sportsmen: any[] = sportsmenRes.status === 'fulfilled' ? (sportsmenRes.value?.data?.data ?? []) : []

  const [checks, metrics] = await Promise.all([
    Promise.allSettled(sportsmen.map(s => api.get(`/analytic/${s.id}/medical/check`).catch(() => null))),
    Promise.allSettled(sportsmen.map(s => api.get(`/analytic/${s.id}/medical`).catch(() => null))),
  ])

  allItems.value = sportsmen
    .map((s, i) => {
      const checkRes = checks[i]
      if (checkRes.status !== 'fulfilled' || !checkRes.value?.data?.data) return null
      const d = checkRes.value.data.data
      const mr = metrics[i]
      const metricsData = mr.status === 'fulfilled' ? (mr.value?.data?.data?.metrics ?? null) : null
      const checkResult = d.checkResult ?? {}
      const m = metricsData
      const hasWarnings = !!m && (
        playerLoadStatus(m.playerLoad, checkResult.loadOk) === 'warning' ||
        recoveryStatus(m.timeInLowIntensity, checkResult.recoveryOk) === 'warning' ||
        acuteChronicStatus(m.acuteChronicRatio, checkResult.injuryRiskOk) === 'warning' ||
        fatigueStatus(m.fatigueIndex, checkResult.fatigueOk) === 'warning' ||
        consistencyStatus(m.consistency, checkResult.consistencyOk) === 'warning'
      )
      return {
        sportsmanId:   d.sportsmanId ?? s.id,
        sportsmanName: d.sportsmanName ?? s.fio ?? '—',
        position:      s.position ?? '',
        isHealthy:     checkResult.isHealthy ?? true,
        checkResult,
        metrics:       metricsData,
        hasWarnings,
      }
    })
    .filter(Boolean)

  loading.value = false
})

// MetricValue inline component
const MetricValue = defineComponent({
  props: {
    value:  { type: Number, default: undefined },
    status: { type: String as () => MetricStatus, default: 'unknown' },
    suffix: { type: String, default: '' },
  },
  setup(props) {
    return () => {
      if (props.value == null) return h('span', { class: 'text-neutral-300' }, '—')
      const cls = [
        'inline-flex items-center gap-1 font-semibold',
        props.status === 'critical' ? 'text-red-600'
        : props.status === 'caution' ? 'text-orange-500'
        : props.status === 'warning'  ? 'text-yellow-500'
        : props.status === 'ok'       ? 'text-green-700'
        :                               'text-neutral-500',
      ]
      // стрелка только для caution и critical
      const arrow = (props.status === 'caution' || props.status === 'critical')
        ? h('svg', { class: 'w-3 h-3 flex-shrink-0', fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2.5' }, [
            h('line', { x1: '12', y1: '19', x2: '12', y2: '5', 'stroke-linecap': 'round' }),
            h('polyline', { points: '5 12 12 5 19 12', 'stroke-linecap': 'round', 'stroke-linejoin': 'round' }),
          ])
        : null
      return h('span', { class: cls }, [`${props.value}${props.suffix}`, arrow])
    }
  }
})
</script>

<style scoped>
.modal-enter-active, .modal-leave-active { transition: opacity 0.15s ease; }
.modal-enter-from, .modal-leave-to { opacity: 0; }
</style>
