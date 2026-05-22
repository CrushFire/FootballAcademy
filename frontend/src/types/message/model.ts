export type SenderRole = 'Admin' | 'Trainer' | 'Medical'

export interface Message {
  id: number
  senderId: number
  senderRole: SenderRole
  receiverId: number
  text: string
  isRead: boolean
  broadcastId: number | null
  createdAt: string
}
