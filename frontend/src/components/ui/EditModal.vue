<template>
  <Teleport to="body">
    <div class="fixed inset-0 bg-black/30 z-50 flex items-center justify-center p-4" @click.self="$emit('cancel')">
      <div class="bg-white rounded-2xl border border-neutral-200 shadow-xl p-6 w-full max-w-md">
        <div class="flex items-center gap-3 mb-4">
          <div class="w-10 h-10 rounded-xl bg-blue-50 flex items-center justify-center flex-shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5 text-blue-500">
              <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
            </svg>
          </div>
          <div class="text-sm font-bold text-neutral-900">{{ title ?? 'Редактировать запись' }}</div>
        </div>

        <div class="flex flex-col gap-3">
          <slot />
        </div>

        <div class="flex gap-2 justify-end mt-4">
          <button @click="$emit('cancel')" class="px-4 py-2 rounded-xl border border-neutral-200 text-xs font-semibold text-neutral-600 hover:bg-neutral-50 transition-colors">Отмена</button>
          <button @click="$emit('save')" :disabled="saveDisabled" class="px-4 py-2 rounded-xl text-xs font-semibold transition-colors" :class="saveDisabled ? 'bg-neutral-200 text-neutral-400 cursor-not-allowed' : 'bg-blue-600 text-white hover:bg-blue-700'">Сохранить</button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
defineProps<{ title?: string, saveDisabled?: boolean }>()
defineEmits<{ save: [], cancel: [] }>()
</script>
