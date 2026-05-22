import type { Position, Specialization } from './model'

export interface SportsmanCreateRequest {
  userId: number
  fio: string
  birthDate: string
  height: number
  weight: number
  position?: Position | null
  groupId?: number | null
  gender: string
  specialization: Specialization
}

export interface SportsmanUpdateRequest {
  fio?: string
  birthDate?: string
  height?: number
  weight?: number
  position?: Position | null
  gender?: string
  specialization?: Specialization
}

export interface SportsmanResponse {
  id: number
  userId: number
  fio: string
  birthDate: string
  age: number
  height: number
  weight: number
  position: Position | null
  teamId: number | null
  gender: string
  specialization: Specialization
  createdAt: string
  images: string[] | null
}

export interface PersonalWorkoutResponse {
  id: number
  sportsmanId: number
  date: string
  description: string
  createdAt: string
}
