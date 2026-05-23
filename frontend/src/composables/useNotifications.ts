import { ref, computed } from 'vue'
import { useSignalR } from './useSignalR'
import api from '@/services/api'
import type { MessageResponse } from '@/types/message/dto'
import type { BroadcastResponse } from '@/types/broadcast/dto'

export interface NotificationItem {
  id: string
  type: 'message' | 'broadcast'
  title: string
  preview: string
  createdAt: string
  isRead: boolean
  senderId?: number
  broadcastId?: number
}

const notifications = ref<NotificationItem[]>([])
const started = ref(false)

export function useNotifications() {
  const { start, on } = useSignalR()

  // Считаем только непрочитанные, которые ещё не «протухли» (старше 24ч).
  // Иначе старое непрочитанное из глобального state раздувает счётчик даже когда в выпадашке пусто.
  const unreadCount = computed(() => {
    const dayAgoMs = Date.now() - 24 * 60 * 60 * 1000
    const toUtc = (iso: string) => /[Zz]|[+-]\d{2}:?\d{2}$/.test(iso) ? iso : `${iso}Z`
    return notifications.value.filter(n =>
      !n.isRead && new Date(toUtc(n.createdAt)).getTime() >= dayAgoMs
    ).length
  })

  async function init() {
    if (started.value) return
    started.value = true

    // В колокольчик кладём: все непрочитанные рассылки (без ограничения по дате — важное не пропустить),
    // прочитанные за последние 24 часа отфильтруются в UI (см. sortedNotifications в AppHeader).
    // Старое прочитанное в БД не подгружаем вообще.
    const toUtc = (iso: string) => /[Zz]|[+-]\d{2}:?\d{2}$/.test(iso) ? iso : `${iso}Z`
    const dayAgoMs = Date.now() - 24 * 60 * 60 * 1000

    try {
      const res = await api.get('/message/broadcast', { params: { onlyForMe: true } }).catch(() => null)
      const broadcasts: BroadcastResponse[] = res?.data?.data ?? []
      for (const b of broadcasts) {
        if (notifications.value.find(n => n.broadcastId === b.id)) continue
        const isRead = !!b.isReadByMe
        const createdMs = new Date(toUtc(b.createdAt)).getTime()
        // Прочитанные старше 24ч не показываем; непрочитанные — всегда (даже старые).
        if (isRead && createdMs < dayAgoMs) continue
        notifications.value.push({
          id: `broadcast-${b.id}`,
          type: 'broadcast',
          title: b.title,
          preview: b.text.slice(0, 80),
          createdAt: b.createdAt,
          isRead,
          broadcastId: b.id,
        })
      }
    } catch { /* ignore */ }

    // Загружаем непрочитанные личные сообщения из активных диалогов
    try {
      const res = await api.get('/message/dialogs').catch(() => null)
      const dialogs: any[] = res?.data?.data ?? []
      for (const d of dialogs) {
        if (!d.hasUnread) continue
        if (notifications.value.find(n => n.type === 'message' && n.senderId === d.userId)) continue
        notifications.value.push({
          id: `msg-${d.userId}`,
          type: 'message',
          title: d.userName || `Пользователь ${d.userId}`,
          preview: d.lastMessage?.slice(0, 80) ?? '',
          createdAt: d.lastMessageAt,
          isRead: false,
          senderId: d.userId,
        })
      }
    } catch { /* ignore */ }

    await start()

    on<MessageResponse>('ReceiveMessage', (msg) => {
      const existing = notifications.value.find(n => n.type === 'message' && n.senderId === msg.senderId)
      if (existing) {
        existing.preview = msg.text.slice(0, 80)
        existing.createdAt = msg.createdAt
        existing.isRead = false
      } else {
        notifications.value.unshift({
          id: `msg-${msg.id}-${Date.now()}`,
          type: 'message',
          title: `Сообщение`,
          preview: msg.text.slice(0, 80),
          createdAt: msg.createdAt,
          isRead: false,
          senderId: msg.senderId,
        })
      }
    })

    on<{ broadcastId: number; title: string; text: string }>('ReceiveBroadcast', (payload) => {
      const existing = notifications.value.find(n => n.broadcastId === payload.broadcastId)
      if (!existing) {
        notifications.value.unshift({
          id: `broadcast-${payload.broadcastId}`,
          type: 'broadcast',
          title: payload.title,
          preview: payload.text.slice(0, 80),
          createdAt: new Date().toISOString(),
          isRead: false,
          broadcastId: payload.broadcastId,
        })
      }
    })
  }

  function markRead(id: string) {
    const n = notifications.value.find(n => n.id === id)
    if (n) n.isRead = true
  }

  async function markAllRead() {
    // Помечаем локально + дёргаем бэк.
    const unreadBroadcasts = notifications.value.filter(n => !n.isRead && n.type === 'broadcast' && n.broadcastId)
    const unreadMessageSenders = notifications.value.filter(n => !n.isRead && n.type === 'message' && n.senderId)
    notifications.value.forEach(n => { n.isRead = true })

    // Рассылки — PUT /message/broadcast/{id}/read
    for (const n of unreadBroadcasts) {
      api.put(`/message/broadcast/${n.broadcastId}/read`).catch(() => null)
    }
    // Личные — для каждого собеседника отмечаем все его непрочитанные сообщения как прочитанные.
    // Подгружаем dialog → берём msg.senderId === собеседник && !isRead → PUT по каждому.
    for (const n of unreadMessageSenders) {
      try {
        const res = await api.get(`/message/dialog/${n.senderId}`)
        const msgs: any[] = res?.data?.data?.messages ?? []
        for (const msg of msgs) {
          if (msg.senderId === n.senderId && !msg.isRead) {
            api.put(`/message/${msg.id}/read`).catch(() => null)
          }
        }
      } catch { /* ignore */ }
    }

    // Чистим протухшее (старше 24ч) — чтобы счётчик гарантированно обнулился
    const dayAgoMs = Date.now() - 24 * 60 * 60 * 1000
    const toUtc = (iso: string) => /[Zz]|[+-]\d{2}:?\d{2}$/.test(iso) ? iso : `${iso}Z`
    notifications.value = notifications.value.filter(n =>
      new Date(toUtc(n.createdAt)).getTime() >= dayAgoMs
    )
  }

  // Полный сброс состояния (вызывается при логине, чтобы новый юзер получил свои уведомления).
  async function reset() {
    notifications.value = []
    started.value = false
    await init()
  }

  // Вызывается когда пользователь открыл диалог с senderId
  function markMessageReadBySender(senderId: number) {
    notifications.value
      .filter(n => n.type === 'message' && n.senderId === senderId)
      .forEach(n => { n.isRead = true })
  }

  return { notifications, unreadCount, init, reset, markRead, markAllRead, markMessageReadBySender }
}
