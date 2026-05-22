export type Position =
  | 'GK'
  | 'CB' | 'LB' | 'RB' | 'LWB' | 'RWB'
  | 'CM' | 'CDM' | 'CAM'
  | 'LW' | 'RW'
  | 'ST' | 'CF' | 'SS'

export type Specialization = 'Football' | 'Minifootball'

export interface Sportsman {
  id: number
  userId: number
  fio: string
  birthDate: string
  age: number
  height: number
  weight: number
  position: Position | null
  teamId: number | null
  gender: string
  specialization: Specialization
  createdAt: string
  images: string[] | null
}
