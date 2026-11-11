import { useState } from 'react'
import type { LobbyDto } from './types'

interface Props {
  hasIdentity: boolean
  lobby: LobbyDto | null
  isHost: boolean
  error: string | null
  onIdentify: (nickname: string) => void
  onCreateLobby: (mapCode: string | null, roundCount: number) => void
  onJoinLobby: (joinCode: string) => void
  onStartMatch: () => void
}

export default function LobbyScreen({
  hasIdentity,
  lobby,
  isHost,
  error,
  onIdentify,
  onCreateLobby,
  onJoinLobby,
  onStartMatch,
}: Props) {
  const [nickname, setNickname] = useState('')
  const [joinCode, setJoinCode] = useState('')

  if (!hasIdentity) {
    return (
      <div className="screen lobby-screen">
        <h1>Multiplayer</h1>
        <p>Pick a nickname to play with friends.</p>
        <form
          onSubmit={(e) => {
            e.preventDefault()
            if (nickname.trim()) onIdentify(nickname.trim())
          }}
        >
          <input value={nickname} onChange={(e) => setNickname(e.target.value)} placeholder="Nickname" maxLength={32} />
          <button type="submit">Continue</button>
        </form>
        {error && <p className="error-text">{error}</p>}
      </div>
    )
  }

  if (!lobby) {
    return (
      <div className="screen lobby-screen">
        <h1>Multiplayer</h1>
        <button onClick={() => onCreateLobby(null, 5)}>Create lobby</button>
        <form
          onSubmit={(e) => {
            e.preventDefault()
            if (joinCode.trim()) onJoinLobby(joinCode.trim().toUpperCase())
          }}
        >
          <input value={joinCode} onChange={(e) => setJoinCode(e.target.value)} placeholder="Join code" maxLength={6} />
          <button type="submit">Join lobby</button>
        </form>
        {error && <p className="error-text">{error}</p>}
      </div>
    )
  }

  return (
    <div className="screen lobby-screen">
      <h1>Lobby {lobby.joinCode}</h1>
      <p>Share this code with friends so they can join.</p>
      <ul className="member-list">
        {lobby.members.map((m) => (
          <li key={m.playerId}>
            {m.nickname} {m.isHost && '(host)'}
          </li>
        ))}
      </ul>
      <p>
        {lobby.members.length} / {lobby.maxPlayers} players · {lobby.roundCount} rounds
      </p>
      {isHost ? <button onClick={onStartMatch}>Start match</button> : <p>Waiting for the host to start the match...</p>}
      {error && <p className="error-text">{error}</p>}
    </div>
  )
}
