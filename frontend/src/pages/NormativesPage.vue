<template>
  <div class="p-3 h-full">
    <AppCard no-border class="h-full">

      <!-- Заголовок + пагинация периодов -->
      <div class="mb-4 pb-4 border-b border-neutral-100">
        <div class="flex items-center justify-between gap-3">
          <div>
            <h1 class="text-xl font-bold text-neutral-900">Нормативы</h1>
            <p class="text-sm text-neutral-500 mt-0.5">{{ periodLabel }}</p>
          </div>
          <div class="flex items-center gap-2 shrink-0">
            <button
              @click="nextPeriod"
              class="p-1.5 rounded-lg border border-neutral-200 bg-white hover:bg-neutral-100 transition-colors"
            >
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500">
                <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
              </svg>
            </button>

            <span class="text-xs text-neutral-500 min-w-[80px] text-center">
              {{ periodOffset === 0 ? 'Сейчас' : `${periodOffset * 6} мес назад` }}
            </span>

            <button
              @click="prevPeriod"
              :disabled="periodOffset === 0"
              class="p-1.5 rounded-lg border border-neutral-200 bg-white hover:bg-neutral-100 disabled:opacity-30 disabled:cursor-not-allowed transition-colors"
            >
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/>
              </svg>
            </button>
          </div>
        </div>
      </div>

      <div v-if="loading" class="text-sm text-neutral-400">Загрузка...</div>

      <div v-else class="grid grid-cols-2 gap-6">

        <!-- ГТО -->
        <div class="flex flex-col gap-2">
          <div class="text-xs font-bold text-neutral-400 uppercase tracking-wide mb-1">ГТО</div>
          <div v-if="!periodGto.length" class="text-sm text-neutral-400">Нет результатов за этот период</div>
          <template v-else>
            <div
              v-for="item in pagedGto" :key="item.id"
              class="rounded-xl border p-3"
              :class="gradeBorder(item.grade)"
            >
              <div class="flex items-start justify-between gap-2">
                <div class="flex-1 min-w-0">
                  <div class="text-sm font-semibold text-neutral-800 truncate">{{ item.type }}</div>
                  <div class="text-xs text-neutral-400 mt-0.5">{{ formatDate(item.createdAt) }}</div>
                </div>
                <div class="flex flex-col items-end gap-1 flex-shrink-0">
                  <span class="text-sm font-bold text-neutral-900">{{ item.result }} {{ item.unit }}</span>
                  <span class="text-xs font-bold px-2 py-0.5 rounded-full" :class="gradeBadge(item.grade)">{{ gradeLabel(item.grade) }}</span>
                </div>
              </div>
            </div>
            <div class="flex items-center justify-between pt-1">
              <button @click="gtoPage > 1 && gtoPage--" :disabled="gtoPage === 1" class="p-1.5 rounded-lg hover:bg-neutral-100 disabled:opacity-30 transition-colors">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/></svg>
              </button>
              <span class="text-xs text-neutral-500">{{ gtoPage }} / {{ gtoTotalPages }}</span>
              <button @click="gtoPage < gtoTotalPages && gtoPage++" :disabled="gtoPage === gtoTotalPages" class="p-1.5 rounded-lg hover:bg-neutral-100 disabled:opacity-30 transition-colors">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/></svg>
              </button>
            </div>
          </template>
        </div>

        <!-- Локальные -->
        <div class="flex flex-col gap-2">
          <div class="text-xs font-bold text-neutral-400 uppercase tracking-wide mb-1">Локальные</div>
          <div v-if="!periodLocal.length" class="text-sm text-neutral-400">Нет результатов за этот период</div>
          <template v-else>
            <div
              v-for="item in pagedLocal" :key="item.id"
              class="rounded-xl border p-3"
              :class="localGradeBorder(item.grade)"
            >
              <div class="flex items-start justify-between gap-2">
                <div class="flex-1 min-w-0">
                  <div class="text-sm font-semibold text-neutral-800 truncate">{{ item.type }}</div>
                  <div class="text-xs text-neutral-400 mt-0.5">{{ formatDate(item.createdAt) }}</div>
                </div>
                <div class="flex flex-col items-end gap-1 flex-shrink-0">
                  <span class="text-sm font-bold text-neutral-900">{{ item.result }} {{ item.unit }}</span>
                  <span class="text-xs font-bold px-2 py-0.5 rounded-full" :class="localGradeBadge(item.grade)">{{ localGradeLabel(item.grade) }}</span>
                </div>
              </div>
            </div>
            <div class="flex items-center justify-between pt-1">
              <button @click="localPage > 1 && localPage--" :disabled="localPage === 1" class="p-1.5 rounded-lg hover:bg-neutral-100 disabled:opacity-30 transition-colors">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/></svg>
              </button>
              <span class="text-xs text-neutral-500">{{ localPage }} / {{ localTotalPages }}</span>
              <button @click="localPage < localTotalPages && localPage++" :disabled="localPage === localTotalPages" class="p-1.5 rounded-lg hover:bg-neutral-100 disabled:opacity-30 transition-colors">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/></svg>
              </button>
            </div>
          </template>
        </div>

      </div>

    </AppCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import api from '@/services/api'
