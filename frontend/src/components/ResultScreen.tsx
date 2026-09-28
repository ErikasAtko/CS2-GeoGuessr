import { assetUrl } from '../api'
import type { GuessPoint, GuessResultDto, RoundDto } from '../types'
import MinimapGuesser from './MinimapGuesser'

interface Props {
  round: RoundDto
  guess: GuessPoint
  result: GuessResultDto
  onNext: () => void
}

export default function ResultScreen({ round, guess, result, onNext }: Props) {
  return (
    <div className="screen result-screen">
      <h2>Score: {result.score}</h2>
      <p>Distance: {Math.round(result.distanceUnits)} units</p>
      <MinimapGuesser
        minimapUrl={assetUrl(round.minimapUrl)}
        guess={guess}
        actual={{ x: result.actualX, y: result.actualY }}
      />
      <button onClick={onNext}>Next round</button>
    </div>
  )
}
