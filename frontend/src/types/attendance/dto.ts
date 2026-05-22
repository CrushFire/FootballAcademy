import type { AttendanceStatus } from './model'

export interface AttendanceCreateRequest {
  sportsmanId: number
  status: AttendanceStatus
}

export interface AttendanceResponse {
  id: number
  trainingId: number
  sportsmanId: number
  status: AttendanceStatus
  createdAt: string
}
