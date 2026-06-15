<template>
  <div class="p-3 h-full relative">
  <TrainerPageCard color="blue" class="h-full flex flex-col">
  <AdminListLayout
    title="Нормативы учеников"
    :search="search"
    :loading="loading"
    :items="filteredSportsmen"
    :page="page"
    :total="filteredSportsmen.length"
    :total-pages="totalPages"
    :sort-dir="sortDir"
    :sort-label="currentSortLabel"
    @update:search="v => { search = v; page = 1 }"
    @prev="page > 1 && page--"
    @next="page < totalPages && page++"
    @toggle-dir="sortDir = sortDir === 'asc' ? 'desc' : 'asc'"
  >
    <template #filter>
      <select v-model="sourceFilter" @change="page = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
        <option value="">Все</option>
        <option value="own-group">Мои группы</option>
        <option value="own-team">Мои команды</option>
        <option value="other-group">Чужие группы</option>
        <option value="other-team">Чужие команды</option>
      </select>
    </template>
    <template #actions>
      <div class="flex items-center gap-1.5 flex-shrink-0">
        <button @click="nextPeriod" class="p-1.5 rounded-lg border border-neutral-200 bg-neutral-50 hover:bg-neutral-100 transition-colors shrink-0">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/></svg>
        </button>
        <span class="text-xs text-neutral-500 min-w-[100px] md:min-w-[110px] text-center whitespace-nowrap">{{ periodLabel }}</span>
        <button @click="prevPeriod" :disabled="periodOffset === 0" class="p-1.5 rounded-lg border border-neutral-200 bg-neutral-50 hover:bg-neutral-100 disabled:opacity-30 disabled:cursor-not-allowed transition-colors shrink-0">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/></svg>
        </button>
      </div>
    </template>

