<template>
  <AdminListLayout
    title="Персонал"
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
      <select v-model="filterType" @change="page = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
        <option value="">Все</option>
        <option value="Trainer">Тренеры</option>
        <option value="Medical">Медицинские</option>
      </select>
    </template>

    <template #sort="{ close }">
      <button v-for="o in sortOptions" :key="o.value" @click="sortBy = o.value; close()"
        class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors"
        :class="sortBy === o.value ? 'text-blue-600 font-semibold' : 'text-neutral-600'">{{ o.label }}</button>
    </template>

    <template #items>
      <AdminEntityCard v-for="p in paged" :key="p.id" @edit="openEdit(p)" @delete="openDelete(p)">
        <template #actions>
          <button v-if="p.type === 'Trainer'" @click.stop="openAssignments(p)"
            class="w-8 h-8 rounded-lg border border-sky-200 bg-sky-50 flex items-center justify-center hover:bg-sky-100 transition-colors"
            title="Группы и команды">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-sky-600">
              <path stroke-linecap="round" stroke-linejoin="round" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"/>
            </svg>
          </button>
        </template>
        <div class="text-sm font-semibold text-neutral-800">{{ p.fio }}</div>
        <div class="text-xs text-neutral-400 mt-0.5">{{ p.position }} · {{ TYPE_LABEL[p.type] ?? p.type }}</div>
      </AdminEntityCard>
    </template>
  </AdminListLayout>

  <PersonalAssignmentsModal
    v-if="assignPersonal"
    :personal-name="assignPersonal.fio"
    :personal-id="assignPersonal.id"
    :all-groups="allGroups"
    :all-teams="allTeams"
    :personal-map="personalMap"
    :all-trainers="trainersList"
    @assign-group="assignGroup"
    @assign-team="assignTeam"
    @transfer-group="transferGroup"
    @transfer-team="transferTeam"
    @close="assignPersonal = null"
  />

  <EditModal v-if="editItem !== undefined" :title="editItem ? 'Редактировать сотрудника' : 'Новый сотрудник'" @save="saveEdit" @cancel="editItem = undefined">
    <FormField label="ФИО">
      <input v-model="editForm.fio" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField label="Должность">
      <input v-model="editForm.position" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField label="Тип">
      <select v-model="editForm.type" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400">
        <option value="Trainer">Тренер</option>
        <option value="Medical">Мед. персонал</option>
      </select>
    </FormField>
    <FormField label="Описание">
      <input v-model="editForm.description" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField v-if="!editItem" label="ID пользователя">
      <input v-model.number="editForm.userId" type="number" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>

    <!-- Images -->
    <FormField label="Фотографии">
      <div v-if="editItem && existingImages.length" class="flex flex-wrap gap-2 mb-2">
        <div v-for="img in existingImages" :key="img.path" class="relative w-16 h-16 rounded-xl overflow-hidden border border-neutral-200 group">
          <img :src="`/api/images/${img.path.replace('images/', '')}`" class="w-full h-full object-cover" />
          <button
            @click.prevent="removeExistingImage(img)"
            class="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 flex items-center justify-center transition-opacity"
          >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5 text-white">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>
      </div>
      <div v-if="newImagePreviews.length" class="flex flex-wrap gap-2 mb-2">
        <div v-for="(src, i) in newImagePreviews" :key="i" class="relative w-16 h-16 rounded-xl overflow-hidden border border-blue-200 group">
          <img :src="src" class="w-full h-full object-cover" />
          <button
            @click.prevent="removeNewImage(i)"
            class="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 flex items-center justify-center transition-opacity"
          >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5 text-white">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>
      </div>
      <label class="flex items-center gap-2 px-3 py-2 rounded-xl border border-dashed border-neutral-300 cursor-pointer hover:border-blue-400 hover:bg-blue-50/40 transition-colors">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-400">
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/>
        </svg>
        <span class="text-xs text-neutral-500">Добавить фото</span>
        <input type="file" accept="image/*" multiple class="hidden" @change="onFilesSelected" />
      </label>
    </FormField>
  </EditModal>

  <ConfirmDeleteModal v-if="deleteItem" :message="`Удалить «${deleteItem.fio}»?`" @confirm="confirmDelete" @cancel="deleteItem = null" />
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
import PersonalAssignmentsModal from '@/components/ui/PersonalAssignmentsModal.vue'

const { toast, showToast } = useToast()

const TYPE_LABEL: Record<string, string> = { Trainer: 'Тренер', Medical: 'Мед. персонал' }
const PER_PAGE = 15
const sortOptions = [
  { value: 'fio',      label: 'По имени' },
  { value: 'position', label: 'По должности' },
]

const loading          = ref(true)
const items            = ref<any[]>([])
const search           = ref('')
const filterType       = ref('')
const sortBy           = ref('fio')
const sortDir          = ref<'asc'|'desc'>('asc')
const page             = ref(1)
const editItem         = ref<any>(undefined)
const editForm         = ref<any>({})
const deleteItem       = ref<any>(null)
const existingImages   = ref<{ path: string }[]>([])
const newFiles         = ref<File[]>([])
const newImagePreviews = ref<string[]>([])

const assignPersonal = ref<any>(null)
const allGroups      = ref<any[]>([])
const allTeams       = ref<any[]>([])

const personalMap = computed<Record<number, string>>(() => {
  const m: Record<number, string> = {}
  for (const p of items.value) m[p.id] = p.fio
  return m
})

const trainersList = computed(() =>
  items.value.filter((p: any) => p.type === 'Trainer').map((p: any) => ({ id: p.id, fio: p.fio }))
)