import { formatDate } from '@/utils/formatDate'
import AppCard from '@/components/ui/AppCard.vue'

const loading = ref(true)
const route = useRoute()
const PER_PAGE = 5

// периоды: 0 = последние 6 мес, 1 = предыдущие 6 мес и т.д.
const periodOffset = ref(0)
const gtoPage   = ref(1)
const localPage = ref(1)

const gtoResults   = ref<any[]>([])
const gtoNorms     = ref<any[]>([])
const localResults = ref<any[]>([])
const localNorms   = ref<any[]>([])

// границы текущего периода
const periodFrom = computed(() => {
  const d = new Date()
  d.setMonth(d.getMonth() - (periodOffset.value + 1) * 6)
  d.setHours(0, 0, 0, 0)
  return d
})
const periodTo = computed(() => {
  const d = new Date()
  d.setMonth(d.getMonth() - periodOffset.value * 6)
  d.setHours(23, 59, 59, 999)
  return d
})

const periodLabel = computed(() => {
  const fmt = (d: Date) => d.toLocaleDateString('ru-RU', { day: 'numeric', month: 'short', year: 'numeric' })
  return `${fmt(periodFrom.value)} — ${fmt(periodTo.value)}`
})

function prevPeriod() { if (periodOffset.value > 0) { periodOffset.value--; resetPages() } }
function nextPeriod() { periodOffset.value++; resetPages() }
function resetPages() { gtoPage.value = 1; localPage.value = 1 }

// фильтрация по периоду + обогащение оценкой
const periodGto = computed(() =>
  gtoResults.value
    .filter(r => { const d = new Date(r.createdAt); return d >= periodFrom.value && d <= periodTo.value })
    .map(r => {
      const norm = gtoNorms.value.find(n => n.id === r.normativeId)
      return { ...r, type: norm?.type ?? '—', unit: norm?.unit ?? '', grade: calcGrade(r.result, norm) }
    })
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
)

const periodLocal = computed(() =>
  localResults.value
    .filter(r => { const d = new Date(r.createdAt); return d >= periodFrom.value && d <= periodTo.value })
    .map(r => {
      const norm = localNorms.value.find(n => n.id === r.localNormativeId)
      return { ...r, type: norm?.type ?? '—', unit: norm?.unit ?? '', grade: calcLocalGrade(r.result, norm) }
    })
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
)

