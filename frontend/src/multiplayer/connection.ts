import * as signalR from '@microsoft/signalr'
import { BASE_URL } from '../api'

/** One hub connection per browser tab, subscribed to a single lobby's broadcast group. */
export function createConnection(): signalR.HubConnection {
  return new signalR.HubConnectionBuilder().withUrl(`${BASE_URL}/hubs/multiplayer`).withAutomaticReconnect().build()
}

/**
 * Starts the connection and subscribes it to the lobby's group, and re-subscribes after every
 * automatic reconnect: a reconnected client gets a new connection id that isn't in any group,
 * so without this it would silently stop receiving lobby and match updates.
 */
export async function joinLobbyGroup(connection: signalR.HubConnection, lobbyId: number, guestToken: string) {
  connection.onreconnected(() => {
    connection.invoke('JoinLobbyGroup', lobbyId, guestToken).catch(() => {})
  })
  if (connection.state === signalR.HubConnectionState.Disconnected) await connection.start()
  await connection.invoke('JoinLobbyGroup', lobbyId, guestToken)
}
