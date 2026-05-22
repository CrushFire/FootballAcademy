<template>
  <div class="p-3 h-full relative">
    <AppCard no-border class="h-full flex flex-col">

      <!-- Вкладки -->
      <div class="flex items-center gap-1 mb-4 pb-4 border-b border-neutral-100 flex-shrink-0">
        <button
          v-for="tab in tabs" :key="tab.key"
          @click="activeTab = tab.key"
          class="text-xs px-4 py-1.5 rounded-xl font-medium transition-colors"
          :class="activeTab === tab.key ? 'bg-blue-500 text-white' : 'text-neutral-500 hover:bg-neutral-100'"
        >{{ tab.label }}</button>
        <div class="flex-1" />
        <button v-if="activeTab === 'users'" @click="openCreate"
          class="text-xs px-3 py-1.5 rounded-xl bg-blue-500 text-white hover:bg-blue-600 transition-colors">
          + Добавить
        </button>
      </div>

      <!-- Вкладка: Пользователи -->
      <template v-if="activeTab === 'users'">
        <div class="relative mb-3 flex-shrink-0">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2"
            class="w-4 h-4 text-neutral-400 absolute left-3 top-1/2 -translate-y-1/2 pointer-events-none">
            <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-4.35-4.35M17 11A6 6 0 115 11a6 6 0 0112 0z"/>
          </svg>
          <input v-model="search" type="text" placeholder="Поиск..."
            class="w-full pl-9 pr-3 py-2 text-sm rounded-xl border border-neutral-200 bg-neutral-50 focus:outline-none focus:border-blue-400 placeholder-neutral-300" />
        </div>
        <div class="flex-1 overflow-y-auto flex flex-col gap-1.5">
          <div v-if="loading" class="text-sm text-neutral-400 py-4 text-center">Загрузка...</div>
          <div v-else-if="!filtered.length" class="text-sm text-neutral-400 py-4 text-center">Нет записей</div>
          <AdminEntityCard v-for="u in filtered" :key="u.id" @edit="openEdit(u)" @delete="openDelete(u)">
            <div class="text-sm font-semibold text-neutral-800">{{ u.login }}</div>
            <div class="text-xs text-neutral-400 mt-0.5">{{ u.email }} · {{ ROLE_LABEL[u.role] ?? u.role }}</div>
          </AdminEntityCard>
        </div>
      </template>

      <!-- Вкладка: Мониторинг -->
      <template v-if="activeTab === 'monitor'">

        <!-- Заголовок + действия -->
        <div class="flex items-center justify-between mb-4 flex-shrink-0">
          <div>
            <div class="text-sm font-bold text-neutral-800">Мониторинг системы</div>
            <div class="text-xs text-neutral-400 mt-0.5">Состояние сервера, uptime и ошибки 5xx у пользователей</div>
          </div>
          <div class="flex items-center gap-2">
            <span v-if="monLastCheck" class="text-xs text-neutral-400">{{ monLastCheck }}</span>
            <button @click="loadMonitoring" :disabled="monLoading"
              class="text-xs px-3 py-1.5 rounded-xl bg-blue-500 text-white hover:bg-blue-600 disabled:opacity-50 transition-colors flex items-center gap-1">
              <svg v-if="monLoading" class="w-3 h-3 animate-spin" fill="none" viewBox="0 0 24 24">
                <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4"/>
                <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8z"/>
              </svg>
              Обновить
            </button>
            <button @click="clearMonErrors"
              class="text-xs px-3 py-1.5 rounded-xl border border-neutral-200 text-neutral-500 hover:bg-neutral-50 transition-colors">
              Очистить лог
            </button>
          </div>
        </div>

        <!-- 4 KPI карточки -->
        <div class="grid grid-cols-2 gap-3 mb-4 flex-shrink-0">

          <!-- Backend статус -->
          <div class="p-4 rounded-2xl border flex items-start gap-3"
            :class="backendAlive ? 'border-blue-200 bg-blue-50/50' : monFetchError === 'no_access' ? 'border-amber-200 bg-amber-50' : 'border-neutral-200 bg-neutral-50'">
            <div class="w-8 h-8 rounded-lg border flex items-center justify-center flex-shrink-0"
              :class="backendAlive ? 'border-blue-200 bg-blue-100' : monFetchError === 'no_access' ? 'border-amber-200 bg-amber-100' : 'border-neutral-200 bg-neutral-100'">
              <svg class="w-4 h-4" :class="backendAlive ? 'text-blue-500' : monFetchError === 'no_access' ? 'text-amber-500' : 'text-neutral-400'"
                fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path v-if="backendAlive" stroke-linecap="round" stroke-linejoin="round" d="M5 12h14M12 5l7 7-7 7"/>
                <path v-else-if="monFetchError === 'no_access'" stroke-linecap="round" stroke-linejoin="round" d="M12 15v2m0-6v2m-6 4h12a2 2 0 002-2V7a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2z"/>
                <path v-else stroke-linecap="round" stroke-linejoin="round" d="M8.111 16.404a5.5 5.5 0 017.778 0M12 20h.01m-7.08-7.071c3.904-3.905 10.236-3.905 14.141 0M1.394 9.393c5.857-5.857 15.355-5.857 21.213 0"/>
              </svg>
            </div>
            <div>
              <div class="text-xs font-bold"
                :class="backendAlive ? 'text-blue-700' : monFetchError === 'no_access' ? 'text-amber-700' : 'text-neutral-500'">
                {{ backendAlive ? 'Сервер доступен' : monFetchError === 'no_access' ? 'Нет доступа' : 'Нет ответа' }}
              </div>
              <div class="text-xs text-neutral-400 mt-0.5">
                {{ backendAlive
                  ? (monStatus ? 'Сервер отвечает нормально' : 'Сервер отвечает на /api/health')
                  : monFetchError === 'no_access' ? 'Нет прав на мониторинг'
                  : 'Не удалось получить статус сервера' }}
              </div>
            </div>
          </div>

          <!-- Uptime -->
          <div class="p-4 rounded-2xl border-2 border-blue-100 bg-white flex items-start gap-3">
            <div class="w-8 h-8 rounded-xl bg-blue-100 flex items-center justify-center flex-shrink-0">
              <svg class="w-4 h-4 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <circle cx="12" cy="12" r="10"/>
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 6v6l4 2"/>
              </svg>
            </div>
            <div>
              <div class="text-xs font-bold text-neutral-800">{{ uptimeStr }}</div>
              <div class="text-xs text-neutral-400 mt-0.5">С момента запуска сервера</div>
            </div>
          </div>

          <!-- Ошибки 5xx -->
          <div class="p-4 rounded-2xl border-2 flex items-start gap-3"
            :class="monStatus && monStatus.criticalErrorsCount > 0 ? 'border-red-200 bg-red-50' : 'border-neutral-100 bg-white'">
            <div class="w-8 h-8 rounded-xl flex items-center justify-center flex-shrink-0"
              :class="monStatus && monStatus.criticalErrorsCount > 0 ? 'bg-red-500' : 'bg-neutral-200'">
              <svg class="w-4 h-4" :class="monStatus && monStatus.criticalErrorsCount > 0 ? 'text-white' : 'text-neutral-500'"
                fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v4m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/>
              </svg>
            </div>
            <div>
              <div class="text-xs font-bold" :class="monStatus && monStatus.criticalErrorsCount > 0 ? 'text-red-700' : 'text-neutral-700'">
                {{ monStatus ? monStatus.criticalErrorsCount + ' ошибок 5xx' : '—' }}
              </div>
              <div class="text-xs text-neutral-400 mt-0.5">
                {{ !monStatus ? 'Проверка невозможна'
                  : monStatus.lastCriticalErrorAt ? 'Последняя: ' + formatTime(new Date(monStatus.lastCriticalErrorAt))
                  : 'Серверных сбоев не было' }}
              </div>
            </div>
          </div>

          <!-- Пользователи с ошибками -->
          <div class="p-4 rounded-2xl border-2 border-neutral-100 bg-white flex items-start gap-3">
            <div class="w-8 h-8 rounded-xl bg-blue-100 flex items-center justify-center flex-shrink-0">
              <svg class="w-4 h-4 text-blue-600" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
                <path stroke-linecap="round" stroke-linejoin="round" d="M17 20h5v-2a4 4 0 00-5-5M9 20H4v-2a4 4 0 015-5m4-4a4 4 0 110-8 4 4 0 010 8z"/>
              </svg>
            </div>
            <div>
              <div class="text-xs font-bold text-neutral-800">
                {{ monStatus ? monStatus.affectedUsersCount + ' польз.' : '—' }}
              </div>
              <div class="text-xs text-neutral-400 mt-0.5">
                {{ !monStatus ? 'Проверка невозможна'
                  : monStatus.affectedUsersCount > 0 ? 'Получили ошибку 5xx'
                  : 'Ошибок 5xx у пользователей не было' }}
              </div>
            </div>
          </div>
        </div>

        <!-- Журнал ошибок -->
        <div class="flex items-center justify-between mb-2 flex-shrink-0">
          <div class="text-xs font-semibold text-neutral-600">Журнал ошибок</div>
          <span class="text-xs px-2 py-0.5 rounded-lg bg-neutral-100 text-neutral-500">{{ monErrors.length }}</span>
        </div>
        <div class="flex-1 overflow-y-auto flex flex-col gap-1.5 min-h-0">

          <div v-if="!monErrors.length" class="flex flex-col items-center justify-center py-8 gap-2">
            <div class="w-12 h-12 rounded-2xl flex items-center justify-center"
              :class="!backendAlive ? 'bg-neutral-100' : 'bg-blue-50'">
              <svg class="w-6 h-6" :class="!backendAlive ? 'text-neutral-400' : 'text-blue-400'"
                fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
                <path v-if="backendAlive" stroke-linecap="round" stroke-linejoin="round" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"/>
                <path v-else stroke-linecap="round" stroke-linejoin="round" d="M12 9v4m0 4h.01M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/>
              </svg>
            </div>
            <div class="text-sm font-semibold text-neutral-700">
              {{ !backendAlive ? 'Статус backend недоступен' : 'Критических ошибок нет' }}
            </div>
            <div class="text-xs text-neutral-400 text-center">
              {{ !backendAlive ? 'Проверка журнала ошибок невозможна' : 'Ошибки 500/502/503/504 пока не зафиксированы' }}
            </div>
          </div>

          <div v-for="e in monErrors" :key="e.timestamp + e.url"
            class="flex items-start gap-3 px-3 py-2.5 rounded-xl border text-xs"
            :class="e.status >= 500 ? 'border-red-200 bg-red-50' : 'border-amber-100 bg-amber-50'">
            <span class="font-mono font-bold flex-shrink-0 px-2 py-1 rounded-lg text-white text-[11px]"
              :class="e.status >= 500 ? 'bg-red-500' : 'bg-amber-500'">
              {{ e.status }}
            </span>
            <div class="flex-1 min-w-0">
              <div class="font-mono text-neutral-700 truncate">{{ e.method }} {{ e.url }}</div>
              <div class="text-neutral-500 mt-0.5 truncate">{{ e.message }}</div>
            </div>
            <div class="flex flex-col items-end gap-1 flex-shrink-0">
              <span class="text-neutral-400">{{ formatTime(new Date(e.timestamp)) }}</span>
              <span v-if="e.userId" class="text-[10px] px-1.5 py-0.5 rounded bg-blue-100 text-blue-600">uid {{ e.userId }}</span>
            </div>
          </div>
        </div>

        <!-- Системная информация -->
        <div class="mt-3 pt-3 border-t border-neutral-100 flex-shrink-0">
          <div class="text-xs font-semibold text-neutral-500 mb-2">Системная информация</div>
          <div class="grid grid-cols-2 gap-x-6 gap-y-1">
            <div class="flex items-center justify-between text-xs">
              <span class="text-neutral-400">Старт сервера</span>
              <span class="text-neutral-600 font-mono">{{ monStatus ? formatTime(new Date(monStatus.serverStartedAt)) : '—' }}</span>
            </div>
            <div class="flex items-center justify-between text-xs">
              <span class="text-neutral-400">Последняя проверка</span>
              <span class="text-neutral-600 font-mono">{{ monLastCheck || '—' }}</span>
            </div>
          </div>
        </div>

      </template>

    </AppCard>

    <div
      v-if="toast"
      class="absolute top-6 left-1/2 -translate-x-1/2 text-white text-xs font-semibold px-4 py-2.5 rounded-2xl shadow-lg z-10 pointer-events-none whitespace-nowrap"
      :class="toast === 'saved' ? 'bg-green-600' : 'bg-neutral-600'"
    >
      {{ toast === 'saved' ? 'Изменения сохранены' : 'Запись удалена' }}
    </div>

  </div>

  <EditModal v-if="editItem !== undefined" :title="editItem ? 'Редактировать пользователя' : 'Новый пользователь'" @save="saveEdit" @cancel="editItem = undefined">
    <FormField label="Логин">
      <input v-model="editForm.login" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField label="Email">
      <input v-model="editForm.email" type="email" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField label="Пароль">
      <input v-model="editForm.password" type="password" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400" />
    </FormField>
    <FormField v-if="!editItem" label="Роль">
      <select v-model="editForm.role" class="w-full px-3 py-2 text-sm rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400">
        <option value="admin">Администратор</option>
        <option value="trainer">Тренер</option>
        <option value="medical">Медицинский</option>
        <option value="sportsman">Спортсмен</option>
      </select>
    </FormField>
  </EditModal>

  <ConfirmDeleteModal v-if="deleteItem" message="Удалить пользователя?" @confirm="confirmDelete" @cancel="deleteItem = null" />
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import api from '@/services/api'
import { useToast } from '@/composables/useToast'
import { useErrorLogStore } from '@/store/errorLog'
import AppCard from '@/components/ui/AppCard.vue'
import AdminEntityCard from '@/components/ui/AdminEntityCard.vue'
import ConfirmDeleteModal from '@/components/ui/ConfirmDeleteModal.vue'
import EditModal from '@/components/ui/EditModal.vue'
import FormField from '@/components/ui/FormField.vue'

