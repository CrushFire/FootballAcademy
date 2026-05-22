<template>
  <div class="flex items-center justify-between p-3 rounded-xl border border-neutral-200 bg-white hover:bg-[#DDE3EB] transition-colors">
    <div class="flex-1 min-w-0">
      <slot />
    </div>
    <div class="flex items-center gap-2 flex-shrink-0 ml-3" @click.stop>
      <slot name="actions" />
      <button v-if="!noEdit"
        @click.stop="$emit('edit')"
        class="w-8 h-8 rounded-lg border border-green-300 bg-green-50 flex items-center justify-center hover:bg-green-100 transition-colors"
        title="Редактировать"
      >
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-green-600">
          <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
        </svg>
      </button>
      <button
        @click.stop="$emit('delete')"
        class="w-8 h-8 rounded-lg border border-blue-200 bg-blue-50 flex items-center justify-center hover:bg-blue-100 transition-colors"
        title="Удалить"
      >
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-blue-500">
          <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{ noEdit?: boolean }>()
defineEmits<{ edit: [], delete: [] }>()
</script>
