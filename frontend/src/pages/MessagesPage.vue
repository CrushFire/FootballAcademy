<template>
  <div class="p-0 md:p-3 h-full">
  <AppCard no-border class="h-full flex flex-col p-0 overflow-hidden">
  <div class="flex h-full overflow-hidden bg-neutral-50 md:rounded-2xl">

    <!-- Левая панель. На мобиле скрываем когда что-то выбрано. -->
    <aside
      class="w-full md:w-72 bg-white border-r border-neutral-200 flex-col shrink-0"
      :class="selectedKey ? 'hidden md:flex' : 'flex'"
    >

      <!-- Заголовок -->
      <div class="px-5 pt-0 pb-3 border-b border-neutral-100">
        <h1 class="text-xl font-bold text-neutral-900 leading-none">Чаты</h1>
      </div>

      <!-- Поиск -->
      <div class="p-3 border-b border-neutral-100">
        <input v-model="search" placeholder="Поиск..."
          class="w-full px-3 py-1.5 text-xs rounded-xl border border-neutral-200 focus:outline-none focus:border-blue-400 bg-neutral-50"/>
      </div>

      <!-- Сортировка -->
      <div class="px-3 py-2 border-b border-neutral-100 flex items-center gap-2">
        <span class="text-[10px] text-neutral-400 uppercase tracking-wide">Сортировка</span>
        <select v-model="sortBy" class="flex-1 text-xs rounded-lg border border-neutral-200 px-2 py-1 focus:outline-none bg-neutral-50">
          <option value="date">По дате</option>
          <option value="name">По имени</option>
          <option value="unread">По непрочитанным</option>
        </select>
        <button @click="sortDir = sortDir === 'asc' ? 'desc' : 'asc'" class="text-neutral-400 hover:text-neutral-600">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3.5 h-3.5" :class="sortDir === 'desc' ? 'rotate-180' : ''">
            <path stroke-linecap="round" stroke-linejoin="round" d="M3 4h13M3 8h9M3 12h5"/>
          </svg>
        </button>
      </div>

      <!-- Единый список: диалоги + рассылки вперемешку -->
      <div class="flex-1 overflow-y-auto">
        <div v-if="loading" class="p-6 text-center text-xs text-neutral-400">Загрузка...</div>
        <div v-else-if="combinedList.length === 0" class="p-6 text-center text-xs text-neutral-400">Нет сообщений</div>

        <div v-for="item in combinedList" :key="item.key"
          @click="selectItem(item)"
          class="flex items-center gap-3 px-4 py-3 cursor-pointer hover:bg-neutral-50 transition-colors border-b border-neutral-50"
          :class="selectedKey === item.key ? 'bg-blue-50 border-r-2 border-r-blue-500' : ''">

          <!-- Аватар: для рассылки — мегафон, для чата — инициалы -->
          <div v-if="item.type === 'broadcast'"
            class="w-9 h-9 rounded-full bg-blue-100 flex items-center justify-center shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-blue-500">
              <path stroke-linecap="round" stroke-linejoin="round" d="M11 5.882V19.24a1.76 1.76 0 01-3.417.592l-2.147-6.15M18 13a3 3 0 100-6M5.436 13.683A4.001 4.001 0 017 6h1.832c4.1 0 7.625-1.234 9.168-3v14c-1.543-1.766-5.067-3-9.168-3H7a3.988 3.988 0 01-1.564-.317z"/>
            </svg>
          </div>
          <div v-else
            class="w-9 h-9 rounded-full flex items-center justify-center text-white font-bold text-sm shrink-0"
            :class="roleColor(item.role)">
            {{ initials(item.name) }}
          </div>

          <div class="flex-1 min-w-0">
            <div class="text-sm font-semibold text-neutral-800 truncate">{{ item.name }}</div>
            <div class="text-xs text-neutral-400 truncate">{{ item.preview }}</div>
          </div>

          <div class="flex flex-col items-end gap-1 shrink-0">
            <span class="text-[10px] text-neutral-300">{{ item.dateStr }}</span>
            <span v-if="item.unread" class="bg-blue-500 text-white text-[10px] font-bold rounded-full min-w-[18px] h-[18px] flex items-center justify-center px-1">
              {{ item.unread > 99 ? '99+' : item.unread }}
            </span>
          </div>
        </div>
      </div>
    </aside>

    <!-- Основная область. На мобиле скрываем когда ничего не выбрано. -->
    <div
      class="flex-1 flex-col overflow-hidden"
      :class="selectedKey ? 'flex' : 'hidden md:flex'"
    >

      <!-- Чат -->
      <div v-if="selectedKey && selectedItem?.type === 'chat'" class="flex flex-col flex-1 min-h-0">
        <div class="px-3 md:px-5 py-3 bg-white border-b border-neutral-200 flex items-center gap-2 md:gap-3 shrink-0">
          <button
            @click="clearSelection"
            class="md:hidden w-8 h-8 rounded-lg flex items-center justify-center text-neutral-500 hover:bg-neutral-100 transition-colors shrink-0"
            aria-label="К списку"
          >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-4 h-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
            </svg>
          </button>
          <div class="w-9 h-9 rounded-full flex items-center justify-center text-white font-bold text-sm shrink-0"
            :class="roleColor(selectedItem.role)">
            {{ initials(selectedItem.name) }}
          </div>
          <div>
            <div class="font-semibold text-neutral-900 text-sm">{{ selectedItem.name }}</div>
            <div class="text-xs text-neutral-400">{{ roleLabel(selectedItem.role) }}</div>
          </div>
        </div>

        <div ref="messagesEl" class="flex-1 overflow-y-auto p-4 space-y-2">
          <div v-if="dialogLoading" class="text-center text-xs text-neutral-400 py-8">Загрузка...</div>
          <template v-else>
            <div v-for="msg in messages" :key="msg.id" class="flex"
              :class="msg.senderId === myId ? 'justify-end' : 'justify-start'">
              <div class="max-w-xs lg:max-w-md px-4 py-2.5 rounded-2xl text-sm shadow-sm"
                :class="msg.senderId === myId ? 'bg-blue-500 text-white rounded-br-sm' : 'bg-white text-neutral-900 rounded-bl-sm'">
                <p class="leading-relaxed">{{ msg.text }}</p>
                <div class="flex items-center gap-1 mt-1" :class="msg.senderId === myId ? 'justify-end' : 'justify-start'">
                  <span class="text-[10px] opacity-60">{{ formatTime(msg.createdAt) }}</span>
                </div>
              </div>
            </div>
            <div v-if="messages.length === 0" class="text-center text-xs text-neutral-400 py-8">Нет сообщений</div>
          </template>
        </div>

      </div>

      <!-- Рассылка как личное сообщение -->
      <div v-else-if="selectedKey && selectedItem?.type === 'broadcast'" class="flex flex-col flex-1 min-h-0">
        <div class="px-3 md:px-5 py-3 bg-white border-b border-neutral-200 flex items-center gap-2 md:gap-3 shrink-0">
          <button
            @click="clearSelection"
            class="md:hidden w-8 h-8 rounded-lg flex items-center justify-center text-neutral-500 hover:bg-neutral-100 transition-colors shrink-0"
            aria-label="К списку"
          >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-4 h-4">
              <path stroke-linecap="round" stroke-linejoin="round" d="M15 19l-7-7 7-7"/>
            </svg>
          </button>
          <div class="w-9 h-9 rounded-full bg-blue-100 flex items-center justify-center shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-blue-500">
              <path stroke-linecap="round" stroke-linejoin="round" d="M11 5.882V19.24a1.76 1.76 0 01-3.417.592l-2.147-6.15M18 13a3 3 0 100-6M5.436 13.683A4.001 4.001 0 017 6h1.832c4.1 0 7.625-1.234 9.168-3v14c-1.543-1.766-5.067-3-9.168-3H7a3.988 3.988 0 01-1.564-.317z"/>
            </svg>
          </div>
          <div>
            <div class="font-semibold text-neutral-900 text-sm">{{ selectedBroadcast?.title }}</div>
            <div class="text-xs text-neutral-400">{{ formatDate(selectedBroadcast?.createdAt ?? '') }}</div>
          </div>
        </div>

        <div class="flex-1 overflow-y-auto p-4">
          <div class="flex justify-start">
            <div class="max-w-xs lg:max-w-lg px-4 py-3 rounded-2xl rounded-bl-sm bg-white text-neutral-900 text-sm shadow-sm border border-neutral-100">
              <p class="leading-relaxed whitespace-pre-wrap">{{ selectedBroadcast?.text }}</p>
              <div class="mt-2 text-[10px] text-neutral-400">{{ formatDate(selectedBroadcast?.createdAt ?? '') }}</div>
            </div>
          </div>
        </div>

      </div>

      <!-- Пусто -->
      <div v-else class="flex-1 flex flex-col items-center justify-center gap-3 text-neutral-300">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5" class="w-14 h-14">
          <path stroke-linecap="round" stroke-linejoin="round" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-4 4-1-4z"/>
        </svg>
        <span class="text-sm">Выберите чат</span>
      </div>
    </div>
  </div>
  </AppCard>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue'
