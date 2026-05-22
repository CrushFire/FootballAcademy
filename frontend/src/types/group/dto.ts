export interface GroupCreateRequest {
  trainerId: number
  name: string
  description?: string
}

export interface GroupUpdateRequest {
  name?: string
  description?: string
}

export interface GroupResponse {
  id: number
  trainerId: number
  name: string
  description: string | null
  createdAt: string
}
