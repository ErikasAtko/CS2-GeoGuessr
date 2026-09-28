import type { GuessResultDto, MapDto, RoundDto } from './types'

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

export function submitGuess(locationId: number, x: number, y: number) {
  return request<GuessResultDto>(`/api/rounds/${locationId}/guess`, {
    method: 'POST',
    body: JSON.stringify({ x, y }),
  })
}
