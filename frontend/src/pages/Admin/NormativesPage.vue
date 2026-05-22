<template>
  <AdminListLayout
    title="Нормативы ГТО"
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
      <select v-model="filterGender" @change="page = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
        <option value="">Все</option>
        <option value="M">Мужской</option>
        <option value="F">Женской</option>
      </select>
    </template>

    <template #sort="{ close }">
      <!-- По типу -->
      <button @click="sortBy = 'type'; close()"
        class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors"
        :class="sortBy === 'type' ? 'text-blue-600 font-semibold' : 'text-neutral-600'">
        По типу
      </button>

      <!-- По полу -->
      <button @click="sortBy = 'gender'; close()"
        class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors"
        :class="sortBy === 'gender' ? 'text-blue-600 font-semibold' : 'text-neutral-600'">
        По полу
      </button>

      <!-- По возрасту — подменю вправо -->
      <div class="relative" @mouseenter="ageSubmenu = true" @mouseleave="ageSubmenu = false">
        <button
          class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors flex items-center justify-between"
          :class="sortBy === 'age' ? 'text-blue-600 font-semibold' : 'text-neutral-600'">
          По возрасту{{ filterAgeRange ? `: ${filterAgeRange.label}` : '' }}
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3 h-3 ml-1 flex-shrink-0">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
          </svg>
        </button>

        <!-- Подменю: прозрачный мост + само меню -->
        <template v-if="ageSubmenu">
          <div
            class="absolute right-full top-0 z-40 bg-white border border-neutral-200 rounded-xl shadow-lg py-1 min-w-[140px] pr-2"
            style="margin-right: -8px; padding-right: 10px;"
            @mouseenter="ageSubmenu = true"
            @mouseleave="ageSubmenu = false"
          >
            <button
              @click="sortBy = 'age'; filterAgeRange = null; page = 1; close()"
              class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors"
              :class="sortBy === 'age' && !filterAgeRange ? 'text-blue-600 font-semibold' : 'text-neutral-600'">
              Все возрасты
            </button>
            <div class="border-t border-neutral-100 my-1" />
            <button
              v-for="g in AGE_GROUPS" :key="g.label"
              @click="sortBy = 'age'; filterAgeRange = g; page = 1; close()"
              class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors"
              :class="filterAgeRange?.label === g.label ? 'text-blue-600 font-semibold' : 'text-neutral-600'">
              {{ g.label }} ({{ g.min }}–{{ g.max }})
            </button>
          </div>
        </template>
      </div>
    </template>

    <template #items>
      <AdminEntityCard v-for="n in paged" :key="n.id" @edit="openEdit(n)" @delete="openDelete(n)">
        <div class="text-sm font-semibold text-neutral-800">{{ n.type }} <span class="text-neutral-400 font-normal">({{ n.unit }})</span></div>
        <div class="text-xs text-neutral-400 mt-0.5">
          {{ n.ageGroup }} лет · {{ genderLabel(n.gender) }} · Отл: {{ n.gradeExcellent }} · Хор: {{ n.gradeGood }} · Уд: {{ n.gradeSatisfactory }}
        </div>
      </AdminEntityCard>
    </template>
  </AdminListLayout>

  <EditModal v-if="editItem !== undefined" :title="editItem ? 'Редактировать норматив' : 'Новый норматив'" @save="saveEdit" @cancel="editItem = undefined">
    <FormField label="Тип (название упражнения)">
      <input v-model="editForm.type" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField label="Единица измерения">
      <input v-model="editForm.unit" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" placeholder="сек, м, раз..." />
    </FormField>
    <div class="grid grid-cols-2 gap-3">
      <FormField label="Возрастная группа (лет)">
        <input v-model.number="editForm.ageGroup" type="number" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
      </FormField>
      <FormField label="Пол">
        <select v-model="editForm.gender" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400">
          <option value="M">Мужской</option>
          <option value="F">Женский</option>
        </select>
      </FormField>
    </div>
    <div class="grid grid-cols-3 gap-3">
      <FormField label="Отлично">
        <input v-model.number="editForm.gradeExcellent" type="number" step="0.01" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
      </FormField>
      <FormField label="Хорошо">
        <input v-model.number="editForm.gradeGood" type="number" step="0.01" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
      </FormField>
      <FormField label="Удовл.">
        <input v-model.number="editForm.gradeSatisfactory" type="number" step="0.01" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
      </FormField>
    </div>
    <FormField label="">
      <label class="flex items-center gap-2 text-xs text-neutral-600 cursor-pointer">
        <input v-model="editForm.isAboveYearOfStudy" type="checkbox" class="rounded" />
        Выше года обучения
      </label>
    </FormField>
  </EditModal>

  <ConfirmDeleteModal v-if="deleteItem" :message="`Удалить норматив «${deleteItem.type}»?`" @confirm="confirmDelete" @cancel="deleteItem = null" />
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

