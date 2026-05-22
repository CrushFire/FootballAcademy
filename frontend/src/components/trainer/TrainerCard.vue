<template>
  <div
    class="bg-white dark:bg-theme-surface-2 rounded-2xl border border-neutral-200 dark:border-theme-border cursor-pointer hover:border-blue-200 hover:bg-blue-50/30 dark:hover:border-blue-700 dark:hover:bg-theme-surface transition-all"
    :class="padding"
  >
    <slot />
  </div>
</template>

<script setup lang="ts">
withDefaults(defineProps<{
  padding?: string
}>(), {
  padding: 'p-4'
})
</script>
