<template>
  <div class="p-3 h-full">
    <TrainerPageCard color="blue" class="h-full flex flex-col">
      <AdminListLayout
        title="Спортсмены"
        :no-padding="true"
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
    :toast="toast"
  >
    <template #filter>
      <select v-model="filterSpec" @change="page = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
        <option value="">Все специализации</option>
        <option value="Football">Футбол</option>
        <option value="Minifootball">Мини-футбол</option>
      </select>
    </template>

    <template #sort="{ close }">
      <button v-for="o in sortOptions" :key="o.value" @click="sortBy = o.value; close()"
        class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors"
        :class="sortBy === o.value ? 'text-blue-600 font-semibold' : 'text-neutral-600'">{{ o.label }}</button>
    </template>

    <template #items>
      <TrainerCard v-for="s in paged" :key="s.id" padding="p-3" @click="goProfile(s.id)">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-neutral-100 flex items-center justify-center flex-shrink-0 text-base font-bold text-neutral-500">
            {{ initials(s.fio) }}
          </div>
          <div class="flex-1 min-w-0">
            <div class="text-base font-semibold text-neutral-800">{{ s.fio }}</div>
            <div class="text-sm text-neutral-400 mt-0.5">
              {{ s.position ? `${s.position} (${POSITION_LABEL[s.position] ?? s.position})` : '—' }} · {{ SPEC_LABEL[s.specialization] ?? s.specialization }} · {{ s.age }} лет · {{ s.height }} см · {{ s.weight }} кг
            </div>
          </div>
          <button
            @click.stop="openEdit(s)"
            class="w-8 h-8 rounded-lg border border-green-300 bg-green-50 flex items-center justify-center hover:bg-green-100 transition-colors ml-3 flex-shrink-0"
            title="Редактировать"
          >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-green-600">
              <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
            </svg>
          </button>
        </div>
      </TrainerCard>
    </template>
      </AdminListLayout>
    </TrainerPageCard>
  </div>

  <EditModal v-if="editItem !== undefined" title="Редактировать спортсмена" @save="saveEdit" @cancel="editItem = undefined">
    <div class="grid grid-cols-2 gap-3">
      <FormField label="Рост (см)">
        <input v-model.number="editForm.height" type="number" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
      </FormField>
      <FormField label="Вес (кг)">
        <input v-model.number="editForm.weight" type="number" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
      </FormField>
    </div>
    <FormField label="Позиция">
      <PositionSelect v-model="editForm.position" />
    </FormField>
  </EditModal>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import api from '@/services/api'
import { useToast } from '@/composables/useToast'
import AdminListLayout from '@/components/ui/AdminListLayout.vue'
import EditModal from '@/components/ui/EditModal.vue'
import FormField from '@/components/ui/FormField.vue'
import PositionSelect from '@/components/ui/PositionSelect.vue'
import TrainerCard from '@/components/trainer/TrainerCard.vue'
import TrainerPageCard from '@/components/trainer/TrainerPageCard.vue'
import { POSITION_LABEL } from '@/constants'

const SPEC_LABEL: Record<string, string> = { Football: 'Футбол', Minifootball: 'Мини-футбол' }
const PER_PAGE = 15
const sortOptions = [
  { value: 'fio',    label: 'По имени' },
  { value: 'age',    label: 'По возрасту' },
  { value: 'height', label: 'По росту' },
  { value: 'weight', label: 'По весу' },
]

const router    = useRouter()
const { toast, showToast } = useToast()
const loading   = ref(true)
const items     = ref<any[]>([])
const search    = ref('')
const filterSpec = ref('')
const sortBy    = ref('fio')
const sortDir   = ref<'asc'|'desc'>('asc')
const page      = ref(1)
const editItem  = ref<any>(undefined)
const editForm  = ref<any>({})

const currentSortLabel = computed(() => sortOptions.find(o => o.value === sortBy.value)?.label ?? '')

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

function goProfile(id: number) { router.push(`/trainer/sportsman/${id}`) }

function initials(fio: string) {
  if (!fio) return ''
  return fio.split(' ').map(w => w[0]).filter(Boolean).slice(0, 2).join('').toUpperCase()
}

function openEdit(s: any) {
  editItem.value = s
  editForm.value = { height: s.height, weight: s.weight, position: s.position ?? '' }
}

async function saveEdit() {
  const s = editItem.value
  await api.put(`/sportsman/${s.id}`, {
    fio: s.fio,
    birthDate: s.birthDate,
    gender: s.gender,
    specialization: s.specialization,
    height: editForm.value.height,
    weight: editForm.value.weight,
    position: editForm.value.position || null,
  })
  editItem.value = undefined
  showToast('saved')
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
