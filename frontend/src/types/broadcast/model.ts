import type { SenderRole } from '../message/model'

export type BroadcastTargetType = 'All' | 'Personal' | 'Team' | 'Group' | 'Individual'

export interface Broadcast {
  id: number
  title: string
  text: string
  createdById: number
  createdByRole: SenderRole
  targetType: BroadcastTargetType
  targetId: number | null
  expireAt: string | null
  createdAt: string
  recipientsCount: number
}