<template #sort="{ close }">
      <button v-for="o in sortOptions" :key="o.value" @click="sortBy = o.value; page = 1; close()"
        class="w-full text-left px-3 py-2 text-xs hover:bg-neutral-50 transition-colors"
        :class="sortBy === o.value ? 'text-blue-600 font-semibold' : 'text-neutral-600'">{{ o.label }}</button>
    </template>

    <template #items>
      <!-- На десктопе: ФИО слева + ГТО/Лок. справа в одну строку (как было раньше).
           На мобиле: ФИО сверху, ГТО/Лок. снизу отдельной строкой — иначе зажимается. -->
      <div
        v-for="s in pagedSportsmen"
        :key="s.id"
        @click="openModal(s)"
        class="bg-white rounded-2xl border border-neutral-200 p-3 flex flex-col md:flex-row md:items-center gap-3 cursor-pointer hover:border-blue-200 hover:bg-blue-50/30 transition-all"
      >
        <!-- Аватарка + ФИО -->
        <div class="flex items-center gap-3 flex-1 min-w-0">
          <div class="w-10 h-10 rounded-xl bg-neutral-100 flex items-center justify-center flex-shrink-0 text-base font-bold text-neutral-500">
            {{ initials(s.fio) }}
          </div>
          <div class="flex-1 min-w-0">
            <div class="text-base font-semibold text-neutral-800 truncate">{{ s.fio }}</div>
            <div class="flex items-center gap-1.5 mt-0.5 flex-wrap">
              <span v-if="s.age" class="text-xs text-neutral-500">{{ s.age }} лет</span>
              <span v-if="s.groupName" class="text-xs px-2 py-0.5 rounded-full bg-blue-100 text-blue-600 font-medium">{{ s.groupName }}</span>
              <span v-if="s.teamName" class="text-xs px-2 py-0.5 rounded-full bg-green-100 text-green-600 font-medium">{{ s.teamName }}</span>
            </div>
          </div>
          <!-- Стрелка справа от ФИО — только на мобиле, на десктопе она в конце карточки -->
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="md:hidden w-4 h-4 text-neutral-300 flex-shrink-0">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/>
          </svg>
        </div>

        <!-- Блок с результатами ГТО/Локальные.
             На десктопе фиксированной ширины справа; на мобиле — на всю ширину под ФИО, в 2 колонки. -->
        <div class="grid grid-cols-2 md:grid-cols-1 md:min-w-[160px] md:flex-shrink-0 gap-2 md:gap-1">
          <div>
            <div class="text-[10px] font-bold text-neutral-600 uppercase tracking-wide mb-0.5">ГТО:</div>
            <div class="flex items-center gap-1 flex-wrap md:justify-end">
              <span v-if="miniStats[s.id]?.excellent"    class="text-[10px] md:text-xs px-1.5 md:px-2 py-0.5 rounded-full bg-green-100 text-green-700 font-semibold whitespace-nowrap">{{ miniStats[s.id].excellent }} отл</span>
              <span v-if="miniStats[s.id]?.good"         class="text-[10px] md:text-xs px-1.5 md:px-2 py-0.5 rounded-full bg-blue-100 text-blue-700 font-semibold whitespace-nowrap">{{ miniStats[s.id].good }} хор</span>
              <span v-if="miniStats[s.id]?.satisfactory" class="text-[10px] md:text-xs px-1.5 md:px-2 py-0.5 rounded-full bg-yellow-100 text-yellow-700 font-semibold whitespace-nowrap">{{ miniStats[s.id].satisfactory }} удовл</span>
              <span v-if="!miniStats[s.id]?.excellent && !miniStats[s.id]?.good && !miniStats[s.id]?.satisfactory" class="text-xs text-neutral-400">—</span>
            </div>
          </div>
          <div>
            <div class="text-[10px] font-bold text-neutral-600 uppercase tracking-wide mb-0.5">Локальные:</div>
            <div class="flex items-center gap-1 flex-wrap md:justify-end">
              <span v-if="miniStats[s.id]?.pass" class="text-[10px] md:text-xs px-1.5 md:px-2 py-0.5 rounded-full bg-teal-100 text-teal-700 font-semibold whitespace-nowrap">{{ miniStats[s.id].pass }} выпол</span>
              <span v-else class="text-xs text-neutral-400">—</span>
            </div>
          </div>
        </div>

        <!-- Стрелка справа — только на десктопе -->
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="hidden md:block w-4 h-4 text-neutral-300 flex-shrink-0">
          <path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/>
        </svg>
      </div>
    </template>
  </AdminListLayout>

    <!-- Modal -->
    <Teleport to="body">
      <div v-if="modalSportsman" class="fixed inset-0 bg-black/30 z-50 flex items-end sm:items-center justify-center p-3" @click.self="modalSportsman = null">
        <div class="bg-white rounded-2xl border border-neutral-200 shadow-xl w-full max-w-lg flex flex-col max-h-[85vh]">

          <!-- Modal header -->
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

          <!-- Period nav -->
          <div class="flex items-center justify-between gap-2 px-4 py-2 border-b border-neutral-100">
            <button @click="nextModalPeriod" class="p-1 rounded-lg hover:bg-neutral-100 transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/></svg>
            </button>
            <span class="text-xs text-neutral-500">{{ modalPeriodLabel }}</span>
            <button @click="prevModalPeriod" :disabled="modalPeriodOffset === 0" class="p-1 rounded-lg hover:bg-neutral-100 disabled:opacity-30 transition-colors">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-500"><path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7"/></svg>
            </button>
          </div>

          <!-- Modal body: две колонки на десктопе, стек на мобиле -->
          <div v-if="modalLoading" class="p-6 text-center text-sm text-neutral-400">Загрузка...</div>
          <div v-else class="flex-1 overflow-y-auto p-4">
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">

              <!-- GTO -->
              <div class="flex flex-col gap-2">
                <div class="text-xs font-bold text-neutral-400 uppercase tracking-wide mb-1">ГТО</div>
                <div v-if="!modalPeriodGto.length" class="text-xs text-neutral-400">Нет результатов</div>
                <div
                  v-for="item in modalPeriodGto"
                  :key="item.id"
                  class="rounded-xl border p-2.5"
                  :class="gradeBorder(item.grade)"
                >
                  <div class="text-xs font-semibold text-neutral-800 truncate">{{ item.type }}</div>
                  <div class="flex items-center justify-between mt-1 gap-1">
                    <span class="text-xs font-bold text-neutral-900">{{ item.result }} {{ item.unit }}</span>
                    <span class="text-[10px] font-bold px-1.5 py-0.5 rounded-full" :class="gradeBadge(item.grade)">{{ gradeLabel(item.grade) }}</span>
                  </div>
                  <div class="text-[10px] text-neutral-400 mt-1.5 flex flex-col gap-0.5">
                    <span>Отл: {{ item.norm?.gradeExcellent }} · Хор: {{ item.norm?.gradeGood }} · Удовл: {{ item.norm?.gradeSatisfactory }} {{ item.unit }}</span>
                  </div>
                  <div class="text-[10px] text-neutral-400 mt-0.5">{{ formatDate(item.createdAt) }}</div>
                </div>
              </div>

              <!-- Local -->
              <div class="flex flex-col gap-2">
                <div class="text-xs font-bold text-neutral-400 uppercase tracking-wide mb-1">Локальные</div>
                <div v-if="!modalPeriodLocal.length" class="text-xs text-neutral-400">Нет результатов</div>
                <div
                  v-for="item in modalPeriodLocal"
                  :key="item.id"
                  class="rounded-xl border p-2.5"
                  :class="localGradeBorder(item.grade)"
                >
                  <div class="text-xs font-semibold text-neutral-800 truncate">{{ item.type }}</div>
                  <div class="flex items-center justify-between mt-1 gap-1">
                    <span class="text-xs font-bold text-neutral-900">{{ item.result }} {{ item.unit }}</span>
                    <span class="text-[10px] font-bold px-1.5 py-0.5 rounded-full" :class="localGradeBadge(item.grade)">{{ localGradeLabel(item.grade) }}</span>
                  </div>
                  <div class="text-[10px] text-neutral-400 mt-1.5">
                    Порог: {{ item.norm?.value }} {{ item.unit }} ({{ item.norm?.isMoreBetter ? '≥' : '≤' }})
                  </div>
                  <div class="text-[10px] text-neutral-400 mt-0.5">{{ formatDate(item.createdAt) }}</div>
                </div>
              </div>

            </div>
          </div>
        </div>
      </div>
    </Teleport>
  </TrainerPageCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import api from '@/services/api'
