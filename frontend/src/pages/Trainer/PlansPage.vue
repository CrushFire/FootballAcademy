<template>
  <div class="p-3 h-full relative">
    <TrainerPageCard color="blue" class="h-full flex flex-col">
      <AdminListLayout
        title="Планы тренировок"
        :toast="toast"
        :search="search"
        :loading="loading"
        :items="filteredPlans"
        :page="page"
        :total-pages="totalPages"
        :total="filteredPlans.length"
        :sort-dir="sortDir"
        :sort-label="sortOptions.find(o => o.value === sortBy)?.label"
        @update:search="search = $event; page = 1"
        @prev="page--"
        @next="page++"
        @toggle-dir="sortDir = sortDir === 'asc' ? 'desc' : 'asc'"
      >
        <template #actions>
          <button @click="openCreate"
            class="text-xs px-4 py-1.5 rounded-xl text-white font-semibold transition-colors border bg-blue-600 border-blue-400 hover:bg-blue-700 hover:border-blue-500 inline-flex items-center gap-1.5">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/>
            </svg>
            Создать
          </button>
        </template>

        <template #filter>
          <select v-model="sourceFilter" @change="page = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
            <option value="">Все планы</option>
            <option value="mine">Мои</option>
            <option value="others">Чужие</option>
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
            v-for="plan in pagedPlans"
            :key="plan.id"
            padding="p-3"
            @click="openView(plan)"
          >
            <div class="flex items-start gap-3">
              <!-- Иконка -->
              <div class="w-10 h-10 rounded-xl flex items-center justify-center flex-shrink-0 mt-0.5"
                :class="isMyPlan(plan) ? 'bg-blue-100' : 'bg-neutral-100'">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"
                  class="w-4 h-4" :class="isMyPlan(plan) ? 'text-blue-500' : 'text-neutral-400'">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
                </svg>
              </div>

              <!-- Контент -->
              <div class="flex-1 min-w-0">
                <div class="flex items-center gap-2 flex-wrap">
                  <span class="text-base font-semibold text-neutral-800 truncate">{{ plan.name }}</span>
                  <span v-if="isMyPlan(plan)" class="text-[10px] px-1.5 py-0.5 rounded-full bg-blue-100 text-blue-600 font-semibold flex-shrink-0">Мои тренировки</span>
                  <span v-else class="text-[10px] px-1.5 py-0.5 rounded-full bg-neutral-100 text-neutral-500 font-semibold flex-shrink-0">Другие тренеры</span>
                </div>
                <div class="text-sm text-neutral-400 mt-0.5">{{ plan.trainerFIO }} · {{ formatDate(plan.createdAt) }}</div>
                <div v-if="plan.description" class="text-sm text-neutral-600 mt-1.5 line-clamp-2">{{ plan.description }}</div>
                <div v-else class="text-sm text-neutral-400 mt-1.5">Без описания</div>
              </div>

              <!-- Действия (только для своих) -->
              <div v-if="isMyPlan(plan)" class="flex items-center gap-1 flex-shrink-0" @click.stop>
                <button @click.stop="openEdit(plan)"
                  class="w-8 h-8 rounded-lg border border-green-300 bg-green-50 flex items-center justify-center hover:bg-green-100 transition-colors"
                  title="Редактировать">
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-green-600">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
                  </svg>
                </button>
                <button @click.stop="openDelete(plan)"
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

  <!-- Модалка создания/редактирования -->
  <EditModal
    v-if="editItem !== undefined"
    :title="editItem ? 'Редактировать план' : 'Новый план тренировки'"
    :save-disabled="!editForm.name.trim()"
    @save="saveEdit"
    @cancel="editItem = undefined"
  >
    <FormField label="Название">
      <input v-model="editForm.name" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" placeholder="Например: Силовая тренировка А" />
    </FormField>
    <FormField label="Описание">
      <textarea v-model="editForm.description" rows="3" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 resize-none" placeholder="Краткое описание плана..." />
    </FormField>
    <FormField label="Программа тренировки">
      <textarea v-model="editForm.workouts" rows="5" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 resize-none font-mono" placeholder="1' — Разминка&#10;5' — Упражнение 1&#10;15' — Упражнение 2..." />
    </FormField>
  </EditModal>

  <!-- Модалка просмотра -->
  <Teleport to="body">
    <div v-if="viewPlan" class="fixed inset-0 bg-black/30 z-50 flex items-center justify-center p-4" @click.self="viewPlan = null">
      <div class="bg-white rounded-2xl border border-neutral-200 shadow-xl w-full max-w-lg flex flex-col max-h-[80vh]">
        <!-- Хедер -->
        <div class="flex items-center gap-3 p-5 border-b border-neutral-100">
          <div class="w-9 h-9 rounded-xl flex items-center justify-center flex-shrink-0"
            :class="isMyPlan(viewPlan) ? 'bg-blue-100' : 'bg-neutral-100'">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"
              class="w-4 h-4" :class="isMyPlan(viewPlan) ? 'text-blue-500' : 'text-neutral-400'">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"/>
            </svg>
          </div>
          <div class="flex-1 min-w-0">
            <div class="text-sm font-bold text-neutral-900 truncate">{{ viewPlan.name }}</div>
            <div class="text-xs text-neutral-400 mt-0.5">{{ viewPlan.trainerFIO }} · {{ formatDate(viewPlan.createdAt) }}</div>
          </div>
          <button @click="viewPlan = null" class="w-8 h-8 rounded-lg flex items-center justify-center text-neutral-400 hover:bg-neutral-100 transition-colors flex-shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>
        <!-- Тело -->
        <div class="flex-1 overflow-y-auto p-5 space-y-4">
          <div v-if="viewPlan.description">
            <div class="text-[10px] font-bold text-neutral-400 uppercase tracking-wide mb-1.5">Описание</div>
            <div class="text-sm text-neutral-700">{{ viewPlan.description }}</div>
          </div>
          <div v-if="viewPlan.workouts">
            <div class="text-[10px] font-bold text-neutral-400 uppercase tracking-wide mb-1.5">Программа</div>
            <pre class="text-sm text-neutral-700 whitespace-pre-wrap font-sans leading-relaxed">{{ viewPlan.workouts }}</pre>
          </div>
          <div v-if="!viewPlan.description && !viewPlan.workouts" class="text-sm text-neutral-400 text-center py-4">Нет содержимого</div>
        </div>
        <!-- Футер -->
        <div class="p-4 border-t border-neutral-100 flex gap-2">
          <button @click="viewPlan = null" class="flex-1 py-2 rounded-xl border border-neutral-200 text-xs font-semibold text-neutral-600 hover:bg-neutral-50 transition-colors">Закрыть</button>
          <button v-if="isMyPlan(viewPlan)" @click="openEdit(viewPlan); viewPlan = null"
            class="px-4 py-2 rounded-xl bg-blue-50 border border-blue-200 text-xs font-semibold text-blue-600 hover:bg-blue-100 transition-colors">
            Редактировать
          </button>
        </div>
      </div>
    </div>
  </Teleport>

  <!-- Подтверждение удаления -->
  <ConfirmDeleteModal
    v-if="deleteItem"
    :message="`Удалить план «${deleteItem.name}»?`"
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
import EditModal from '@/components/ui/EditModal.vue'
import FormField from '@/components/ui/FormField.vue'
import ConfirmDeleteModal from '@/components/ui/ConfirmDeleteModal.vue'

