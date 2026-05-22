import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface ErrorEntry {
  id: number
  timestamp: Date
  status: number
  method: string
  url: string
  message: string
  critical: boolean
}

// Ошибки которые НЕ считаем критическими
const IGNORED_STATUSES = new Set([401, 403, 404])

function isCritical(status: number): boolean {
  return status >= 500 || (status >= 400 && !IGNORED_STATUSES.has(status))
}

const STORAGE_KEY = 'fa_error_log'
const MAX_ENTRIES = 100

export const useErrorLogStore = defineStore('errorLog', () => {
  const entries = ref<ErrorEntry[]>(loadFromStorage())
  const sessionStart = ref<Date>(new Date())
  let nextId = entries.value.length ? Math.max(...entries.value.map(e => e.id)) + 1 : 1

  function loadFromStorage(): ErrorEntry[] {
    try {
      const raw = localStorage.getItem(STORAGE_KEY)
      if (!raw) return []
      return JSON.parse(raw).map((e: any) => ({ ...e, timestamp: new Date(e.timestamp) }))
    } catch { return [] }
  }

  function saveToStorage() {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(entries.value.slice(-MAX_ENTRIES)))
    } catch {}
  }

  function push(status: number, method: string, url: string, message: string) {
    if (IGNORED_STATUSES.has(status)) return
    const entry: ErrorEntry = {
      id: nextId++,
      timestamp: new Date(),
      status,
      method: method.toUpperCase(),
      url: url.replace('/api', ''),
      message,
      critical: isCritical(status),
    }
    entries.value.push(entry)
    if (entries.value.length > MAX_ENTRIES) entries.value.shift()
    saveToStorage()
  }

  function clear() {
    entries.value = []
    localStorage.removeItem(STORAGE_KEY)
  }

  // Только критические
  const critical = computed(() => entries.value.filter(e => e.critical))

  // Последний критический сбой
  const lastCritical = computed(() =>
    critical.value.length ? critical.value[critical.value.length - 1] : null
  )

  // Аптайм — время с последнего критического или с sessionStart
  const uptimeSince = computed(() =>
    lastCritical.value ? lastCritical.value.timestamp : sessionStart.value
  )

  // Частые ошибки — топ-5 по URL
  const topErrors = computed(() => {
    const counts: Record<string, { url: string, count: number, lastStatus: number }> = {}
    for (const e of entries.value) {
      const key = `${e.method} ${e.url}`
      if (!counts[key]) counts[key] = { url: key, count: 0, lastStatus: e.status }
      counts[key].count++
      counts[key].lastStatus = e.status
    }
    return Object.values(counts).sort((a, b) => b.count - a.count).slice(0, 5)
  })

  // Всплески — более 5 ошибок за последние 60 секунд
  const recentSpike = computed(() => {
    const now = Date.now()
    const recent = entries.value.filter(e => now - e.timestamp.getTime() < 60_000)
    return recent.length >= 5 ? recent.length : 0
  })

  return { entries, critical, lastCritical, uptimeSince, topErrors, recentSpike, push, clear }
})
