<template>
  <div class="p-3 h-full relative">
    <TrainerPageCard color="green" class="h-full flex flex-col">
      <AdminListLayout
        title="Индивидуальные задания"
        :toast="toast"
        :search="search"
        :loading="loading"
        :items="filteredWorkouts"
        :page="page"
        :total-pages="totalPages"
        :total="filteredWorkouts.length"
        :sort-dir="sortDir"
        :sort-label="sortOptions.find(o => o.value === sortBy)?.label"
        @update:search="search = $event; page = 1"
        @prev="page--"
        @next="page++"
        @toggle-dir="sortDir = sortDir === 'asc' ? 'desc' : 'asc'"
      >
        <template #actions>
          <button @click="openCreate"
            class="text-xs px-4 py-1.5 rounded-xl text-white font-semibold transition-colors border bg-green-600 border-green-400 hover:bg-green-700 hover:border-green-500 inline-flex items-center gap-1.5">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/>
            </svg>
            Создать
          </button>
        </template>

        <template #filter>
          <select v-model="groupFilter" @change="page = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
            <option value="">Все</option>
            <optgroup v-if="groups.length" label="Группы">
              <option v-for="g in groups" :key="'g-' + g.id" :value="'g-' + g.id">{{ g.name }}</option>
            </optgroup>
            <optgroup v-if="teams.length" label="Команды">
              <option v-for="t in teams" :key="'t-' + t.id" :value="'t-' + t.id">{{ t.name }}</option>
            </optgroup>
          </select>
        </template>

        <template #sort="{ close }">
          <button v-for="o in sortOptions" :key="o.value"
            @click="sortBy = o.value; page = 1; close()"
            class="w-full text-left px-3 py-2 text-xs hover:bg-neutral-50 transition-colors"
            :class="sortBy === o.value ? 'text-blue-600 font-semibold' : 'text-neutral-600'"
          >{{ o.label }}</button>
        </template>

        <template #items>
          <TrainerCard
            v-for="w in pagedWorkouts"
            :key="w.id"
            padding="p-3"
            @click="openView(w)"
          >
            <div class="flex items-start gap-3">
              <!-- Аватар -->
              <div class="w-10 h-10 rounded-xl bg-green-100 flex items-center justify-center flex-shrink-0 mt-0.5 text-sm font-bold text-green-600">
                {{ initials(w.sportsmanFIO) }}
              </div>

              <!-- Контент -->
              <div class="flex-1 min-w-0">
                <div class="text-base font-semibold text-neutral-800 truncate">{{ w.sportsmanFIO }}</div>
                <div class="text-sm text-neutral-400 mt-0.5">{{ formatDate(w.createdAt) }} · от {{ w.personalFIO }}</div>
                <div class="text-sm text-neutral-600 mt-1.5 line-clamp-2">{{ w.workout }}</div>
              </div>

              <!-- Действия -->
              <div class="flex items-center gap-1 flex-shrink-0" @click.stop>
                <button @click.stop="openEdit(w)"
                  class="w-8 h-8 rounded-lg border border-green-300 bg-green-50 flex items-center justify-center hover:bg-green-100 transition-colors"
                  title="Редактировать">
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-green-600">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
                  </svg>
                </button>
                <button @click.stop="openDelete(w)"
                  class="w-8 h-8 rounded-lg border border-blue-200 bg-blue-50 flex items-center justify-center hover:bg-blue-100 transition-colors"
                  title="Удалить">
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-blue-500">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
                  </svg>
                </button>
              </div>
            </div>
          </TrainerCard>
        </template>
      </AdminListLayout>
    </TrainerPageCard>

  </div>

  <!-- Модалка создания / редактирования -->
  <Teleport to="body">
    <div v-if="editOpen" class="fixed inset-0 bg-black/30 z-50 flex items-center justify-center p-4" @click.self="editOpen = false">
      <div class="bg-white rounded-2xl border border-neutral-200 shadow-xl p-6 w-full max-w-md">
        <div class="flex items-center gap-3 mb-4">
          <div class="w-10 h-10 rounded-xl bg-green-50 flex items-center justify-center flex-shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5 text-green-500">
              <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
            </svg>
          </div>
          <div class="text-sm font-bold text-neutral-900">{{ editItem ? 'Редактировать задание' : 'Новое задание' }}</div>
        </div>

        <div class="flex flex-col gap-3">
          <!-- Группа/команда для выбора спортсмена -->
          <div v-if="!editItem">
            <div class="text-xs font-semibold text-neutral-500 mb-1.5">Группа / команда</div>
            <select v-model="editForm.groupKey" @change="editForm.sportsmanId = 0"
              class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400">
              <option value="">— Выберите группу или команду —</option>
              <optgroup v-if="groups.length" label="Группы">
                <option v-for="g in groups" :key="'g-' + g.id" :value="'g-' + g.id">{{ g.name }}</option>
              </optgroup>
              <optgroup v-if="teams.length" label="Команды">
                <option v-for="t in teams" :key="'t-' + t.id" :value="'t-' + t.id">{{ t.name }}</option>
              </optgroup>
            </select>
          </div>
          <!-- Спортсмен -->
          <div>
            <div class="text-xs font-semibold text-neutral-500 mb-1.5">Спортсмен</div>
            <template v-if="!editItem">
              <SearchableSelect
                v-if="editForm.groupKey"
                v-model="sportsmanIdModel"
                :items="modalSportsmen"
                label-field="fio"
                placeholder="— Выберите спортсмена —"
                search-placeholder="Поиск по ФИО..."
              />
              <div v-else class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 bg-neutral-50 text-neutral-400">
                — Сначала выберите группу или команду —
              </div>
            </template>
            <input
              v-else
              :value="editForm.sportsmanId ? (sportsmen.find(s => s.id === editForm.sportsmanId)?.fio ?? '') : ''"
              disabled
              class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 bg-neutral-50 text-neutral-500"
            />
          </div>
          <!-- Задание -->
          <div>
            <div class="text-xs font-semibold text-neutral-500 mb-1.5">Задание / рекомендации</div>
            <textarea v-model="editForm.workout" rows="6"
              class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 resize-none"
              placeholder="Опишите индивидуальное задание или рекомендации..." />
          </div>
        </div>

        <div class="flex gap-2 justify-end mt-4">
          <button @click="editOpen = false" class="px-4 py-2 rounded-xl border border-neutral-200 text-xs font-semibold text-neutral-600 hover:bg-neutral-50 transition-colors">Отмена</button>
          <button @click="saveEdit" :disabled="!editForm.sportsmanId || !editForm.workout.trim()"
            class="px-4 py-2 rounded-xl text-xs font-semibold transition-colors"
            :class="(!editForm.sportsmanId || !editForm.workout.trim()) ? 'bg-neutral-200 text-neutral-400 cursor-not-allowed' : 'bg-green-600 text-white hover:bg-green-700'">
            Сохранить
          </button>
        </div>
      </div>
    </div>
  </Teleport>

  <!-- Модалка просмотра -->
  <Teleport to="body">
    <div v-if="viewWorkout" class="fixed inset-0 bg-black/30 z-50 flex items-center justify-center p-4" @click.self="viewWorkout = null">
      <div class="bg-white rounded-2xl border border-neutral-200 shadow-xl w-full max-w-lg flex flex-col max-h-[80vh]">
        <div class="flex items-center gap-3 p-5 border-b border-neutral-100">
          <div class="w-9 h-9 rounded-xl bg-green-100 flex items-center justify-center flex-shrink-0 text-xs font-bold text-green-600">
            {{ initials(viewWorkout.sportsmanFIO) }}
          </div>
          <div class="flex-1 min-w-0">
            <div class="text-sm font-bold text-neutral-900 truncate">{{ viewWorkout.sportsmanFIO }}</div>
            <div class="text-xs text-neutral-400 mt-0.5">{{ formatDate(viewWorkout.createdAt) }} · от {{ viewWorkout.personalFIO }}</div>
          </div>
          <button @click="viewWorkout = null" class="w-8 h-8 rounded-lg flex items-center justify-center text-neutral-400 hover:bg-neutral-100 transition-colors flex-shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>
        <div class="flex-1 overflow-y-auto p-5">
          <div class="text-[10px] font-bold text-neutral-400 uppercase tracking-wide mb-2">Задание</div>
          <div class="text-sm text-neutral-700 whitespace-pre-wrap leading-relaxed">{{ viewWorkout.workout }}</div>
        </div>
        <div class="p-4 border-t border-neutral-100 flex gap-2">
          <button @click="viewWorkout = null" class="flex-1 py-2 rounded-xl border border-neutral-200 text-xs font-semibold text-neutral-600 hover:bg-neutral-50 transition-colors">Закрыть</button>
          <button @click="openEdit(viewWorkout); viewWorkout = null"
            class="px-4 py-2 rounded-xl bg-green-50 border border-green-200 text-xs font-semibold text-green-700 hover:bg-green-100 transition-colors">
            Редактировать
          </button>
        </div>
      </div>
    </div>
  </Teleport>

  <!-- Подтверждение удаления -->
  <ConfirmDeleteModal
    v-if="deleteItem"
    :message="`Удалить задание для «${deleteItem.sportsmanFIO}»?`"
    @confirm="confirmDelete"
    @cancel="deleteItem = null"
  />
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import { useAuthStore } from '@/store/auth'
import { useToast } from '@/composables/useToast'
import { formatDate } from '@/utils/formatDate'
import AdminListLayout from '@/components/ui/AdminListLayout.vue'
import TrainerCard from '@/components/trainer/TrainerCard.vue'
import TrainerPageCard from '@/components/trainer/TrainerPageCard.vue'
import ConfirmDeleteModal from '@/components/ui/ConfirmDeleteModal.vue'
import SearchableSelect from '@/components/trainer/SearchableSelect.vue'

