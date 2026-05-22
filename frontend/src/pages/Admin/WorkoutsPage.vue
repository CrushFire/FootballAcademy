<template>
  <AdminListLayout
    title="Индивидуальные задания"
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
      <AdminEntityCard v-for="w in paged" :key="w.id" @edit="openEdit(w)" @delete="openDelete(w)">
        <div class="text-sm font-semibold text-neutral-800">{{ w.sportsmanFIO }}</div>
        <div class="text-xs text-neutral-400 mt-0.5">
          От: {{ w.personalFIO ?? '—' }} · {{ formatDate(w.createdAt) }}
        </div>
        <div v-if="w.workout" class="text-xs text-neutral-600 mt-1.5 line-clamp-2">{{ w.workout }}</div>
      </AdminEntityCard>
    </template>
  </AdminListLayout>

  <EditModal v-if="editItem !== undefined" title="Редактировать задание" @save="saveEdit" @cancel="editItem = undefined">
    <FormField label="Спортсмен">
      <input :value="editItem?.sportsmanFIO ?? '—'" disabled
        class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 bg-neutral-50 text-neutral-500" />
    </FormField>
    <FormField label="Тренер">
      <input :value="editItem?.personalFIO ?? '—'" disabled
        class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 bg-neutral-50 text-neutral-500" />
    </FormField>
    <FormField label="Задание / рекомендации">
      <textarea v-model="editForm.workout" rows="6"
        class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 resize-none" />
    </FormField>
  </EditModal>

  <ConfirmDeleteModal v-if="deleteItem"
    :message="`Удалить задание для «${deleteItem.sportsmanFIO}»?`"
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
  { value: 'date',      label: 'По дате' },
  { value: 'sportsman', label: 'По спортсмену' },
  { value: 'trainer',   label: 'По тренеру' },
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

function openEdit(w: any) {
  editItem.value = w
  editForm.value = { workout: w.workout ?? '' }
}

async function saveEdit() {
  if (!editItem.value) return
  // У workouts нет PUT — делаем DELETE + POST с новым текстом
  await api.delete(`/personal/workout/${editItem.value.id}`).catch(() => null)
  const res = await api.post('/personal/workout', {
    sportsmanId: editItem.value.sportsmanId,
    personalId:  editItem.value.personalId,
    workout:     editForm.value.workout,
  }).catch(() => null)
  const created = res?.data?.data
  if (created) {
    const idx = items.value.findIndex(x => x.id === editItem.value.id)
    if (idx !== -1) items.value.splice(idx, 1, created)
  }
  editItem.value = undefined
  showToast('saved')
}

const currentSortLabel = computed(() => sortOptions.find(o => o.value === sortBy.value)?.label ?? '')

const trainersList = computed(() => {
  const m = new Map<number, { id: number; fio: string }>()
  for (const w of items.value) if (w.personalId) m.set(w.personalId, { id: w.personalId, fio: w.personalFIO ?? `#${w.personalId}` })
  return Array.from(m.values()).sort((a, b) => a.fio.localeCompare(b.fio, 'ru'))
})

const filtered = computed(() => {
  let list = items.value
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(w =>
      w.sportsmanFIO?.toLowerCase().includes(q) ||
      w.workout?.toLowerCase().includes(q) ||
      w.personalFIO?.toLowerCase().includes(q)
    )
  }
  if (filterTrainer.value) list = list.filter(w => w.personalId === filterTrainer.value)
  return [...list].sort((a, b) => {
    let res = 0
    if (sortBy.value === 'date')           res = new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    else if (sortBy.value === 'sportsman') res = (a.sportsmanFIO ?? '').localeCompare(b.sportsmanFIO ?? '', 'ru')
    else if (sortBy.value === 'trainer')   res = (a.personalFIO ?? '').localeCompare(b.personalFIO ?? '', 'ru')
    return sortDir.value === 'asc' ? -res : res
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / PER_PAGE)))
const paged = computed(() => filtered.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

function openDelete(w: any) { deleteItem.value = w }

async function confirmDelete() {
  if (!deleteItem.value) return
  await api.delete(`/personal/workout/${deleteItem.value.id}`).catch(() => null)
  items.value = items.value.filter(w => w.id !== deleteItem.value.id)
  deleteItem.value = null
  showToast('deleted')
}

async function load() {
  loading.value = true
  const res = await api.get('/personal/workouts').catch(() => null)
  items.value = res?.data?.data ?? []
  loading.value = false
}

onMounted(load)
</script>
