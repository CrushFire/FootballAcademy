<template>
  <div class="p-3 h-full">
  <MedicalPageCard class="h-full flex flex-col">
  <AdminListLayout
    title="Нормативы спортсменов"
    :search="search"
    :loading="loading"
    :items="filteredSportsmen"
    :page="page"
    :total="filteredSportsmen.length"
    :total-pages="totalPages"
    :sort-dir="sortDir"
    :sort-label="sortOptions.find(o => o.value === sortBy)?.label"
    @update:search="v => { search = v; page = 1 }"
    @prev="page > 1 && page--"
    @next="page < totalPages && page++"
    @toggle-dir="sortDir = sortDir === 'asc' ? 'desc' : 'asc'"
  >
    <template #actions>
      <div class="flex items-center gap-1.5">
        <button @click="nextPeriod" class="p-1.5 rounded-lg border border-neutral-200 bg-neutral-50 hover:bg-neutral-100 transition-colors">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/></svg>
        </button>
        <span class="text-xs text-neutral-500 min-w-[110px] text-center">{{ periodLabel }}</span>
        <button @click="prevPeriod" :disabled="periodOffset === 0" class="p-1.5 rounded-lg border border-neutral-200 bg-neutral-50 hover:bg-neutral-100 disabled:opacity-30 disabled:cursor-not-allowed transition-colors">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/></svg>
        </button>
      </div>
    </template>

    <template #filter>
      <select v-model="filterSpec" @change="page = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
        <option value="">Все специализации</option>
        <option value="Football">Футбол</option>
        <option value="Minifootball">Мини-футбол</option>
      </select>
    </template>

    <template #sort="{ close }">
      <button v-for="o in sortOptions" :key="o.value" @click="sortBy = o.value; page = 1; close()"
        class="w-full text-left px-3 py-2 text-xs hover:bg-neutral-50 transition-colors"
        :class="sortBy === o.value ? 'text-blue-600 font-semibold' : 'text-neutral-600'">{{ o.label }}</button>
    </template>

    <template #items>
      <div
        v-for="s in pagedSportsmen"
        :key="s.id"
        @click="openModal(s)"
        class="bg-white rounded-2xl border border-neutral-200 p-3 flex items-center gap-3 cursor-pointer hover:border-sky-200 hover:bg-sky-50/30 transition-all shadow-[0_6px_0_-2px_#BAE6FD] dark:shadow-[0_6px_0_-2px_#0C4A6E] mb-1.5"
      >
        <div class="w-9 h-9 rounded-xl bg-neutral-100 flex items-center justify-center flex-shrink-0 text-sm font-bold text-neutral-500">
          {{ initials(s.fio) }}
        </div>
        <div class="flex-1 min-w-0">
          <div class="text-sm font-semibold text-neutral-800 truncate">{{ s.fio }}</div>
          <div class="text-xs text-neutral-400 mt-0.5">{{ s.position ?? '—' }} · {{ SPEC_LABEL[s.specialization] ?? s.specialization }} · {{ s.age }} лет</div>
        </div>
        <div class="flex flex-col items-end gap-1 flex-shrink-0 min-w-[140px]">
          <div class="w-full">
            <div class="text-[9px] font-bold text-neutral-600 uppercase tracking-wide mb-0.5">ГТО:</div>
            <div class="flex items-center gap-1 flex-wrap justify-end">
              <span v-if="miniStats[s.id]?.excellent"    class="text-[10px] px-1.5 py-0.5 rounded-full bg-green-100 text-green-700 font-semibold whitespace-nowrap">{{ miniStats[s.id].excellent }} отл</span>
              <span v-if="miniStats[s.id]?.good"         class="text-[10px] px-1.5 py-0.5 rounded-full bg-blue-100 text-blue-700 font-semibold whitespace-nowrap">{{ miniStats[s.id].good }} хор</span>
              <span v-if="miniStats[s.id]?.satisfactory" class="text-[10px] px-1.5 py-0.5 rounded-full bg-yellow-100 text-yellow-700 font-semibold whitespace-nowrap">{{ miniStats[s.id].satisfactory }} удовл</span>
              <span v-if="!miniStats[s.id]?.excellent && !miniStats[s.id]?.good && !miniStats[s.id]?.satisfactory" class="text-[10px] text-neutral-400">—</span>
            </div>
          </div>
          <div class="w-full">
            <div class="text-[9px] font-bold text-neutral-600 uppercase tracking-wide mb-0.5">Локальные:</div>
            <div class="flex items-center gap-1 flex-wrap justify-end">
              <span v-if="miniStats[s.id]?.pass" class="text-[10px] px-1.5 py-0.5 rounded-full bg-teal-100 text-teal-700 font-semibold whitespace-nowrap">{{ miniStats[s.id].pass }} выпол</span>
              <span v-else class="text-[10px] text-neutral-400">—</span>
            </div>
          </div>
        </div>
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-300 flex-shrink-0">
          <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/>
        </svg>
      </div>
    </template>
  </AdminListLayout>
  </MedicalPageCard>
  </div>

  <!-- Modal -->
  <Teleport to="body">
    <div v-if="modalSportsman" class="fixed inset-0 bg-black/30 z-50 flex items-end sm:items-center justify-center p-3" @click.self="modalSportsman = null">
      <div class="bg-white rounded-2xl border border-neutral-200 shadow-xl w-full max-w-lg flex flex-col max-h-[85vh]">

        <div class="flex items-center gap-3 p-4 border-b border-neutral-100">
          <div class="w-9 h-9 rounded-xl bg-neutral-100 flex items-center justify-center flex-shrink-0 text-sm font-bold text-neutral-500">
            {{ initials(modalSportsman.fio) }}
          </div>
          <div class="flex-1 min-w-0">
            <div class="text-sm font-bold text-neutral-900 truncate">{{ modalSportsman.fio }}</div>
          </div>
          <button @click="modalSportsman = null" class="p-1.5 rounded-lg hover:bg-neutral-100 transition-colors flex-shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-400">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>

        <div class="flex items-center justify-between gap-2 px-4 py-2 border-b border-neutral-100">
          <button @click="nextModalPeriod" class="p-1 rounded-lg hover:bg-neutral-100 transition-colors">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/></svg>
          </button>
          <span class="text-xs text-neutral-500">{{ modalPeriodLabel }}</span>
          <button @click="prevModalPeriod" :disabled="modalPeriodOffset === 0" class="p-1 rounded-lg hover:bg-neutral-100 disabled:opacity-30 transition-colors">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/></svg>
          </button>
        </div>

        <div v-if="modalLoading" class="p-6 text-center text-sm text-neutral-400">Загрузка...</div>
        <div v-else class="flex-1 overflow-y-auto p-4">
          <div class="grid grid-cols-2 gap-4">
            <div class="flex flex-col gap-2">
              <div class="text-xs font-bold text-neutral-400 uppercase tracking-wide mb-1">ГТО</div>
              <div v-if="!modalPeriodGto.length" class="text-xs text-neutral-400">Нет результатов</div>
              <div v-for="item in modalPeriodGto" :key="item.id" class="rounded-xl border p-2.5" :class="gradeBorder(item.grade)">
                <div class="text-xs font-semibold text-neutral-800 truncate">{{ item.type }}</div>
                <div class="flex items-center justify-between mt-1 gap-1">
                  <span class="text-xs font-bold text-neutral-900">{{ item.result }} {{ item.unit }}</span>
                  <span class="text-[10px] font-bold px-1.5 py-0.5 rounded-full" :class="gradeBadge(item.grade)">{{ gradeLabel(item.grade) }}</span>
                </div>
                <div class="text-[10px] text-neutral-400 mt-0.5">{{ formatDate(item.createdAt) }}</div>
              </div>
            </div>
            <div class="flex flex-col gap-2">
              <div class="text-xs font-bold text-neutral-400 uppercase tracking-wide mb-1">Локальные</div>
              <div v-if="!modalPeriodLocal.length" class="text-xs text-neutral-400">Нет результатов</div>
              <div v-for="item in modalPeriodLocal" :key="item.id" class="rounded-xl border p-2.5" :class="localGradeBorder(item.grade)">
                <div class="text-xs font-semibold text-neutral-800 truncate">{{ item.type }}</div>
                <div class="flex items-center justify-between mt-1 gap-1">
                  <span class="text-xs font-bold text-neutral-900">{{ item.result }} {{ item.unit }}</span>
                  <span class="text-[10px] font-bold px-1.5 py-0.5 rounded-full" :class="localGradeBadge(item.grade)">{{ localGradeLabel(item.grade) }}</span>
                </div>
                <div class="text-[10px] text-neutral-400 mt-0.5">{{ formatDate(item.createdAt) }}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import api from '@/services/api'
