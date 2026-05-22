<template>
  <div class="relative" ref="rootEl">
    <button
      type="button"
      @click="toggle"
      class="w-full flex items-center justify-between text-left text-sm rounded-xl border border-neutral-200 bg-white px-3 py-2.5 focus:outline-none focus:border-blue-400 transition-colors"
      :class="open ? 'border-blue-400' : 'hover:border-neutral-300'"
    >
      <span :class="selectedItem ? 'text-neutral-800' : 'text-neutral-400'" class="truncate">
        <slot name="selected" :item="selectedItem">
          {{ selectedItem ? itemLabel(selectedItem) : (placeholder || 'Выберите...') }}
        </slot>
      </span>
      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-neutral-400 shrink-0 ml-2">
        <path stroke-linecap="round" stroke-linejoin="round" d="M19 9l-7 7-7-7" />
      </svg>
    </button>

    <div v-if="open" class="absolute z-30 mt-1 w-full bg-white rounded-xl border border-neutral-200 shadow-lg overflow-hidden">
      <div class="p-2 border-b border-neutral-100">
        <input
          ref="searchEl"
          v-model="query"
          :placeholder="searchPlaceholder || 'Поиск...'"
          class="w-full text-sm rounded-lg border border-neutral-200 px-3 py-1.5 focus:outline-none focus:border-blue-400"
        />
      </div>
      <div class="max-h-60 overflow-y-auto divide-y divide-neutral-50">
        <button
          v-if="allowEmpty"
          type="button"
          @click="select(null)"
          class="w-full text-left px-3 py-2 text-sm transition-colors"
          :class="modelValue == null ? 'bg-blue-50 text-blue-700 font-semibold' : 'text-neutral-400 hover:bg-neutral-50'"
        >
          {{ emptyLabel || 'Не выбрано' }}
        </button>
        <template v-if="grouped">
          <template v-for="grp in groupedFiltered" :key="grp.key">
            <div v-if="grp.items.length" class="px-3 pt-2 pb-1 text-[10px] font-bold uppercase tracking-wider text-neutral-400 bg-neutral-50">
              {{ grp.label }}
            </div>
            <button
              type="button"
              v-for="it in grp.items"
              :key="grp.key + '-' + it.id"
              @click="select(it.id)"
              class="w-full text-left px-3 py-2 text-sm transition-colors"
              :class="modelValue === it.id ? 'bg-blue-50 text-blue-700 font-semibold' : 'text-neutral-700 hover:bg-neutral-50'"
            >
              <slot name="option" :item="it">{{ itemLabel(it) }}</slot>
            </button>
          </template>
        </template>
        <template v-else>
          <button
            type="button"
            v-for="it in filtered"
            :key="it.id"
            @click="select(it.id)"
            class="w-full text-left px-3 py-2 text-sm transition-colors"
            :class="modelValue === it.id ? 'bg-blue-50 text-blue-700 font-semibold' : 'text-neutral-700 hover:bg-neutral-50'"
          >
            <slot name="option" :item="it">{{ itemLabel(it) }}</slot>
          </button>
        </template>
        <div v-if="emptyResult" class="px-3 py-3 text-xs text-neutral-400 italic text-center">Ничего не найдено</div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onBeforeUnmount, nextTick, watch } from 'vue'

interface ItemBase {
  id: number | string
  [k: string]: any
}
interface Group {
  key: string
  label: string
  items: ItemBase[]
}

const props = defineProps<{
  modelValue: number | string | null
  items?: ItemBase[]
  groups?: Group[]
  labelField?: string
  placeholder?: string
  searchPlaceholder?: string
  emptyLabel?: string
  allowEmpty?: boolean
  searchKeys?: string[]
}>()

const emit = defineEmits<{ 'update:modelValue': [value: number | string | null] }>()

const open = ref(false)
const query = ref('')
const rootEl = ref<HTMLDivElement>()
const searchEl = ref<HTMLInputElement>()

const flatItems = computed<ItemBase[]>(() => {
  if (props.groups) return props.groups.flatMap(g => g.items)
  return props.items ?? []
})

const selectedItem = computed(() => flatItems.value.find(i => i.id === props.modelValue) ?? null)
const grouped = computed(() => !!props.groups)

function itemLabel(it: ItemBase): string {
  const k = props.labelField || 'name'
  return String(it[k] ?? '')
}

function matches(it: ItemBase): boolean {
  const q = query.value.trim().toLowerCase()
  if (!q) return true
  const keys = props.searchKeys && props.searchKeys.length ? props.searchKeys : [props.labelField || 'name']
  return keys.some(k => String(it[k] ?? '').toLowerCase().includes(q))
}

const filtered = computed(() => (props.items ?? []).filter(matches))
const groupedFiltered = computed<Group[]>(() =>
  (props.groups ?? []).map(g => ({ ...g, items: g.items.filter(matches) }))
)

const emptyResult = computed(() => {
  if (grouped.value) return groupedFiltered.value.every(g => g.items.length === 0)
  return filtered.value.length === 0
})

function toggle() {
  open.value = !open.value
  if (open.value) {
    query.value = ''
    nextTick(() => searchEl.value?.focus())
  }
}

function select(id: number | string | null) {
  emit('update:modelValue', id)
  open.value = false
}

function onClickOutside(e: MouseEvent) {
  if (!open.value) return
  if (rootEl.value && !rootEl.value.contains(e.target as Node)) open.value = false
}

onMounted(() => document.addEventListener('mousedown', onClickOutside))
onBeforeUnmount(() => document.removeEventListener('mousedown', onClickOutside))

watch(open, v => { if (!v) query.value = '' })
</script>