const { toast, showToast } = useToast()

const tabs = [
  { key: 'users',   label: 'Пользователи' },
  { key: 'monitor', label: 'Мониторинг' },
] as const
const activeTab = ref<'users' | 'monitor'>('users')

// --- Пользователи ---
const ROLE_LABEL: Record<string, string> = {
  admin: 'Администратор', trainer: 'Тренер',
  medical: 'Медицинский', sportsman: 'Спортсмен',
}
const loading   = ref(false)
const items     = ref<any[]>([])
const search    = ref('')
const editItem  = ref<any>(undefined)
const editForm  = ref<any>({})
const deleteItem = ref<any>(null)

const filtered = computed(() => {
  const q = search.value.toLowerCase().trim()
  if (!q) return items.value
  return items.value.filter(u => u.login?.toLowerCase().includes(q) || u.email?.toLowerCase().includes(q))
})

function openCreate() { editItem.value = null; editForm.value = { login: '', email: '', password: '', role: 'trainer' } }
function openEdit(u: any) { editItem.value = u; editForm.value = { login: u.login, email: u.email, password: '' } }
function openDelete(u: any) { deleteItem.value = u }

async function saveEdit() {
  if (editItem.value) await api.put(`/users/update/${editItem.value.id}`, editForm.value)
  else await api.post('/users/create', editForm.value)
  editItem.value = undefined
  showToast('saved')
  await load()
}
async function confirmDelete() {
  await api.delete(`/users?userId=${deleteItem.value.id}`)
  deleteItem.value = null
  showToast('deleted')
  await load()
}
async function load() {
  loading.value = true
  const res = await api.get('/users/admin').catch(() => null)
  items.value = res?.data?.data ?? []
  loading.value = false
}

