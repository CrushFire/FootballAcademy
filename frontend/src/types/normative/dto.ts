import type { Specialization } from './model'

export interface NormativeCreateRequest {
  ageGroup: number
  gender: string
  type: string
  unit: string
  isAboveYearOfStudy: boolean
  gradeExcellent: number
  gradeGood: number
  gradeSatisfactory: number
}

export interface NormativeUpdateRequest {
  gradeExcellent?: number
  gradeGood?: number
  gradeSatisfactory?: number
}

export interface NormativeResponse {
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

export interface LocalNormativeCreateRequest {
  specialization: Specialization
  type: string
  unit: string
  gender: string
  value: number
  isMoreBetter: boolean
}

export interface LocalNormativeUpdateRequest {
  value?: number
  isMoreBetter?: boolean
}

export interface LocalNormativeResponse {
  id: number
  specialization: Specialization
  type: string
  unit: string
  gender: string
  value: number
  isMoreBetter: boolean
  createdAt: string
}

export interface NormativeSportsmanCreateRequest {
  sportsmanId: number
  normativeId: number
  result: number
}

export interface NormativeSportsmanResponse {
  id: number
  sportsmanId: number
  normativeId: number
  result: number
  createdAt: string
}

export interface LocalNormativeSportsmanCreateRequest {
  sportsmanId: number
  localNormativeId: number
  result: number
}

export interface LocalNormativeSportsmanResponse {
  id: number
  sportsmanId: number
  localNormativeId: number
  result: number
  createdAt: string
}
