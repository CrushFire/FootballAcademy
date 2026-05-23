<template>
  <AdminListLayout
    title="Группы"
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

    <template #sort="{ close }">
      <button v-for="o in sortOptions" :key="o.value" @click="sortBy = o.value; close()"
        class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors"
        :class="sortBy === o.value ? 'text-blue-600 font-semibold' : 'text-neutral-600'">{{ o.label }}</button>
    </template>

    <template #items>
      <AdminEntityCard v-for="g in paged" :key="g.id" @edit="openEdit(g)" @delete="openDelete(g)">
        <template #actions>
          <button @click.stop="openRoster(g)"
            class="w-8 h-8 rounded-lg border border-sky-200 bg-sky-50 flex items-center justify-center hover:bg-sky-100 transition-colors"
            title="Изменить состав">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-sky-600">
              <path stroke-linecap="round" stroke-linejoin="round" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z"/>
            </svg>
          </button>
        </template>
        <div class="text-sm font-semibold text-neutral-800">{{ g.name }}</div>
        <div class="text-xs text-neutral-400 mt-0.5">{{ g.description || '—' }} · {{ trainerFio(g.trainerId) }}</div>
      </AdminEntityCard>
    </template>
  </AdminListLayout>

  <RosterEditModal
    v-if="rosterGroup"
    :title="`Состав группы «${rosterGroup.name}»`"
    :subtitle="`В группе: ${rosterInside.length} · Доступно: ${rosterOutside.length}`"
    inside-label="В группе"
    outside-label="Можно добавить"
    label-field="fio"
    sub-field="position"
    :search-keys="['fio']"
    search-placeholder="Поиск по ФИО..."
    :inside-items="rosterInside"
    :outside-items="rosterOutside"
    @add="addToGroup"
    @remove="removeFromGroup"
    @close="rosterGroup = null"
  />

  <EditModal v-if="editItem !== undefined" :title="editItem ? 'Редактировать группу' : 'Новая группа'" @save="saveEdit" @cancel="editItem = undefined">
    <FormField label="Название">
      <input v-model="editForm.name" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField label="Описание">
      <input v-model="editForm.description" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField v-if="!editItem" label="Тренер">
      <select v-model.number="editForm.trainerId" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 bg-white">
        <option :value="null" disabled>Выберите тренера</option>
        <option v-for="t in trainers" :key="t.id" :value="t.id">{{ t.fio }}</option>
      </select>
    </FormField>
  </EditModal>

  <ConfirmDeleteModal v-if="deleteItem" :message="`Удалить группу «${deleteItem.name}»?`" @confirm="confirmDelete" @cancel="deleteItem = null" />
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
import RosterEditModal from '@/components/ui/RosterEditModal.vue'

const { toast, showToast } = useToast()

const PER_PAGE = 15
const sortOptions = [
  { value: 'name', label: 'По названию' },
  { value: 'date', label: 'По дате' },
]

const loading = ref(true)
const items = ref<any[]>([])
const personals = ref<any[]>([])
const search = ref('')
const sortBy = ref('name')
const sortDir = ref<'asc'|'desc'>('asc')
const page = ref(1)
const editItem = ref<any>(undefined)
const editForm = ref<any>({})
const deleteItem = ref<any>(null)

const rosterGroup = ref<any>(null)
const rosterInside = ref<any[]>([])
const allSportsmen = ref<any[]>([])
const rosterOutside = computed(() => {
  const insideIds = new Set(rosterInside.value.map(s => s.id))
  return allSportsmen.value.filter(s => !insideIds.has(s.id))
})

const personalMap = computed<Record<number, string>>(() => {
  const m: Record<number, string> = {}
  for (const p of personals.value) m[p.id] = p.fio
  return m
})

// В дропдауне группы — только тренеры (не медики). PersonalType.Trainer = 0 на бэке.
const trainers = computed(() => personals.value.filter(p => p.type === 'Trainer' || p.type === 0))

function trainerFio(trainerId: number | null | undefined): string {
  if (!trainerId) return 'Без тренера'
  return personalMap.value[trainerId] ?? `Тренер #${trainerId}`
}

async function openRoster(g: any) {
  rosterGroup.value = g
  const [inRes, allRes] = await Promise.all([
    api.get(`/sportsman/group/${g.id}`).catch(() => null),
    allSportsmen.value.length ? Promise.resolve(null) : api.get('/sportsman').catch(() => null),
  ])
  rosterInside.value = inRes?.data?.data ?? []
  if (allRes) allSportsmen.value = allRes?.data?.data ?? []
}

async function addToGroup(sportsmanId: number | string) {
  if (!rosterGroup.value) return
  await api.post(`/group/${rosterGroup.value.id}/sportsman/${sportsmanId}`).catch(() => null)
  const found = allSportsmen.value.find(s => s.id === Number(sportsmanId))
  if (found && !rosterInside.value.find(s => s.id === found.id)) rosterInside.value.push(found)
  showToast('saved')
}

async function removeFromGroup(sportsmanId: number | string) {
  if (!rosterGroup.value) return
  await api.delete(`/group/${rosterGroup.value.id}/sportsman/${sportsmanId}`).catch(() => null)
  rosterInside.value = rosterInside.value.filter(s => s.id !== Number(sportsmanId))
  showToast('saved')
}

const currentSortLabel = computed(() => sortOptions.find(o => o.value === sortBy.value)?.label ?? '')

const filtered = computed(() => {
  let list = items.value
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(g => g.name?.toLowerCase().includes(q) || g.description?.toLowerCase().includes(q))
  }
  return [...list].sort((a, b) => {
    let res = sortBy.value === 'date'
      ? new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime()
      : (a.name ?? '').localeCompare(b.name ?? '')
    if (res === 0) res = (a.name ?? '').localeCompare(b.name ?? '')
    return sortDir.value === 'asc' ? res : -res
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / PER_PAGE)))
const paged = computed(() => filtered.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

function openCreate() {
  editItem.value = null
  editForm.value = { name: '', description: '', trainerId: null }
}
function openEdit(g: any) {
  editItem.value = g
  editForm.value = { name: g.name, description: g.description }
}
function openDelete(g: any) { deleteItem.value = g }

async function saveEdit() {
  if (editItem.value) {
    await api.put(`/group/${editItem.value.id}`, { name: editForm.value.name, description: editForm.value.description })
  } else {
    await api.post('/group', editForm.value)
  }
  editItem.value = undefined
  showToast('saved')
  await load()
}

async function confirmDelete() {
  await api.delete(`/group/${deleteItem.value.id}`)
  deleteItem.value = null
  showToast('deleted')
  await load()
}

async function load() {
  loading.value = true
  const [groupsRes, personalsRes] = await Promise.all([
    api.get('/group').catch(() => null),
    api.get('/personal').catch(() => null),
  ])
  items.value = groupsRes?.data?.data ?? []
  personals.value = personalsRes?.data?.data ?? []
  loading.value = false
}

onMounted(load)
</script>
