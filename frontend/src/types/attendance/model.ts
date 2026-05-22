export type AttendanceStatus = 'Present' | 'Late' | 'Absent' | 'ExcusedAbsent'

export interface Attendance {
  id: number
  trainingId: number
  sportsmanId: number
  status: AttendanceStatus
  createdAt: string
}