async function openAssignments(p: any) {
  assignPersonal.value = p
  const [gRes, tRes] = await Promise.all([
    api.get('/group').catch(() => null),
    api.get('/team').catch(() => null),
  ])
  allGroups.value = gRes?.data?.data ?? []
  allTeams.value  = tRes?.data?.data ?? []
}

async function assignGroup(groupId: number) {
  if (!assignPersonal.value) return
  const g = allGroups.value.find(x => x.id === groupId)
  if (!g) return
  await api.put(`/group/${groupId}`, { name: g.name, description: g.description, trainerId: assignPersonal.value.id }).catch(() => null)
  g.trainerId = assignPersonal.value.id
  showToast('saved')
}
async function assignTeam(teamId: number) {
  if (!assignPersonal.value) return
  const t = allTeams.value.find(x => x.id === teamId)
  if (!t) return
  await api.put(`/team/${teamId}`, { name: t.name, ageGroup: t.ageGroup, trainerId: assignPersonal.value.id }).catch(() => null)
  t.trainerId = assignPersonal.value.id
  showToast('saved')
}
async function transferGroup(groupId: number, newTrainerId: number) {
  const g = allGroups.value.find(x => x.id === groupId)
  if (!g) return
  await api.put(`/group/${groupId}`, { name: g.name, description: g.description, trainerId: newTrainerId }).catch(() => null)
  g.trainerId = newTrainerId
  showToast('saved')
}
async function transferTeam(teamId: number, newTrainerId: number) {
  const t = allTeams.value.find(x => x.id === teamId)
  if (!t) return
  await api.put(`/team/${teamId}`, { name: t.name, ageGroup: t.ageGroup, trainerId: newTrainerId }).catch(() => null)
  t.trainerId = newTrainerId
  showToast('saved')
}

const currentSortLabel = computed(() => sortOptions.find(o => o.value === sortBy.value)?.label ?? '')

const filtered = computed(() => {
  let list = items.value
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(p => p.fio?.toLowerCase().includes(q) || p.position?.toLowerCase().includes(q))
  }
  if (filterType.value) list = list.filter(p => p.type === filterType.value)
  return [...list].sort((a, b) => {
    let res = sortBy.value === 'position'
      ? (a.position ?? '').localeCompare(b.position ?? '')
      : (a.fio ?? '').localeCompare(b.fio ?? '')
    if (res === 0) res = (a.fio ?? '').localeCompare(b.fio ?? '')
    return sortDir.value === 'asc' ? res : -res
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / PER_PAGE)))
const paged = computed(() => filtered.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

function resetImageState() {
  existingImages.value = []
  newFiles.value = []
  newImagePreviews.value = []
}

function openCreate() {
  editItem.value = null
  editForm.value = { fio: '', position: '', type: 'Trainer', description: '', userId: null }
  resetImageState()
}
function openEdit(p: any) {
  editItem.value = p
  editForm.value = { fio: p.fio, position: p.position, type: p.type, description: p.description }
  resetImageState()
  existingImages.value = (p.images ?? []).map((img: any) => typeof img === 'string' ? { id: null, path: img } : { id: img.id, path: img.path })
}
function openDelete(p: any) { deleteItem.value = p }

function onFilesSelected(e: Event) {
  const files = Array.from((e.target as HTMLInputElement).files ?? [])
  for (const f of files) {
    newFiles.value.push(f)
    const reader = new FileReader()
    reader.onload = ev => newImagePreviews.value.push(ev.target?.result as string)
    reader.readAsDataURL(f)
  }
  ;(e.target as HTMLInputElement).value = ''
}

function removeNewImage(i: number) {
  newFiles.value.splice(i, 1)
  newImagePreviews.value.splice(i, 1)
}

function removeExistingImage(img: { path: string }) {
  existingImages.value = existingImages.value.filter(x => x !== img)
}

async function saveEdit() {
  if (editItem.value) {
    await api.put(`/personal/${editItem.value.id}`, {
      fio: editForm.value.fio,
      position: editForm.value.position,
      type: editForm.value.type,
      description: editForm.value.description,
    })

    // Delete removed images
    const originalIds = (editItem.value.images ?? []).map((img: any) => typeof img === 'object' ? img.id : null).filter(Boolean)
    const remainingIds = existingImages.value.map((x: any) => x.id).filter(Boolean)
    const deletedIds = originalIds.filter((id: any) => !remainingIds.includes(id))
    for (const id of deletedIds) {
      await api.delete(`/personal/images/${id}`).catch(() => null)
    }

    // Upload new images
    if (newFiles.value.length) {
      const fd = new FormData()
      newFiles.value.forEach(f => fd.append('images', f))
      await api.post(`/personal/${editItem.value.id}/images`, fd, { headers: { 'Content-Type': 'multipart/form-data' } }).catch(() => null)
    }
  } else {
    const fd = new FormData()
    fd.append('fio', editForm.value.fio)
    fd.append('position', editForm.value.position)
    fd.append('type', editForm.value.type)
    fd.append('description', editForm.value.description)
    fd.append('userId', String(editForm.value.userId))
    newFiles.value.forEach(f => fd.append('images', f))
    await api.post('/personal', fd)
  }
  editItem.value = undefined
  showToast('saved')
  await load()
}

async function confirmDelete() {
  await api.delete(`/personal/${deleteItem.value.id}`)
  deleteItem.value = null
  showToast('deleted')
  await load()
}

async function load() {
  loading.value = true
  const res = await api.get('/personal').catch(() => null)
  items.value = res?.data?.data ?? []
  loading.value = false
}

onMounted(load)
</script>