import { formatDate } from '@/utils/formatDate'
import AdminListLayout from '@/components/ui/AdminListLayout.vue'
import MedicalPageCard from '@/components/medical/MedicalPageCard.vue'

const SPEC_LABEL: Record<string, string> = { Football: 'Футбол', Minifootball: 'Мини-футбол' }
const PER_PAGE = 15

const loading      = ref(true)
const search       = ref('')
const filterSpec   = ref('')
const page         = ref(1)
const periodOffset = ref(0)
const sortBy       = ref('fio')
const sortDir      = ref<'asc' | 'desc'>('asc')

const sortOptions = [
  { value: 'fio',       label: 'По имени' },
  { value: 'excellent', label: 'Отличных' },
  { value: 'good',      label: 'Хороших' },
]

const gtoNorms   = ref<any[]>([])
const localNorms = ref<any[]>([])
const allSportsmen = ref<any[]>([])
const miniStats    = ref<Record<number, { excellent: number; good: number; satisfactory: number; pass: number }>>({})

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
  const fmt = (d: Date) => d.toLocaleDateString('ru-RU', { month: 'short', year: 'numeric' })
  return `${fmt(periodFrom.value)} — ${fmt(periodTo.value)}`
})
function prevPeriod() { if (periodOffset.value > 0) periodOffset.value-- }
function nextPeriod() { periodOffset.value++ }

