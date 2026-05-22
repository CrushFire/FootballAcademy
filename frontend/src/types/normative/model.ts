export type Specialization = 'Football' | 'Minifootball'

export interface Normative {
  id: number
  ageGroup: number
  gender: string
  type: string
  unit: string
  isAboveYearOfStudy: boolean
  gradeExcellent: number
  gradeGood: number
  gradeSatisfactory: number
  createdAt: string
}

export interface LocalNormative {
  id: number
  specialization: Specialization
  type: string
  unit: string
  gender: string
  value: number
  isMoreBetter: boolean
  createdAt: string
}