import api from '@/services/api'
import { useAuthStore } from '@/store/auth'
import { useSignalR } from '@/composables/useSignalR'
import AppCard from '@/components/ui/AppCard.vue'
import { useNotifications } from '@/composables/useNotifications'
import type { MessageResponse } from '@/types/message/dto'
import type { BroadcastResponse } from '@/types/broadcast/dto'

const auth = useAuthStore()
const { start, stop, on, off } = useSignalR()
const { markMessageReadBySender } = useNotifications()
const myId = computed(() => auth.userId)

const search = ref('')
const sortBy = ref<'date' | 'name' | 'unread'>('unread')
const sortDir = ref<'asc' | 'desc'>('desc')

const dialogUsers = ref<any[]>([])
const broadcasts = ref<BroadcastResponse[]>([])
const loading = ref(false)

const selectedKey = ref<string | null>(null)
const selectedItem = ref<any>(null)
const selectedBroadcast = ref<BroadcastResponse | null>(null)

const messages = ref<MessageResponse[]>([])
const dialogLoading = ref(false)
const messagesEl = ref<HTMLElement | null>(null)
const unread = ref<Record<number, number>>({})

function roleLabel(role: string): string {
  const map: Record<string, string> = { admin: 'Администратор', personal: 'Сотрудник', trainer: 'Тренер', medical: 'Медик', sportsman: 'Спортсмен' }
  return map[role?.toLowerCase()] ?? role
}
function roleColor(role: string): string {
  const map: Record<string, string> = { admin: 'bg-red-400', personal: 'bg-blue-500', trainer: 'bg-green-500', medical: 'bg-teal-500', sportsman: 'bg-neutral-400' }
  return map[role?.toLowerCase()] ?? 'bg-neutral-400'
}
function initials(name: string): string {
  if (!name) return '?'
  const parts = name.trim().split(' ')
  return parts.length >= 2 ? (parts[0][0] + parts[1][0]).toUpperCase() : name.slice(0, 2).toUpperCase()
}
function formatTime(iso: string): string { return new Date(iso).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' }) }
function formatDate(iso: string): string { return new Date(iso).toLocaleDateString('ru-RU', { day: 'numeric', month: 'short', year: 'numeric' }) }

function formatRelative(iso: string): string {
  if (!iso) return ''
  const d = new Date(iso)
  const now = new Date()
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())
  const yesterday = new Date(today.getTime() - 86400000)
  const msgDay = new Date(d.getFullYear(), d.getMonth(), d.getDate())
  if (msgDay.getTime() === today.getTime()) return d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })
  if (msgDay.getTime() === yesterday.getTime()) return 'Вчера'
  return d.toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' })
}

