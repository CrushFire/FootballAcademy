import { ref } from 'vue'

export function useToast() {
  const toast = ref<'saved' | 'deleted' | null>(null)
  let timer: ReturnType<typeof setTimeout> | null = null

  function showToast(type: 'saved' | 'deleted') {
    if (timer) clearTimeout(timer)
    toast.value = type
    timer = setTimeout(() => { toast.value = null }, 2500)
  }

  return { toast, showToast }
}
