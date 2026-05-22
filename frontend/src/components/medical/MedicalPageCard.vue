<template>
  <div class="bg-white dark:bg-theme-surface rounded-2xl shadow-sm dark:shadow-none border border-neutral-200 dark:border-theme-border h-full flex flex-col p-4">
    <div class="h-1 bg-gradient-to-r -mx-4 -mt-4 mb-3" :class="gradientClass" />
    <slot />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  color?: 'blue' | 'sky' | 'emerald'
}>()

const gradientClass = computed(() => {
  if (props.color === 'blue')    return 'from-blue-500 to-blue-400 dark:from-theme-stripe-blue dark:to-blue-900'
  if (props.color === 'emerald') return 'from-emerald-500 to-teal-400 dark:from-theme-stripe-emerald dark:to-teal-900'
  return 'from-sky-400 to-cyan-400 dark:from-theme-stripe-teal dark:to-cyan-900'
})
</script>
