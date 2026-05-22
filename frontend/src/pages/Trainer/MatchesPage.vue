<template>
  <div class="p-3 h-full">
    <TrainerPageCard color="blue" class="h-full">
      <AdminListLayout
        title="Матчи моих команд"
        :search="search"
        :loading="loading"
        :items="paged"
        :page="page"
        :total-pages="totalPages"
        :total="filtered.length"
        :sort-dir="sortDir"
        :sort-label="currentSortLabel"
        @update:search="search = $event; page = 1"
        @prev="page--"
        @next="page++"
        @toggle-dir="sortDir = sortDir === 'asc' ? 'desc' : 'asc'"
      >
    <template #filter>
      <select v-model="filterTeam" @change="page = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
        <option value="">Все команды</option>
        <option v-for="t in teams" :key="t.id" :value="String(t.id)">{{ t.name }}</option>
      </select>
      <select v-model="filterResult" @change="page = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
        <option value="">Все результаты</option>
        <option value="Win">Победы</option>
        <option value="Draw">Ничьи</option>
        <option value="Loss">Поражения</option>
      </select>
    </template>

    <template #sort="{ close }">
      <button v-for="o in sortOptions" :key="o.value" @click="sortBy = o.value; close()"
        class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors"
        :class="sortBy === o.value ? 'text-blue-600 font-semibold' : 'text-neutral-600'">{{ o.label }}</button>
    </template>

    <template #items>
      <div v-for="m in paged" :key="m.id" @click="$router.push(`/trainer/matches/${m.id}`)" class="bg-white rounded-2xl border border-neutral-200 p-4 cursor-pointer hover:border-blue-200 hover:bg-blue-50/30 transition-all">
        <div class="flex items-center justify-between mb-3 pb-3 border-b border-neutral-100">
          <span class="text-xs font-semibold px-2 py-0.5 rounded-full" :class="statusClass(m.status)">
            {{ statusLabel[m.status] ?? m.status }}
          </span>
          <span class="text-xs text-neutral-400">{{ formatDate(m.date) }}</span>
        </div>
        
        <div class="flex items-center justify-between gap-4">
          <div class="flex flex-col items-center gap-2 flex-1">
            <div class="w-14 h-14 rounded-full overflow-hidden border-2 border-neutral-200 flex items-center justify-center bg-neutral-50 flex-shrink-0">
              <img v-if="teamImages[m.homeTeamId]" :src="teamImages[m.homeTeamId]" class="w-full h-full object-cover" />
              <span v-else class="text-sm font-bold text-blue-600">{{ initials(m.homeTeamName) }}</span>
            </div>
            <span class="text-xs font-semibold text-neutral-800 text-center leading-tight max-w-[100px] line-clamp-2">{{ m.homeTeamName }}</span>
          </div>

          <div class="flex flex-col items-center gap-1 min-w-[70px]">
            <div v-if="m.status !== 'Scheduled'" class="text-2xl font-extrabold text-neutral-900">
              {{ m.homeStats?.goals ?? 0 }} : {{ m.awayStats?.goals ?? 0 }}
            </div>
            <div v-else class="text-2xl font-extrabold text-neutral-400">VS</div>
            <span v-if="m.result" class="text-xs font-bold px-2 py-0.5 rounded-full mt-0.5" :class="resultClass(m.result)">
              {{ resultLabel[m.result] }}
            </span>
            <span class="text-xs text-neutral-400 mt-1">{{ typeLabel[m.type] ?? m.type }}</span>
          </div>

          <div class="flex flex-col items-center gap-2 flex-1">
            <div class="w-14 h-14 rounded-full overflow-hidden border-2 border-neutral-200 flex items-center justify-center bg-neutral-50 flex-shrink-0">
              <img v-if="m.opponentTeamId && teamImages[m.opponentTeamId]" :src="teamImages[m.opponentTeamId]" class="w-full h-full object-cover" />
              <span v-else class="text-sm font-bold text-neutral-500">{{ initials(m.opponentTeamName ?? '?') }}</span>
            </div>
            <span class="text-xs font-semibold text-neutral-800 text-center leading-tight max-w-[100px] line-clamp-2">{{ m.opponentTeamName ?? 'Соперник' }}</span>
          </div>
        </div>
      </div>
    </template>
      </AdminListLayout>
    </TrainerPageCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import { useAuthStore } from '@/store/auth'
