<template>
  <div
    @click="$emit('click')"
    class="rounded-xl border border-neutral-200 bg-white p-4 cursor-pointer hover:bg-[#DDE3EB] hover:border-neutral-300 hover:shadow-sm transition-all"
  >
    <div class="w-10 h-10 rounded-xl flex items-center justify-center mb-3" :class="iconBg">
      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.8"
        class="w-5 h-5" :class="iconColor" v-html="iconPath"></svg>
    </div>
    <div class="text-sm font-bold text-neutral-700 break-words">{{ label }}</div>
    <div class="text-xs text-neutral-400 mt-0.5 break-words">{{ desc }}</div>
    <div class="text-xl font-extrabold mt-2" :class="countColor">{{ count !== undefined ? count : '—' }}</div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  label: string
  desc: string
  iconBg: string
  iconColor: string
  iconPath: string
  countColor: string
  count?: number
}>()
defineEmits<{ click: [] }>()
</script>
