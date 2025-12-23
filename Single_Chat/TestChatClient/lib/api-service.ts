import type { ChatSession, ChatMessage } from "./signalr-service"

const API_BASE_URL = "https://localhost:7025/api" // Updated to match your .NET URL

export interface CreateSessionRequest {
  sessionName: string
  userId: string
}

export interface SessionActionRequest {
  userId: string
}

class ApiService {
  private async request<T>(endpoint: string, options?: RequestInit): Promise<T> {
    try {
      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        headers: {
          "Content-Type": "application/json",
          ...options?.headers,
        },
        ...options,
      })

      if (!response.ok) {
        const errorText = await response.text()
        console.error(`API Error ${response.status}:`, errorText)
        throw new Error(`API Error: ${response.status} ${response.statusText}`)
      }

      const data = await response.json()
      console.log(`API Success ${endpoint}:`, data)
      return data
    } catch (error) {
      console.error(`API Request failed for ${endpoint}:`, error)
      throw error
    }
  }

  async createSession(request: CreateSessionRequest): Promise<ChatSession> {
    return this.request<ChatSession>("/chatsessions", {
      method: "POST",
      body: JSON.stringify(request),
    })
  }

  async getSession(sessionId: number): Promise<ChatSession> {
    return this.request<ChatSession>(`/chatsessions/${sessionId}`)
  }

  async getUserSessions(userId: string): Promise<ChatSession[]> {
    return this.request<ChatSession[]>(`/chatsessions/user/${userId}`)
  }

  async closeSession(sessionId: number, userId: string): Promise<ChatSession> {
    return this.request<ChatSession>(`/chatsessions/${sessionId}/close`, {
      method: "PUT",
      body: JSON.stringify({ userId }),
    })
  }

  async reactivateSession(sessionId: number, userId: string): Promise<ChatSession> {
    return this.request<ChatSession>(`/chatsessions/${sessionId}/reactivate`, {
      method: "PUT",
      body: JSON.stringify({ userId }),
    })
  }

  async getSessionMessages(sessionId: number, page = 1, pageSize = 50): Promise<ChatMessage[]> {
    return this.request<ChatMessage[]>(`/chatsessions/${sessionId}/messages?page=${page}&pageSize=${pageSize}`)
  }
}

export const apiService = new ApiService()
