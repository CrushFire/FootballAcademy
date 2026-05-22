import * as signalR from '@microsoft/signalr'
import { ref } from 'vue'
import { useAuthStore } from '@/store/auth'

const connection = ref<signalR.HubConnection | null>(null)

export function useSignalR() {
  const auth = useAuthStore()

  async function start() {
    if (connection.value?.state === signalR.HubConnectionState.Connected) return

    connection.value = new signalR.HubConnectionBuilder()
      .withUrl('/api/hubs/chat', {
        accessTokenFactory: () => auth.token ?? ''
      })
      .withAutomaticReconnect()
      .build()

    await connection.value.start()
  }

  async function stop() {
    await connection.value?.stop()
  }

  function on<T>(method: string, callback: (data: T) => void) {
    connection.value?.on(method, callback)
  }

  function off(method: string) {
    connection.value?.off(method)
  }

  return { connection, start, stop, on, off }
}
