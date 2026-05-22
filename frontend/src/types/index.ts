// Общие типы ответа API
export interface ApiResponse<T> {
  success: boolean
  data: T
  error?: string
}

// User
export interface User {
  id: number
  login: string
  email: string
  role: string
  createdAt: string
}

// Sportsman
export interface Sportsman {
  id: number
  userId: number
  fio: string
  birthDate: string
  age: number
  height: number
  weight: number
  position: string | null
  teamId: number | null
  gender: string
  specialization: string
  createdAt: string
  images: string[] | null
}

// Group
export interface Group {
  id: number
  trainerId: number
  name: string
  description: string | null
  createdAt: string
}

// Team
export interface Team {
  id: number
  trainerId: number
  name: string
  ageGroup: string
  createdAt: string
  images: string[] | null
}

// Training
export interface Training {
  id: number
  trainerId: number
  groupId: number
  groupName: string
  type: string
  date: string
  otherInformation: string | null
  createdAt: string
}

// Match
export interface Match {
  id: number
  homeTeamId: number
  homeTeamName: string
  opponentTeamId: number | null
  opponentTeamName: string
  type: string
  status: string
  result: string | null
  date: string
  trainerComment: string | null
  homeStats: MatchStats
  awayStats: MatchStats
  events: MatchEvent[]
  lineup: LineupEntry[]
}

export interface LineupEntry {
  sportsmanId: number
  position: string
  type: 'Main' | 'Reserve'
}

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
  type: string
  isHomeTeam: boolean
  minute: number
  comment: string | null
}

// Message
export interface Message {
  id: number
  senderId: number
  senderRole: string
  receiverId: number
  text: string
  isRead: boolean
  broadcastId: number | null
  createdAt: string
}

// Normative
export interface Normative {
  id: number
  ageGroup: number
  gender: string
  type: string
  unit: string
  gradeExcellent: number
  gradeGood: number
  gradeSatisfactory: number
  createdAt: string
}

// Attendance
export interface Attendance {
  id: number
  trainingId: number
  sportsmanId: number
  status: string
  createdAt: string
}

// Filter
export interface Filter {
  filters?: Record<string, unknown>
  sort?: { fields: { field: string; direction: string }[] }
  pagination?: { page: number; pageSize: number }
}