import { useAuthStore } from '@/store/auth'
import { formatDate } from '@/utils/formatDate'
import AdminListLayout from '@/components/ui/AdminListLayout.vue'
import TrainerPageCard from '@/components/trainer/TrainerPageCard.vue'

const auth = useAuthStore()

const PER_PAGE = 15

const loading      = ref(true)
const search       = ref('')
const sourceFilter = ref('')
const page         = ref(1)
const periodOffset = ref(0)
const sortBy       = ref('fio')
const sortDir      = ref<'asc' | 'desc'>('asc')

const sortOptions = [
  { value: 'fio',   label: 'По имени' },
  { value: 'total', label: 'По сданным' },
]

const currentSortLabel = computed(() => {
  const o = sortOptions.find(o => o.value === sortBy.value)
  if (!o) return ''
  if (sortBy.value === 'fio') return `По имени: ${sortDir.value === 'asc' ? 'А → Я' : 'Я → А'}`
  return sortDir.value === 'desc' ? 'Лучшие → худшие' : 'Худшие → лучшие'
})

const gtoNorms   = ref<any[]>([])
const localNorms = ref<any[]>([])

interface SportsmanEntry {
  id: number
  fio: string
  age: number | null
  groupName: string
  teamName: string
  ownGroup: boolean
  ownTeam: boolean
  sources: { id: number; type: 'group' | 'team'; name: string }[]
}

const allSportsmen = ref<SportsmanEntry[]>([])
const miniStats    = ref<Record<number, { excellent: number; good: number; satisfactory: number; pass: number }>>({})

// period helpers
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

