import type { PlayerStandingDto } from './types'

interface Props {
  standings: PlayerStandingDto[]
}

export default function MultiplayerSummaryScreen({ standings }: Props) {
  return (
    <div className="screen summary-screen">
      <h2>Match over</h2>
      <table className="summary-table">
        <thead>
          <tr>
            <th>Rank</th>
            <th>Player</th>
            <th>Total score</th>
          </tr>
        </thead>
        <tbody>
          {standings.map((s, i) => (
            <tr key={s.playerId}>
              <td>{i + 1}</td>
              <td>{s.nickname}</td>
              <td>{s.totalScore}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
