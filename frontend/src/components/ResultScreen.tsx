import { assetUrl } from '../api'
import type { GuessPoint, GuessResultDto } from '../types'
import MinimapGuesser from './MinimapGuesser'

interface Props {
  minimapUrl: string
  guess: GuessPoint
  result: GuessResultDto
  nextLabel: string
  onNext: () => void
}

export default function ResultScreen({ minimapUrl, guess, result, nextLabel, onNext }: Props) {
  return (
    <div className="screen result-screen">
      <h2>
        Score: {result.score} ({result.tier})
      </h2>
      <p>Distance: {Math.round(result.distanceUnits)} units</p>
      <MinimapGuesser
        minimapUrl={assetUrl(minimapUrl)}
        guess={guess}
        actual={{ x: result.actualX, y: result.actualY }}
      />
      <button onClick={onNext}>{nextLabel}</button>
    </div>
  )
}
