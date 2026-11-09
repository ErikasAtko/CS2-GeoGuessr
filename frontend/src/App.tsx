import './App.css'
import { useGame } from './hooks/useGame'
import StartScreen from './components/StartScreen'
import GameScreen from './components/GameScreen'
import ResultScreen from './components/ResultScreen'
import SummaryScreen from './components/SummaryScreen'

function App() {
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

export default App
