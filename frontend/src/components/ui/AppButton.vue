<template>
  <button
    v-bind="$attrs"
    :class="[
      'inline-flex items-center gap-2 px-4 py-2 rounded-lg font-medium text-sm transition-colors disabled:opacity-50',
      variants[variant]
    ]"
  >
    <slot />
  </button>
</template>

<script setup lang="ts">
withDefaults(defineProps<{ variant?: 'primary' | 'secondary' | 'danger' | 'ghost' }>(), {
  variant: 'primary'
})

const variants = {
  primary:   'bg-primary-600 hover:bg-primary-700 text-white',
  secondary: 'bg-gray-100 hover:bg-gray-200 text-gray-700',
  danger:    'bg-red-500 hover:bg-red-600 text-white',
  ghost:     'hover:bg-gray-100 text-gray-600'
}
</script>
