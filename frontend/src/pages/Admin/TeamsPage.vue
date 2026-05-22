<template>
  <AdminListLayout
    title="Команды"
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
      <select v-model="filterAge" @change="page = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
        <option value="">Все возрасты</option>
        <option v-for="a in AGE_GROUPS" :key="a" :value="a">{{ a }}</option>
      </select>
    </template>

    <template #sort="{ close }">
      <button v-for="o in sortOptions" :key="o.value" @click="sortBy = o.value; close()"
        class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors"
        :class="sortBy === o.value ? 'text-blue-600 font-semibold' : 'text-neutral-600'">{{ o.label }}</button>
    </template>

    <template #items>
      <AdminEntityCard v-for="t in paged" :key="t.id" @edit="openEdit(t)" @delete="openDelete(t)">
        <template #actions>
          <button @click.stop="openRoster(t)"
            class="w-8 h-8 rounded-lg border border-sky-200 bg-sky-50 flex items-center justify-center hover:bg-sky-100 transition-colors"
            title="Изменить состав">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-sky-600">
              <path stroke-linecap="round" stroke-linejoin="round" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z"/>
            </svg>
          </button>
        </template>
        <div class="text-sm font-semibold text-neutral-800">{{ t.name }}</div>
        <div class="text-xs text-neutral-400 mt-0.5">{{ t.ageGroup }} · {{ trainerFio(t.trainerId) }}</div>
      </AdminEntityCard>
    </template>
  </AdminListLayout>

  <RosterEditModal
    v-if="rosterTeam"
    :title="`Состав команды «${rosterTeam.name}»`"
    :subtitle="`В команде: ${rosterInside.length} · Доступно: ${rosterOutside.length}`"
    inside-label="В команде"
    outside-label="Можно добавить"
    label-field="fio"
    sub-field="position"
    :search-keys="['fio']"
    search-placeholder="Поиск по ФИО..."
    :inside-items="rosterInside"
    :outside-items="rosterOutside"
    @add="addToTeam"
    @remove="removeFromTeam"
    @close="rosterTeam = null"
  />

  <EditModal v-if="editItem !== undefined" :title="editItem ? 'Редактировать команду' : 'Новая команда'" @save="saveEdit" @cancel="editItem = undefined">
    <FormField label="Название">
      <input v-model="editForm.name" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField label="Возрастная группа">
      <select v-model="editForm.ageGroup" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400">
        <option v-for="a in AGE_GROUPS" :key="a" :value="a">{{ a }}</option>
      </select>
    </FormField>
    <FormField v-if="!editItem" label="ID тренера">
      <input v-model.number="editForm.trainerId" type="number" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
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

  <ConfirmDeleteModal v-if="deleteItem" :message="`Удалить команду «${deleteItem.name}»?`" @confirm="confirmDelete" @cancel="deleteItem = null" />
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

const AGE_GROUPS = ['U10','U12','U14','U16','U18','U21','Senior','Mixed']
const PER_PAGE = 15
const sortOptions = [
  { value: 'name', label: 'По названию' },
  { value: 'age',  label: 'По возрасту' },
]

const loading          = ref(true)
const items            = ref<any[]>([])
const personals        = ref<any[]>([])
const search           = ref('')
const filterAge        = ref('')
const sortBy           = ref('name')
const sortDir          = ref<'asc'|'desc'>('asc')
const page             = ref(1)
const editItem         = ref<any>(undefined)
const editForm         = ref<any>({})
const deleteItem       = ref<any>(null)
const existingImages   = ref<{ path: string }[]>([])
const newFiles         = ref<File[]>([])
const newImagePreviews = ref<string[]>([])

const rosterTeam   = ref<any>(null)
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

function trainerFio(trainerId: number | null | undefined): string {
  if (!trainerId) return 'Без тренера'
  return personalMap.value[trainerId] ?? `Тренер #${trainerId}`
}

