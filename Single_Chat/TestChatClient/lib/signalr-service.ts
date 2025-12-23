import * as signalR from "@microsoft/signalr"

export interface ChatMessage {
  id: number
  chatSessionId: number
  userId: string
  userName: string
  content: string
  timestamp: string
  type: "User" | "System" | "Bot"
}

export interface ChatSession {
  id: number
  sessionName: string
  userId: string
  createdAt: string
  closedAt?: string
  isActive: boolean
  connectionId?: string
  messages: ChatMessage[]
}

class SignalRService {
  private connection: signalR.HubConnection | null = null
  private listeners: Map<string, Function[]> = new Map()

  async connect(apiUrl = "https://localhost:7025") {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return
    }

    this.connection = new signalR.HubConnectionBuilder().withUrl(`${apiUrl}/chathub`).withAutomaticReconnect().build()

    // Set up event listeners
    this.connection.on("ReceiveMessage", (message: ChatMessage) => {
      this.emit("ReceiveMessage", message)
    })

    this.connection.on("MessageHistory", (messages: ChatMessage[]) => {
      this.emit("MessageHistory", messages)
    })

    this.connection.on("UserJoined", (userName: string, timestamp: string) => {
      this.emit("UserJoined", { userName, timestamp })
    })

    this.connection.on("Error", (error: string) => {
      this.emit("Error", error)
    })

    try {
      await this.connection.start()
      console.log("SignalR Connected")
    } catch (err) {
      console.error("SignalR Connection Error: ", err)
      throw err
    }
  }

  async disconnect() {
    if (this.connection) {
      await this.connection.stop()
      this.connection = null
    }
  }

  async joinSession(sessionId: number, userId: string, userName: string) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke("JoinSession", sessionId, userId, userName)
    }
  }

  async sendMessage(sessionId: number, userId: string, userName: string, message: string) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke("SendMessage", sessionId, userId, userName, message)
    }
  }

  async leaveSession(sessionId: number) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke("LeaveSession", sessionId)
    }
  }

  on(event: string, callback: Function) {
    if (!this.listeners.has(event)) {
      this.listeners.set(event, [])
    }
    this.listeners.get(event)!.push(callback)
  }

  off(event: string, callback: Function) {
    const eventListeners = this.listeners.get(event)
    if (eventListeners) {
      const index = eventListeners.indexOf(callback)
      if (index > -1) {
        eventListeners.splice(index, 1)
      }
    }
  }

  private emit(event: string, data: any) {
    const eventListeners = this.listeners.get(event)
    if (eventListeners) {
      eventListeners.forEach((callback) => callback(data))
    }
  }

  get connectionState() {
    return this.connection?.state
  }
}

export const signalRService = new SignalRService()
