<template>
  <div class="p-3 h-full">
  <TrainerPageCard color="violet" class="h-full flex flex-col p-0 overflow-hidden">
  <div class="flex h-full overflow-hidden bg-neutral-50 rounded-2xl">

    <!-- Левая панель -->
    <aside class="w-72 bg-white border-r border-neutral-200 flex flex-col shrink-0">

      <!-- Шапка -->
      <div class="px-4 py-3 border-b border-neutral-200 flex items-center gap-2">
        <div class="w-8 h-8 rounded-xl bg-violet-100 flex items-center justify-center shrink-0">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-violet-600">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"/>
          </svg>
        </div>
        <div class="flex-1 min-w-0">
          <div class="text-sm font-semibold text-neutral-800">AI-ассистент</div>
        </div>
        <button
          @click="createChat"
          :disabled="creating"
          class="w-8 h-8 rounded-xl bg-violet-500 flex items-center justify-center hover:bg-violet-600 transition-colors shrink-0 disabled:opacity-40"
          title="Новый чат"
        >
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="w-4 h-4 text-white">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 4v16m8-8H4"/>
          </svg>
        </button>
      </div>

      <!-- Список чатов -->
      <div class="flex-1 overflow-y-auto">
        <div v-if="chatsLoading" class="p-6 text-center text-xs text-neutral-400">Загрузка...</div>
        <div v-else-if="chats.length === 0" class="p-6 text-center text-xs text-neutral-400">
          Нет чатов.<br>Нажмите + чтобы начать
        </div>
        <div
          v-for="chat in chats" :key="chat.id"
          @click="selectChat(chat)"
          class="group flex items-start gap-3 px-4 py-3 cursor-pointer hover:bg-neutral-50 transition-colors border-b border-neutral-50"
          :class="selectedChat?.id === chat.id ? 'bg-violet-50 border-r-2 border-r-violet-500' : ''"
        >
          <div class="w-8 h-8 rounded-xl flex items-center justify-center shrink-0 mt-0.5"
            :class="selectedChat?.id === chat.id ? 'bg-violet-100' : 'bg-neutral-100'">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4"
              :class="selectedChat?.id === chat.id ? 'text-violet-600' : 'text-neutral-400'">
              <path stroke-linecap="round" stroke-linejoin="round" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-4 4-1-4z"/>
            </svg>
          </div>
          <div class="flex-1 min-w-0">
            <div class="text-sm font-semibold text-neutral-800 truncate">{{ chat.title }}</div>
            <div class="text-[10px] text-neutral-400 mt-0.5">{{ formatRelative(chat.updatedAt) }}</div>
          </div>
          <button
            @click.stop="deleteChat(chat)"
            class="opacity-0 group-hover:opacity-100 w-6 h-6 rounded-lg flex items-center justify-center text-neutral-300 hover:text-red-500 hover:bg-red-50 transition-all shrink-0 mt-0.5"
          >
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-3.5 h-3.5">
              <path stroke-linecap="round" stroke-linejoin="round" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16"/>
            </svg>
          </button>
        </div>
      </div>
    </aside>

    <!-- Основная область -->
    <div class="flex-1 flex flex-col overflow-hidden">

      <!-- Чат открыт -->
      <template v-if="selectedChat">

        <!-- Шапка чата -->
        <div class="px-5 py-3 bg-white border-b border-neutral-200 flex items-center gap-3 shrink-0">
          <div class="w-9 h-9 rounded-xl bg-violet-100 flex items-center justify-center shrink-0">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5 text-violet-600">
              <path stroke-linecap="round" stroke-linejoin="round" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"/>
            </svg>
          </div>
          <div>
            <div class="font-semibold text-neutral-900 text-sm">{{ selectedChat.title }}</div>
            <div class="text-xs text-neutral-400">AI-ассистент · спортивные данные академии</div>
          </div>
        </div>

        <!-- Сообщения -->
        <div ref="messagesEl" class="flex-1 overflow-y-auto p-4 space-y-3">
          <div v-if="messagesLoading" class="text-center text-xs text-neutral-400 py-8">Загрузка...</div>
          <template v-else>

            <!-- Приветствие если нет сообщений -->
            <div v-if="currentMessages.length === 0 && !aiTyping" class="flex flex-col items-center justify-center py-16 gap-4 text-neutral-400">
              <div class="w-16 h-16 rounded-2xl bg-violet-50 flex items-center justify-center">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5" class="w-9 h-9 text-violet-400">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"/>
                </svg>
              </div>
              <div class="text-center">
                <div class="text-sm font-semibold text-neutral-600 mb-1">Спросите AI-ассистента</div>
                <div class="text-xs text-neutral-400 max-w-xs leading-relaxed">Анализ GPS-метрик, подбор позиций, оценка готовности к матчу, сравнение спортсменов</div>
              </div>
            </div>

            <div
              v-for="msg in currentMessages" :key="msg.id"
              class="flex"
              :class="msg.role === 'user' ? 'justify-end' : 'justify-start'"
            >
              <!-- Аватар AI -->
              <div v-if="msg.role === 'assistant'" class="w-7 h-7 rounded-xl bg-violet-100 flex items-center justify-center shrink-0 mr-2 mt-0.5">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-violet-600">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"/>
                </svg>
              </div>

              <div
                class="max-w-xs lg:max-w-2xl px-4 py-2.5 rounded-2xl text-sm shadow-sm"
                :class="msg.role === 'user'
                  ? 'bg-violet-500 text-white rounded-br-sm'
                  : 'bg-white text-neutral-900 rounded-bl-sm border border-neutral-100'"
              >
                <p class="leading-relaxed whitespace-pre-wrap" v-html="formatMessage(msg.id === streamingMessageId ? streamingText : msg.content)"></p>
                <div class="mt-1" :class="msg.role === 'user' ? 'text-right' : 'text-left'">
                  <span class="text-[10px] opacity-50">{{ formatTime(msg.createdAt) }}</span>
                </div>
              </div>
            </div>

          </template>

          <!-- Индикатор печатания AI — вне template чтобы всегда рендерился -->
          <div v-if="aiTyping" class="flex justify-start">
            <div class="w-7 h-7 rounded-xl bg-violet-100 flex items-center justify-center shrink-0 mr-2 mt-0.5">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-violet-600">
                <path stroke-linecap="round" stroke-linejoin="round" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"/>
              </svg>
            </div>
            <div class="bg-white border border-neutral-100 rounded-2xl rounded-bl-sm px-4 py-3 shadow-sm">
              <div class="flex gap-1 items-center h-4">
                <span class="w-1.5 h-1.5 rounded-full bg-violet-400 animate-bounce" style="animation-delay: 0ms"></span>
                <span class="w-1.5 h-1.5 rounded-full bg-violet-400 animate-bounce" style="animation-delay: 150ms"></span>
                <span class="w-1.5 h-1.5 rounded-full bg-violet-400 animate-bounce" style="animation-delay: 300ms"></span>
              </div>
            </div>
          </div>
        </div>

        <!-- Ввод сообщения -->
        <div class="px-4 py-3 bg-white border-t border-neutral-200 shrink-0">
          <div class="flex gap-2 items-end">
            <textarea
              v-model="newText"
              @keydown.enter.exact.prevent="sendMessage"
              @input="autoResize"
              ref="textareaEl"
              rows="1"
              placeholder="Спросить AI-ассистента..."
              class="flex-1 px-4 py-2.5 text-sm border border-neutral-200 rounded-2xl focus:outline-none focus:border-violet-400 resize-none overflow-hidden leading-relaxed"
              style="max-height: 120px"
            />
            <button
              @click="sendMessage"
              :disabled="!newText.trim() || sending"
              class="w-10 h-10 bg-violet-500 text-white rounded-2xl flex items-center justify-center hover:bg-violet-600 disabled:opacity-40 transition-colors shrink-0"
            >
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5">
                <path stroke-linecap="round" stroke-linejoin="round" d="M5 12h14M13 6l6 6-6 6"/>
              </svg>
            </button>
          </div>
          <div class="mt-1.5 text-[10px] text-neutral-400 text-center">ИИ-ассистент может допускать ошибки. Проверяйте важную информацию.</div>
        </div>
      </template>

      <!-- Пусто — нет выбранного чата -->
      <div v-else class="flex-1 flex flex-col items-center justify-center gap-4 text-neutral-300">
        <div class="w-20 h-20 rounded-2xl bg-violet-50 flex items-center justify-center">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5" class="w-11 h-11 text-violet-300">
            <path stroke-linecap="round" stroke-linejoin="round" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 11-4 0v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z"/>
          </svg>
        </div>
        <div class="text-center">
          <div class="text-sm font-semibold text-neutral-500 mb-1">Выберите чат или создайте новый</div>
          <div class="text-xs text-neutral-400">AI-ассистент отвечает на вопросы по данным спортсменов</div>
        </div>
        <button
          @click="createChat"
          :disabled="creating"
          class="px-5 py-2.5 bg-violet-500 text-white text-sm font-semibold rounded-2xl hover:bg-violet-600 disabled:opacity-40 transition-colors"
        >
          Новый чат
        </button>
      </div>
    </div>
  </div>
  </TrainerPageCard>
  </div>