import AdminListLayout from '@/components/ui/AdminListLayout.vue'
import TrainerPageCard from '@/components/trainer/TrainerPageCard.vue'
import { formatDate } from '@/utils/formatDate'
import { MATCH_TYPE, MATCH_STATUS, MATCH_RESULT } from '@/constants'

const PER_PAGE = 15
const sortOptions = [
  { value: 'date', label: 'По дате' },
  { value: 'result', label: 'По результату' },
]

const loading = ref(true)
const matches = ref<any[]>([])
const teams = ref<any[]>([])
const teamImages = ref<Record<number, string>>({})
const search = ref('')
const filterTeam = ref('')
const filterResult = ref('')
const sortBy = ref('date')
const sortDir = ref<'asc'|'desc'>('desc')
const page = ref(1)

const statusLabel = MATCH_STATUS
const resultLabel = MATCH_RESULT
const typeLabel = MATCH_TYPE

const currentSortLabel = computed(() => sortOptions.find(o => o.value === sortBy.value)?.label ?? '')

const filtered = computed(() => {
  let list = matches.value
  
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(m => 
      m.homeTeamName?.toLowerCase().includes(q) || 
      m.opponentTeamName?.toLowerCase().includes(q)
    )
  }
  
  if (filterTeam.value) {
    const teamId = Number(filterTeam.value)
    // Команда может быть как домашней, так и соперником (для матчей между нашими)
    list = list.filter(m => m.homeTeamId === teamId || m.opponentTeamId === teamId)
  }
  
  if (filterResult.value) {
    list = list.filter(m => m.result === filterResult.value)
  }
  
  return [...list].sort((a, b) => {
    let res = 0
    if (sortBy.value === 'date') {
      res = new Date(a.date).getTime() - new Date(b.date).getTime()
    } else if (sortBy.value === 'result') {
      // Сортировка по разнице голов (наши голы - голы соперника)
      const aDiff = (a.homeStats?.goals ?? 0) - (a.awayStats?.goals ?? 0)
      const bDiff = (b.homeStats?.goals ?? 0) - (b.awayStats?.goals ?? 0)
      res = aDiff - bDiff
    }
    return sortDir.value === 'asc' ? res : -res
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / PER_PAGE)))
const paged = computed(() => filtered.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

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

const auth = useAuthStore()

async function load() {
  loading.value = true
  const tid = auth.personalId
  const [mRes, tRes] = await Promise.allSettled([
    api.get('/match', { params: tid ? { filters: { trainerId: [tid] } } : undefined }),
    api.get('/team',  { params: tid ? { filters: { trainerId: [tid] } } : undefined }),
  ])
  
  if (mRes.status === 'fulfilled') {
    matches.value = (mRes.value.data.data ?? [])
      .filter((m: any) => m.status === 'Finished')
      .sort((a: any, b: any) => new Date(b.date).getTime() - new Date(a.date).getTime())
  }
  
  if (tRes.status === 'fulfilled') {
    teams.value = tRes.value.data.data ?? []
    const ids = [...new Set(matches.value.flatMap((m: any) => [m.homeTeamId, m.opponentTeamId].filter(Boolean)))] as number[]
    await Promise.allSettled(ids.map(async id => {
      const t = await api.get(`/team/${id}`).catch(() => null)
      const img = t?.data?.data?.images?.[0]?.path ?? null
      if (img) teamImages.value[id] = img
    }))
  }
  
  loading.value = false
}

onMounted(load)
</script>
