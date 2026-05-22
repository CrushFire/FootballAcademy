import type { BroadcastTargetType, Broadcast } from './model'

export interface BroadcastCreateRequest {
  title: string
  text: string
  targetType: BroadcastTargetType
  targetId?: number | null
  expireAt?: string | null
}

export interface BroadcastResponse extends Broadcast {}

export interface BroadcastRecipientDto {
  userId: number
  userName: string
  isRead: boolean
}

export interface BroadcastDetailsResponse {
  id: number
  title: string
  text: string
  createdAt: string
  expireAt: string | null
  recipients: BroadcastRecipientDto[]
}
