export type GameType = 'Friendly' | 'League' | 'Cup' | 'Tournament'
export type MatchStatus = 'Scheduled' | 'InProgress' | 'Finished'
export type MatchResult = 'Win' | 'Draw' | 'Loss'
export type MatchEventType = 'Goal' | 'YellowCard' | 'RedCard' | 'Corner' | 'Foul' | 'Penalty'

export interface MatchStats {
  goals: number
  yellowCards: number
  redCards: number
  corners: number
  fouls: number
  penalties: number
}

export interface MatchEvent {
  id: number
  matchId: number
  type: MatchEventType
  isHomeTeam: boolean
  minute: number
  comment: string | null
  createdAt: string
}

export interface Match {
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
  events: MatchEvent[]
}
