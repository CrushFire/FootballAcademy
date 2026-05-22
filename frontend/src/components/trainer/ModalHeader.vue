<template>
  <div class="flex items-center justify-between p-6 border-b-2 border-blue-200 flex-shrink-0 bg-blue-100">
    <div>
      <h2 class="text-lg font-bold text-neutral-900">{{ title }}</h2>
      <p v-if="subtitle" class="text-xs text-neutral-500 mt-1">{{ subtitle }}</p>
      <div v-if="info" class="mt-3 text-sm text-neutral-600 font-medium">
        {{ info }}
      </div>
    </div>
    <button @click="$emit('close')" class="p-2 rounded-lg bg-neutral-100 hover:bg-neutral-200 transition-colors">
      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5 text-neutral-600">
        <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
      </svg>
    </button>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  title: string
  subtitle?: string
  info?: string
}>()

defineEmits<{
  close: []
}>()
</script>
