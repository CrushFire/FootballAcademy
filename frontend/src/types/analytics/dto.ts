import type { PentagonScores, MedicalCheckResult, MedicalMetrics, CommonMetrics, PlayerProfile, GraphPoint } from './model'

export interface PentagonResponse {
  sportsmanId: number
  sportsmanName: string
  pentagon: PentagonScores
}

export interface MedicalCheckResponse {
  sportsmanId: number
  sportsmanName: string
  checkResult: MedicalCheckResult
}

export interface MedicalMetricsResponse {
  sportsmanId: number
  sportsmanName: string
  metrics: MedicalMetrics
}

export interface CalculatedMetricsResponse {
  sportsmanId: number
  sportsmanName: string
  metrics: CommonMetrics
}

export interface ProfileResponse {
  sportsmanId: number
  sportsmanName: string
  profiles: PlayerProfile[] | null
}

export interface GraphResponse {
  points: GraphPoint[]
}