async function openRoster(t: any) {
  rosterTeam.value = t
  const [inRes, allRes] = await Promise.all([
    api.get('/sportsman', { params: { filters: { teamId: [t.id] } } }).catch(() => null),
    allSportsmen.value.length ? Promise.resolve(null) : api.get('/sportsman').catch(() => null),
  ])
  rosterInside.value = inRes?.data?.data ?? []
  if (allRes) allSportsmen.value = allRes?.data?.data ?? []
}

async function addToTeam(sportsmanId: number | string) {
  if (!rosterTeam.value) return
  await api.post(`/team/${rosterTeam.value.id}/sportsman/${sportsmanId}`).catch(() => null)
  const found = allSportsmen.value.find(s => s.id === Number(sportsmanId))
  if (found && !rosterInside.value.find(s => s.id === found.id)) rosterInside.value.push(found)
  showToast('saved')
}

async function removeFromTeam(sportsmanId: number | string) {
  await api.delete(`/team/sportsman/${sportsmanId}`).catch(() => null)
  rosterInside.value = rosterInside.value.filter(s => s.id !== Number(sportsmanId))
  showToast('saved')
}

const currentSortLabel = computed(() => sortOptions.find(o => o.value === sortBy.value)?.label ?? '')

const filtered = computed(() => {
  let list = items.value
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(t => t.name?.toLowerCase().includes(q))
  }
  if (filterAge.value) list = list.filter(t => t.ageGroup === filterAge.value)
  return [...list].sort((a, b) => {
    let res = sortBy.value === 'age'
      ? AGE_GROUPS.indexOf(a.ageGroup) - AGE_GROUPS.indexOf(b.ageGroup)
      : (a.name ?? '').localeCompare(b.name ?? '')
    if (res === 0) res = (a.name ?? '').localeCompare(b.name ?? '')
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
  editForm.value = { name: '', ageGroup: 'U14', trainerId: null }
  resetImageState()
}
function openEdit(t: any) {
  editItem.value = t
  editForm.value = { name: t.name, ageGroup: t.ageGroup }
  resetImageState()
  existingImages.value = (t.images ?? []).map((img: any) => typeof img === 'string' ? { id: null, path: img } : { id: img.id, path: img.path })
}
function openDelete(t: any) { deleteItem.value = t }

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
    await api.put(`/team/${editItem.value.id}`, { name: editForm.value.name, ageGroup: editForm.value.ageGroup })

    // Delete removed images
    const originalIds = (editItem.value.images ?? []).map((img: any) => typeof img === 'object' ? img.id : null).filter(Boolean)
    const remainingIds = existingImages.value.map((x: any) => x.id).filter(Boolean)
    const deletedIds = originalIds.filter((id: any) => !remainingIds.includes(id))
    for (const id of deletedIds) {
      await api.delete(`/team/images/${id}`).catch(() => null)
    }

    // Upload new images
    if (newFiles.value.length) {
      const fd = new FormData()
      newFiles.value.forEach(f => fd.append('images', f))
      await api.post(`/team/${editItem.value.id}/images`, fd, { headers: { 'Content-Type': 'multipart/form-data' } }).catch(() => null)
    }
  } else {
    const fd = new FormData()
    fd.append('name', editForm.value.name)
    fd.append('ageGroup', editForm.value.ageGroup)
    fd.append('trainerId', String(editForm.value.trainerId))
    newFiles.value.forEach(f => fd.append('images', f))
    await api.post('/team', fd)
  }
  editItem.value = undefined
  showToast('saved')
  await load()
}

async function confirmDelete() {
  await api.delete(`/team/${deleteItem.value.id}`)
  deleteItem.value = null
  showToast('deleted')
  await load()
}

async function load() {
  loading.value = true
  const [teamsRes, personalsRes] = await Promise.all([
    api.get('/team').catch(() => null),
    api.get('/personal').catch(() => null),
  ])
  items.value = teamsRes?.data?.data ?? []
  personals.value = personalsRes?.data?.data ?? []
  loading.value = false
}

onMounted(load)
</script>
