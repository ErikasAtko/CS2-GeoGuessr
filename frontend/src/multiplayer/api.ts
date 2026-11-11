import { BASE_URL } from '../api'
import type { GuestSessionDto, LobbyDto, StartMatchResultDto, SubmitGuessAckDto } from './types'

async function request<T>(path: string, token: string | null, init?: RequestInit): Promise<T> {
  const headers: Record<string, string> = { 'Content-Type': 'application/json' }
  if (token) headers['X-Guest-Token'] = token

  const res = await fetch(`${BASE_URL}${path}`, { headers, ...init })
  if (!res.ok) throw new Error((await res.text().catch(() => '')) || `${res.status} ${res.statusText}`)
  return res.json() as Promise<T>
}

export function createGuestSession(nickname: string) {
  return request<GuestSessionDto>('/api/guest-sessions', null, {
    method: 'POST',
    body: JSON.stringify({ nickname }),
  })
}

export function createLobby(token: string, mapCode: string | null, roundCount: number) {
  return request<LobbyDto>('/api/lobbies', token, {
    method: 'POST',
    body: JSON.stringify({ mapCode, roundCount }),
  })
}

export function joinLobby(token: string, joinCode: string) {
  return request<LobbyDto>(`/api/lobbies/${joinCode}/join`, token, { method: 'POST' })
}

export function startMatch(token: string, joinCode: string) {
  return request<StartMatchResultDto>('/api/matches/start', token, {
    method: 'POST',
    body: JSON.stringify({ joinCode }),
  })
}

export function submitMatchGuess(token: string, matchId: number, x: number, y: number) {
  return request<SubmitGuessAckDto>(`/api/matches/${matchId}/guess`, token, {
    method: 'POST',
    body: JSON.stringify({ x, y }),
  })
}
