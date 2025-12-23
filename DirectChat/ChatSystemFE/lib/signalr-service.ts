import * as signalR from "@microsoft/signalr"
import type { ChatMessage, ChatSession } from "./types"

class SignalRService {
  private connection: signalR.HubConnection | null = null
  private listeners: Map<string, Function[]> = new Map()

  async connect(token: string, apiUrl = "https://localhost:7025") {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${apiUrl}/chathub`, {
        accessTokenFactory: () => token,
      })
      .withAutomaticReconnect()
      .build()

    // Set up event listeners
    this.connection.on("ReceiveMessage", (message: ChatMessage) => {
      this.emit("ReceiveMessage", message)
    })

    this.connection.on("MessageHistory", (messages: ChatMessage[]) => {
      this.emit("MessageHistory", messages)
    })

    this.connection.on("SessionAssigned", (session: ChatSession) => {
      this.emit("SessionAssigned", session)
    })

    this.connection.on("UserOnlineStatusChanged", (userId: string, isOnline: boolean) => {
      this.emit("UserOnlineStatusChanged", { userId, isOnline })
    })

    this.connection.on("MessagesMarkedAsRead", (sessionId: string) => {
      this.emit("MessagesMarkedAsRead", { sessionId })
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

  async joinSession(sessionId: string) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke("JoinSession", sessionId)
    }
  }

  async leaveSession(sessionId: string) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke("LeaveSession", sessionId)
    }
  }

  async sendMessage(sessionId: string, content: string) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke("SendMessage", sessionId, content)
    }
  }

  async assignSession(sessionId: string, adminId: string) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke("AssignSession", sessionId, adminId)
    }
  }

  async markMessagesAsRead(sessionId: string) {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      await this.connection.invoke("MarkMessagesAsRead", sessionId)
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
