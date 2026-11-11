export interface GuestSessionDto {
  playerId: number
  nickname: string
  token: string
}

export interface LobbyMemberDto {
  playerId: number
  nickname: string
  isReady: boolean
  isHost: boolean
}

export interface LobbyDto {
  id: number
  joinCode: string
  status: string
  mapCode: string | null
  roundCount: number
  maxPlayers: number
  members: LobbyMemberDto[]
}

export interface MatchRoundDto {
  roundNumber: number
  locationId: number
  imageUrl: string
  minimapUrl: string
}

export interface StartMatchResultDto {
  matchId: number
  lobbyId: number
  totalRounds: number
  round: MatchRoundDto
}

export interface SubmitGuessAckDto {
  score: number
}

export interface PlayerRoundResultDto {
  playerId: number
  nickname: string
  x: number
  y: number
  distanceUnits: number
  score: number
}

export interface RoundCompletedDto {
  roundNumber: number
  actualX: number
  actualY: number
  results: PlayerRoundResultDto[]
  nextRound: MatchRoundDto | null
}

export interface PlayerStandingDto {
  playerId: number
  nickname: string
  totalScore: number
}