// Объединённый список: чаты + рассылки, отсортированы по дате
const combinedList = computed(() => {
  const chatItems = dialogUsers.value.map(u => ({
    key: `chat-${u.id}`,
    type: 'chat' as const,
    id: u.id,
    name: u.login ?? `ID ${u.id}`,
    role: u.role ?? '',
    preview: u.lastMessage || roleLabel(u.role),
    date: new Date(u.lastMessageAt ?? 0),
    dateStr: u.lastMessageAt ? formatRelative(u.lastMessageAt) : '',
    unread: unread.value[u.id] ?? 0,
  }))

  const broadcastItems = broadcasts.value.map(b => ({
    key: `broadcast-${b.id}`,
    type: 'broadcast' as const,
    id: b.id,
    name: b.title,
    role: '',
    preview: b.text.slice(0, 50),
    date: new Date(b.createdAt),
    dateStr: formatRelative(b.createdAt),
    unread: 0,
  }))

  let list = [...chatItems, ...broadcastItems]

  if (search.value.trim()) {
    const q = search.value.toLowerCase()
    list = list.filter(i => i.name?.toLowerCase().includes(q) || i.preview?.toLowerCase().includes(q))
  }

  return list.sort((a, b) => {
    if (sortBy.value === 'unread') {
      const diff = (b.unread ?? 0) - (a.unread ?? 0)
      return sortDir.value === 'desc' ? diff : -diff
    }
    if (sortBy.value === 'name') {
      const diff = a.name.localeCompare(b.name)
      return sortDir.value === 'asc' ? diff : -diff
    }
    // date
    const diff = b.date.getTime() - a.date.getTime()
    return sortDir.value === 'desc' ? diff : -diff
  })
})

