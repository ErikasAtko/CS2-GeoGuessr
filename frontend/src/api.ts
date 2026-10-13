import type { GameDto, GameSummaryDto, GuessResultDto, MapDto, RoundDto, RoundGuessDto } from './types'

const BASE_URL = 'http://localhost:5080'

export function assetUrl(path: string) {
  return `${BASE_URL}${path}`
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: { 'Content-Type': 'application/json' },
    ...init,
  })
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`)
  return res.json() as Promise<T>
}

export function getMaps() {
  return request<MapDto[]>('/api/maps')
}

export function getRandomRound(mapCode?: string) {
  const query = mapCode ? `?map=${encodeURIComponent(mapCode)}` : ''
  return request<RoundDto>(`/api/rounds/random${query}`)
}

export function startGame(mapCode?: string, rounds = 5) {
  const params = new URLSearchParams({ rounds: String(rounds) })
  if (mapCode) params.set('map', mapCode)
  return request<GameDto>(`/api/games/new?${params}`)
}

export function getSummary(guesses: RoundGuessDto[]) {
  return request<GameSummaryDto>('/api/games/summary', {
    method: 'POST',
    body: JSON.stringify({ guesses }),
  })
}

export function submitGuess(locationId: number, x: number, y: number) {
  return request<GuessResultDto>(`/api/rounds/${locationId}/guess`, {
    method: 'POST',
    body: JSON.stringify({ x, y }),
  })
}
