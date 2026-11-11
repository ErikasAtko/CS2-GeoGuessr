import { useEffect, useRef, useState } from 'react'
import type { HubConnection } from '@microsoft/signalr'
import { createGuestSession, createLobby, joinLobby, startMatch, submitMatchGuess } from './api'
import { createConnection, joinLobbyGroup } from './connection'
import type {
  GuestSessionDto,
  LobbyDto,
  PlayerStandingDto,
  RoundCompletedDto,
  StartMatchResultDto,
} from './types'
import type { GuessPoint } from '../types'

export type MultiplayerScreen = 'nickname' | 'lobby' | 'playing' | 'roundResult' | 'finished'

const STORAGE_KEY = 'cs2geo.guest'

// sessionStorage (not localStorage) is deliberate: it's per-tab, so two tabs in the same
// browser naturally become two different guest players instead of sharing one identity.
function loadStoredIdentity(): GuestSessionDto | null {
  const raw = sessionStorage.getItem(STORAGE_KEY)
  return raw ? (JSON.parse(raw) as GuestSessionDto) : null
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

  const connectionRef = useRef<HubConnection | null>(null)

  useEffect(() => {
    if (!lobby) return

    const connection = createConnection()
    connectionRef.current = connection

    connection.on('LobbyUpdated', (dto: LobbyDto) => setLobby(dto))
    connection.on('MatchStarted', (dto: StartMatchResultDto) => {
      setMatch(dto)
      setSubmitted(false)
      setRoundResult(null)
      setStandings(null)
      setScreen('playing')
    })
    connection.on('RoundCompleted', (dto: RoundCompletedDto) => {
      setRoundResult(dto)
      setScreen('roundResult')
    })
    connection.on('MatchFinished', (dto: PlayerStandingDto[]) => {
      setStandings(dto)
      setScreen('finished')
    })

    joinLobbyGroup(connection, lobby.id).catch((e) => setError(String(e)))

    return () => {
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
    createLobby(identity.token, mapCode, roundCount).then(setLobby).catch((e) => setError(String(e)))
  }

  const joinLobbyAction = (joinCode: string) => {
    if (!identity) return
    joinLobby(identity.token, joinCode).then(setLobby).catch((e) => setError(String(e)))
  }

  const startMatchAction = () => {
    if (!identity || !lobby) return
    // No local state update here: the MatchStarted broadcast drives every connection in the
    // group, including this one, so the host transitions the same way everyone else does.
    startMatch(identity.token, lobby.joinCode).catch((e) => setError(String(e)))
  }

  // Takes the point directly rather than reading it back from state, since the caller just
  // picked it in the same click and a separate "set pending, then submit" pair would read a
  // stale value until React re-renders.
  const submitGuess = (point: GuessPoint) => {
    if (!identity || !match || submitted) return
    setSubmitted(true)
    submitMatchGuess(identity.token, match.matchId, point.x, point.y).catch((e) => {
      setError(String(e))
      setSubmitted(false)
    })
  }

  const continueToNextRound = () => {
    if (!match || !roundResult?.nextRound) return
    setMatch({ ...match, round: roundResult.nextRound })
    setSubmitted(false)
    setRoundResult(null)
    setScreen('playing')
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
    identify,
    createLobbyAction,
    joinLobbyAction,
    startMatchAction,
    submitGuess,
    continueToNextRound,
  }
}