const filteredSportsmen = computed(() => {
  const q = search.value.toLowerCase()
  let list = allSportsmen.value.filter(s => {
    const matchName = !q || s.fio.toLowerCase().includes(q)
    const matchSpec = !filterSpec.value || s.specialization === filterSpec.value
    return matchName && matchSpec
  })
  return [...list].sort((a, b) => {
    let res = 0
    if (sortBy.value === 'fio') {
      res = a.fio.localeCompare(b.fio, 'ru')
    } else if (sortBy.value === 'excellent') {
      res = (miniStats.value[a.id]?.excellent ?? 0) - (miniStats.value[b.id]?.excellent ?? 0)
    } else if (sortBy.value === 'good') {
      res = (miniStats.value[a.id]?.good ?? 0) - (miniStats.value[b.id]?.good ?? 0)
    }
    if (res === 0) res = a.fio.localeCompare(b.fio, 'ru')
    return sortDir.value === 'asc' ? res : -res
  })
})
const totalPages     = computed(() => Math.max(1, Math.ceil(filteredSportsmen.value.length / PER_PAGE)))
const pagedSportsmen = computed(() => filteredSportsmen.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))
watch([search, filterSpec], () => { page.value = 1 })

// modal
const modalSportsman    = ref<any | null>(null)
const modalLoading      = ref(false)
const modalPeriodOffset = ref(0)
const modalGtoResults   = ref<any[]>([])
const modalLocalResults = ref<any[]>([])

const modalPeriodFrom = computed(() => {
  const d = new Date()
  d.setMonth(d.getMonth() - (modalPeriodOffset.value + 1) * 6)
  d.setHours(0, 0, 0, 0)
  return d
})
const modalPeriodTo = computed(() => {
  const d = new Date()
  d.setMonth(d.getMonth() - modalPeriodOffset.value * 6)
  d.setHours(23, 59, 59, 999)
  return d
})
const modalPeriodLabel = computed(() => {
  const fmt = (d: Date) => d.toLocaleDateString('ru-RU', { month: 'short', year: 'numeric' })
  return `${fmt(modalPeriodFrom.value)} — ${fmt(modalPeriodTo.value)}`
})
function prevModalPeriod() { if (modalPeriodOffset.value > 0) modalPeriodOffset.value-- }
function nextModalPeriod() { modalPeriodOffset.value++ }

const modalPeriodGto = computed(() =>
  modalGtoResults.value
    .filter(r => { const d = new Date(r.createdAt); return d >= modalPeriodFrom.value && d <= modalPeriodTo.value })
    .map(r => {
      const norm = gtoNorms.value.find(n => n.id === r.normativeId)
      return { ...r, type: norm?.type ?? '—', unit: norm?.unit ?? '', grade: calcGrade(r.result, norm) }
    })
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
)
const modalPeriodLocal = computed(() =>
  modalLocalResults.value
    .filter(r => { const d = new Date(r.createdAt); return d >= modalPeriodFrom.value && d <= modalPeriodTo.value })
    .map(r => {
      const norm = localNorms.value.find(n => n.id === r.localNormativeId)
      return { ...r, type: norm?.type ?? '—', unit: norm?.unit ?? '', grade: calcLocalGrade(r.result, norm) }
    })
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
)

async function openModal(s: any) {
  modalSportsman.value = s
  modalPeriodOffset.value = 0
  modalLoading.value = true
  modalGtoResults.value = []
  modalLocalResults.value = []
  const [gtoRes, localRes] = await Promise.allSettled([
    api.get(`/normative/gto/results/sportsman/${s.id}`),
    api.get(`/normative/local/results/sportsman/${s.id}`),
  ])
  if (gtoRes.status === 'fulfilled')   modalGtoResults.value   = gtoRes.value.data.data ?? []
  if (localRes.status === 'fulfilled') modalLocalResults.value = localRes.value.data.data ?? []
  modalLoading.value = false
}

