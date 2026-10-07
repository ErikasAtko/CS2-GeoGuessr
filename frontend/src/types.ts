export interface MapDto {
  id: number
  code: string
  displayName: string
  minimapUrl: string
  locationCount: number
}

export interface RoundDto {
  locationId: number
  mapCode: string
  minimapUrl: string
  imageUrl: string
}

export interface GuessResultDto {
  actualX: number
  actualY: number
  distanceUnits: number
  score: number
  tier: string
}

export interface GameDto {
  mapCode: string | null
  rounds: RoundDto[]
}

export interface RoundGuessDto {
  locationId: number
  x: number
  y: number
}

export interface RoundSummaryDto {
  roundNumber: number
  imageUrl: string
  distanceUnits: number
  score: number
  tier: string
}

export interface GameSummaryDto {
  totalScore: number
  maxPossibleScore: number
  averageScore: number
  rounds: RoundSummaryDto[]
}

export interface GuessPoint {
  x: number
  y: number
}