const auth = useAuthStore()
const { toast, showToast } = useToast()

const PER_PAGE = 15

const loading      = ref(true)
const allWorkouts  = ref<any[]>([])
const sportsmen    = ref<any[]>([])
const groups       = ref<any[]>([])
const teams        = ref<any[]>([])
const sportsmenByGroup = ref<Record<number, any[]>>({})
const sportsmenByTeam  = ref<Record<number, any[]>>({})
const search       = ref('')
const groupFilter  = ref('')
const sortBy       = ref('date')
const sortDir      = ref<'asc' | 'desc'>('desc')
const page         = ref(1)

const editOpen   = ref(false)
const editItem   = ref<any>(null)
const editForm   = ref({ sportsmanId: 0, workout: '', groupKey: '' })
const deleteItem = ref<any>(null)
const viewWorkout = ref<any>(null)

const sortOptions = [
  { value: 'date',      label: 'По дате' },
  { value: 'sportsman', label: 'По спортсмену' },
]

const modalSportsmen = computed(() => {
  if (!editForm.value.groupKey) return sportsmen.value
  if (editForm.value.groupKey.startsWith('g-')) {
    const id = Number(editForm.value.groupKey.slice(2))
    return sportsmenByGroup.value[id] ?? []
  }
  if (editForm.value.groupKey.startsWith('t-')) {
    const id = Number(editForm.value.groupKey.slice(2))
    return sportsmenByTeam.value[id] ?? []
  }
  return sportsmen.value
})

