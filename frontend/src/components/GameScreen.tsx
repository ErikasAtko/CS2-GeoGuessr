import { useState } from 'react'
import { assetUrl } from '../api'
import type { GuessPoint, RoundDto } from '../types'
import MinimapGuesser from './MinimapGuesser'

interface Props {
  round: RoundDto
  onGuess: (point: GuessPoint) => void
}

export default function GameScreen({ round, onGuess }: Props) {
  const [pending, setPending] = useState<GuessPoint | null>(null)

  return (
    <div className="screen game-screen">
      <img className="screenshot" src={assetUrl(round.imageUrl)} alt="Guess the location" />
      <div className="minimap-panel">
        <MinimapGuesser minimapUrl={assetUrl(round.minimapUrl)} guess={pending ?? undefined} onGuess={setPending} />
        <button disabled={!pending} onClick={() => pending && onGuess(pending)}>
          Guess
        </button>
      </div>
    </div>
  )
}
