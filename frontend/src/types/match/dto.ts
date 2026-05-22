import type { GameType, MatchResult, MatchEventType, MatchStats, MatchEvent, MatchStatus } from './model'

export interface MatchCreateRequest {
  homeTeamId: number
  opponentTeamId?: number | null
  opponentTeamName?: string | null
  // Опционально: бэк создаст Training (Type="Матч") для каждой указанной группы и привяжет к матчу
  homeGroupId?: number | null
  opponentGroupId?: number | null
  type: GameType
  date: string
}

export interface MatchFinishRequest {
  result: MatchResult
  trainerComment?: string | null
}

export interface MatchEventCreateRequest {
  type: MatchEventType
  isHomeTeam: boolean
  minute: number
  comment?: string | null
}

export interface MatchEventResponse extends MatchEvent {}

export interface MatchResponse {
  id: number
  homeTeamId: number
  homeTeamName: string
  opponentTeamId: number | null
  opponentTeamName: string | null
  trainingIds: number[]
  type: GameType
  status: MatchStatus
  result: MatchResult | null
  trainerComment: string | null
  date: string
  createdAt: string
  homeStats: MatchStats
  awayStats: MatchStats
  events: MatchEventResponse[]
}
