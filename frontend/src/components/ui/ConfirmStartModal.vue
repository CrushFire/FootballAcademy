<template>
  <Teleport to="body">
    <div class="fixed inset-0 bg-black/30 z-50 flex items-center justify-center p-4" @click.self="$emit('cancel')">
      <div class="bg-white rounded-2xl border border-green-200 shadow-xl p-6 w-full max-w-sm">
        <div class="flex items-center gap-3 mb-4">
          <div class="w-10 h-10 rounded-xl bg-green-50 flex items-center justify-center flex-shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5 text-green-600">
              <path stroke-linecap="round" stroke-linejoin="round" d="M5.25 5.653c0-.856.917-1.398 1.667-.986l11.54 6.347a1.125 1.125 0 010 1.972l-11.54 6.347a1.125 1.125 0 01-1.667-.986V5.653z"/>
            </svg>
          </div>
          <div>
            <div class="text-sm font-bold text-neutral-900">Начать матч?</div>
            <div class="text-xs text-neutral-400 mt-0.5">{{ message ?? 'Состав будет зафиксирован, таймер запустится.' }}</div>
          </div>
        </div>
        <div class="flex gap-2 justify-end">
          <button @click="$emit('cancel')" class="px-4 py-2 rounded-xl border border-neutral-200 text-xs font-semibold text-neutral-600 hover:bg-neutral-50 transition-colors">Отмена</button>
          <button @click="$emit('confirm')" class="px-4 py-2 rounded-xl bg-green-600 text-white text-xs font-semibold hover:bg-green-700 transition-colors">Начать</button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
defineProps<{ message?: string }>()
defineEmits<{ confirm: [], cancel: [] }>()
</script>