</template>

<script lang="ts">
export default { name: 'TrainerAiEval' }
</script>

<script setup lang="ts">
import { ref, nextTick, onMounted } from 'vue'
import api from '@/services/api'
import TrainerPageCard from '@/components/trainer/TrainerPageCard.vue'

interface AiChat {
  id: number
  title: string
  createdAt: string
  updatedAt: string
}

interface AiMessage {
  id: number
  role: 'user' | 'assistant'
  content: string
  createdAt: string
}

const chats = ref<AiChat[]>([])
const chatsLoading = ref(false)
const creating = ref(false)

const selectedChat = ref<AiChat | null>(null)
const currentMessages = ref<AiMessage[]>([])
const messagesLoading = ref(false)

const newText = ref('')
const sending = ref(false)
const aiTyping = ref(false)
const streamingMessageId = ref<number | null>(null)
const streamingText = ref('')

const messagesEl = ref<HTMLElement | null>(null)
const textareaEl = ref<HTMLTextAreaElement | null>(null)

function toLocal(iso: string): Date {
  const d = new Date(iso)
  d.setHours(d.getHours() + 3)
  return d
}

function formatTime(iso: string): string {
  return toLocal(iso).toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })
}

function formatMessage(text: string): string {
  return text
    .split('\n')
    .map(line => {
      // Сначала экранируем HTML в сегментах между **
      const parts = line.split('**')
      return parts
        .map((part, i) => i % 2 === 1
          ? `<strong style="font-weight:700">${escapeHtml(part)}</strong>`
          : escapeHtml(part))
        .join('')
    })
    .join('\n')
}

