import { ref } from 'vue'
import { useAuthStore } from '@/store/auth'

export function useAuth() {
  const auth = useAuthStore()
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function login(identifier: string, password: string, mode: 'login' | 'email' = 'login') {
    loading.value = true
    error.value = null
    try {
      await auth.login(identifier, password, mode)
    } catch (e: unknown) {
      const err = e as { response?: { data?: { error?: string } } }
      error.value = err.response?.data?.error ?? 'Ошибка входа'
    } finally {
      loading.value = false
    }
  }

  return { login, logout: auth.logout, isAuthenticated: auth.isAuthenticated, loading, error }
}
