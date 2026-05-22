<template>
  <AdminListLayout
    title="Матчи"
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
    <template #actions>
      <button @click="openCreate" class="text-xs px-3 py-1.5 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-colors">+ Добавить</button>
    </template>

    <template #filter>
      <select v-model="filterStatus" @change="page = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
        <option value="">Все статусы</option>
        <option value="Scheduled">Запланирован</option>
        <option value="InProgress">Идёт</option>
        <option value="Finished">Завершён</option>
      </select>
    </template>

    <template #sort="{ close }">
      <button v-for="o in sortOptions" :key="o.value" @click="sortBy = o.value; close()"
        class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors"
        :class="sortBy === o.value ? 'text-blue-600 font-semibold' : 'text-neutral-600'">{{ o.label }}</button>
    </template>

    <template #items>
      <AdminEntityCard v-for="m in paged" :key="m.id" @edit="openEdit(m)" @delete="openDelete(m)">
        <div class="text-sm font-semibold text-neutral-800">
          {{ m.homeTeamName }} vs {{ m.opponentTeamName || 'Соперник' }}
        </div>
        <div class="text-xs text-neutral-400 mt-0.5">
          {{ TYPE_LABEL[m.type] ?? m.type }} · {{ STATUS_LABEL[m.status] ?? m.status }} · {{ formatDate(m.date) }}
        </div>
      </AdminEntityCard>
    </template>
  </AdminListLayout>

  <EditModal v-if="createOpen" title="Новый матч" @save="saveCreate" @cancel="createOpen = false">
    <FormField label="ID домашней команды">
      <input v-model.number="createForm.homeTeamId" type="number" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField label="Название соперника">
      <input v-model="createForm.opponentTeamName" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField label="Тип матча">
      <select v-model="createForm.type" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400">
        <option v-for="(label, val) in TYPE_LABEL" :key="val" :value="val">{{ label }}</option>
      </select>
    </FormField>
    <FormField label="Дата">
      <input v-model="createForm.date" type="datetime-local" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
  </EditModal>

  <EditModal v-if="editItem !== undefined" title="Редактировать матч" @save="saveEdit" @cancel="editItem = undefined">
    <FormField label="Название соперника">
      <input v-model="editForm.opponentTeamName" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField label="Тип матча">
      <select v-model="editForm.type" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400">
        <option v-for="(label, val) in TYPE_LABEL" :key="val" :value="val">{{ label }}</option>
      </select>
    </FormField>
    <FormField label="Дата">
      <input v-model="editForm.date" type="datetime-local" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
  </EditModal>

  <ConfirmDeleteModal v-if="deleteItem" :message="`Удалить матч?`" @confirm="confirmDelete" @cancel="deleteItem = null" />
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import { useToast } from '@/composables/useToast'
import AdminListLayout from '@/components/ui/AdminListLayout.vue'
import AdminEntityCard from '@/components/ui/AdminEntityCard.vue'
import ConfirmDeleteModal from '@/components/ui/ConfirmDeleteModal.vue'
import EditModal from '@/components/ui/EditModal.vue'
import FormField from '@/components/ui/FormField.vue'

const { toast, showToast } = useToast()

const TYPE_LABEL: Record<string, string> = { Friendly: 'Товарищеский', League: 'Лига', Cup: 'Кубок', Tournament: 'Турнир' }
const STATUS_LABEL: Record<string, string> = { Scheduled: 'Запланирован', InProgress: 'Идёт', Finished: 'Завершён' }
const PER_PAGE = 15
const sortOptions = [
  { value: 'date', label: 'По дате' },
  { value: 'type', label: 'По типу' },
]

const loading = ref(true)
const items = ref<any[]>([])
const search = ref('')
const filterStatus = ref('')
const sortBy = ref('date')
const sortDir = ref<'asc'|'desc'>('desc')
const page = ref(1)
const createOpen = ref(false)
const createForm = ref<any>({})
const editItem   = ref<any>(undefined)
const editForm   = ref<any>({})
const deleteItem = ref<any>(null)

const currentSortLabel = computed(() => sortOptions.find(o => o.value === sortBy.value)?.label ?? '')

function formatDate(d: string) {
  return d ? new Date(d).toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit', year: 'numeric' }) : '—'
}

const filtered = computed(() => {
  let list = items.value
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(m => m.homeTeamName?.toLowerCase().includes(q) || m.opponentTeamName?.toLowerCase().includes(q))
  }
  if (filterStatus.value) list = list.filter(m => m.status === filterStatus.value)
  return [...list].sort((a, b) => {
    let res = sortBy.value === 'type'
      ? (a.type ?? '').localeCompare(b.type ?? '')
      : new Date(a.date).getTime() - new Date(b.date).getTime()
    if (res === 0) res = (a.homeTeamName ?? '').localeCompare(b.homeTeamName ?? '')
    return sortDir.value === 'asc' ? res : -res
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / PER_PAGE)))
const paged = computed(() => filtered.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

function openCreate() {
  createOpen.value = true
  createForm.value = { homeTeamId: null, opponentTeamName: '', type: 'Friendly', date: '' }
}
function openEdit(m: any) {
  editItem.value = m
  const d = m.date ? new Date(m.date) : null
  editForm.value = {
    opponentTeamName: m.opponentTeamName ?? '',
    type: m.type,
    date: d ? `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}T${String(d.getHours()).padStart(2,'0')}:${String(d.getMinutes()).padStart(2,'0')}` : ''
  }
}
function openDelete(m: any) { deleteItem.value = m }

async function saveCreate() {
  await api.post('/match', {
    homeTeamId: createForm.value.homeTeamId,
    opponentTeamName: createForm.value.opponentTeamName || null,
    type: createForm.value.type,
    date: createForm.value.date ? new Date(createForm.value.date).toISOString() : null,
  })
  createOpen.value = false
  showToast('saved')
  await load()
}

async function saveEdit() {
  await api.put(`/match/${editItem.value.id}`, {
    opponentTeamName: editForm.value.opponentTeamName || null,
    type: editForm.value.type,
    date: editForm.value.date ? new Date(editForm.value.date).toISOString() : null,
  })
  editItem.value = undefined
  showToast('saved')
  await load()
}

async function confirmDelete() {
  await api.delete(`/match/${deleteItem.value.id}`)
  deleteItem.value = null
  showToast('deleted')
  await load()
}

async function load() {
  loading.value = true
  const res = await api.get('/match').catch(() => null)
  items.value = res?.data?.data ?? []
  loading.value = false
}

onMounted(load)
</script>
