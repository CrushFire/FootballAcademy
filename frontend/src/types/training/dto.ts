export interface TrainingCreateRequest {
  trainerId: number
  planTrainingId?: number | null
  groupId: number
  type: string
  date: string
  otherInformation?: string | null
}

export interface TrainingUpdateRequest {
  type?: string
  date?: string
  otherInformation?: string | null
}

export interface TrainingResponse {
  id: number
  trainerId: number
  planTrainingId: number | null
  planTrainingName: string | null
  groupId: number
  groupName: string
  type: string
  date: string
  otherInformation: string | null
  createdAt: string
}
