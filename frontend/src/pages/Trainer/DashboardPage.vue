<template>
  <div class="p-3 h-full overflow-y-auto">
    <TrainerPageCard color="blue" class="min-h-full flex flex-col gap-5">

      <!-- Шапка тренера -->
      <div class="rounded-2xl border border-neutral-100 bg-white px-5 py-4 flex items-center gap-5">
        <div class="w-28 h-28 rounded-2xl bg-blue-600 flex items-center justify-center shrink-0 overflow-hidden">
          <img v-if="avatarUrl" :src="avatarUrl" class="w-full h-full object-cover object-top" alt="Аватар" />
          <span v-else class="text-white font-bold text-3xl">{{ initials(profile?.fio ?? '') }}</span>
        </div>
        <div class="flex-1 min-w-0">
          <div class="text-base font-bold text-neutral-900 truncate">{{ profile?.fio ?? '—' }}</div>
          <div class="text-sm text-neutral-500 mt-0.5">{{ profile?.position ?? 'Тренер' }}</div>
          <div class="flex gap-4 mt-1.5 text-xs text-neutral-400">
            <span v-if="teams.length">Команд: <span class="font-semibold text-neutral-700">{{ teams.length }}</span></span>
            <span v-if="groups.length">Групп: <span class="font-semibold text-neutral-700">{{ groups.length }}</span></span>
          </div>
        </div>
        <router-link to="/trainer/profile" class="text-xs font-semibold text-blue-500 hover:underline shrink-0">Профиль</router-link>
      </div>

      <!-- Быстрые ссылки -->
      <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-2.5">
        <router-link
          v-for="link in quickLinks" :key="link.to"
          :to="link.to"
          class="rounded-2xl border border-neutral-100 bg-white px-4 py-3 flex items-center gap-3 hover:border-blue-200 hover:bg-blue-50/50 transition-all group"
        >
          <div class="w-9 h-9 rounded-xl flex items-center justify-center shrink-0" :class="link.color">
            <component :is="link.icon" class="w-5 h-5" />
          </div>
          <span class="text-sm font-semibold text-neutral-700 group-hover:text-blue-700 transition-colors">{{ link.label }}</span>
        </router-link>
      </div>

      <!-- Расписание / ближайшее занятие — широкая плашка -->
      <router-link
        to="/trainer/schedule"
        class="rounded-2xl border border-blue-200 bg-blue-50 hover:bg-blue-100 transition-colors p-4 flex flex-col items-center gap-2 group"
      >
        <div class="flex items-center gap-3 w-full justify-center">
          <div class="w-9 h-9 rounded-xl bg-blue-100 border border-blue-200 flex items-center justify-center shrink-0">
            <component :is="IconCalendar" class="w-5 h-5 text-blue-600" />
          </div>
          <div class="text-sm font-bold text-blue-700">Расписание</div>
        </div>
        <div v-if="loadingSchedule" class="text-xs text-blue-400">Загрузка...</div>
        <div v-else-if="nextTraining" class="text-xs text-blue-600 text-center">
          Ближайшее: {{ formatDate(nextTraining.date) }}, {{ formatTime(nextTraining.date) }}
          <span v-if="nextTraining.group"> · {{ nextTraining.group.name }}</span>
        </div>
        <div v-else class="text-xs text-blue-400">Нет предстоящих занятий</div>
      </router-link>

      <!-- Live матч + Оценка нормативов -->
      <div class="grid grid-cols-2 gap-4">
        <router-link
          to="/trainer/match-live"
          class="rounded-2xl border border-red-200 bg-red-50 hover:bg-red-100 transition-colors p-4 flex items-center gap-3 group"
        >
          <div class="w-10 h-10 rounded-xl bg-red-100 border border-red-200 flex items-center justify-center shrink-0">
            <component :is="IconLive" class="w-5 h-5 text-red-600" />
          </div>
          <div>
            <div class="text-sm font-bold text-red-700">Live матч</div>
            <div class="text-xs text-red-400 mt-0.5">Фиксация событий</div>
          </div>
        </router-link>

        <router-link
          to="/trainer/normative-record"
          class="rounded-2xl border border-green-200 bg-green-50 hover:bg-green-100 transition-colors p-4 flex items-center gap-3 group"
        >
          <div class="w-10 h-10 rounded-xl bg-green-100 border border-green-200 flex items-center justify-center shrink-0">
            <component :is="IconStar" class="w-5 h-5 text-green-600" />
          </div>
          <div>
            <div class="text-sm font-bold text-green-700">Оценка нормативов</div>
            <div class="text-xs text-green-500 mt-0.5">Записать результаты группы</div>
          </div>
        </router-link>
      </div>

    </TrainerPageCard>
  </div>
</template>

<script lang="ts">
export default { name: 'TrainerDashboard' }
</script>

