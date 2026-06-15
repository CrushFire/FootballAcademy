<template>
  <header class="flex items-center justify-between px-4 md:px-6 shrink-0 bg-primary-700 dark:bg-blue-900 shadow-lg" style="height:60px">
    <div class="flex items-center gap-2 md:gap-3">
      <!-- Мобила: гамбургер с эмблемой на фоне (объединяем две кнопки в одну). -->
      <button
        type="button"
        @click="openMobileMenu"
        class="md:hidden relative w-11 h-11 rounded-xl overflow-hidden flex items-center justify-center transition-transform active:scale-95"
        aria-label="Меню"
      >
        <img :src="logoUrl" alt="" class="absolute inset-0 w-full h-full object-cover opacity-90" />
        <span class="absolute inset-0 bg-black/35" />
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2.5" class="relative w-6 h-6 text-neutral-200 drop-shadow">
          <path stroke-linecap="round" stroke-linejoin="round" d="M4 6h16M4 12h16M4 18h16"/>
        </svg>
      </button>
      <!-- Десктоп: обычная эмблема -->
      <img :src="logoUrl" alt="Академия футбола" class="hidden md:block h-10 w-10 object-cover rounded-xl" />
      <span class="hidden sm:inline text-lg font-bold text-white tracking-tight">FootballAcademy</span>
    </div>

    <div class="flex items-center gap-2">
      <!-- Колокольчик -->
      <div class="relative">
        <button
          @click="toggleDrawer"
          class="relative w-10 h-10 rounded-full flex items-center justify-center bg-sky-500/30 hover:bg-sky-400/40 ring-1 ring-sky-300/40 transition-colors"
        >
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-5 h-5 text-white">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/>
          </svg>
          <span
            v-if="unreadCount > 0"
            class="absolute -top-0.5 -right-0.5 min-w-[18px] h-[18px] bg-red-500 text-white text-[10px] font-bold rounded-full flex items-center justify-center px-1 leading-none"
          >{{ unreadCount > 99 ? '99+' : unreadCount }}</span>
        </button>

        <!-- Dropdown панель -->
        <Transition name="drawer">
          <div
            v-if="drawerOpen"
            class="fixed md:absolute right-2 md:right-0 left-2 md:left-auto top-14 md:top-12 md:w-80 max-w-[calc(100vw-1rem)] bg-white rounded-2xl shadow-2xl border border-neutral-100 z-50 overflow-hidden flex flex-col"
            style="max-height: 320px"
          >
            <!-- Хедер панели -->
            <div class="flex items-center justify-between px-4 py-3 border-b border-neutral-100 bg-neutral-50">
              <span class="text-sm font-bold text-neutral-800">Уведомления</span>
              <button
                v-if="unreadCount > 0"
                @click="markAllRead"
                class="text-xs text-blue-600 hover:text-blue-800 font-medium transition-colors"
              >Прочитать все</button>
            </div>

            <!-- Список -->
            <div class="overflow-y-auto flex-1">
              <div v-if="notifications.length === 0" class="flex flex-col items-center justify-center py-12 gap-2">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5" class="w-10 h-10 text-neutral-300">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"/>
                </svg>
                <span class="text-xs text-neutral-400">Нет уведомлений</span>
              </div>

              <div
                v-for="n in sortedNotifications"
                :key="n.id"
                @click="openNotification(n)"
                class="flex items-start gap-3 px-4 py-3 cursor-pointer hover:bg-neutral-50 transition-colors border-b border-neutral-50 last:border-0"
                :class="!n.isRead ? 'bg-blue-50/60' : ''"
              >
                <!-- Иконка -->
                <div class="w-9 h-9 rounded-xl flex items-center justify-center shrink-0 mt-0.5"
                  :class="n.type === 'broadcast' ? 'bg-green-100' : 'bg-blue-100'">
                  <svg v-if="n.type === 'broadcast'" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-green-600">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M11 5.882V19.24a1.76 1.76 0 01-3.417.592l-2.147-6.15M18 13a3 3 0 100-6M5.436 13.683A4.001 4.001 0 017 6h1.832c4.1 0 7.625-1.234 9.168-3v14c-1.543-1.766-5.067-3-9.168-3H7a3.988 3.988 0 01-1.564-.317z"/>
                  </svg>
                  <svg v-else xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" class="w-4 h-4 text-blue-600">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M8 10h.01M12 10h.01M16 10h.01M9 16H5a2 2 0 01-2-2V6a2 2 0 012-2h14a2 2 0 012 2v8a2 2 0 01-2 2h-5l-4 4-1-4z"/>
                  </svg>
                </div>

                <!-- Контент -->
                <div class="flex-1 min-w-0">
                  <div class="flex items-center gap-1.5">
                    <span class="text-xs font-semibold text-neutral-800 truncate">{{ n.title }}</span>
                    <span v-if="!n.isRead" class="w-1.5 h-1.5 rounded-full bg-blue-500 shrink-0"></span>
                  </div>
                  <div class="text-xs text-neutral-500 mt-0.5 line-clamp-2 leading-relaxed">{{ n.preview }}</div>
                  <div class="text-[10px] text-neutral-400 mt-1">{{ formatTime(n.createdAt) }}</div>
                </div>
              </div>
            </div>

            <!-- Ссылка на все сообщения -->
            <div class="border-t border-neutral-100 px-4 py-3">
              <RouterLink
                :to="messagesRoute"
                @click="drawerOpen = false"
                class="block text-center text-xs font-semibold text-blue-600 hover:text-blue-800 transition-colors"
              >Все сообщения →</RouterLink>
            </div>
          </div>
        </Transition>
      </div>

      <!-- Переключатель темы -->
      <button
        @click="toggleTheme"
        class="w-10 h-10 rounded-full flex items-center justify-center bg-blue-400/30 hover:bg-blue-300/40 ring-1 ring-blue-300/40 transition-colors"
        :title="theme === 'dark' ? 'Светлая тема' : 'Тёмная тема'"
      >
        <!-- Светлая тема активна → сова С ЗАКРЫТЫМИ глазами (вариант 12, наоборот) -->
        <svg v-if="theme !== 'dark'" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" class="w-6 h-6 text-white">
          <!-- голова с треугольными ушками сверху -->
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 4l-3 1.5C7 6.5 5.5 8.5 5.5 11v5c0 2.5 2.5 4.5 6.5 4.5s6.5-2 6.5-4.5v-5c0-2.5-1.5-4.5-3.5-5.5L12 4z"/>
          <!-- ушки -->
          <path stroke-linecap="round" stroke-linejoin="round" d="M9 5.5L8 3M15 5.5L16 3"/>
          <!-- закрытые дуги -->
          <path stroke-linecap="round" stroke-linejoin="round" d="M7.5 11c.7-.8 3.3-.8 4 0M12.5 11c.7-.8 3.3-.8 4 0"/>
          <!-- клюв -->
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 13.5l-.7 1.5h1.4z"/>
          <!-- крылышки -->
          <path stroke-linecap="round" stroke-linejoin="round" d="M5.5 14c0 2 .5 3.5 2 4M18.5 14c0 2-.5 3.5-2 4"/>
        </svg>
        <!-- Тёмная тема активна → сова С ОТКРЫТЫМИ глазами (вариант 12, наоборот) -->
        <svg v-else xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" class="w-6 h-6 text-white">
          <!-- голова -->
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 4l-3 1.5C7 6.5 5.5 8.5 5.5 11v5c0 2.5 2.5 4.5 6.5 4.5s6.5-2 6.5-4.5v-5c0-2.5-1.5-4.5-3.5-5.5L12 4z"/>
          <!-- ушки -->
          <path stroke-linecap="round" stroke-linejoin="round" d="M9 5.5L8 3M15 5.5L16 3"/>
          <!-- большие круглые глаза заполненные -->
          <circle cx="9.5" cy="11" r="1.7" fill="currentColor"/>
          <circle cx="14.5" cy="11" r="1.7" fill="currentColor"/>
          <!-- блики -->
          <circle cx="9" cy="10.5" r="0.4" fill="#1D4ED8"/>
          <circle cx="14" cy="10.5" r="0.4" fill="#1D4ED8"/>
          <!-- клюв -->
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 13.5l-.7 1.5h1.4z"/>
          <!-- крылышки -->
          <path stroke-linecap="round" stroke-linejoin="round" d="M5.5 14c0 2 .5 3.5 2 4M18.5 14c0 2-.5 3.5-2 4"/>
        </svg>
      </button>

      <!-- Профиль -->
      <RouterLink :to="profileRoute" class="flex items-center gap-2 pr-2 md:pr-3 rounded-full no-underline transition-colors bg-white/10 hover:bg-white/20 min-w-0" :class="avatarUrl ? 'pl-0' : 'pl-1 py-1'">
        <img v-if="avatarUrl" :src="avatarUrl" class="w-10 h-10 md:w-11 md:h-11 rounded-full object-cover object-top border border-white/30 shrink-0" />
        <div v-else class="w-8 h-8 rounded-full flex items-center justify-center bg-white/20 text-white shrink-0" v-html="ICON_USER" />
        <span class="hidden sm:inline text-sm font-semibold text-white truncate max-w-[140px]">{{ userName }}</span>
      </RouterLink>
    </div>
  </header>

  <!-- Клик вне закрывает drawer -->
  <div v-if="drawerOpen" class="fixed inset-0 z-40" @click="drawerOpen = false" />
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/store/auth'
import { useNotifications } from '@/composables/useNotifications'
import { useTheme } from '@/composables/useTheme'
import { useMobileMenu } from '@/composables/useMobileMenu'
import type { NotificationItem } from '@/composables/useNotifications'
import logoUrl from '@/assets/images/Академия_футбола.png'
import api from '@/services/api'
import { imageUrl } from '@/utils/imageUrl'