// --- Мониторинг ---
const log = useErrorLogStore()
const monLoading    = ref(false)
const monStatus     = ref<any>(null)
const monErrors     = ref<any[]>([])
const monLastCheck  = ref('')
const monFetchError = ref<'no_access' | 'no_response' | null>(null)
const backendAlive  = ref(false)   // true если /api/health ответил
const serverStartedAt = ref<Date | null>(null)
const tickNow = ref(Date.now())
let monInterval:  ReturnType<typeof setInterval> | null = null
let tickInterval: ReturnType<typeof setInterval> | null = null

async function loadMonitoring() {
  monLoading.value = true
  monFetchError.value = null

  // 1. Сначала проверяем /api/health — без авторизации
  try {
    await api.get('/health')
    backendAlive.value = true
  } catch {
    backendAlive.value = false
  }

  // 2. Если backend живой — пробуем получить данные мониторинга
  if (backendAlive.value) {
    try {
      const [statusRes, errorsRes] = await Promise.all([
        api.get('/admin/monitoring/status'),
        api.get('/admin/monitoring/errors'),
      ])
      monStatus.value = statusRes.data
      monErrors.value = errorsRes.data
      serverStartedAt.value = new Date(statusRes.data.serverStartedAt)
    } catch (e: any) {
      monStatus.value = null
      const status = e?.response?.status
      monFetchError.value = (status === 401 || status === 403) ? 'no_access' : 'no_response'
    }
  } else {
    monStatus.value = null
    monFetchError.value = 'no_response'
  }

  monLastCheck.value = new Date().toLocaleTimeString('ru-RU')
  monLoading.value = false
}

async function clearMonErrors() {
  await api.delete('/admin/monitoring/errors').catch(() => {})
  log.clear()
  await loadMonitoring()
}

function formatTime(d: Date): string {
  return d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
}

const uptimeStr = computed(() => {
  if (!serverStartedAt.value) return '—'
  const secs = Math.floor((tickNow.value - serverStartedAt.value.getTime()) / 1000)
  const h = Math.floor(secs / 3600)
  const m = Math.floor((secs % 3600) / 60)
  const s = secs % 60
  return `${h}ч ${m}м ${s}с`
})

onMounted(() => {
  load()
  loadMonitoring()
  monInterval  = setInterval(loadMonitoring, 30_000)
  tickInterval = setInterval(() => { tickNow.value = Date.now() }, 1_000)
})
onUnmounted(() => {
  if (monInterval)  clearInterval(monInterval)
  if (tickInterval) clearInterval(tickInterval)
})
</script>
