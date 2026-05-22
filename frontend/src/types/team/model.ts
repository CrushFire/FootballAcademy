export type AgeGroup = 'U10' | 'U12' | 'U14' | 'U16' | 'U18' | 'U21' | 'Senior' | 'Mixed'

export interface Team {
  id: number
  trainerId: number
  name: string
  ageGroup: AgeGroup
  createdAt: string
  images: string[] | null
}