const auth = useAuthStore()
const router = useRouter()
const { notifications, unreadCount, init, markRead, markAllRead } = useNotifications()
const { theme, toggle: toggleTheme } = useTheme()
const { open: openMobileMenu } = useMobileMenu()

const drawerOpen = ref(false)
const avatarUrl = ref<string | null>(null)

const userName = computed(() => {
  const fio = auth.userLogin ?? `ID ${auth.userId}`
  return fio.split(' ').map(w => w.charAt(0).toUpperCase() + w.slice(1).toLowerCase()).join(' ')
})

const profileRoute = computed(() => {
  const role = auth.userRole
  const pType = auth.personalType?.toLowerCase()
  if (role === 'admin') return '/admin/profile'
  if (role === 'personal' && pType === 'trainer') return '/trainer/profile'
  if (role === 'personal' && pType === 'medical') return '/medical/profile'
  return `/sportsmen/${auth.userId}`
})

const messagesRoute = computed(() => {
  const role = auth.userRole
  const pType = auth.personalType?.toLowerCase()
  if (role === 'admin') return '/admin/messages'
  if (role === 'personal' && pType === 'trainer') return '/trainer/messages'
  if (role === 'personal' && pType === 'medical') return '/medical/messages'
  return '/messages'
})

// В колокольчике: все непрочитанные (даже старые) + прочитанные за последние 24 часа.
const sortedNotifications = computed(() => {
  const dayAgoMs = Date.now() - 24 * 60 * 60 * 1000
  const toUtc = (iso: string) => /[Zz]|[+-]\d{2}:?\d{2}$/.test(iso) ? iso : `${iso}Z`
  return [...notifications.value]
    .filter(n => !n.isRead || new Date(toUtc(n.createdAt)).getTime() >= dayAgoMs)
    .sort((a, b) => new Date(toUtc(b.createdAt)).getTime() - new Date(toUtc(a.createdAt)).getTime())
})

