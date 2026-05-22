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

  const unreadCount = computed(() => notifications.value.filter(n => !n.isRead).length)

  async function init() {
    if (started.value) return
    started.value = true

    // Загружаем непрочитанные рассылки
    try {
      const res = await api.get('/message/broadcast').catch(() => null)
      const broadcasts: BroadcastResponse[] = res?.data?.data ?? []
      for (const b of broadcasts) {
        if (!notifications.value.find(n => n.broadcastId === b.id)) {
          notifications.value.push({
            id: `broadcast-${b.id}`,
            type: 'broadcast',
            title: b.title,
            preview: b.text.slice(0, 80),
            createdAt: b.createdAt,
            isRead: false,
            broadcastId: b.id,
          })
        }
      }
    } catch { /* ignore */ }

    // Загружаем непрочитанные личные сообщения из активных диалогов
    try {
      const res = await api.get('/message/dialogs').catch(() => null)
      const dialogs: any[] = res?.data?.data ?? []
      for (const d of dialogs) {
        if (d.hasUnread && !notifications.value.find(n => n.type === 'message' && n.senderId === d.userId)) {
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

  function markAllRead() {
    notifications.value.forEach(n => { n.isRead = true })
  }

  // Вызывается когда пользователь открыл диалог с senderId
  function markMessageReadBySender(senderId: number) {
    notifications.value
      .filter(n => n.type === 'message' && n.senderId === senderId)
      .forEach(n => { n.isRead = true })
  }

  return { notifications, unreadCount, init, markRead, markAllRead, markMessageReadBySender }
}
