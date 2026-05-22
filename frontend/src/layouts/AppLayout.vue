<template>
  <div class="flex flex-col h-screen bg-surface-light font-sans">
    <AppHeader />
    <div class="flex flex-1 overflow-hidden">
      <NavAdmin    v-if="role === 'admin'" />
      <NavTrainer  v-else-if="role === 'trainer'" />
      <NavMedical  v-else-if="role === 'medical'" />
      <NavSportsman v-else />
      <main class="flex-1 overflow-hidden h-full">
        <RouterView v-slot="{ Component, route }">
          <keep-alive :include="['TrainerAiEval']">
            <component :is="Component" :key="route.name" />
          </keep-alive>
        </RouterView>
      </main>
    </div>
    <AppFooter />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/store/auth'
import AppHeader from '@/components/layout/AppHeader.vue'
import AppFooter from '@/components/layout/AppFooter.vue'
import NavAdmin from '@/components/layout/NavAdmin.vue'
import NavTrainer from '@/components/layout/NavTrainer.vue'
import NavMedical from '@/components/layout/NavMedical.vue'
import NavSportsman from '@/components/layout/NavSportsman.vue'

const auth = useAuthStore()
const role = computed(() => {
  const r = auth.userRole?.toLowerCase().trim() ?? ''
  if (r === 'personal') return auth.personalType?.toLowerCase() ?? ''
  return r
})
</script>
