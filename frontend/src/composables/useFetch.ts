import { ref } from 'vue'
import api from '@/services/api'
import type { Filter } from '@/types'

export function useFetch<T>(url: string) {
  const data = ref<T | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetch(filter?: Filter) {
    loading.value = true
    error.value = null
    try {
      const res = await api.get(url, { params: filter })
      data.value = res.data.data
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      error.value = err.response?.data?.error ?? 'Ошибка загрузки'
    } finally {
      loading.value = false
    }
  }

  return { data, loading, error, fetch }
}
