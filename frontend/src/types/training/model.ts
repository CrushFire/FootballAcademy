export interface Training {
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
