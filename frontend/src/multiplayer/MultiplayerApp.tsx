import { useMultiplayer } from './useMultiplayer'
import LobbyScreen from './LobbyScreen'
import MultiplayerGameScreen from './MultiplayerGameScreen'
import MultiplayerSummaryScreen from './MultiplayerSummaryScreen'

export default function MultiplayerApp() {
  const mp = useMultiplayer()

  return (
    <>
      {(mp.screen === 'nickname' || mp.screen === 'lobby') && (
        <LobbyScreen
          hasIdentity={!!mp.identity}
          lobby={mp.lobby}
          isHost={mp.isHost}
          startingMatch={mp.startingMatch}
          error={mp.error}
          onIdentify={mp.identify}
          onCreateLobby={mp.createLobbyAction}
          onJoinLobby={mp.joinLobbyAction}
          onStartMatch={mp.startMatchAction}
        />
      )}
      {(mp.screen === 'playing' || mp.screen === 'roundResult') && mp.match && (
        <MultiplayerGameScreen
          key={mp.match.round.roundNumber}
          round={mp.match.round}
          totalRounds={mp.match.totalRounds}
          submitted={mp.submitted}
          roundResult={mp.screen === 'roundResult' ? mp.roundResult : null}
          playerId={mp.identity?.playerId ?? null}
          error={mp.error}
          standingsReady={!!mp.standings}
          autoAdvanceSeconds={mp.autoAdvanceSeconds}
          onSubmit={mp.submitGuess}
          onShowStandings={mp.showStandings}
        />
      )}
      {mp.screen === 'finished' && mp.standings && <MultiplayerSummaryScreen standings={mp.standings} />}
    </>
  )
}
