import { useState } from 'react'
import { assetUrl } from '../api'
import type { GuessPoint } from '../types'
import MinimapGuesser from '../components/MinimapGuesser'
import type { RoundCompletedDto } from './types'

interface MatchRound {
  roundNumber: number
  locationId: number
  imageUrl: string
  minimapUrl: string
}

interface Props {
  round: MatchRound
  totalRounds: number
  submitted: boolean
  roundResult: RoundCompletedDto | null
  playerId: number | null
  error: string | null
  /** True once the final standings have arrived, which unlocks leaving the last round's results. */
  standingsReady: boolean
  onSubmit: (point: GuessPoint) => void
  onContinue: () => void
  onShowStandings: () => void
}

export default function MultiplayerGameScreen({
  round,
  totalRounds,
  submitted,
  roundResult,
  playerId,
  error,
  standingsReady,
  onSubmit,
  onContinue,
  onShowStandings,
}: Props) {
  const [pending, setPending] = useState<GuessPoint | null>(null)

  if (roundResult) {
    const mine = roundResult.results.find((r) => r.playerId === playerId)
    return (
      <div className="screen result-screen">
        <h2>Round {roundResult.roundNumber} results</h2>
        <MinimapGuesser
          minimapUrl={assetUrl(round.minimapUrl)}
          guess={mine ? { x: mine.x, y: mine.y } : undefined}
          actual={{ x: roundResult.actualX, y: roundResult.actualY }}
        />
        <table className="summary-table">
          <thead>
            <tr>
              <th>Player</th>
              <th>Distance</th>
              <th>Score</th>
            </tr>
          </thead>
          <tbody>
            {roundResult.results
              .slice()
              .sort((a, b) => b.score - a.score)
              .map((r) => (
                <tr key={r.playerId}>
                  <td>{r.nickname}</td>
                  <td>{Math.round(r.distanceUnits)} units</td>
                  <td>{r.score}</td>
                </tr>
              ))}
          </tbody>
        </table>
        {roundResult.nextRound ? (
          <button onClick={onContinue}>Next round</button>
        ) : (
          <button onClick={onShowStandings} disabled={!standingsReady}>
            {standingsReady ? 'Final standings' : 'Finishing match...'}
          </button>
        )}
        {error && <p className="error-text">{error}</p>}
      </div>
    )
  }

  return (
    <div className="screen game-screen">
      <div className="game-progress">
        Round {round.roundNumber} / {totalRounds}
      </div>
      <div className="minimap-panel">
        <MinimapGuesser
          minimapUrl={assetUrl(round.minimapUrl)}
          guess={pending ?? undefined}
          onGuess={submitted ? undefined : setPending}
        />
        {submitted ? (
          <p>Waiting for other players to guess...</p>
        ) : (
          <button disabled={!pending} onClick={() => pending && onSubmit(pending)}>
            Guess
          </button>
        )}
        {error && <p className="error-text">{error}</p>}
      </div>
      <img className="screenshot" src={assetUrl(round.imageUrl)} alt="Guess the location" />
    </div>
  )
}