<script setup lang="ts">
import { ref, onMounted, computed, defineComponent, h } from 'vue'
import { RouterLink } from 'vue-router'
import api from '@/services/api'
import { imageUrl } from '@/utils/imageUrl'
import TrainerPageCard from '@/components/trainer/TrainerPageCard.vue'

interface Profile { fio: string; position?: string; id?: number; images?: { path: string }[] }
interface Team { id: number; name: string }
interface Group { id: number; name: string }
interface Training { id: number; date: string; type?: string; group?: { id: number; name: string } }

const profile = ref<Profile | null>(null)
const avatarUrl = ref<string | null>(null)
const teams = ref<Team[]>([])
const groups = ref<Group[]>([])
const schedule = ref<Training[]>([])
const loadingSchedule = ref(false)

function initials(fio: string) {
  return fio.split(' ').slice(0, 2).map(w => w[0]).join('').toUpperCase()
}

// Иконки
const IconUsers = defineComponent({ render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z' })]) })
const IconCalendar = defineComponent({ render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z' })]) })
const IconLive = defineComponent({ render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M15 10l4.553-2.069A1 1 0 0121 8.82v6.36a1 1 0 01-1.447.894L15 14M3 8a2 2 0 012-2h8a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2V8z' })]) })
const IconStar = defineComponent({ render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M11.049 2.927c.3-.921 1.603-.921 1.902 0l1.519 4.674a1 1 0 00.95.69h4.915c.969 0 1.371 1.24.588 1.81l-3.976 2.888a1 1 0 00-.363 1.118l1.518 4.674c.3.922-.755 1.688-1.538 1.118l-3.976-2.888a1 1 0 00-1.176 0l-3.976 2.888c-.783.57-1.838-.197-1.538-1.118l1.518-4.674a1 1 0 00-.363-1.118l-3.976-2.888c-.784-.57-.38-1.81.588-1.81h4.914a1 1 0 00.951-.69l1.519-4.674z' })]) })
const IconAi = defineComponent({ render: () => h('svg', { xmlns: 'http://www.w3.org/2000/svg', fill: 'none', viewBox: '0 0 24 24', stroke: 'currentColor', 'stroke-width': '2' }, [h('path', { 'stroke-linecap': 'round', 'stroke-linejoin': 'round', d: 'M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z' })]) })

const quickLinks = [
  { to: '/trainer/sportsmen',  label: 'Спортсмены',  color: 'bg-blue-50 text-blue-600',     icon: IconUsers },
  { to: '/trainer/schedule',   label: 'Расписание',  color: 'bg-green-50 text-green-600',   icon: IconCalendar },
  { to: '/trainer/ai-eval',    label: 'AI-ассистент', color: 'bg-violet-50 text-violet-600', icon: IconAi },
  { to: '/trainer/matches',    label: 'Матчи',       color: 'bg-sky-50 text-sky-600',       icon: IconCalendar },
  { to: '/trainer/groups',     label: 'Группы',      color: 'bg-indigo-50 text-indigo-600', icon: IconUsers },
  { to: '/trainer/normatives', label: 'Нормативы',   color: 'bg-orange-50 text-orange-600', icon: IconStar },
]

function toLocal(iso: string) {
  const d = new Date(iso)
  d.setHours(d.getHours() + 3)
  return d
}

const nextTraining = computed(() => {
  const now = new Date()
  return schedule.value
    .filter(t => toLocal(t.date) >= now)
    .sort((a, b) => toLocal(a.date).getTime() - toLocal(b.date).getTime())[0] ?? null
})

function formatDate(iso: string) {
  return toLocal(iso).toLocaleDateString('ru-RU', { day: 'numeric', month: 'long' })
}

function formatTime(iso: string) {
  return toLocal(iso).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })
}

async function load() {
  const [pm] = await Promise.allSettled([api.get('/personal/me')])
  profile.value = (pm as any).value?.data?.data ?? null
  const images = profile.value?.images ?? []
  if (images.length > 0) avatarUrl.value = imageUrl(images) ?? ''

  if (profile.value?.id) {
    const tid = profile.value.id
    const [gr, tm] = await Promise.allSettled([
      api.get('/group', { params: { filters: { trainerId: [tid] } } }),
      api.get('/team',  { params: { filters: { trainerId: [tid] } } }),
    ])
    groups.value = (gr as any).value?.data?.data ?? []
    teams.value  = (tm as any).value?.data?.data ?? []
  }
}

async function loadSchedule() {
  loadingSchedule.value = true
  try {
    const tid = profile.value?.id
    const res = await api.get('/training', tid ? { params: { filters: { trainerId: [tid] } } } : undefined).catch(() => null)
    schedule.value = res?.data?.data ?? []
  } finally {
    loadingSchedule.value = false
  }
}

onMounted(async () => {
  await load()
  await loadSchedule()
})
</script>