const PER_PAGE = 15

const AGE_GROUPS = [
  { label: 'U10', min: 0,  max: 10 },
  { label: 'U12', min: 10, max: 12 },
  { label: 'U14', min: 12, max: 14 },
  { label: 'U16', min: 14, max: 16 },
  { label: 'U18', min: 16, max: 18 },
  { label: 'U21', min: 18, max: 21 },
]

function normalizeGender(g: any): string {
  if (typeof g === 'number') return g === 77 ? 'M' : 'F'
  const s = String(g).trim()
  if (s === 'М' || s === 'м') return 'M'
  if (s === 'Ж' || s === 'ж' || s === 'Ф' || s === 'ф') return 'F'
  return s.toUpperCase()
}

function genderLabel(g: any): string {
  return normalizeGender(g) === 'M' ? 'Муж' : 'Жен'
}

const loading      = ref(true)
const items        = ref<any[]>([])
const search       = ref('')
const filterGender   = ref('')
const filterAgeRange = ref<{ label: string, min: number, max: number } | null>(null)
const ageSubmenu     = ref(false)
const sortBy       = ref('type')
const sortDir      = ref<'asc'|'desc'>('asc')
const page         = ref(1)
const editItem     = ref<any>(undefined)
const editForm     = ref<any>({})
const deleteItem   = ref<any>(null)

const currentSortLabel = computed(() => {
  if (sortBy.value === 'gender') return 'По полу'
  if (sortBy.value === 'age') return filterAgeRange.value ? filterAgeRange.value.label : 'По возрасту'
  return 'По типу'
})

const filtered = computed(() => {
  let list = items.value
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(n => n.type?.toLowerCase().includes(q) || n.unit?.toLowerCase().includes(q))
  }
  if (filterGender.value) list = list.filter(n => normalizeGender(n.gender) === filterGender.value)
  if (filterAgeRange.value) list = list.filter(n => n.ageGroup >= filterAgeRange.value!.min && n.ageGroup <= filterAgeRange.value!.max)
  return [...list].sort((a, b) => {
    let res = 0
    if (sortBy.value === 'age')         res = (a.ageGroup ?? 0) - (b.ageGroup ?? 0)
    else if (sortBy.value === 'gender') res = normalizeGender(a.gender).localeCompare(normalizeGender(b.gender))
    else                                res = (a.type ?? '').localeCompare(b.type ?? '')
    if (res === 0) res = (a.type ?? '').localeCompare(b.type ?? '')
    return sortDir.value === 'asc' ? res : -res
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / PER_PAGE)))
const paged = computed(() => filtered.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

function openCreate() {
  editItem.value = null
  editForm.value = { type: '', unit: '', ageGroup: 14, gender: 'M', gradeExcellent: 0, gradeGood: 0, gradeSatisfactory: 0, isAboveYearOfStudy: false }
}
function openEdit(n: any) {
  editItem.value = n
  editForm.value = { type: n.type, unit: n.unit, ageGroup: n.ageGroup, gender: normalizeGender(n.gender), gradeExcellent: n.gradeExcellent, gradeGood: n.gradeGood, gradeSatisfactory: n.gradeSatisfactory, isAboveYearOfStudy: n.isAboveYearOfStudy }
}
function openDelete(n: any) { deleteItem.value = n }

async function saveEdit() {
  if (editItem.value) {
    await api.put(`/normative/gto/${editItem.value.id}`, editForm.value)
  } else {
    await api.post('/normative/gto', editForm.value)
  }
  editItem.value = undefined
  showToast('saved')
  await load()
}

async function confirmDelete() {
  await api.delete(`/normative/gto/${deleteItem.value.id}`)
  deleteItem.value = null
  showToast('deleted')
  await load()
}

async function load() {
  loading.value = true
  const res = await api.get('/normative/gto').catch(() => null)
  items.value = res?.data?.data ?? []
  loading.value = false
}

onMounted(load)
</script>