const auth = useAuthStore()
const { toast, showToast } = useToast()

const PER_PAGE = 15

const loading     = ref(true)
const allPlans    = ref<any[]>([])
const search      = ref('')
const sourceFilter = ref('')
const sortBy      = ref('date')
const sortDir     = ref<'asc' | 'desc'>('desc')
const page        = ref(1)

const editItem  = ref<any>(undefined)
const editForm  = ref({ name: '', description: '', workouts: '' })
const deleteItem = ref<any>(null)
const viewPlan  = ref<any>(null)

const sortOptions = [
  { value: 'date',  label: 'По дате' },
  { value: 'name',  label: 'По названию' },
]

function isMyPlan(plan: any): boolean {
  return plan.trainerId === auth.personalId
}

const filteredPlans = computed(() => {
  let list = allPlans.value

  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(p =>
      p.name?.toLowerCase().includes(q) ||
      p.description?.toLowerCase().includes(q) ||
      p.trainerFIO?.toLowerCase().includes(q)
    )
  }

  if (sourceFilter.value === 'mine') {
    list = list.filter(p => isMyPlan(p))
  } else if (sourceFilter.value === 'others') {
    list = list.filter(p => !isMyPlan(p))
  }

  return [...list].sort((a, b) => {
    let res = 0
    if (sortBy.value === 'date') {
      res = new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    } else if (sortBy.value === 'name') {
      res = (a.name ?? '').localeCompare(b.name ?? '', 'ru')
    }
    return sortDir.value === 'asc' ? -res : res
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filteredPlans.value.length / PER_PAGE)))
const pagedPlans = computed(() => filteredPlans.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

function openCreate() {
  editForm.value = { name: '', description: '', workouts: '' }
  editItem.value = null
}

function openEdit(plan: any) {
  editForm.value = { name: plan.name ?? '', description: plan.description ?? '', workouts: plan.workouts ?? '' }
  editItem.value = plan
}

function openDelete(plan: any) {
  deleteItem.value = plan
}

function openView(plan: any) {
  viewPlan.value = plan
}

async function saveEdit() {
  const tid = auth.personalId
  if (!tid) return

  if (editItem.value) {
    await api.put(`/personal/plan/${editItem.value.id}`, {
      name:        editForm.value.name,
      description: editForm.value.description,
      workouts:    editForm.value.workouts,
    })
    const idx = allPlans.value.findIndex(p => p.id === editItem.value.id)
    if (idx !== -1) {
      allPlans.value[idx] = { ...allPlans.value[idx], ...editForm.value }
    }
  } else {
    const res = await api.post('/personal/plan', {
      trainerId:   tid,
      name:        editForm.value.name,
      description: editForm.value.description,
      workouts:    editForm.value.workouts,
    })
    const created = res.data.data
    if (created) allPlans.value.unshift(created)
  }

  const wasEdit = editItem.value !== null
  editItem.value = undefined
  if (wasEdit) showToast('saved')
}

async function confirmDelete() {
  if (!deleteItem.value) return
  await api.delete(`/personal/plan/${deleteItem.value.id}`)
  allPlans.value = allPlans.value.filter(p => p.id !== deleteItem.value.id)
  deleteItem.value = null
  showToast('deleted')
}

async function loadAllPlans() {
  loading.value = true
  // Загружаем всех тренеров, потом их планы параллельно
  const personalsRes = await api.get('/personal').catch(() => null)
  const personals: any[] = personalsRes?.data?.data ?? []

  if (!personals.length) {
    loading.value = false
    return
  }

  const results = await Promise.allSettled(
    personals.map(p => api.get(`/personal/${p.id}/plans`).catch(() => null))
  )

  const plans: any[] = []
  results.forEach(r => {
    if (r.status === 'fulfilled' && r.value) {
      plans.push(...(r.value.data?.data ?? []))
    }
  })

  // Мои планы сверху
  const myId = auth.personalId
  allPlans.value = plans.sort((a, b) => {
    if (a.trainerId === myId && b.trainerId !== myId) return -1
    if (a.trainerId !== myId && b.trainerId === myId) return 1
    return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  })

  loading.value = false
}

onMounted(loadAllPlans)
</script>