function escapeHtml(s: string): string {
  return s.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
}

function formatRelative(iso: string): string {
  if (!iso) return ''
  const d = toLocal(iso)
  const nowLocal = toLocal(new Date().toISOString())
  const today = new Date(nowLocal.getFullYear(), nowLocal.getMonth(), nowLocal.getDate())
  const yesterday = new Date(today.getTime() - 86400000)
  const msgDay = new Date(d.getFullYear(), d.getMonth(), d.getDate())
  if (msgDay.getTime() === today.getTime()) return d.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' })
  if (msgDay.getTime() === yesterday.getTime()) return 'Вчера'
  return d.toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' })
}

async function loadChats() {
  chatsLoading.value = true
  try {
    const res = await api.get('/rag/chats').catch(() => null)
    chats.value = res?.data?.data ?? []
  } finally {
    chatsLoading.value = false
  }
}

async function createChat() {
  if (creating.value) return
  creating.value = true
  try {
    const res = await api.post('/rag/chats')
    const chat: AiChat = res.data.data
    chats.value = [chat, ...chats.value]
    await selectChat(chat)
  } finally {
    creating.value = false
  }
}

async function selectChat(chat: AiChat) {
  selectedChat.value = chat
  currentMessages.value = []
  messagesLoading.value = true
  try {
    const res = await api.get(`/rag/chats/${chat.id}`).catch(() => null)
    currentMessages.value = res?.data?.data?.messages ?? []
    await scrollToBottom()
  } finally {
    messagesLoading.value = false
  }
}

