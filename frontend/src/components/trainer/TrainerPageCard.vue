<template>
  <div class="bg-white dark:bg-theme-surface rounded-2xl shadow-sm dark:shadow-none border border-neutral-200 dark:border-theme-border h-full flex flex-col p-4">
    <div class="h-1 bg-gradient-to-r -mx-4 -mt-4 mb-3" :class="gradientClass" />
    <slot />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{
  color?: 'blue' | 'green' | 'violet' | 'sky'
}>()

const gradientClass = computed(() => {
  if (props.color === 'green')  return 'from-green-500 to-green-400 dark:from-theme-stripe-emerald dark:to-emerald-900'
  if (props.color === 'violet') return 'from-violet-500 to-purple-400 dark:from-violet-900 dark:to-purple-900'
  if (props.color === 'sky')    return 'from-sky-500 to-cyan-400 dark:from-sky-900 dark:to-cyan-900'
  return 'from-blue-500 to-blue-400 dark:from-theme-stripe-blue dark:to-blue-900'
})
</script>
