import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '@/services/api'
import router from '@/router'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(localStorage.getItem('token'))
  const userId = ref<number | null>(
    localStorage.getItem('userId') ? Number(localStorage.getItem('userId')) : null
  )
  const userLogin = ref<string | null>(localStorage.getItem('userLogin'))
  const personalId = ref<number | null>(
    localStorage.getItem('personalId') ? Number(localStorage.getItem('personalId')) : null
  )
  const personalType = ref<string | null>(localStorage.getItem('personalType') ?? null)

  // Роль: сначала из localStorage, если нет — парсим из JWT
  function getRoleFromToken(t: string | null): string | null {
    if (!t) return null
    try {
      const payload = JSON.parse(atob(t.split('.')[1]))
      return payload.role ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? null
    } catch { return null }
  }
  function getPersonalTypeFromToken(t: string | null): string | null {
    if (!t) return null
    try {
      const payload = JSON.parse(atob(t.split('.')[1]))
      return payload.personalType ?? null
    } catch { return null }
  }
  const userRole = ref<string | null>(
    (localStorage.getItem('userRole') ?? getRoleFromToken(localStorage.getItem('token')))?.toLowerCase().trim() ?? null
  )
  if (!personalType.value) {
    personalType.value = getPersonalTypeFromToken(localStorage.getItem('token'))
  }

  const isAuthenticated = computed(() => !!token.value)

  async function login(identifier: string, password: string) {
    // Универсальный вход: identifier = логин или email. Бэк сам разрулит.
    const { data } = await api.post('/auth', { identifier, password })
    token.value = data.data.token
    userId.value = data.data.userId
    localStorage.setItem('token', data.data.token)
    localStorage.setItem('userId', String(data.data.userId))
    // Берём роль из JWT payload
    try {
      const payload = JSON.parse(atob(data.data.token.split('.')[1]))
      const role = payload.role
        ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
        ?? payload['role']
      if (role) {
        const normalizedRole = String(role).toLowerCase().trim()
        userRole.value = normalizedRole
        localStorage.setItem('userRole', normalizedRole)
      }
      const pType = payload.personalType
      if (pType) { personalType.value = pType; localStorage.setItem('personalType', pType) }
    } catch { /* ignore */ }
    // Подгружаем ФИО
    try {
      const personalRes = await api.get('/personal/me').catch(() => null)
      const name = personalRes?.data?.data?.fio
      if (personalRes?.data?.data?.id) {
        personalId.value = personalRes.data.data.id
        localStorage.setItem('personalId', String(personalRes.data.data.id))
      }
      if (name) {
        userLogin.value = name; localStorage.setItem('userLogin', name)
      } else {
        const sportsmanRes = await api.get('/sportsman/me').catch(() => null)
        const sfio = sportsmanRes?.data?.data?.fio
        if (sfio) {
          userLogin.value = sfio; localStorage.setItem('userLogin', sfio)
        } else {
          const userRes = await api.get('/users/me').catch(() => null)
          const login = userRes?.data?.data?.login ?? identifier
          userLogin.value = login; localStorage.setItem('userLogin', login)
        }
      }
    } catch {
      userLogin.value = identifier
      localStorage.setItem('userLogin', identifier)
    }
    const role = userRole.value
    const pType = personalType.value?.toLowerCase()
    if (role === 'admin') router.push({ name: 'AdminDashboard' })
    else if (role === 'personal' && pType === 'trainer') router.push({ name: 'TrainerDashboard' })
    else if (role === 'personal' && pType === 'medical') router.push({ name: 'MedicalDashboard' })
    else router.push({ name: 'Dashboard' })
  }

  function logout() {
    token.value = null
    userId.value = null
    userLogin.value = null
    userRole.value = null
    personalId.value = null
    personalType.value = null
    localStorage.removeItem('token')
    localStorage.removeItem('userId')
    localStorage.removeItem('userLogin')
    localStorage.removeItem('userRole')
    localStorage.removeItem('personalId')
    localStorage.removeItem('personalType')
    router.push({ name: 'Login' })
  }

  return { token, userId, userLogin, userRole, personalId, personalType, isAuthenticated, login, logout }
})
