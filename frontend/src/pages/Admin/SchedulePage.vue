<template>
  <AdminListLayout
    title="Расписание"
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
      <select v-model="filterDay" @change="page = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
        <option value="">Все дни</option>
        <option v-for="(label, val) in DAY_LABEL" :key="val" :value="val">{{ label }}</option>
      </select>
    </template>

    <template #sort="{ close }">
      <button v-for="o in sortOptions" :key="o.value" @click="sortBy = o.value; close()"
        class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors"
        :class="sortBy === o.value ? 'text-blue-600 font-semibold' : 'text-neutral-600'">{{ o.label }}</button>
    </template>

    <template #items>
      <AdminEntityCard v-for="c in paged" :key="c.id" @edit="openEdit(c)" @delete="openDelete(c)">
        <div class="text-sm font-semibold text-neutral-800">{{ c.groupName }} · {{ c.sportHall }}</div>
        <div class="text-xs text-neutral-400 mt-0.5">
          {{ DAY_LABEL[c.dayOfWeek] ?? c.dayOfWeek }} · {{ c.beginTime }} · {{ WEEK_LABEL[c.weekType] ?? c.weekType }}
        </div>
      </AdminEntityCard>
    </template>
  </AdminListLayout>

  <EditModal v-if="editItem !== undefined" :title="editItem ? 'Редактировать занятие' : 'Новое занятие'" @save="saveEdit" @cancel="editItem = undefined">
    <FormField v-if="!editItem" label="Группа">
      <SearchableSelect
        v-model="groupIdModel"
        :items="groups"
        label-field="name"
        :search-keys="['name', 'description']"
        placeholder="Выберите группу..."
        search-placeholder="Поиск группы..."
      />
    </FormField>
    <FormField label="Зал">
      <input v-model="editForm.sportHall" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField label="День недели">
      <select v-model="editForm.dayOfWeek" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400">
        <option v-for="(label, val) in DAY_LABEL" :key="val" :value="val">{{ label }}</option>
      </select>
    </FormField>
    <FormField label="Время начала (ЧЧ:ММ)">
      <input v-model="editForm.beginTime" type="time" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField label="Тип недели">
      <select v-model="editForm.weekType" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400">
        <option v-for="(label, val) in WEEK_LABEL" :key="val" :value="val">{{ label }}</option>
      </select>
    </FormField>
  </EditModal>

  <ConfirmDeleteModal v-if="deleteItem" :message="`Удалить занятие?`" @confirm="confirmDelete" @cancel="deleteItem = null" />
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
import SearchableSelect from '@/components/trainer/SearchableSelect.vue'

const { toast, showToast } = useToast()

const DAY_LABEL: Record<string, string> = {
  Monday: 'Пн', Tuesday: 'Вт', Wednesday: 'Ср',
  Thursday: 'Чт', Friday: 'Пт', Saturday: 'Сб', Sunday: 'Вс'
}
const WEEK_LABEL: Record<string, string> = { Any: 'Каждую неделю', Even: 'Чётные', Odd: 'Нечётные' }
const PER_PAGE = 15
const sortOptions = [
  { value: 'group', label: 'По группе' },
  { value: 'hall',  label: 'По залу' },
  { value: 'day',   label: 'По дню' },
  { value: 'time',  label: 'По времени' },
]

const loading = ref(true)
const items = ref<any[]>([])
const groups = ref<any[]>([])
const search = ref('')
const filterDay = ref('')
const sortBy = ref('group')
const sortDir = ref<'asc'|'desc'>('asc')
const page = ref(1)
const editItem = ref<any>(undefined)
const editForm = ref<any>({})
const deleteItem = ref<any>(null)

const DAY_ORDER = ['Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday']
const currentSortLabel = computed(() => sortOptions.find(o => o.value === sortBy.value)?.label ?? '')

const filtered = computed(() => {
  let list = items.value
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(c => c.groupName?.toLowerCase().includes(q) || c.sportHall?.toLowerCase().includes(q))
  }
  if (filterDay.value) list = list.filter(c => c.dayOfWeek === filterDay.value)
  return [...list].sort((a, b) => {
    let res = 0
    if (sortBy.value === 'day')        res = DAY_ORDER.indexOf(a.dayOfWeek) - DAY_ORDER.indexOf(b.dayOfWeek)
    else if (sortBy.value === 'time')  res = (a.beginTime ?? '').localeCompare(b.beginTime ?? '')
    else if (sortBy.value === 'hall')  res = (a.sportHall ?? '').localeCompare(b.sportHall ?? '')
    else                               res = (a.groupName ?? '').localeCompare(b.groupName ?? '')
    if (res === 0) res = (a.groupName ?? '').localeCompare(b.groupName ?? '')
    return sortDir.value === 'asc' ? res : -res
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / PER_PAGE)))
const paged = computed(() => filtered.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

const groupIdModel = computed<number | string | null>({
  get: () => editForm.value.groupId ?? null,
  set: (v) => { editForm.value.groupId = v == null ? null : Number(v) },
})

function openCreate() {
  editItem.value = null
  editForm.value = { groupId: null, sportHall: '', dayOfWeek: 'Monday', beginTime: '10:00', weekType: 'Any' }
}
function openEdit(c: any) {
  editItem.value = c
  editForm.value = { sportHall: c.sportHall, dayOfWeek: c.dayOfWeek, beginTime: c.beginTime, weekType: c.weekType }
}
function openDelete(c: any) { deleteItem.value = c }

async function saveEdit() {
  const body = { sportHall: editForm.value.sportHall, dayOfWeek: editForm.value.dayOfWeek, beginTime: editForm.value.beginTime, weekType: editForm.value.weekType }
  if (editItem.value) {
    await api.put(`/schedule/${editItem.value.id}`, body)
  } else {
    await api.post('/schedule', { ...body, groupId: editForm.value.groupId })
  }
  editItem.value = undefined
  showToast('saved')
  await load()
}

async function confirmDelete() {
  await api.delete(`/schedule/${deleteItem.value.id}`)
  deleteItem.value = null
  showToast('deleted')
  await load()
}

async function load() {
  loading.value = true
  const [scheduleRes, groupsRes] = await Promise.all([
    api.get('/schedule').catch(() => null),
    api.get('/group').catch(() => null),
  ])
  items.value = scheduleRes?.data?.data ?? []
  groups.value = groupsRes?.data?.data ?? []
  loading.value = false
}

onMounted(load)
</script>
