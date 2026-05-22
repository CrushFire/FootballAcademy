import type { AgeGroup } from './model'

export interface TeamCreateRequest {
  trainerId: number
  name: string
  ageGroup: AgeGroup
}

export interface TeamUpdateRequest {
  name?: string
  ageGroup?: AgeGroup
}

export interface TeamResponse {
  id: number
  trainerId: number
  name: string
  ageGroup: AgeGroup
  createdAt: string
  images: string[] | null
}

export interface TeamStatsResponse {
  id: number
  name: string
  ageGroup: AgeGroup
  wins: number
  draws: number
  losses: number
  totalMatches: number
  winRate: number
}