async function deleteChat(chat: AiChat) {
  await api.delete(`/rag/chats/${chat.id}`).catch(() => null)
  chats.value = chats.value.filter(c => c.id !== chat.id)
  if (selectedChat.value?.id === chat.id) {
    selectedChat.value = null
    currentMessages.value = []
  }
}

async function sendMessage() {
  const text = newText.value.trim()
  if (!text || !selectedChat.value || sending.value) return

  sending.value = true
  newText.value = ''
  resetTextarea()

  const tempUserMsg: AiMessage = {
    id: Date.now(),
    role: 'user',
    content: text,
    createdAt: new Date().toISOString(),
  }
  currentMessages.value.push(tempUserMsg)
  await scrollToBottom()

  aiTyping.value = true
  await scrollToBottom()

  try {
    await api.post(`/rag/chats/${selectedChat.value.id}/messages`, JSON.stringify(text), {
      headers: { 'Content-Type': 'application/json' },
    })
    const detail = await api.get(`/rag/chats/${selectedChat.value.id}`).catch(() => null)
    if (detail) {
      const messages: AiMessage[] = detail.data.data.messages ?? []
      const updatedTitle: string = detail.data.data.title

      // Показываем все сообщения кроме последнего ответа AI
      const lastAi = [...messages].reverse().find(m => m.role === 'assistant')
      if (lastAi) {
        currentMessages.value = messages.filter(m => m.id !== lastAi.id)
        aiTyping.value = false
        await scrollToBottom()
        await animateMessage(lastAi)
        currentMessages.value = messages
      } else {
        currentMessages.value = messages
      }

      const idx = chats.value.findIndex(c => c.id === selectedChat.value!.id)
      if (idx !== -1) {
        const updated = { ...chats.value[idx], title: updatedTitle, updatedAt: detail.data.data.updatedAt ?? chats.value[idx].updatedAt }
        chats.value.splice(idx, 1)
        chats.value.unshift(updated)
        selectedChat.value = updated
      }
    }
  } catch {
    currentMessages.value = currentMessages.value.filter(m => m !== tempUserMsg)
  } finally {
    sending.value = false
    aiTyping.value = false
    streamingMessageId.value = null
    streamingText.value = ''
    await scrollToBottom()
  }
}

async function animateMessage(msg: AiMessage) {
  const full = msg.content
  streamingMessageId.value = msg.id
  streamingText.value = ''
  // Добавляем сообщение с пустым контентом — будем анимировать через streamingText
  currentMessages.value.push({ ...msg, content: '' })

  let i = 0
  const chunkSize = 3 // символов за тик
  const delay = 12    // мс между тиками

  await new Promise<void>(resolve => {
    const tick = () => {
      if (i >= full.length) {
        streamingText.value = full
        resolve()
        return
      }
      i = Math.min(i + chunkSize, full.length)
      streamingText.value = full.slice(0, i)
      scrollToBottom()
      setTimeout(tick, delay)
    }
    tick()
  })
}

async function scrollToBottom() {
  await nextTick()
  if (messagesEl.value) messagesEl.value.scrollTop = messagesEl.value.scrollHeight
}

function autoResize() {
  const el = textareaEl.value
  if (!el) return
  el.style.height = 'auto'
  el.style.height = Math.min(el.scrollHeight, 120) + 'px'
}

function resetTextarea() {
  const el = textareaEl.value
  if (el) el.style.height = 'auto'
}

onMounted(loadChats)
</script>
