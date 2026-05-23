<template>
  <Teleport to="body">
    <div class="fixed inset-0 bg-black/40 z-50 flex items-center justify-center p-4" @click.self="$emit('close')">
      <div class="bg-white rounded-2xl border border-neutral-200 shadow-xl w-full max-w-2xl flex flex-col max-h-[85vh]">
        <!-- Шапка -->
        <div class="flex items-center gap-3 p-5 border-b border-neutral-100">
          <div class="w-10 h-10 rounded-xl bg-blue-50 flex items-center justify-center shrink-0 text-blue-500">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z"/>
            </svg>
          </div>
          <div class="flex-1 min-w-0">
            <div class="text-sm font-bold text-neutral-900 truncate">{{ title }}</div>
            <div class="text-xs text-neutral-400 mt-0.5">{{ subtitle }}</div>
          </div>
          <button @click="$emit('close')" class="w-8 h-8 rounded-lg flex items-center justify-center text-neutral-400 hover:bg-neutral-100 transition-colors">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
            </svg>
          </button>
        </div>

        <!-- Контент: 2 колонки -->
        <div class="flex-1 overflow-hidden grid grid-cols-2 gap-4 p-5">
          <!-- Левая: уже добавлены -->
          <div class="flex flex-col min-h-0">
            <div class="text-[10px] font-bold text-neutral-400 uppercase tracking-wider mb-2">
              {{ insideLabel }} · {{ insideItems.length }}
            </div>
            <input v-model="searchIn" :placeholder="searchPlaceholder"
              class="w-full text-xs rounded-lg border border-neutral-200 bg-white px-3 py-2 focus:outline-none focus:border-blue-400 mb-2" />
            <div class="flex-1 overflow-y-auto rounded-xl border border-neutral-200 divide-y divide-neutral-50 bg-white">
              <div v-if="!insideFiltered.length" class="px-3 py-4 text-xs text-neutral-400 italic text-center">
                {{ insideItems.length ? 'Ничего не найдено' : 'Пусто' }}
              </div>
              <div v-for="it in insideFiltered" :key="it.id"
                class="flex items-center justify-between gap-2 px-3 py-2 hover:bg-neutral-50 transition-colors">
                <div class="flex-1 min-w-0">
                  <div class="text-sm font-semibold text-neutral-800 truncate">{{ itemLabel(it) }}</div>
                  <div v-if="itemSub(it)" class="text-[11px] text-neutral-400 truncate">{{ itemSub(it) }}</div>
                </div>
                <button @click="$emit('remove', it.id)"
                  class="w-7 h-7 rounded-lg border border-blue-200 bg-blue-50 text-blue-500 hover:bg-blue-100 transition-colors inline-flex items-center justify-center shrink-0"
                  title="Убрать">
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12"/>
                  </svg>
                </button>
              </div>
            </div>
          </div>

          <!-- Правая: можно добавить -->
          <div class="flex flex-col min-h-0">
            <div class="text-[10px] font-bold text-neutral-400 uppercase tracking-wider mb-2">
              {{ outsideLabel }} · {{ outsideItems.length }}
            </div>
            <input v-model="searchOut" :placeholder="searchPlaceholder"
              class="w-full text-xs rounded-lg border border-neutral-200 bg-white px-3 py-2 focus:outline-none focus:border-blue-400 mb-2" />
            <div class="flex-1 overflow-y-auto rounded-xl border border-neutral-200 divide-y divide-neutral-50 bg-white">
              <div v-if="!outsideFiltered.length" class="px-3 py-4 text-xs text-neutral-400 italic text-center">
                {{ outsideItems.length ? 'Ничего не найдено' : 'Нет доступных' }}
              </div>
              <div v-for="it in outsideFiltered" :key="it.id"
                class="flex items-center justify-between gap-2 px-3 py-2 hover:bg-neutral-50 transition-colors">
                <div class="flex-1 min-w-0">
                  <div class="text-sm font-semibold text-neutral-800 truncate">{{ itemLabel(it) }}</div>
                  <div v-if="itemSub(it)" class="text-[11px] text-neutral-400 truncate">{{ itemSub(it) }}</div>
                </div>
                <button @click="$emit('add', it.id)"
                  class="w-7 h-7 rounded-lg border border-blue-200 bg-blue-50 text-blue-600 hover:bg-blue-100 transition-colors inline-flex items-center justify-center shrink-0"
                  title="Добавить">
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/>
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </div>

        <div class="p-4 border-t border-neutral-100 flex justify-end">
          <button @click="$emit('close')"
            class="px-5 py-2 rounded-xl bg-blue-600 text-white text-xs font-semibold hover:bg-blue-700 transition-colors">
            Готово
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { POSITION_AND_GROUP_LABEL } from '@/constants'

interface Item { id: number | string; [k: string]: any }

const props = defineProps<{
  title: string
  subtitle?: string
  insideItems: Item[]
  outsideItems: Item[]
  insideLabel?: string
  outsideLabel?: string
  searchPlaceholder?: string
  labelField?: string
  subField?: string
  searchKeys?: string[]
}>()

defineEmits<{ add: [id: number | string], remove: [id: number | string], close: [] }>()

const searchIn = ref('')
const searchOut = ref('')

function itemLabel(it: Item): string {
  return String(it[props.labelField || 'name'] ?? '')
}
function itemSub(it: Item): string {
  if (!props.subField) return ''
  const raw = String(it[props.subField] ?? '')
  if (!raw) return ''
  // Если в subField лежит код позиции (ST, CB, GK...) — дописываем русский лейбл в скобках
  const ru = POSITION_AND_GROUP_LABEL[raw]
  return ru ? `${raw} (${ru})` : raw
}
function matches(it: Item, q: string): boolean {
  const query = q.trim().toLowerCase()
  if (!query) return true
  const keys = props.searchKeys && props.searchKeys.length ? props.searchKeys : [props.labelField || 'name']
  return keys.some(k => String(it[k] ?? '').toLowerCase().includes(query))
}

const insideFiltered  = computed(() => props.insideItems.filter(it => matches(it, searchIn.value)))
const outsideFiltered = computed(() => props.outsideItems.filter(it => matches(it, searchOut.value)))
</script>
