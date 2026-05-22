<template>
  <AdminListLayout
    title="Спортсмены"
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
      <select v-model="filterSpec" @change="page = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
        <option value="">Все специализации</option>
        <option value="Football">Футбол</option>
        <option value="Minifootball">Мини-футбол</option>
      </select>
    </template>

    <template #sort="{ close }">
      <button
        v-for="o in sortOptions" :key="o.value"
        @click="sortBy = o.value; close()"
        class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors"
        :class="sortBy === o.value ? 'text-blue-600 font-semibold' : 'text-neutral-600'"
      >{{ o.label }}</button>
    </template>

    <template #items>
      <AdminEntityCard
        v-for="s in paged" :key="s.id"
        @edit="openEdit(s)"
        @delete="openDelete(s)"
      >
        <div class="text-sm font-semibold text-neutral-800">{{ s.fio }}</div>
        <div class="text-xs text-neutral-400 mt-0.5">
          {{ s.position ?? '—' }} · {{ SPEC_LABEL[s.specialization] ?? s.specialization }} · {{ s.age }} лет · {{ s.height }} см · {{ s.weight }} кг
        </div>
      </AdminEntityCard>
    </template>
  </AdminListLayout>

  <EditModal v-if="editItem !== undefined" :title="editItem ? 'Редактировать спортсмена' : 'Новый спортсмен'" @save="saveEdit" @cancel="editItem = undefined">
    <FormField label="ФИО">
      <input v-model="editForm.fio" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField v-if="!editItem" label="ID пользователя">
      <input v-model.number="editForm.userId" type="number" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField :label="`Дата рождения${currentAge !== null ? ' (' + currentAge + ' ' + ageWord(currentAge) + ')' : ''}`">
      <input v-model="editForm.birthDate" type="date" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField v-if="!editItem" label="Пол">
      <select v-model="editForm.gender" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400">
        <option value="M">Мужской</option>
        <option value="F">Женский</option>
      </select>
    </FormField>
    <div class="grid grid-cols-2 gap-3">
      <FormField label="Рост (см)">
        <input v-model.number="editForm.height" type="number" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
      </FormField>
      <FormField label="Вес (кг)">
        <input v-model.number="editForm.weight" type="number" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
      </FormField>
    </div>
    <FormField label="Позиция">
      <select v-model="editForm.position" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400">
        <option value="">—</option>
        <option v-for="p in POSITIONS" :key="p" :value="p">{{ p }}</option>
      </select>
    </FormField>
    <FormField label="Специализация">
      <select v-model="editForm.specialization" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400">
        <option value="Football">Футбол</option>
        <option value="Minifootball">Мини-футбол</option>
      </select>
    </FormField>

    <!-- Images -->
    <FormField label="Фотографии">
      <!-- Existing images (edit mode) -->
      <div v-if="editItem && existingImages.length" class="flex flex-wrap gap-2 mb-2">
        <div v-for="img in existingImages" :key="img.id" class="relative w-16 h-16 rounded-xl overflow-hidden border border-neutral-200 group">
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
      <!-- New files preview -->
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

  <ConfirmDeleteModal
    v-if="deleteItem"
    :message="`Удалить спортсмена «${deleteItem.fio}»?`"
    @confirm="confirmDelete"
    @cancel="deleteItem = null"
  />
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

const SPEC_LABEL: Record<string, string> = { Football: 'Футбол', Minifootball: 'Мини-футбол' }
const POSITIONS = ['GK','CB','LB','RB','LWB','RWB','CM','CDM','CAM','LW','RW','ST','CF','SS']
const PER_PAGE = 15
const sortOptions = [
  { value: 'fio',    label: 'По имени' },
  { value: 'age',    label: 'По возрасту' },
  { value: 'height', label: 'По росту' },
  { value: 'weight', label: 'По весу' },
]

const editForm         = ref<any>({})
const loading          = ref(true)
const items            = ref<any[]>([])
const search           = ref('')
const filterSpec       = ref('')
const sortBy           = ref('fio')
const page             = ref(1)
const sortDir          = ref<'asc'|'desc'>('asc')
const editItem         = ref<any>(undefined)
const deleteItem       = ref<any>(null)
const existingImages   = ref<any[]>([])
const newFiles         = ref<File[]>([])
const newImagePreviews = ref<string[]>([])

const currentSortLabel = computed(() => sortOptions.find(o => o.value === sortBy.value)?.label ?? '')

