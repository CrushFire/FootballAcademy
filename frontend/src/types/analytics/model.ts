export interface PentagonScores {
  speed: number
  power: number
  sprints: number
  endurance: number
  explosive: number
}

export interface MedicalCheckResult {
  isHealthy: boolean
  cardiovascularOk: boolean
  loadOk: boolean
  recoveryOk: boolean
  injuryRiskOk: boolean
  fatigueOk: boolean
  issues: string[]
}

export interface CommonMetrics {
  maxSpeed: number
  sprintRatio: number
  highSpeedRatio: number
  avgSpeed: number
  hrRedPercent: number
  playerLoad: number
  sprintEffortsPerMin: number
  explosiveIndex: number
  rsa: number
  aerobicLoad: number
  cardioEfficiency: number
  hrStability: number
  recoveryIndex: number
  hi_LO_Ratio: number
  anaerobicRatio: number
  metabolicPower: number
  workRatio: number
  energyEfficiency: number
  fatigueIndex: number
  consistency: number
}

export interface MedicalMetrics {
  averageHeartRate: number
  maxHeartRate: number
  hrStability: number
  hrRedZonePercent: number
  cardioEfficiency: number
  heartRateExertion: number
  playerLoad: number
  playerLoadPerMinute: number
  workRatio: number
  energy: number
  energyEfficiency: number
  recoveryIndex: number
  timeInLowIntensity: number
  timeInHighIntensity: number
  intensityRatio: number
  impacts: number
  impactsPerMinute: number
  explosiveEfforts: number
  neuroMuscularLoad: number
  acuteChronicRatio: number
  highSpeedExposure: number
  accelDecelLoad: number
  fatigueIndex: number
  consistency: number
}

export type PlayerProfile =
  | 'Undefined' | 'Sprinter' | 'EnduranceRunner' | 'PowerPlayer'
  | 'ExplosivePlayer' | 'FlankPlayer' | 'DefenderType' | 'Universal'
  | 'CentralMidfielder' | 'DefensiveMidfielder' | 'AttackingMidfielder'
  | 'Forward' | 'Goalkeeper' | 'DynamicPlayer' | 'StaticPlayer'
  | 'Offensive' | 'Defensive'

export interface GraphPoint {
  date: string
  metrics: CommonMetrics
}
