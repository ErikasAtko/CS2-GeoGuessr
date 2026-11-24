import type { MouseEvent } from 'react'
import type { GuessPoint } from '../types'

export interface ExtraGuess extends GuessPoint {
  color: string
  label: string
}

interface Props {
  minimapUrl: string
  guess?: GuessPoint
  actual?: GuessPoint
  extraGuesses?: ExtraGuess[]
  onGuess?: (point: GuessPoint) => void
}

export default function MinimapGuesser({ minimapUrl, guess, actual, extraGuesses, onGuess }: Props) {
  const handleClick = (event: MouseEvent<HTMLDivElement>) => {
    if (!onGuess) return
    const rect = event.currentTarget.getBoundingClientRect()
    onGuess({
      x: (event.clientX - rect.left) / rect.width,
      y: (event.clientY - rect.top) / rect.height,
    })
  }

  return (
    <div className={`minimap${onGuess ? ' minimap-clickable' : ''}`} onClick={handleClick}>
      <img src={minimapUrl} alt="Minimap" draggable={false} />
      {extraGuesses?.map((g, i) => (
        <div
          key={i}
          className="pin"
          title={g.label}
          style={{ left: `${g.x * 100}%`, top: `${g.y * 100}%`, background: g.color }}
        />
      ))}
      {guess && <div className="pin pin-guess" style={{ left: `${guess.x * 100}%`, top: `${guess.y * 100}%` }} />}
      {actual && <div className="pin pin-actual" style={{ left: `${actual.x * 100}%`, top: `${actual.y * 100}%` }} />}
    </div>
  )
}