function toggleDrawer() {
  drawerOpen.value = !drawerOpen.value
  // При открытии — помечаем все непрочитанные как прочитанные (локально + на бэке)
  if (drawerOpen.value && unreadCount.value > 0) {
    markAllRead()
  }
}

function formatTime(iso: string): string {
  // Бэк отдаёт UTC, но без явного 'Z' — добавляем чтобы парсить как UTC, иначе браузер считает локальным.
  const utcIso = /[Zz]|[+-]\d{2}:?\d{2}$/.test(iso) ? iso : `${iso}Z`
  const d = new Date(utcIso)
  const now = new Date()
  const diffMs = now.getTime() - d.getTime()
  if (diffMs < 0) return 'только что'
  const diffMin = Math.floor(diffMs / 60000)
  if (diffMin < 1) return 'только что'
  if (diffMin < 60) return `${diffMin} мин. назад`
  const diffH = Math.floor(diffMin / 60)
  if (diffH < 24) return `${diffH} ч. назад`
  return d.toLocaleDateString('ru-RU', { day: 'numeric', month: 'short' })
}

function openNotification(n: NotificationItem) {
  markRead(n.id)
  drawerOpen.value = false
  if (n.type === 'message' && n.senderId) {
    router.push(messagesRoute.value)
  } else if (n.type === 'broadcast') {
    // Помечаем рассылку прочитанной на бэке (Message с этим BroadcastId для текущего юзера → IsRead=true).
    // Это нужно чтобы автор рассылки видел статус "Прочитано" в деталях.
    if (n.broadcastId) api.put(`/message/broadcast/${n.broadcastId}/read`).catch(() => null)
    router.push(messagesRoute.value)
  }
}

async function loadAvatar() {
  const role = auth.userRole
  if (role === 'admin') return
  const endpoint = role === 'personal' ? '/personal/me' : '/sportsman/me'
  const res = await api.get(endpoint).catch(() => null)
  avatarUrl.value = imageUrl(res?.data?.data?.images)
}

onMounted(() => {
  init()
  loadAvatar()
})

const ICON_USER = `<svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2" style="width:18px;height:18px"><path stroke-linecap="round" stroke-linejoin="round" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"/></svg>`
</script>

<style scoped>
.drawer-enter-active, .drawer-leave-active {
  transition: opacity 0.15s ease, transform 0.15s ease;
}
.drawer-enter-from, .drawer-leave-to {
  opacity: 0;
  transform: translateY(-8px) scale(0.97);
}
</style>
