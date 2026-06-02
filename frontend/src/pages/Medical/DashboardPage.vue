<template>
  <div class="p-3 h-full overflow-y-auto">
    <MedicalPageCard color="emerald" class="min-h-full flex flex-col gap-5">

      <!-- Хедер: нейтральный, с левым акцентом -->
      <div class="flex items-center gap-4 pl-4 py-1 border-l-4 border-emerald-500">
        <div class="w-10 h-10 rounded-xl bg-emerald-50 flex items-center justify-center flex-shrink-0">
          <svg class="w-5 h-5 text-emerald-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z"/>
          </svg>
        </div>
        <div class="flex-1 min-w-0">
          <h1 class="text-xl font-bold text-neutral-900">Медицинский модуль</h1>
          <p class="text-sm text-neutral-500 mt-0.5">Мониторинг состояния спортсменов</p>
        </div>
        <RouterLink to="/medical/checks" class="text-xs font-medium text-emerald-600 hover:text-emerald-700 transition-colors flex-shrink-0">
          Все проверки →
        </RouterLink>
      </div>

      <!-- Metric-карточки -->
      <div class="grid grid-cols-3 gap-3">
        <RouterLink
          v-for="stat in statCards" :key="stat.to"
          :to="stat.to"
          class="rounded-2xl border bg-neutral-50 px-4 py-4 flex flex-col gap-1 hover:shadow-sm transition-all group"
          :class="stat.border"
        >
          <div class="flex items-center gap-2 mb-1">
            <div class="w-8 h-8 rounded-xl flex items-center justify-center flex-shrink-0" :class="stat.iconBg">
              <component :is="stat.icon" class="w-4 h-4" />
            </div>
            <div class="text-xs text-neutral-500 group-hover:text-neutral-700 transition-colors truncate">{{ stat.label }}</div>
          </div>
          <div class="text-3xl font-semibold leading-tight" :class="stat.valueClass">{{ stat.value }}</div>
        </RouterLink>
      </div>

      <!-- Список тревог -->
      <div class="rounded-2xl border border-neutral-200 bg-white flex flex-col flex-1 min-h-0 overflow-hidden">
        <!-- Заголовок с фильтрами -->
        <div class="px-4 py-3 border-b border-neutral-100 flex items-center justify-between gap-3 flex-shrink-0">
          <div class="text-sm font-semibold text-neutral-800 flex items-center gap-2">
            Требуют внимания
            <span v-if="alerts.length" class="text-[11px] px-2 py-0.5 rounded-full bg-red-100 text-red-600 font-semibold">{{ alerts.length }}</span>
          </div>
          <div class="flex items-center gap-1">
            <button
              v-for="f in filters" :key="f.value"
              @click="activeFilter = f.value"
              class="text-xs px-3 py-1 rounded-full font-medium transition-colors"
              :class="activeFilter === f.value
                ? 'bg-emerald-100 text-emerald-700'
                : 'text-neutral-500 hover:text-neutral-700'"
            >{{ f.label }}</button>
            <RouterLink to="/medical/checks" class="text-xs text-emerald-600 hover:text-emerald-700 transition-colors ml-1 font-medium">Все →</RouterLink>
          </div>
        </div>

        <!-- Список -->
        <div class="flex-1 overflow-y-auto">
          <div v-if="loading" class="flex items-center justify-center py-10">
            <div class="w-5 h-5 border-2 border-emerald-400 border-t-transparent rounded-full animate-spin" />
          </div>

          <div v-else-if="!alerts.length" class="flex flex-col items-center py-10 gap-2">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-8 h-8 text-green-400">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
            </svg>
            <div class="text-xs text-neutral-400">Все показатели в норме</div>
          </div>

          <div v-else-if="!filteredAlerts.length" class="flex flex-col items-center py-10 gap-2">
            <div class="text-xs text-neutral-400">Нет записей по этому фильтру</div>
          </div>

          <div v-else class="divide-y divide-neutral-50">
            <div
              v-for="a in filteredAlerts"
              :key="a.sportsmanId"
              class="flex items-center gap-3 px-4 py-3 hover:bg-neutral-50 transition-colors cursor-pointer group"
              @click="markViewed(a.sportsmanId); $router.push(`/medical/sportsman/${a.sportsmanId}`)"
            >
              <!-- Аватар -->
              <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0 text-sm font-bold"
                :class="isViewed(a.sportsmanId) ? 'bg-neutral-200 text-neutral-500' : 'bg-emerald-100 text-emerald-700'">
                {{ initials(a.sportsmanName) }}
              </div>

              <!-- Контент -->
              <div class="flex-1 min-w-0">
                <div class="text-sm font-medium text-neutral-900 flex items-center gap-2">
                  {{ a.sportsmanName }}
                  <span v-if="isViewed(a.sportsmanId)"
                    class="text-[10px] px-1.5 py-0.5 rounded-full bg-neutral-100 text-neutral-500 font-medium">
                    Просмотрено
                  </span>
                </div>
                <div class="flex flex-wrap gap-1 mt-1">
                  <span v-for="(issue, i) in a.issues.slice(0, 2)" :key="i"
                    class="text-[11px] text-neutral-500">
                    {{ i === 0 ? shortIssue(issue) : `+${a.issues.length - 1} ещё` }}
                  </span>
                  <span v-if="a.issues.length === 0" class="text-[11px] text-neutral-400">Нет описания</span>
                </div>
              </div>

              <!-- Правая часть -->
              <div class="flex flex-col items-end gap-1.5 flex-shrink-0">
                <span class="text-[11px] px-2.5 py-0.5 rounded-full font-medium"
                  :class="isViewed(a.sportsmanId) ? 'bg-neutral-100 text-neutral-500' : 'bg-red-100 text-red-500'">
                  {{ a.issues.length }} {{ plural(a.issues.length) }}
                </span>
                <button
                  v-if="!isViewed(a.sportsmanId)"
                  class="text-[10px] text-neutral-400 hover:text-neutral-600 transition-colors opacity-0 group-hover:opacity-100"
                  @click.stop="markViewed(a.sportsmanId)"
                >
                  ✓ Отметить
                </button>
                <button
                  v-else
                  class="text-[10px] text-neutral-300 hover:text-neutral-500 transition-colors"
                  @click.stop="unmarkViewed(a.sportsmanId)"
                >
                  Снять отметку
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

    </MedicalPageCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, defineComponent, h } from 'vue'
