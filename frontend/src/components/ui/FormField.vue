<template>
  <div>
    <label class="text-xs text-neutral-500 mb-1 block">{{ label }}</label>
    <slot />
  </div>
</template>

<script setup lang="ts">
defineProps<{ label: string }>()
</script>