const sportsmanIdModel = computed<number | string | null>({
  get: () => editForm.value.sportsmanId || null,
  set: (v) => { editForm.value.sportsmanId = v == null ? 0 : Number(v) },
})

function initials(fio: string): string {
  return (fio ?? '').trim().split(' ').slice(0, 2).map(w => w[0]?.toUpperCase() ?? '').join('')
}


const filteredGroupIds = computed<Set<number> | null>(() => {
  if (!groupFilter.value) return null
  if (groupFilter.value.startsWith('g-')) {
    const id = Number(groupFilter.value.slice(2))
    const ids = (sportsmenByGroup.value[id] ?? []).map((s: any) => s.id)
    return new Set(ids)
  }
  if (groupFilter.value.startsWith('t-')) {
    const id = Number(groupFilter.value.slice(2))
    const ids = (sportsmenByTeam.value[id] ?? []).map((s: any) => s.id)
    return new Set(ids)
  }
  return null
})

const filteredWorkouts = computed(() => {
  let list = allWorkouts.value

  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(w =>
      w.sportsmanFIO?.toLowerCase().includes(q) ||
      w.workout?.toLowerCase().includes(q) ||
      w.personalFIO?.toLowerCase().includes(q)
    )
  }

  if (filteredGroupIds.value) {
    list = list.filter(w => filteredGroupIds.value!.has(w.sportsmanId))
  }

  return [...list].sort((a, b) => {
    let res = 0
    if (sortBy.value === 'date') {
      res = new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    } else if (sortBy.value === 'sportsman') {
      res = (a.sportsmanFIO ?? '').localeCompare(b.sportsmanFIO ?? '', 'ru')
    }
    return sortDir.value === 'asc' ? -res : res
  })
})

