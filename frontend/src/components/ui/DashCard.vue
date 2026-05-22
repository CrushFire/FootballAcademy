<template>
  <div
    :class="['flex flex-col gap-1.5 p-4 rounded-xl transition-colors', noBorder ? '' : 'border border-neutral-200 dark:border-theme-border', clickable ? 'cursor-pointer hover:bg-[#DDE3EB] dark:hover:bg-theme-surface-2' : '']"
    @click="clickable ? $emit('click') : undefined"
  >
    <div v-if="icon" class="w-9 h-9 rounded-xl flex items-center justify-center mb-1" :class="iconBg">
      <slot name="icon" />
    </div>
    <div class="card-title">{{ title }}</div>
    <slot />
  </div>
</template>

<script setup lang="ts">
defineProps<{
  title: string
  icon?: boolean
  iconBg?: string
  clickable?: boolean
  noBorder?: boolean
}>()
defineEmits<{ click: [] }>()
</script>

<style scoped>
.card-title { font-size: 11px; font-weight: 700; color: #94a3b8; text-transform: uppercase; letter-spacing: 0.5px; margin-bottom: 8px; }
</style>
