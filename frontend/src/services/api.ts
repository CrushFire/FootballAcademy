import axios from 'axios'
import { useAuthStore } from '@/store/auth'
import { useErrorLogStore } from '@/store/errorLog'

// ASP.NET Core [FromQuery] ожидает dot-нотацию: filters.trainerId[0]=1
// Axios по умолчанию даёт bracket-нотацию: filters[trainerId][0]=1
function dotNotationSerializer(params: Record<string, any>, prefix = ''): string {
  const parts: string[] = []
  for (const key of Object.keys(params)) {
    const val = params[key]
    const fullKey = prefix ? `${prefix}.${key}` : key
    if (val === null || val === undefined) continue
    if (Array.isArray(val)) {
      val.forEach((item, i) => {
        if (item !== null && typeof item === 'object') {
          parts.push(dotNotationSerializer(item, `${fullKey}[${i}]`))
        } else {
          parts.push(`${fullKey}[${i}]=${encodeURIComponent(item)}`)
        }
      })
    } else if (typeof val === 'object') {
      parts.push(dotNotationSerializer(val, fullKey))
    } else {
      parts.push(`${fullKey}=${encodeURIComponent(val)}`)
    }
  }
  return parts.join('&')
}

const api = axios.create({
  baseURL: '/api',
  timeout: 15000,
  headers: { 'Content-Type': 'application/json' },
  paramsSerializer: (params) => dotNotationSerializer(params),
})

api.interceptors.request.use((config) => {
  const auth = useAuthStore()
  if (auth.token) {
    config.headers.Authorization = `Bearer ${auth.token}`
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error.response?.status ?? 0
    const method = error.config?.method ?? 'GET'
    const url = error.config?.url ?? ''
    const message = error.response?.data?.message ?? error.message ?? 'Unknown error'

    if (status === 401) {
      const auth = useAuthStore()
      auth.logout()
    } else {
      try {
        const log = useErrorLogStore()
        log.push(status, method, url, message)
      } catch {}

      // Отправка Серверных ошибок на бэк
      if (status >= 500 && !url.includes('/admin/monitoring')) {
        try {
          api.post('/admin/monitoring/error', { status, method: method.toUpperCase(), url, message })
            .catch(() => {})
        } catch {}
      }
    }

    return Promise.reject(error)
  }
)

export default api
