<template>
  <div class="p-3 h-full">
    <MedicalPageCard class="h-full flex flex-col">
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
          <TrainerCard v-for="s in paged" :key="s.id" padding="p-3">
            <div class="flex items-center justify-between">
              <div class="flex-1 min-w-0">
                <div class="text-sm font-semibold text-neutral-800">{{ s.fio }}</div>
                <div class="text-xs text-neutral-400 mt-0.5">
                  {{ POSITION_LABEL[s.position] ?? s.position ?? '—' }} · {{ SPEC_LABEL[s.specialization] ?? s.specialization }} · {{ s.age }} лет · {{ s.height }} см · {{ s.weight }} кг
                </div>
              </div>
            </div>
          </TrainerCard>
        </template>
      </AdminListLayout>
    </MedicalPageCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import AdminListLayout from '@/components/ui/AdminListLayout.vue'
import TrainerCard from '@/components/trainer/TrainerCard.vue'
import MedicalPageCard from '@/components/medical/MedicalPageCard.vue'

const SPEC_LABEL: Record<string, string> = { Football: 'Футбол', Minifootball: 'Мини-футбол' }
const POSITION_LABEL: Record<string, string> = {
  GK: 'GK (Вратарь)', CB: 'CB (Центральный защитник)', LB: 'LB (Левый защитник)', RB: 'RB (Правый защитник)',
  LWB: 'LWB (Левый латераль)', RWB: 'RWB (Правый латераль)', CDM: 'CDM (Опорный полузащитник)',
  CM: 'CM (Центральный полузащитник)', CAM: 'CAM (Атакующий полузащитник)',
  LW: 'LW (Левый вингер)', RW: 'RW (Правый вингер)', ST: 'ST (Нападающий)', CF: 'CF (Центральный нападающий)', SS: 'SS (Второй нападающий)'
}
const PER_PAGE = 15
const sortOptions = [
  { value: 'fio',    label: 'По имени' },
  { value: 'age',    label: 'По возрасту' },
  { value: 'height', label: 'По росту' },
  { value: 'weight', label: 'По весу' },
]

const loading    = ref(true)
const items      = ref<any[]>([])
const search     = ref('')
const filterSpec = ref('')
const sortBy     = ref('fio')
const sortDir    = ref<'asc' | 'desc'>('asc')
const page       = ref(1)

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
    if (sortBy.value === 'fio')         res = (a.fio ?? '').localeCompare(b.fio ?? '', 'ru')
    else if (sortBy.value === 'age')    res = (a.age ?? 0) - (b.age ?? 0)
    else if (sortBy.value === 'height') res = (a.height ?? 0) - (b.height ?? 0)
    else if (sortBy.value === 'weight') res = (a.weight ?? 0) - (b.weight ?? 0)
    if (res === 0) res = (a.fio ?? '').localeCompare(b.fio ?? '', 'ru')
    return sortDir.value === 'asc' ? res : -res
  })
})

const totalPages = computed(() => Math.max(1, Math.ceil(filtered.value.length / PER_PAGE)))
const paged      = computed(() => filtered.value.slice((page.value - 1) * PER_PAGE, page.value * PER_PAGE))

onMounted(async () => {
  loading.value = true
  const res = await api.get('/sportsman').catch(() => null)
  items.value = res?.data?.data ?? []
  loading.value = false
})
</script>
