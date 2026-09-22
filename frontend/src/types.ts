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
}

export interface GuessPoint {
  x: number
  y: number
}