import { RouterLink } from 'vue-router'
import api from '@/services/api'
import MedicalPageCard from '@/components/medical/MedicalPageCard.vue'

const VIEWED_KEY = 'medical_viewed_alerts'

const loading        = ref(true)
const alerts         = ref<{ sportsmanId: number; sportsmanName: string; issues: string[] }[]>([])
const totalSportsmen = ref(0)
const activeFilter   = ref<'all' | 'new' | 'viewed'>('new')
const viewedSet      = ref<Set<number>>(new Set())

const filters: { value: 'all' | 'new' | 'viewed'; label: string }[] = [
  { value: 'all',    label: 'Все' },
  { value: 'new',    label: 'Не просмотренные' },
  { value: 'viewed', label: 'Просмотренные' },
]

function loadViewed() {
  try {
    const raw = localStorage.getItem(VIEWED_KEY)
    viewedSet.value = raw ? new Set(JSON.parse(raw)) : new Set()
  } catch {
    viewedSet.value = new Set()
  }
}

function saveViewed() {
  localStorage.setItem(VIEWED_KEY, JSON.stringify([...viewedSet.value]))
}

function isViewed(id: number) {
  return viewedSet.value.has(id)
}

function markViewed(id: number) {
  viewedSet.value.add(id)
  saveViewed()
}

function unmarkViewed(id: number) {
  viewedSet.value.delete(id)
  saveViewed()
}

function initials(fio: string) {
  return (fio ?? '').split(' ').slice(0, 2).map((w: string) => w[0]).join('').toUpperCase()
}

function shortIssue(issue: string) {
  return issue.length > 60 ? issue.slice(0, 58) + '…' : issue
}

function plural(n: number) {
  const mod10 = n % 10, mod100 = n % 100
  if (mod10 === 1 && mod100 !== 11) return 'проблема'
  if (mod10 >= 2 && mod10 <= 4 && (mod100 < 10 || mod100 >= 20)) return 'проблемы'
  return 'проблем'
}

const filteredAlerts = computed(() => {
  if (activeFilter.value === 'new')    return alerts.value.filter(a => !isViewed(a.sportsmanId))
  if (activeFilter.value === 'viewed') return alerts.value.filter(a => isViewed(a.sportsmanId))
  return alerts.value
})

// Иконки
const IconHeart = defineComponent({ render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M4.318 6.318a4.5 4.5 0 000 6.364L12 20.364l7.682-7.682a4.5 4.5 0 00-6.364-6.364L12 7.636l-1.318-1.318a4.5 4.5 0 00-6.364 0z' })]) })
const IconUsers = defineComponent({ render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z' })]) })
const IconClipboard = defineComponent({ render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2' })]) })

const statCards = computed(() => [
  {
    to: '/medical/sportsmen',
    label: 'Спортсмены',
    value: totalSportsmen.value,
    iconBg: 'bg-emerald-100 text-emerald-600',
    border: 'border-neutral-200',
    valueClass: 'text-emerald-700',
    icon: IconUsers,
  },
  {
    to: '/medical/checks',
    label: 'Под контролем',
    value: alerts.value.length,
    iconBg: 'bg-red-100 text-red-500',
    border: 'border-red-100',
    valueClass: 'text-red-500',
    icon: IconHeart,
  },
  {
    to: '/medical/normatives',
    label: 'Нормативы',
    value: '→',
    iconBg: 'bg-neutral-100 text-neutral-500',
    border: 'border-neutral-200',
    valueClass: 'text-neutral-500',
    icon: IconClipboard,
  },
])

onMounted(async () => {
  loadViewed()
  loading.value = true

  const [checksRes, sportsmenRes] = await Promise.allSettled([
    api.get('/analytic/medical/check').catch(() => null),
    api.get('/sportsman').catch(() => null),
  ])

  const checks: any[]    = checksRes.status === 'fulfilled'    ? (checksRes.value?.data?.data ?? [])    : []
  const sportsmen: any[] = sportsmenRes.status === 'fulfilled' ? (sportsmenRes.value?.data?.data ?? []) : []

  totalSportsmen.value = sportsmen.length
  alerts.value = checks
    .map((c: any) => ({
      sportsmanId:   c.sportsmanId,
      sportsmanName: c.sportsmanName ?? '—',
      issues:        c.checkResult?.issues ?? [],
    }))
    .filter(a => a.issues.length > 0)

  loading.value = false
})
</script>