const totalPages   = computed(() => Math.max(1, Math.ceil(filteredWorkouts.value.length / PER_PAGE)))
const pagedWorkouts = computed(() => filteredWorkouts.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

function openCreate() {
  editItem.value = null
  editForm.value = { sportsmanId: 0, workout: '', groupKey: '' }
  editOpen.value = true
}

function openEdit(w: any) {
  editItem.value = w
  editForm.value = { sportsmanId: w.sportsmanId, workout: w.workout ?? '', groupKey: '' }
  editOpen.value = true
}

function openDelete(w: any) {
  deleteItem.value = w
}

function openView(w: any) {
  viewWorkout.value = w
}

async function saveEdit() {
  const tid = auth.personalId
  if (!tid || !editForm.value.sportsmanId) return

  if (editItem.value) {
    // API не имеет PUT — удаляем старое и создаём новое
    await api.delete(`/personal/workout/${editItem.value.id}`)
    const res = await api.post('/personal/workout', {
      sportsmanId: editItem.value.sportsmanId,
      personalId:  tid,
      workout:     editForm.value.workout,
    })
    const created = res.data.data
    if (created) {
      const idx = allWorkouts.value.findIndex(w => w.id === editItem.value.id)
      if (idx !== -1) allWorkouts.value.splice(idx, 1, created)
      else allWorkouts.value.unshift(created)
    }
  } else {
    const res = await api.post('/personal/workout', {
      sportsmanId: editForm.value.sportsmanId,
      personalId:  tid,
      workout:     editForm.value.workout,
    })
    const created = res.data.data
    if (created) allWorkouts.value.unshift(created)
  }

  const wasEdit = editItem.value !== null
  editOpen.value = false
  if (wasEdit) showToast('saved')
}

async function confirmDelete() {
  if (!deleteItem.value) return
  await api.delete(`/personal/workout/${deleteItem.value.id}`)
  allWorkouts.value = allWorkouts.value.filter(w => w.id !== deleteItem.value.id)
  deleteItem.value = null
  showToast('deleted')
}

onMounted(async () => {
  loading.value = true
  const tid = auth.personalId
  if (!tid) { loading.value = false; return }

  const [groupsRes, teamsRes] = await Promise.allSettled([
    api.get('/group', { params: { filters: { trainerId: [tid] } } }),
    api.get('/team',  { params: { filters: { trainerId: [tid] } } }),
  ])

  const loadedGroups: any[] = groupsRes.status === 'fulfilled' ? (groupsRes.value.data.data ?? []) : []
  const loadedTeams:  any[] = teamsRes.status  === 'fulfilled' ? (teamsRes.value.data.data  ?? []) : []
  groups.value = loadedGroups
  teams.value  = loadedTeams

  const sportsmenMap = new Map<number, any>()

  const groupResults = await Promise.allSettled(
    loadedGroups.map(g => api.get(`/sportsman/group/${g.id}`).catch(() => null))
  )
  groupResults.forEach((r, i) => {
    if (r.status === 'fulfilled' && r.value) {
      const list = r.value.data.data ?? []
      sportsmenByGroup.value[loadedGroups[i].id] = list
      list.forEach((s: any) => sportsmenMap.set(s.id, s))
    }
  })

  const teamResults = await Promise.allSettled(
    loadedTeams.map(t => api.get('/sportsman', { params: { filters: { teamId: [t.id] } } }).catch(() => null))
  )
  teamResults.forEach((r, i) => {
    if (r.status === 'fulfilled' && r.value) {
      const list = r.value.data.data ?? []
      sportsmenByTeam.value[loadedTeams[i].id] = list
      list.forEach((s: any) => sportsmenMap.set(s.id, s))
    }
  })

  sportsmen.value = Array.from(sportsmenMap.values()).sort((a, b) => a.fio.localeCompare(b.fio, 'ru'))

  const workoutsRes = await api.get(`/personal/${tid}/workouts`).catch(() => null)
  allWorkouts.value = workoutsRes?.data?.data ?? []

  loading.value = false
})
</script>