const currentAge = computed<number | null>(() => {
  const bd = editForm.value.birthDate
  if (!bd) return null
  const d = new Date(bd)
  if (isNaN(d.getTime())) return null
  const today = new Date()
  let age = today.getFullYear() - d.getFullYear()
  const beforeBirthday = today.getMonth() < d.getMonth() ||
    (today.getMonth() === d.getMonth() && today.getDate() < d.getDate())
  if (beforeBirthday) age--
  return age >= 0 && age <= 120 ? age : null
})

function ageWord(n: number): string {
  const mod10 = n % 10
  const mod100 = n % 100
  if (mod10 === 1 && mod100 !== 11) return 'год'
  if ([2, 3, 4].includes(mod10) && ![12, 13, 14].includes(mod100)) return 'года'
  return 'лет'
}

const filtered = computed(() => {
  let list = items.value
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(s => s.fio?.toLowerCase().includes(q) || s.position?.toLowerCase().includes(q))
  }
  if (filterSpec.value) list = list.filter(s => s.specialization === filterSpec.value)
  return [...list].sort((a, b) => {
    let res = 0
    if (sortBy.value === 'fio')         res = (a.fio ?? '').localeCompare(b.fio ?? '')
    else if (sortBy.value === 'age')    res = (a.age ?? 0) - (b.age ?? 0)
    else if (sortBy.value === 'height') res = (a.height ?? 0) - (b.height ?? 0)
    else if (sortBy.value === 'weight') res = (a.weight ?? 0) - (b.weight ?? 0)
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
  editForm.value = { fio: '', userId: null, birthDate: '', gender: 'M', height: 0, weight: 0, position: '', specialization: 'Football' }
  resetImageState()
}
function openEdit(s: any) {
  editItem.value = s
  // приводим birthDate к YYYY-MM-DD (нужно для <input type="date">)
  const bd = s.birthDate ? String(s.birthDate).slice(0, 10) : ''
  editForm.value = { fio: s.fio, height: s.height, weight: s.weight, position: s.position ?? '', specialization: s.specialization, gender: s.gender, birthDate: bd }
  resetImageState()
  // images come as array of paths from API; wrap them with synthetic id based on index
  existingImages.value = (s.images ?? []).map((img: any) => typeof img === 'string' ? { id: null, path: img } : { id: img.id, path: img.path })
}
function openDelete(s: any) { deleteItem.value = s }

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

function removeExistingImage(img: any) {
  existingImages.value = existingImages.value.filter(x => x !== img)
}

async function saveEdit() {
  if (editItem.value) {
    // 1. Update basic fields
    await api.put(`/sportsman/${editItem.value.id}`, {
      fio: editForm.value.fio,
      birthDate: editForm.value.birthDate,
      height: editForm.value.height,
      weight: editForm.value.weight,
      position: editForm.value.position || null,
      gender: editForm.value.gender,
      specialization: editForm.value.specialization,
    })
    // 2. Delete removed images
    const originalIds = (editItem.value.images ?? []).map((img: any) => typeof img === 'object' ? img.id : null).filter(Boolean)
    const remainingIds = existingImages.value.map(x => x.id).filter(Boolean)
    const deletedIds = originalIds.filter((id: any) => !remainingIds.includes(id))
    for (const id of deletedIds) {
      await api.delete(`/sportsman/images/${id}`).catch(() => null)
    }
    // 3. Upload new images
    if (newFiles.value.length) {
      const fd = new FormData()
      newFiles.value.forEach(f => fd.append('images', f))
      await api.post(`/sportsman/${editItem.value.id}/images`, fd, { headers: { 'Content-Type': 'multipart/form-data' } }).catch(() => null)
    }
  } else {
    const fd = new FormData()
    fd.append('userId', String(editForm.value.userId))
    fd.append('fio', editForm.value.fio)
    fd.append('birthDate', new Date(editForm.value.birthDate).toISOString())
    fd.append('height', String(editForm.value.height))
    fd.append('weight', String(editForm.value.weight))
    fd.append('gender', editForm.value.gender)
    fd.append('specialization', editForm.value.specialization)
    if (editForm.value.position) fd.append('position', editForm.value.position)
    newFiles.value.forEach(f => fd.append('images', f))
    await api.post('/sportsman', fd)
  }
  editItem.value = undefined
  showToast('saved')
  await load()
}

async function confirmDelete() {
  await api.delete(`/sportsman/${deleteItem.value.id}`)
  deleteItem.value = null
  showToast('deleted')
  await load()
}

async function load() {
  loading.value = true
  const res = await api.get('/sportsman').catch(() => null)
  items.value = res?.data?.data ?? []
  loading.value = false
}

onMounted(load)
</script>
