<template>
  <div class="p-3 h-full">
    <AppCard no-border class="h-full overflow-y-auto">

      <div class="mb-6 pb-4 border-b border-neutral-100">
        <div class="text-base font-bold text-neutral-900">Добро пожаловать, {{ auth.userLogin ?? 'Администратор' }}</div>
        <div class="text-xs text-neutral-400 mt-0.5 capitalize">{{ todayStr }}</div>
      </div>

      <div class="grid grid-cols-2 md:grid-cols-3 gap-3">
        <AdminCategoryCard
          v-for="cat in categories" :key="cat.key"
          :label="cat.label"
          :desc="cat.desc"
          :icon-bg="cat.iconBg"
          :icon-color="cat.iconColor"
          :icon-path="cat.iconPath"
          :count-color="cat.countColor"
          :count="counts[cat.key]"
          @click="router.push(cat.to)"
        />
      </div>

    </AppCard>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/store/auth'
import api from '@/services/api'
import AppCard from '@/components/ui/AppCard.vue'
import AdminCategoryCard from '@/components/ui/AdminCategoryCard.vue'

const auth = useAuthStore()
const router = useRouter()
const todayStr = new Date().toLocaleDateString('ru-RU', { weekday: 'long', day: 'numeric', month: 'long' })
const counts = ref<Record<string, number>>({})

const categories = [
  {
    key: 'users', label: 'Пользователи', desc: 'Все аккаунты системы', to: '/admin/manage',
    iconBg: 'bg-blue-50', iconColor: 'text-blue-500', countColor: 'text-blue-600',
    iconPath: '<path stroke-linecap="round" stroke-linejoin="round" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/>',
    endpoint: '/users',
  },
  {
    key: 'personal', label: 'Персонал', desc: 'Тренеры и медперсонал', to: '/admin/manage',
    iconBg: 'bg-violet-50', iconColor: 'text-violet-500', countColor: 'text-violet-600',
    iconPath: '<path stroke-linecap="round" stroke-linejoin="round" d="M21 13.255A23.931 23.931 0 0112 15c-3.183 0-6.22-.62-9-1.745M16 6V4a2 2 0 00-2-2h-4a2 2 0 00-2 2v2m4 6h.01M5 20h14a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"/>',
    endpoint: '/personal',
  },
  {
    key: 'sportsmen', label: 'Спортсмены', desc: 'Все спортсмены академии', to: '/admin/sportsmen',
    iconBg: 'bg-green-50', iconColor: 'text-green-500', countColor: 'text-green-600',
    iconPath: '<path stroke-linecap="round" stroke-linejoin="round" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z"/>',
    endpoint: '/sportsman',
  },
  {
    key: 'groups', label: 'Группы', desc: 'Тренировочные группы', to: '/admin/groups',
    iconBg: 'bg-amber-50', iconColor: 'text-amber-500', countColor: 'text-amber-600',
    iconPath: '<path stroke-linecap="round" stroke-linejoin="round" d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"/>',
    endpoint: '/group',
  },
  {
    key: 'teams', label: 'Команды', desc: 'Соревновательные команды', to: '/admin/teams',
    iconBg: 'bg-red-50', iconColor: 'text-red-400', countColor: 'text-red-500',
    iconPath: '<path stroke-linecap="round" stroke-linejoin="round" d="M3 21v-4m0 0V5a2 2 0 012-2h6.5l1 1H21l-3 6 3 6H10.5l-1-1H5a2 2 0 00-2 2zm9-13.5V9"/>',
    endpoint: '/team',
  },
  {
    key: 'matches', label: 'Матчи', desc: 'Все матчи', to: '/admin/matches',
    iconBg: 'bg-emerald-50', iconColor: 'text-emerald-500', countColor: 'text-emerald-600',
    iconPath: '<circle cx="12" cy="12" r="10"/><path stroke-linecap="round" stroke-linejoin="round" d="M12 8v4m0 4h.01"/>',
    endpoint: '/match',
  },
  {
    key: 'schedule', label: 'Расписание', desc: 'Занятия по группам', to: '/admin/schedule',
    iconBg: 'bg-sky-50', iconColor: 'text-sky-500', countColor: 'text-sky-600',
    iconPath: '<path stroke-linecap="round" stroke-linejoin="round" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"/>',
    endpoint: '/schedule',
  },
  {
    key: 'normatives', label: 'Нормативы', desc: 'ГТО и локальные нормативы', to: '/admin/normatives',
    iconBg: 'bg-teal-50', iconColor: 'text-teal-500', countColor: 'text-teal-600',
    iconPath: '<path stroke-linecap="round" stroke-linejoin="round" d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z"/>',
    endpoint: '/normative/gto',
  },
  {
    key: 'messages', label: 'Сообщения', desc: 'Чат и рассылки', to: '/admin/messages',
    iconBg: 'bg-pink-50', iconColor: 'text-pink-400', countColor: 'text-pink-500',
    iconPath: '<path stroke-linecap="round" stroke-linejoin="round" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-5 5v-5z"/>',
    endpoint: '/message/count',
  },
]

onMounted(async () => {
  const results = await Promise.allSettled(
    categories.map(c => c.endpoint ? api.get(c.endpoint) : Promise.resolve(null))
  )
  categories.forEach((c, i) => {
    const r = results[i]
    if (r.status === 'fulfilled' && r.value) {
      const payload = r.value.data?.data
      // Может прийти массив (большинство endpoint'ов) или число (для /message/count)
      counts.value[c.key] = typeof payload === 'number' ? payload : (payload ?? []).length
    }
  })
})
</script>
