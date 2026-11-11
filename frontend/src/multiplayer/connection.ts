import * as signalR from '@microsoft/signalr'
import { BASE_URL } from '../api'

/** One hub connection per browser tab, subscribed to a single lobby's broadcast group. */
export function createConnection(): signalR.HubConnection {
  return new signalR.HubConnectionBuilder().withUrl(`${BASE_URL}/hubs/multiplayer`).withAutomaticReconnect().build()
}

export async function joinLobbyGroup(connection: signalR.HubConnection, lobbyId: number) {
  if (connection.state === signalR.HubConnectionState.Disconnected) await connection.start()
  await connection.invoke('JoinLobbyGroup', lobbyId)
}
