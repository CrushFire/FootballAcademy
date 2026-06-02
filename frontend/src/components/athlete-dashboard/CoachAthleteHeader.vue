<template>
  <div class="bg-white rounded-2xl border border-neutral-200 shadow-sm">
    <!-- Синяя полоска сверху -->
    <div class="h-1 bg-gradient-to-r from-blue-500 to-blue-400" />

    <div class="px-5 py-4 flex items-center justify-between gap-4">
      <!-- Кнопка назад + инфо -->
      <div class="flex items-center gap-4 flex-1 min-w-0">
        <button @click="$emit('back')"
          class="flex items-center gap-1.5 px-3 py-2 rounded-xl border border-blue-200 bg-blue-50 text-blue-600 text-xs font-semibold hover:bg-blue-100 transition-colors shrink-0">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-3.5 h-3.5">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
          </svg>
          К списку
        </button>

        <!-- Аватарка спортсмена: на всю высоту плашки. -->
        <img v-if="avatarUrl" :src="avatarUrl" class="w-16 h-16 rounded-full object-cover object-top border-2 border-blue-200 shrink-0" />
        <div v-else class="w-16 h-16 rounded-full bg-blue-100 flex items-center justify-center shrink-0 border-2 border-blue-200">
          <span class="text-blue-600 font-bold text-xl">{{ initials }}</span>
        </div>

        <!-- Данные -->
        <div class="flex-1 min-w-0">
          <div class="text-base font-bold text-neutral-900 truncate">{{ info.fio || '—' }}</div>
          <div class="flex flex-wrap items-center gap-x-2 gap-y-0.5 mt-0.5">
            <span v-if="info.position" class="text-xs font-semibold px-1.5 py-0.5 rounded-md bg-blue-50 text-blue-600">{{ info.position }}</span>
            <span v-if="info.specialization" class="text-xs text-neutral-400">{{ SPEC_LABEL[info.specialization] ?? info.specialization }}</span>
            <span class="text-neutral-200 text-xs">·</span>
            <span v-if="info.age" class="text-xs text-neutral-500">{{ info.age }} лет</span>
            <span v-if="info.height" class="text-xs text-neutral-400">{{ info.height }} см</span>
            <span v-if="info.weight" class="text-xs text-neutral-400">{{ info.weight }} кг</span>
            <span v-if="info.groupName" class="text-xs text-neutral-400 ml-1">· {{ info.groupName }}</span>
          </div>
        </div>
      </div>

      <!-- Кнопка редактировать -->
      <button @click="$emit('edit')"
        class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl border border-green-200 bg-green-50 text-green-600 text-xs font-semibold hover:bg-green-100 transition-colors shrink-0">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3.5 h-3.5">
          <path stroke-linecap="round" stroke-linejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"/>
        </svg>
        Редактировать
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { imageUrl } from '@/utils/imageUrl'

const SPEC_LABEL: Record<string, string> = { Football: 'Футбол', Minifootball: 'Мини-футбол' }

const props = defineProps<{ info: any }>()
defineEmits<{ back: []; edit: [] }>()

const initials = computed(() => {
  const parts = (props.info.fio ?? '').split(' ')
  return parts.slice(0, 2).map((p: string) => p[0] ?? '').join('').toUpperCase() || '?'
})

const avatarUrl = computed(() => imageUrl(props.info?.images))
</script>
