<template>
  <AdminListLayout
    title="Планы тренировок"
    :toast="toast"
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
      <select v-model="filterTrainer" @change="page = 1"
        class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
        <option value="">Все тренеры</option>
        <option v-for="t in trainersList" :key="t.id" :value="t.id">{{ t.fio }}</option>
      </select>
    </template>

    <template #sort="{ close }">
      <button v-for="o in sortOptions" :key="o.value" @click="sortBy = o.value; close()"
        class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors"
        :class="sortBy === o.value ? 'text-blue-600 font-semibold' : 'text-neutral-600'">{{ o.label }}</button>
    </template>

    <template #items>
      <AdminEntityCard v-for="p in paged" :key="p.id" @edit="openEdit(p)" @delete="openDelete(p)">
        <div class="text-sm font-semibold text-neutral-800">{{ p.name }}</div>
        <div class="text-xs text-neutral-400 mt-0.5">
          Тренер: {{ p.trainerFIO ?? '—' }} · {{ formatDate(p.createdAt) }}
        </div>
        <div v-if="p.description" class="text-xs text-neutral-600 mt-1.5 line-clamp-2">{{ p.description }}</div>
      </AdminEntityCard>
    </template>
  </AdminListLayout>

  <EditModal v-if="editItem !== undefined" title="Редактировать план" @save="saveEdit" @cancel="editItem = undefined">
    <FormField label="Название">
      <input v-model="editForm.name" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField label="Описание">
      <textarea v-model="editForm.description" rows="3"
        class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 resize-none" />
    </FormField>
    <FormField label="Программа">
      <textarea v-model="editForm.workouts" rows="6"
        class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 resize-none" />
    </FormField>
  </EditModal>

  <ConfirmDeleteModal v-if="deleteItem"
    :message="`Удалить план «${deleteItem.name}»?`"
    @confirm="confirmDelete" @cancel="deleteItem = null" />
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import { useToast } from '@/composables/useToast'
import { formatDate } from '@/utils/formatDate'
import AdminListLayout from '@/components/ui/AdminListLayout.vue'
import AdminEntityCard from '@/components/ui/AdminEntityCard.vue'
import ConfirmDeleteModal from '@/components/ui/ConfirmDeleteModal.vue'
import EditModal from '@/components/ui/EditModal.vue'
import FormField from '@/components/ui/FormField.vue'

const { toast, showToast } = useToast()

const PER_PAGE = 15
const sortOptions = [
  { value: 'date',    label: 'По дате' },
  { value: 'name',    label: 'По названию' },
  { value: 'trainer', label: 'По тренеру' },
]

const loading = ref(true)
const items = ref<any[]>([])
const search = ref('')
const filterTrainer = ref<number | ''>('')
const sortBy = ref('date')
const sortDir = ref<'asc'|'desc'>('desc')
const page = ref(1)
const deleteItem = ref<any>(null)
const editItem = ref<any>(undefined)
const editForm = ref<any>({})

const currentSortLabel = computed(() => sortOptions.find(o => o.value === sortBy.value)?.label ?? '')

const trainersList = computed(() => {
  const m = new Map<number, { id: number; fio: string }>()
  for (const p of items.value) if (p.trainerId) m.set(p.trainerId, { id: p.trainerId, fio: p.trainerFIO ?? `#${p.trainerId}` })
  return Array.from(m.values()).sort((a, b) => a.fio.localeCompare(b.fio, 'ru'))
})

const filtered = computed(() => {
  let list = items.value
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(p =>
      p.name?.toLowerCase().includes(q) ||
      p.description?.toLowerCase().includes(q) ||
      p.trainerFIO?.toLowerCase().includes(q)
    )
  }
  if (filterTrainer.value) list = list.filter(p => p.trainerId === filterTrainer.value)
  return [...list].sort((a, b) => {
    let res = 0
    if (sortBy.value === 'date')         res = new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    else if (sortBy.value === 'name')    res = (a.name ?? '').localeCompare(b.name ?? '', 'ru')
    else if (sortBy.value === 'trainer') res = (a.trainerFIO ?? '').localeCompare(b.trainerFIO ?? '', 'ru')
    return sortDir.value === 'asc' ? -res : res
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / PER_PAGE)))
const paged = computed(() => filtered.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

function openDelete(p: any) { deleteItem.value = p }

function openEdit(p: any) {
  editItem.value = p
  editForm.value = { name: p.name ?? '', description: p.description ?? '', workouts: p.workouts ?? '' }
}

async function saveEdit() {
  if (!editItem.value) return
  await api.put(`/personal/plan/${editItem.value.id}`, {
    name: editForm.value.name,
    description: editForm.value.description,
    workouts: editForm.value.workouts,
  }).catch(() => null)
  const idx = items.value.findIndex(x => x.id === editItem.value.id)
  if (idx !== -1) {
    items.value[idx] = { ...items.value[idx], ...editForm.value }
  }
  editItem.value = undefined
  showToast('saved')
}

async function confirmDelete() {
  if (!deleteItem.value) return
  await api.delete(`/personal/plan/${deleteItem.value.id}`).catch(() => null)
  items.value = items.value.filter(p => p.id !== deleteItem.value.id)
  deleteItem.value = null
  showToast('deleted')
}

async function load() {
  loading.value = true
  const res = await api.get('/personal/plans').catch(() => null)
  items.value = res?.data?.data ?? []
  loading.value = false
}

onMounted(load)
</script>
