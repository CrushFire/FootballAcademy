<template>
  <div class="relative" ref="containerRef">
    <button type="button" @click="open = !open"
      class="w-full flex items-center justify-between px-3 py-2 text-sm rounded-xl border border-neutral-200 bg-white hover:border-blue-400 focus:outline-none focus:border-blue-400 transition-colors">
      <span v-if="modelValue" class="flex items-center gap-2">
        <span class="font-semibold text-neutral-800">{{ modelValue }}</span>
        <span class="text-xs text-neutral-400">{{ LABELS[modelValue] }}</span>
      </span>
      <span v-else class="text-neutral-800 font-semibold">—</span>
      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"
        class="w-4 h-4 text-neutral-400 transition-transform" :class="open ? 'rotate-180' : ''">
        <path stroke-linecap="round" stroke-linejoin="round" d="M19 9l-7 7-7-7"/>
      </svg>
    </button>

    <div v-if="open"
      class="absolute z-30 left-0 right-0 mt-1 bg-white border border-neutral-200 rounded-xl shadow-lg overflow-y-auto max-h-48">
      <!-- Пустой вариант -->
      <button type="button" @click="select('')"
        class="w-full flex items-center px-3 py-2 text-sm hover:bg-neutral-50 transition-colors text-neutral-800 font-semibold">
        —
      </button>
      <!-- Группы -->
      <template v-for="group in GROUPS" :key="group.label">
        <div class="px-3 py-1 text-[10px] font-bold text-neutral-600 uppercase tracking-wider bg-neutral-50 border-t border-neutral-100">
          {{ group.label }}
        </div>
        <button type="button" v-for="p in group.positions" :key="p"
          @click="select(p)"
          class="w-full flex items-center justify-between px-3 py-2 text-sm hover:bg-blue-50 transition-colors"
          :class="modelValue === p ? 'bg-blue-50' : ''">
          <span class="font-semibold text-neutral-800">{{ p }}</span>
          <span class="text-xs text-neutral-400">{{ LABELS[p] }}</span>
        </button>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import { POSITION_LABEL, POSITIONS } from '@/constants'

const emit = defineEmits<{ 'update:modelValue': [value: string] }>()

defineProps<{ modelValue: string }>()

const open = ref(false)
const containerRef = ref<HTMLElement | null>(null)

// Единый источник лейблов из @/constants
const LABELS = POSITION_LABEL

// Группы строим из POSITIONS (поле group). Используем кастомные названия для UI.
const GROUP_ORDER = ['Вратарь', 'Защитники', 'Полузащитники', 'Атакующие полузащитники / фланги', 'Нападающие']
const GROUP_DISPLAY: Record<string, string> = {
  'Вратарь': 'Вратарь',
  'Защитники': 'Защитники',
  'Полузащитники': 'Полузащитники',
  'Атакующие полузащитники / фланги': 'Вингеры',
  'Нападающие': 'Нападающие',
}
const GROUPS = computed(() => GROUP_ORDER.map(g => ({
  label: GROUP_DISPLAY[g] ?? g,
  positions: POSITIONS.filter(p => p.group === g).map(p => p.value as string),
})))

function select(val: string) {
  emit('update:modelValue', val)
  open.value = false
}

function onClickOutside(e: MouseEvent) {
  if (!containerRef.value?.contains(e.target as Node)) open.value = false
}

onMounted(() => document.addEventListener('click', onClickOutside))
onUnmounted(() => document.removeEventListener('click', onClickOutside))
</script>
