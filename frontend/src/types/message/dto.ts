import type { Message } from './model'

export interface MessageSendRequest {
  receiverId: number
  text: string
}

export interface MessageResponse extends Message {}

export interface DialogResponse {
  withUserId: number
  messages: MessageResponse[]
}
