<template>
  <div class="bg-white rounded-xl border border-gray-200 p-5 flex items-center gap-4">
    <div class="text-3xl">{{ icon }}</div>
    <div>
      <p class="text-2xl font-bold text-gray-900">{{ value }}</p>
      <p class="text-sm text-gray-500">{{ label }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{ label: string; value: string | number; icon: string; color?: string }>()
</script>
