<template>
  <div class="p-3 h-full">
    <TrainerPageCard color="green" class="h-full flex flex-col">
      <AdminListLayout
        title="Мои тренировки"
        :no-padding="true"
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
    <template #actions>
      <button
        @click="$router.push('/trainer/training/create')"
        class="text-xs px-4 py-1.5 rounded-xl text-white font-semibold transition-colors border bg-green-600 border-green-400 hover:bg-green-700 hover:border-green-500 inline-flex items-center gap-1.5"
      >
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/>
        </svg>
        Создать
      </button>
    </template>

    <template #filter>
      <select v-model="selectedGroup" @change="page = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
        <option value="">Все группы</option>
        <option v-for="g in groups" :key="g.id" :value="g.id">{{ g.name }}</option>
      </select>
    </template>

    <template #sort="{ close }">
      <button
        @click="sortBy = 'date'; close()"
        class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors"
        :class="sortBy === 'date' ? 'text-blue-600 font-semibold' : 'text-neutral-600'"
      >По дате</button>
    </template>

    <template #items>
      <TrainerCard v-for="t in paged" :key="t.id" @click="$router.push(`/trainer/trainings/${t.id}`)">
        <div class="flex items-start justify-between gap-2">
          <div class="flex-1 min-w-0">
            <div class="text-base font-semibold text-neutral-800">{{ t.type || 'Тренировка' }}</div>
            <div class="text-sm text-neutral-400 mt-0.5">
              {{ formatDate(t.date) }}<template v-if="t.groupName"> · {{ t.groupName }}</template><template v-if="t.planTrainingName"> · {{ t.planTrainingName }}</template>
            </div>
            <div v-if="t.otherInformation" class="text-sm text-neutral-600 mt-2 line-clamp-2">{{ t.otherInformation }}</div>
            <div v-else class="text-sm text-neutral-400 mt-2">Нет описания</div>
          </div>
          <div class="flex items-center gap-1.5 shrink-0" @click.stop>
            <button
              v-if="canEdit(t)"
              @click.stop="$router.push(`/trainer/training/edit/${t.id}`)"
              class="w-8 h-8 rounded-lg border border-green-300 bg-green-50 text-green-600 inline-flex items-center justify-center hover:bg-green-100 transition-colors"
              title="Редактировать (доступно 24 ч с момента создания)"
            >
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
                <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
              </svg>
            </button>
            <button
              @click.stop="deleteItem = t"
              class="w-8 h-8 rounded-lg border border-blue-200 bg-blue-50 text-blue-500 inline-flex items-center justify-center hover:bg-blue-100 transition-colors"
              title="Удалить"
            >
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
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

  <ConfirmDeleteModal
    v-if="deleteItem"
    :message="`Удалить тренировку «${deleteItem.type}» от ${formatDate(deleteItem.date)}? Метрики и посещаемость также будут удалены.`"
    @confirm="confirmDelete"
    @cancel="deleteItem = null"
  />
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import { formatDate } from '@/utils/formatDate'
import { useAuthStore } from '@/store/auth'
import AdminListLayout from '@/components/ui/AdminListLayout.vue'
import TrainerCard from '@/components/trainer/TrainerCard.vue'
import TrainerPageCard from '@/components/trainer/TrainerPageCard.vue'
import ConfirmDeleteModal from '@/components/ui/ConfirmDeleteModal.vue'

const auth = useAuthStore()
const deleteItem = ref<any>(null)

async function confirmDelete() {
  if (!deleteItem.value) return
  await api.delete(`/training/${deleteItem.value.id}`).catch(() => null)
  trainings.value = trainings.value.filter(t => t.id !== deleteItem.value.id)
  deleteItem.value = null
}

const PER_PAGE = 15

const loading = ref(true)
const trainings = ref<any[]>([])
const groups = ref<any[]>([])
const search = ref('')
const selectedGroup = ref('')
const sortBy = ref('date')
const page = ref(1)
const sortDir = ref<'asc'|'desc'>('desc')

const currentSortLabel = computed(() => 'По дате')

const filtered = computed(() => {
  let list = trainings.value
  
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(t => 
      t.planTrainingName?.toLowerCase().includes(q) || 
      t.type?.toLowerCase().includes(q) ||
      t.groupName?.toLowerCase().includes(q) ||
      t.otherInformation?.toLowerCase().includes(q)
    )
  }
  
  if (selectedGroup.value) {
    list = list.filter(t => t.groupId === Number(selectedGroup.value))
  }
  
  return [...list].sort((a, b) => {
    const res = new Date(b.date).getTime() - new Date(a.date).getTime()
    return sortDir.value === 'asc' ? -res : res
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / PER_PAGE)))
const paged = computed(() => filtered.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

function canEdit(t: any): boolean {
  if (!t.createdAt) return false
  const ageMs = Date.now() - new Date(t.createdAt).getTime()
  return ageMs < 24 * 60 * 60 * 1000
}

onMounted(async () => {
  loading.value = true
  try {
    const tid = auth.personalId
    const filters = tid ? { params: { filters: { trainerId: [tid] } } } : undefined
    const [trainingsRes, groupsRes] = await Promise.all([
      api.get('/training', filters).catch(() => null),
      api.get('/group',    filters).catch(() => null)
    ])

    trainings.value = trainingsRes?.data?.data ?? []
    groups.value = groupsRes?.data?.data ?? []
  } catch (e) {
    console.error(e)
  }
  loading.value = false
})
</script>
