<template>
  <aside
    class="nav-sidebar flex flex-col justify-between flex-shrink-0 transition-all duration-200 overflow-hidden"
    :style="{ width: hovered ? '220px' : '64px' }"
    @mouseenter="hovered = true"
    @mouseleave="hovered = false"
  >
    <nav class="flex flex-col gap-0.5 min-h-0 flex-1 overflow-y-auto pt-2 scrollbar-none">
      <RouterLink
        v-for="item in items" :key="item.name"
        :to="item.to"
        class="nav-link flex items-center no-underline whitespace-nowrap transition-colors"
        active-class=""
        exact-active-class="nav-link--active"
      >
        <span class="w-[22px] h-[22px] flex-shrink-0 flex items-center [&>svg]:w-[22px] [&>svg]:h-[22px]" v-html="item.icon" />
        <span class="text-sm font-medium transition-opacity duration-150" :class="hovered ? 'opacity-100' : 'opacity-0'">{{ item.label }}</span>
      </RouterLink>
    </nav>
    <button
      class="nav-link flex items-center whitespace-nowrap w-full text-left border-none bg-transparent cursor-pointer transition-colors font-sans"
      @click="auth.logout()"
    >
      <span class="w-[22px] h-[22px] flex-shrink-0 flex items-center [&>svg]:w-[22px] [&>svg]:h-[22px]" v-html="ICON_LOGOUT" />
      <span class="font-medium transition-opacity duration-150" :class="hovered ? 'opacity-100' : 'opacity-0'">Выйти</span>
    </button>
  </aside>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/store/auth'

defineProps<{ items: { name: string; to: string; label: string; icon: string }[] }>()

const auth = useAuthStore()
const hovered = ref(false)

const ICON_LOGOUT = `<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"><path stroke-linecap="round" stroke-linejoin="round" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a2 2 0 01-2 2H5a2 2 0 01-2-2V7a2 2 0 012-2h6a2 2 0 012 2v1"/></svg>`
</script>

<style scoped>
.nav-sidebar {
  background: theme('colors.nav.DEFAULT');
}

.nav-link {
  color: theme('colors.nav.text') !important;
  font-size: 0.875rem;
  padding: 7px 14px;
  gap: 10px;
}

.nav-link:hover {
  background: theme('colors.nav.hover');
  color: theme('colors.nav.text-active') !important;
}

.nav-link--active {
  background: theme('colors.nav.active') !important;
  color: theme('colors.nav.text-active') !important;
  border-right: 2px solid theme('colors.nav.text-active');
}

.nav-link span {
  color: inherit !important;
}
</style>
