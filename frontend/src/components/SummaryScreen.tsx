import { assetUrl } from '../api'
import type { GameSummaryDto } from '../types'

interface Props {
  summary: GameSummaryDto
  onPlayAgain: () => void
}

export default function SummaryScreen({ summary, onPlayAgain }: Props) {
  return (
    <div className="screen summary-screen">
      <h2>Game over</h2>
      <p className="summary-total">
        {summary.totalScore} / {summary.maxPossibleScore}
      </p>
      <p>Average: {Math.round(summary.averageScore)} per round</p>
      {/* Rounds arrive already sorted best to worst by the backend */}
      <h3 className="summary-heading">Your rounds, ranked best to worst</h3>
      <table className="summary-table">
        <thead>
          <tr>
            <th>Round</th>
            <th>Location</th>
            <th>Distance</th>
            <th>Score</th>
            <th>Tier</th>
          </tr>
        </thead>
        <tbody>
          {summary.rounds.map((round) => (
            <tr key={round.roundNumber}>
              <td>{round.roundNumber}</td>
              <td>
                <img className="summary-thumb" src={assetUrl(round.imageUrl)} alt={`Round ${round.roundNumber}`} />
              </td>
              <td>{Math.round(round.distanceUnits)} units</td>
              <td>{round.score}</td>
              <td>{round.tier}</td>
            </tr>
          ))}
        </tbody>
      </table>
      <button onClick={onPlayAgain}>Play again</button>
    </div>
  )
}