import { useEffect, useState } from 'react'
import './App.css'
import { getMaps, getSummary, startGame, submitGuess } from './api'
import type { GameDto, GameSummaryDto, GuessPoint, GuessResultDto, MapDto, RoundGuessDto } from './types'
import StartScreen from './components/StartScreen'
import GameScreen from './components/GameScreen'
import ResultScreen from './components/ResultScreen'
import SummaryScreen from './components/SummaryScreen'

type Screen = 'start' | 'playing' | 'result' | 'summary'

function App() {
  const [screen, setScreen] = useState<Screen>('start')
  const [maps, setMaps] = useState<MapDto[]>([])
  const [game, setGame] = useState<GameDto | null>(null)
  const [roundIndex, setRoundIndex] = useState(0)
  const [guesses, setGuesses] = useState<RoundGuessDto[]>([])
  const [totalScore, setTotalScore] = useState(0)
  const [guess, setGuess] = useState<GuessPoint | null>(null)
  const [result, setResult] = useState<GuessResultDto | null>(null)
  const [summary, setSummary] = useState<GameSummaryDto | null>(null)

  useEffect(() => {
    getMaps().then(setMaps).catch(console.error)
  }, [])

  const round = game?.rounds[roundIndex] ?? null
  const totalRounds = game?.rounds.length ?? 0
  const isLastRound = roundIndex === totalRounds - 1

  const handleStart = (code?: string) => {
    startGame(code)
      .then((nextGame) => {
        setGame(nextGame)
        setRoundIndex(0)
        setGuesses([])
        setTotalScore(0)
        setGuess(null)
        setResult(null)
        setSummary(null)
        setScreen('playing')
      })
      .catch(console.error)
  }

  const handleGuess = (point: GuessPoint) => {
    if (!round) return
    setGuess(point)
    submitGuess(round.locationId, point.x, point.y)
      .then((nextResult) => {
        setResult(nextResult)
        setTotalScore((total) => total + nextResult.score)
        setGuesses((previous) => [...previous, { locationId: round.locationId, x: point.x, y: point.y }])
        setScreen('result')
      })
      .catch(console.error)
  }

  const handleNext = () => {
    if (!isLastRound) {
      setRoundIndex((index) => index + 1)
      setGuess(null)
      setResult(null)
      setScreen('playing')
      return
    }
    getSummary(guesses)
      .then((nextSummary) => {
        setSummary(nextSummary)
        setScreen('summary')
      })
      .catch(console.error)
  }

  return (
    <>
      {(screen === 'playing' || screen === 'result') && (
        <div className="game-progress">
          Round {roundIndex + 1} / {totalRounds} · Total: {totalScore}
        </div>
      )}
      {screen === 'start' && <StartScreen maps={maps} onStart={handleStart} />}
      {screen === 'playing' && round && <GameScreen key={round.locationId} round={round} onGuess={handleGuess} />}
      {screen === 'result' && round && guess && result && (
        <ResultScreen
          round={round}
          guess={guess}
          result={result}
          nextLabel={isLastRound ? 'See summary' : 'Next round'}
          onNext={handleNext}
        />
      )}
      {screen === 'summary' && summary && <SummaryScreen summary={summary} onPlayAgain={() => setScreen('start')} />}
    </>
  )
}

export default App