<template>
  <div class="p-3 h-full">
    <TrainerPageCard color="blue" class="h-full flex flex-col">
      <AdminListLayout
        title="Мои группы"
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
      <select v-model="filterName" @change="page = 1" class="text-xs rounded-xl border border-neutral-200 bg-neutral-50 px-3 py-2 focus:outline-none focus:border-blue-400 text-neutral-600">
        <option value="">Все группы</option>
        <option v-for="name in uniqueNames" :key="name" :value="name">{{ name }}</option>
      </select>
    </template>

    <template #sort="{ close }">
      <button v-for="o in sortOptions" :key="o.value" @click="sortBy = o.value; close()"
        class="w-full text-left px-4 py-2 text-xs hover:bg-neutral-50 transition-colors"
        :class="sortBy === o.value ? 'text-blue-600 font-semibold' : 'text-neutral-600'">{{ o.label }}</button>
    </template>

    <template #items>
      <div v-for="g in paged" :key="g.id" class="shadow-[0_6px_0_-2px_#BAE6FD] dark:shadow-[0_6px_0_-2px_#0C4A6E] mb-1.5 rounded-2xl">
        <TrainerCard padding="p-3" @click="openDetail(g)">
          <div class="text-base font-semibold text-neutral-800">{{ g.name }}</div>
          <div class="text-sm text-neutral-400 mt-0.5">{{ g.description || '—' }}</div>
        </TrainerCard>
      </div>
    </template>
      </AdminListLayout>
    </TrainerPageCard>
  </div>

  <GroupDetailModal v-if="selectedGroup" :group="selectedGroup" @close="selectedGroup = null" />
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import api from '@/services/api'
import AdminListLayout from '@/components/ui/AdminListLayout.vue'
import GroupDetailModal from '@/components/trainer/GroupDetailModal.vue'
import TrainerCard from '@/components/trainer/TrainerCard.vue'
import TrainerPageCard from '@/components/trainer/TrainerPageCard.vue'

const PER_PAGE = 15
const sortOptions = [
  { value: 'name', label: 'По названию' },
  { value: 'date', label: 'По дате' },
]

const loading = ref(true)
const items = ref<any[]>([])
const search = ref('')
const filterName = ref('')
const sortBy = ref('name')
const sortDir = ref<'asc'|'desc'>('asc')
const page = ref(1)
const selectedGroup = ref<any>(null)

const currentSortLabel = computed(() => sortOptions.find(o => o.value === sortBy.value)?.label ?? '')

const uniqueNames = computed(() => {
  const names = new Set(items.value.map(g => g.name))
  return Array.from(names).sort()
})

const filtered = computed(() => {
  let list = items.value
  
  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(g => g.name?.toLowerCase().includes(q) || g.description?.toLowerCase().includes(q))
  }
  
  if (filterName.value) {
    list = list.filter(g => g.name === filterName.value)
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

function openDetail(g: any) {
  selectedGroup.value = g
}

async function load() {
  loading.value = true
  const res = await api.get('/group').catch(() => null)
  items.value = res?.data?.data ?? []
  loading.value = false
}

onMounted(load)
</script>