// filtering, sorting & pagination
const filteredSportsmen = computed(() => {
  const q = search.value.toLowerCase()
  const list = allSportsmen.value.filter(s => {
    if (q && !s.fio.toLowerCase().includes(q)) return false
    if (sourceFilter.value === 'own-group')   return s.ownGroup
    if (sourceFilter.value === 'own-team')    return s.ownTeam && !s.ownGroup
    if (sourceFilter.value === 'other-group') return !!s.groupName && !s.ownGroup
    if (sourceFilter.value === 'other-team')  return !!s.teamName && !s.ownTeam && !s.ownGroup
    return true
  })
  const total = (id: number) => {
    const s = miniStats.value[id]
    return s ? s.excellent + s.good + s.pass : 0
  }
  return [...list].sort((a, b) => {
    let res = 0
    if (sortBy.value === 'fio') {
      res = a.fio.localeCompare(b.fio, 'ru')
    } else {
      res = total(a.id) - total(b.id)
    }
    if (res === 0) res = a.fio.localeCompare(b.fio, 'ru')
    return sortDir.value === 'asc' ? res : -res
  })
})
const totalPages     = computed(() => Math.max(1, Math.ceil(filteredSportsmen.value.length / PER_PAGE)))
const pagedSportsmen = computed(() => filteredSportsmen.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))
watch(search, () => { page.value = 1 })
watch(periodOffset, () => { loadMiniStats(allSportsmen.value) })

// modal state
const modalSportsman     = ref<SportsmanEntry | null>(null)
const modalLoading       = ref(false)
const modalPeriodOffset  = ref(0)
const modalGtoResults    = ref<any[]>([])
const modalLocalResults  = ref<any[]>([])

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
      return { ...r, type: norm?.type ?? '—', unit: norm?.unit ?? '', grade: calcGrade(r.result, norm), norm }
    })
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
)
const modalPeriodLocal = computed(() =>
  modalLocalResults.value
    .filter(r => { const d = new Date(r.createdAt); return d >= modalPeriodFrom.value && d <= modalPeriodTo.value })
    .map(r => {
      const norm = localNorms.value.find(n => n.id === r.localNormativeId)
      return { ...r, type: norm?.type ?? '—', unit: norm?.unit ?? '', grade: calcLocalGrade(r.result, norm), norm }
    })
    .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime())
)