const gtoTotalPages   = computed(() => Math.max(1, Math.ceil(periodGto.value.length / PER_PAGE)))
const localTotalPages = computed(() => Math.max(1, Math.ceil(periodLocal.value.length / PER_PAGE)))
const pagedGto        = computed(() => periodGto.value.slice((gtoPage.value - 1) * PER_PAGE, gtoPage.value * PER_PAGE))
const pagedLocal      = computed(() => periodLocal.value.slice((localPage.value - 1) * PER_PAGE, localPage.value * PER_PAGE))

// сброс страниц при смене периода
watch(periodOffset, () => resetPages())

function calcGrade(result: number, norm: any): string {
  if (!norm) return 'none'
  const lower = norm.gradeExcellent < norm.gradeSatisfactory
  if (lower) {
    if (result <= norm.gradeExcellent) return 'excellent'
    if (result <= norm.gradeGood) return 'good'
    if (result <= norm.gradeSatisfactory) return 'satisfactory'
  } else {
    if (result >= norm.gradeExcellent) return 'excellent'
    if (result >= norm.gradeGood) return 'good'
    if (result >= norm.gradeSatisfactory) return 'satisfactory'
  }
  return 'none'
}
function calcLocalGrade(result: number, norm: any): string {
  if (!norm) return 'none'
  return norm.isMoreBetter ? (result >= norm.value ? 'pass' : 'fail') : (result <= norm.value ? 'pass' : 'fail')
}

function gradeLabel(g: string)      { return ({ excellent: 'Отлично', good: 'Хорошо', satisfactory: 'Удовл.', none: '—' } as any)[g] ?? '—' }
function localGradeLabel(g: string) { return ({ pass: 'Выполнено', fail: 'Не выполнено', none: '—' } as any)[g] ?? '—' }
function gradeBadge(g: string)      { return ({ excellent: 'bg-green-100 text-green-700', good: 'bg-blue-100 text-blue-700', satisfactory: 'bg-yellow-100 text-yellow-700', none: 'bg-neutral-100 text-neutral-500' } as any)[g] ?? 'bg-neutral-100 text-neutral-500' }
function gradeBorder(g: string)     { return ({ excellent: 'border-green-200 bg-green-50/40', good: 'border-blue-200 bg-blue-50/40', satisfactory: 'border-yellow-200 bg-yellow-50/40', none: 'border-neutral-200 bg-white' } as any)[g] ?? 'border-neutral-200 bg-white' }
function localGradeBadge(g: string) { return ({ pass: 'bg-green-100 text-green-700', fail: 'bg-red-100 text-red-600', none: 'bg-neutral-100 text-neutral-500' } as any)[g] ?? 'bg-neutral-100 text-neutral-500' }
function localGradeBorder(g: string){ return ({ pass: 'border-green-200 bg-green-50/40', fail: 'border-red-200 bg-red-50/30', none: 'border-neutral-200 bg-white' } as any)[g] ?? 'border-neutral-200 bg-white' }

onMounted(async () => {
  const routeId = route.params.id ? Number(route.params.id) : null
  const meRes = routeId ? null : await api.get('/sportsman/me').catch(() => null)
  const sportsmanId = routeId ?? meRes?.data?.data?.id
  if (!sportsmanId) { loading.value = false; return }

  const [gtoRes, gtoNormRes, localRes, localNormRes] = await Promise.allSettled([
    api.get(`/normative/gto/results/sportsman/${sportsmanId}`),
    api.get('/normative/gto'),
    api.get(`/normative/local/results/sportsman/${sportsmanId}`),
    api.get('/normative/local'),
  ])

  if (gtoRes.status === 'fulfilled')       gtoResults.value   = gtoRes.value.data.data ?? []
  if (gtoNormRes.status === 'fulfilled')   gtoNorms.value     = gtoNormRes.value.data.data ?? []
  if (localRes.status === 'fulfilled')     localResults.value = localRes.value.data.data ?? []
  if (localNormRes.status === 'fulfilled') localNorms.value   = localNormRes.value.data.data ?? []

  loading.value = false
})
</script>