function calcGrade(result: number, norm: any): string {
  if (!norm) return 'none'
  const lower = norm.gradeExcellent < norm.gradeSatisfactory
  if (lower) {
    if (result <= norm.gradeExcellent)    return 'excellent'
    if (result <= norm.gradeGood)         return 'good'
    if (result <= norm.gradeSatisfactory) return 'satisfactory'
  } else {
    if (result >= norm.gradeExcellent)    return 'excellent'
    if (result >= norm.gradeGood)         return 'good'
    if (result >= norm.gradeSatisfactory) return 'satisfactory'
  }
  return 'none'
}
function calcLocalGrade(result: number, norm: any): string {
  if (!norm) return 'none'
  return norm.isMoreBetter ? (result >= norm.value ? 'pass' : 'fail') : (result <= norm.value ? 'pass' : 'fail')
}
function gradeLabel(g: string)       { return ({ excellent: 'Отлично', good: 'Хорошо', satisfactory: 'Удовл.', none: '—' } as any)[g] ?? '—' }
function localGradeLabel(g: string)  { return ({ pass: 'Выполнено', fail: 'Не выполнено', none: '—' } as any)[g] ?? '—' }
function gradeBadge(g: string)       { return ({ excellent: 'bg-green-100 text-green-700', good: 'bg-blue-100 text-blue-700', satisfactory: 'bg-yellow-100 text-yellow-700', none: 'bg-neutral-100 text-neutral-500' } as any)[g] ?? 'bg-neutral-100 text-neutral-500' }
function gradeBorder(g: string)      { return ({ excellent: 'border-green-200 bg-green-50/40', good: 'border-blue-200 bg-blue-50/40', satisfactory: 'border-yellow-200 bg-yellow-50/40', none: 'border-neutral-200 bg-white' } as any)[g] ?? 'border-neutral-200 bg-white' }
function localGradeBadge(g: string)  { return ({ pass: 'bg-teal-100 text-teal-700', fail: 'bg-red-100 text-red-600', none: 'bg-neutral-100 text-neutral-500' } as any)[g] ?? 'bg-neutral-100 text-neutral-500' }
function localGradeBorder(g: string) { return ({ pass: 'border-green-200 bg-green-50/40', fail: 'border-red-200 bg-red-50/30', none: 'border-neutral-200 bg-white' } as any)[g] ?? 'border-neutral-200 bg-white' }

function initials(fio: string) {
  return fio.trim().split(' ').slice(0, 2).map(w => w[0]?.toUpperCase() ?? '').join('')
}

async function loadMiniStats(sportsmen: any[]) {
  const results = await Promise.allSettled(
    sportsmen.map(s => Promise.all([
      api.get(`/normative/gto/results/sportsman/${s.id}`).catch(() => null),
      api.get(`/normative/local/results/sportsman/${s.id}`).catch(() => null),
    ]).then(([gtoRes, localRes]) => ({
      id: s.id,
      gto:   gtoRes?.data?.data   ?? [],
      local: localRes?.data?.data ?? [],
    })))
  )
  const stats: typeof miniStats.value = {}
  for (const r of results) {
    if (r.status !== 'fulfilled') continue
    const { id, gto, local } = r.value
    const from = periodFrom.value
    const to   = periodTo.value
    const periodGto   = gto.filter((x: any)   => { const d = new Date(x.createdAt); return d >= from && d <= to })
    const periodLocal = local.filter((x: any) => { const d = new Date(x.createdAt); return d >= from && d <= to })
    let excellent = 0, good = 0, satisfactory = 0, pass = 0
    for (const item of periodGto) {
      const norm = gtoNorms.value.find((n: any) => n.id === item.normativeId)
      const g = calcGrade(item.result, norm)
      if (g === 'excellent') excellent++
      else if (g === 'good') good++
      else if (g === 'satisfactory') satisfactory++
    }
    for (const item of periodLocal) {
      const norm = localNorms.value.find((n: any) => n.id === item.localNormativeId)
      if (calcLocalGrade(item.result, norm) === 'pass') pass++
    }
    stats[id] = { excellent, good, satisfactory, pass }
  }
  miniStats.value = stats
}

onMounted(async () => {
  loading.value = true
  const [sportsmenRes, gtoNormRes, localNormRes] = await Promise.allSettled([
    api.get('/sportsman'),
    api.get('/normative/gto'),
    api.get('/normative/local'),
  ])
  allSportsmen.value = sportsmenRes.status === 'fulfilled' ? (sportsmenRes.value.data.data ?? []) : []
  gtoNorms.value     = gtoNormRes.status   === 'fulfilled' ? (gtoNormRes.value.data.data   ?? []) : []
  localNorms.value   = localNormRes.status === 'fulfilled' ? (localNormRes.value.data.data ?? []) : []
  loading.value = false
  loadMiniStats(allSportsmen.value)
})
</script>
