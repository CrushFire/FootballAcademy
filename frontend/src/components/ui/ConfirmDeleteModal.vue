<template>
  <Teleport to="body">
    <div class="fixed inset-0 bg-black/30 z-50 flex items-center justify-center p-4" @click.self="$emit('cancel')">
      <div class="bg-white rounded-2xl border border-neutral-200 shadow-xl p-6 w-full max-w-sm">
        <div class="flex items-center gap-3 mb-4">
          <div class="w-10 h-10 rounded-xl bg-red-50 flex items-center justify-center flex-shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5 text-red-500">
              <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
            </svg>
          </div>
          <div>
            <div class="text-sm font-bold text-neutral-900">Удалить запись?</div>
            <div class="text-xs text-neutral-400 mt-0.5">{{ message ?? 'Это действие нельзя отменить.' }}</div>
          </div>
        </div>
        <div class="flex gap-2 justify-end">
          <button @click="$emit('cancel')" class="px-4 py-2 rounded-xl border border-neutral-200 text-xs font-semibold text-neutral-600 hover:bg-neutral-50 transition-colors">Отмена</button>
          <button @click="$emit('confirm')" class="px-4 py-2 rounded-xl bg-red-500 text-white text-xs font-semibold hover:bg-red-600 transition-colors">Удалить</button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
defineProps<{ message?: string }>()
defineEmits<{ confirm: [], cancel: [] }>()
</script>