async function openModal(s: SportsmanEntry) {
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

// grade helpers
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
function gradeLabel(g: string)       { return ({ excellent: 'Отл.', good: 'Хор.', satisfactory: 'Удовл.', none: '—' } as any)[g] ?? '—' }
function localGradeLabel(g: string)  { return ({ pass: 'Выполнено', fail: 'Не выполнено', none: '—' } as any)[g] ?? '—' }
function gradeBadge(g: string)       { return ({ excellent: 'bg-green-100 text-green-700', good: 'bg-blue-100 text-blue-700', satisfactory: 'bg-yellow-100 text-yellow-700', none: 'bg-neutral-100 text-neutral-500' } as any)[g] ?? 'bg-neutral-100 text-neutral-500' }
function gradeBorder(g: string)      { return ({ excellent: 'border-green-200 bg-green-50/40', good: 'border-blue-200 bg-blue-50/40', satisfactory: 'border-yellow-200 bg-yellow-50/40', none: 'border-neutral-200 bg-white' } as any)[g] ?? 'border-neutral-200 bg-white' }
function localGradeBadge(g: string)  { return ({ pass: 'bg-teal-100 text-teal-700', fail: 'bg-red-100 text-red-600', none: 'bg-neutral-100 text-neutral-500' } as any)[g] ?? 'bg-neutral-100 text-neutral-500' }
function localGradeBorder(g: string) { return ({ pass: 'border-green-200 bg-green-50/40', fail: 'border-red-200 bg-red-50/30', none: 'border-neutral-200 bg-white' } as any)[g] ?? 'border-neutral-200 bg-white' }

function initials(fio: string) {
  return fio.trim().split(' ').slice(0, 2).map(w => w[0]?.toUpperCase() ?? '').join('')
}

async function loadMiniStats(sportsmen: SportsmanEntry[]) {
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
      if (g === 'excellent')    excellent++
      else if (g === 'good')    good++
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
  const tid = auth.personalId
  if (!tid) { loading.value = false; return }

  const [allGroupsRes, allTeamsRes, myGroupsRes, myTeamsRes, gtoNormRes, localNormRes] = await Promise.allSettled([
    api.get('/group'),
    api.get('/team'),
    api.get('/group', { params: { filters: { trainerId: [tid] } } }),
    api.get('/team',  { params: { filters: { trainerId: [tid] } } }),
    api.get('/normative/gto'),
    api.get('/normative/local'),
  ])

  if (gtoNormRes.status === 'fulfilled')   gtoNorms.value   = gtoNormRes.value.data.data ?? []
  if (localNormRes.status === 'fulfilled') localNorms.value = localNormRes.value.data.data ?? []

  const allGroups: any[] = allGroupsRes.status === 'fulfilled' ? (allGroupsRes.value.data.data ?? []) : []
  const allTeams:  any[] = allTeamsRes.status  === 'fulfilled' ? (allTeamsRes.value.data.data  ?? []) : []
  const myGroupIds = new Set<number>((myGroupsRes.status === 'fulfilled' ? myGroupsRes.value.data.data ?? [] : []).map((g: any) => g.id))
  const myTeamIds  = new Set<number>((myTeamsRes.status  === 'fulfilled' ? myTeamsRes.value.data.data  ?? [] : []).map((t: any) => t.id))

  const groupSportsmenResults = await Promise.allSettled(
    allGroups.map((g: any) => api.get(`/sportsman/group/${g.id}`).then(r => ({ groupId: g.id, groupName: g.name, own: myGroupIds.has(g.id), sportsmen: r.data.data ?? [] })))
  )

  const sportsmenMap = new Map<number, SportsmanEntry>()

  for (const r of groupSportsmenResults) {
    if (r.status !== 'fulfilled') continue
    const { groupId, groupName, own, sportsmen } = r.value
    for (const s of sportsmen) {
      if (!sportsmenMap.has(s.id)) {
        sportsmenMap.set(s.id, { id: s.id, fio: s.fio, age: s.age ?? null, groupName, teamName: '', ownGroup: own, ownTeam: false, sources: [] })
      }
      const entry = sportsmenMap.get(s.id)!
      if (!entry.groupName) { entry.groupName = groupName; entry.ownGroup = own }
      if (own && !entry.ownGroup) entry.ownGroup = true
      if (!entry.sources.find(src => src.type === 'group' && src.id === groupId)) {
        entry.sources.push({ id: groupId, type: 'group', name: groupName })
      }
    }
  }

  const teamSportsmenResults = await Promise.allSettled(
    allTeams.map((t: any) => api.get('/sportsman', { params: { filters: { teamId: [t.id] } } }).then(r => ({ teamId: t.id, teamName: t.name, own: myTeamIds.has(t.id), sportsmen: r.data.data ?? [] })))
  )
  for (const r of teamSportsmenResults) {
    if (r.status !== 'fulfilled') continue
    const { teamId, teamName, own, sportsmen } = r.value
    for (const s of sportsmen) {
      if (!sportsmenMap.has(s.id)) {
        sportsmenMap.set(s.id, { id: s.id, fio: s.fio, age: s.age ?? null, groupName: '', teamName, ownGroup: false, ownTeam: own, sources: [] })
      }
      const entry = sportsmenMap.get(s.id)!
      if (!entry.teamName) { entry.teamName = teamName; entry.ownTeam = own }
      if (own && !entry.ownTeam) entry.ownTeam = true
      if (!entry.sources.find(src => src.type === 'team' && src.id === teamId)) {
        entry.sources.push({ id: teamId, type: 'team', name: teamName })
      }
    }
  }

  allSportsmen.value = Array.from(sportsmenMap.values()).sort((a, b) => a.fio.localeCompare(b.fio, 'ru'))
  loading.value = false

  loadMiniStats(allSportsmen.value)
})
</script>

