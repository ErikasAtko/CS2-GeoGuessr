import { useEffect, useState } from 'react'
import './App.css'
import { getMaps, getRandomRound, submitGuess } from './api'
import type { GuessPoint, GuessResultDto, MapDto, RoundDto } from './types'
import StartScreen from './components/StartScreen'
import GameScreen from './components/GameScreen'
import ResultScreen from './components/ResultScreen'

type Screen = 'start' | 'playing' | 'result'

function App() {
  const [screen, setScreen] = useState<Screen>('start')
  const [maps, setMaps] = useState<MapDto[]>([])
  const [mapCode, setMapCode] = useState<string | undefined>(undefined)
  const [round, setRound] = useState<RoundDto | null>(null)
  const [guess, setGuess] = useState<GuessPoint | null>(null)
  const [result, setResult] = useState<GuessResultDto | null>(null)

  useEffect(() => {
    getMaps().then(setMaps).catch(console.error)
  }, [])

  const startRound = (code?: string) => {
    setMapCode(code)
    getRandomRound(code).then((nextRound) => {
      setRound(nextRound)
      setGuess(null)
      setResult(null)
      setScreen('playing')
    })
  }

  const handleGuess = (point: GuessPoint) => {
    if (!round) return
    setGuess(point)
    submitGuess(round.locationId, point.x, point.y).then((nextResult) => {
      setResult(nextResult)
      setScreen('result')
    })
  }

  return (
    <>
      {screen === 'start' && <StartScreen maps={maps} onStart={startRound} />}
      {screen === 'playing' && round && <GameScreen round={round} onGuess={handleGuess} />}
      {screen === 'result' && round && guess && result && (
        <ResultScreen round={round} guess={guess} result={result} onNext={() => startRound(mapCode)} />
      )}
    </>
  )
}

export default App
