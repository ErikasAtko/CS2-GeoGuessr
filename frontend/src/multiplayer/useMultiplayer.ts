import { useEffect, useRef, useState } from 'react'
import type { HubConnection } from '@microsoft/signalr'
import { createGuestSession, createLobby, getCurrentSession, joinLobby, startMatch, submitMatchGuess } from './api'
import { createConnection, joinLobbyGroup } from './connection'
import type { GuestSessionDto, LobbyDto, PlayerStandingDto, RoundCompletedDto, StartMatchResultDto } from './types'
import type { GuessPoint } from '../types'

export type MultiplayerScreen = 'nickname' | 'lobby' | 'playing' | 'roundResult' | 'finished'

const STORAGE_KEY = 'cs2geo.guest'
const AUTO_ADVANCE_SECONDS = 5

// sessionStorage (not localStorage) is deliberate: it's per-tab, so two tabs in the same
// browser naturally become two different guest players instead of sharing one identity.
function loadStoredIdentity(): GuestSessionDto | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  return raw ? (JSON.parse(raw) as GuestSessionDto) : null
}

// Whether a refresh should land back in the multiplayer tab at all: App.tsx's mode toggle has no
// memory of its own, so it asks this instead of reimplementing the storage key.
export function hasStoredMultiplayerIdentity(): boolean {
  return loadStoredIdentity() !== null
}

interface CurrentMatch {
  matchId: number
  lobbyId: number
  totalRounds: number
  round: StartMatchResultDto['round']
}

export function useMultiplayer() {
  const [identity, setIdentity] = useState<GuestSessionDto | null>(() => loadStoredIdentity())
  const [screen, setScreen] = useState<MultiplayerScreen>(() => (loadStoredIdentity() ? 'lobby' : 'nickname'))
  const [lobby, setLobby] = useState<LobbyDto | null>(null)
  const [match, setMatch] = useState<CurrentMatch | null>(null)
  const [submitted, setSubmitted] = useState(false)
  const [roundResult, setRoundResult] = useState<RoundCompletedDto | null>(null)
  const [standings, setStandings] = useState<PlayerStandingDto[] | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [startingMatch, setStartingMatch] = useState(false)
  const [autoAdvanceSeconds, setAutoAdvanceSeconds] = useState<number | null>(null)

  const connectionRef = useRef<HubConnection | null>(null)

  // On a fresh page load the stored identity's token is all there is - the lobby/match it was in
  // is gone until this resolves it, which is also what lets the hub-connection effect below run.
  useEffect(() => {
    if (!identity) return
    let cancelled = false
    getCurrentSession(identity.token)
      .then((session) => {
        if (cancelled || !session) return
        setLobby(session.lobby)
        if (session.match) {
          setMatch(session.match)
          setScreen('playing')
        }
      })
      .catch((e) => setError(String(e)))
    return () => {
      cancelled = true
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [identity?.token])

  useEffect(() => {
    if (!lobby || !identity) return

    // React StrictMode mounts, cleans up and mounts again in dev. Stopping the first connection
    // aborts its in-flight start(), and that rejection must not show up as an error on screen.
    let cancelled = false
    const connection = createConnection()
    connectionRef.current = connection

    connection.on('LobbyUpdated', (dto: LobbyDto) => setLobby(dto))
    connection.on('MatchStarted', (dto: StartMatchResultDto) => {
      setMatch(dto)
      setSubmitted(false)
      setRoundResult(null)
      setStandings(null)
      setStartingMatch(false)
      setError(null)
      setScreen('playing')
    })
    connection.on('RoundCompleted', (dto: RoundCompletedDto) => {
      setRoundResult(dto)
      setError(null)
      setScreen('roundResult')
    })
    // On the last round this arrives right after RoundCompleted. Stay on the round results so
    // players can see where the last location was; they move on with showStandings().
    connection.on('MatchFinished', (dto: PlayerStandingDto[]) => {
      setStandings(dto)
      setScreen((current) => (current === 'roundResult' ? current : 'finished'))
    })

    joinLobbyGroup(connection, lobby.id, identity.token).catch((e) => {
      if (!cancelled) setError(String(e))
    })

    return () => {
      cancelled = true
      connection.stop()
    }
    // Only (re)connect when the lobby itself changes, not on every member-list update.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [lobby?.id])

  const identify = (nickname: string) => {
    createGuestSession(nickname)
      .then((session) => {
        sessionStorage.setItem(STORAGE_KEY, JSON.stringify(session))
        setIdentity(session)
        setScreen('lobby')
      })
      .catch((e) => setError(String(e)))
  }

  const createLobbyAction = (mapCode: string | null, roundCount: number) => {
    if (!identity) return
    createLobby(identity.token, mapCode, roundCount)
      .then(setLobby)
      .catch((e) => setError(String(e)))
  }

  const joinLobbyAction = (joinCode: string) => {
    if (!identity) return
    joinLobby(identity.token, joinCode)
      .then(setLobby)
      .catch((e) => setError(String(e)))
  }

  const startMatchAction = () => {
    if (!identity || !lobby || startingMatch) return
    // Disabled until MatchStarted arrives, so a double click can't send a second start.
    // No other local state update here: the MatchStarted broadcast drives every connection in
    // the group, including this one, so the host transitions the same way everyone else does.
    setStartingMatch(true)
    setError(null)
    startMatch(identity.token, lobby.joinCode).catch((e) => {
      setError(String(e))
      setStartingMatch(false)
    })
  }

  // Takes the point directly rather than reading it back from state, since the caller just
  // picked it in the same click and a separate "set pending, then submit" pair would read a
  // stale value until React re-renders.
  const submitGuess = (point: GuessPoint) => {
    if (!identity || !match || submitted) return
    setSubmitted(true)
    setError(null)
    submitMatchGuess(identity.token, match.matchId, match.round.roundNumber, point.x, point.y).catch((e) => {
      setError(String(e))
      setSubmitted(false)
    })
  }

  useEffect(() => {
    if (screen !== 'roundResult' || !roundResult?.nextRound) return
    const nextRound = roundResult.nextRound
    let remaining = AUTO_ADVANCE_SECONDS
    setAutoAdvanceSeconds(remaining)
    const interval = setInterval(() => {
      remaining -= 1
      if (remaining > 0) {
        setAutoAdvanceSeconds(remaining)
        return
      }
      clearInterval(interval)
      setMatch((m) => (m ? { ...m, round: nextRound } : m))
      setSubmitted(false)
      setRoundResult(null)
      setError(null)
      setScreen('playing')
    }, 1000)
    return () => {
      clearInterval(interval)
      setAutoAdvanceSeconds(null)
    }
  }, [screen, roundResult])

  const showStandings = () => {
    if (standings) setScreen('finished')
  }

  const isHost = lobby?.members.find((m) => m.playerId === identity?.playerId)?.isHost ?? false

  return {
    screen,
    identity,
    lobby,
    match,
    submitted,
    roundResult,
    standings,
    error,
    isHost,
    startingMatch,
    autoAdvanceSeconds,
    identify,
    createLobbyAction,
    joinLobbyAction,
    startMatchAction,
    submitGuess,
    showStandings,
  }
}
