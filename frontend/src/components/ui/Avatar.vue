<template>
  <div
    class="rounded-full overflow-hidden flex items-center justify-center shrink-0 select-none"
    :class="[sizeClass, bgClass, borderClass]"
  >
    <img
      v-if="src"
      :src="src"
      :alt="name ?? ''"
      class="w-full h-full object-cover object-top"
      @error="onError"
    />
    <span v-else class="font-bold" :class="textClass">{{ initialsValue }}</span>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { imageUrl, initials as toInitials } from '@/utils/imageUrl'

const props = withDefaults(defineProps<{
  // Принимает: путь строкой, объект {path}, массив таких объектов, или сразу готовый URL
  image?: unknown
  name?: string | null
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | '2xl'
  variant?: 'blue' | 'green' | 'neutral'
  border?: boolean
}>(), {
  size: 'md',
  variant: 'blue',
  border: false,
})

const broken = ref(false)
watch(() => props.image, () => { broken.value = false })

const src = computed(() => {
  if (broken.value) return null
  // Если уже готовый /api/... URL — используем как есть
  if (typeof props.image === 'string' && props.image.startsWith('/')) return props.image
  return imageUrl(props.image)
})

const initialsValue = computed(() => toInitials(props.name))

function onError() { broken.value = true }

const sizeClass = computed(() => {
  switch (props.size) {
    case 'xs':  return 'w-7 h-7 text-[10px]'
    case 'sm':  return 'w-9 h-9 text-xs'
    case 'md':  return 'w-12 h-12 text-sm'
    case 'lg':  return 'w-16 h-16 text-lg'
    case 'xl':  return 'w-20 h-20 text-xl'
    case '2xl': return 'w-28 h-28 text-2xl'
  }
})

const bgClass = computed(() => {
  switch (props.variant) {
    case 'blue':    return 'bg-blue-50'
    case 'green':   return 'bg-green-50'
    case 'neutral': return 'bg-neutral-100'
  }
})

const textClass = computed(() => {
  switch (props.variant) {
    case 'blue':    return 'text-blue-600'
    case 'green':   return 'text-green-700'
    case 'neutral': return 'text-neutral-500'
  }
})

const borderClass = computed(() => props.border ? 'border-2 border-neutral-200' : '')
</script>