function clearSelection() {
  selectedKey.value = null
  selectedItem.value = null
  selectedBroadcast.value = null
}

async function selectItem(item: any) {
  selectedKey.value = item.key
  selectedItem.value = item

  if (item.type === 'chat') {
    selectedBroadcast.value = null
    unread.value[item.id] = 0
    markMessageReadBySender(item.id)
    dialogLoading.value = true
    try {
      const res = await api.get(`/message/dialog/${item.id}`).catch(() => null)
      messages.value = res?.data?.data?.messages ?? []
      for (const msg of messages.value) {
        if (msg.senderId === item.id && !msg.isRead) api.put(`/message/${msg.id}/read`).catch(() => null)
      }
      await scrollToBottom()
    } finally { dialogLoading.value = false }
  } else {
    messages.value = []
    selectedBroadcast.value = broadcasts.value.find(b => b.id === item.id) ?? null
    // Помечаем рассылку прочитанной на бэке (Message с этим BroadcastId для текущего юзера → IsRead=true)
    if (item.id) api.put(`/message/broadcast/${item.id}/read`).catch(() => null)
  }
}

async function scrollToBottom() {
  await nextTick()
  if (messagesEl.value) messagesEl.value.scrollTop = messagesEl.value.scrollHeight
}

async function loadAll() {
  loading.value = true
  try {
    const [dialogsRes, broadcastsRes] = await Promise.allSettled([
      api.get('/message/dialogs'),
      api.get('/message/broadcast'),
    ])
    const dialogs: any[] = (dialogsRes as any).value?.data?.data ?? []
    dialogUsers.value = dialogs.map((d: any) => ({
      id: d.userId,
      login: d.userName,
      role: d.userRole,
      lastMessageAt: d.lastMessageAt,
      lastMessage: d.lastMessage,
    }))
    dialogUsers.value.forEach(u => { unread.value[u.id] = 0 })
    broadcasts.value = (broadcastsRes as any).value?.data?.data ?? []
  } finally { loading.value = false }
}

onMounted(async () => {
  await loadAll()
  await start()

  on<MessageResponse>('ReceiveMessage', (msg) => {
    const item = selectedItem.value
    if (item?.type === 'chat' && item.id === msg.senderId) {
      messages.value.push(msg)
      scrollToBottom()
      api.put(`/message/${msg.id}/read`).catch(() => null)
    } else {
      unread.value[msg.senderId] = (unread.value[msg.senderId] ?? 0) + 1
      if (!dialogUsers.value.find((u: any) => u.id === msg.senderId)) {
        dialogUsers.value.unshift({ id: msg.senderId, login: `ID ${msg.senderId}`, role: 'personal', lastMessageAt: new Date().toISOString() })
      }
    }
  })

  on<{ broadcastId: number; title: string; text: string }>('ReceiveBroadcast', (payload) => {
    if (!broadcasts.value.find(b => b.id === payload.broadcastId)) {
      broadcasts.value.unshift({
        id: payload.broadcastId,
        title: payload.title,
        text: payload.text,
        createdAt: new Date().toISOString(),
        createdById: 0,
        createdByRole: 'Admin',
        targetType: 'All',
        targetId: null,
        expireAt: null,
        recipientsCount: 0,
      })
    }
  })
})

onUnmounted(() => { off('ReceiveMessage'); off('ReceiveBroadcast'); stop() })
</script>
