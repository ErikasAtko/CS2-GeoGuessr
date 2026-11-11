import { useState } from 'react'
import './App.css'
import { useGame } from './hooks/useGame'
import StartScreen from './components/StartScreen'
import GameScreen from './components/GameScreen'
import ResultScreen from './components/ResultScreen'
import SummaryScreen from './components/SummaryScreen'
import MultiplayerApp from './multiplayer/MultiplayerApp'

type Mode = 'single' | 'multiplayer'

function SinglePlayerApp() {
  const game = useGame()
  const { screen, round, guess, result, summary } = game

  return (
    <>
      {(screen === 'playing' || screen === 'result') && (
        <div className="game-progress">
          Round {game.roundIndex + 1} / {game.totalRounds} · Total: {game.totalScore}
        </div>
      )}
      {screen === 'start' && <StartScreen maps={game.maps} onStart={game.start} />}
      {screen === 'playing' && round && <GameScreen key={round.locationId} round={round} onGuess={game.makeGuess} />}
      {screen === 'result' && round && guess && result && (
        <ResultScreen
          minimapUrl={round.minimapUrl}
          guess={guess}
          result={result}
          nextLabel={game.isLastRound ? 'See summary' : 'Next round'}
          onNext={game.next}
        />
      )}
      {screen === 'summary' && summary && <SummaryScreen summary={summary} onPlayAgain={game.backToStart} />}
    </>
  )
}

function App() {
  const [mode, setMode] = useState<Mode>('single')

  return (
    <>
      <div className="mode-toggle">
        <button disabled={mode === 'single'} onClick={() => setMode('single')}>
          Singleplayer
        </button>
        <button disabled={mode === 'multiplayer'} onClick={() => setMode('multiplayer')}>
          Multiplayer
        </button>
      </div>
      {mode === 'single' ? <SinglePlayerApp /> : <MultiplayerApp />}
    </>
  )
}

export default App
