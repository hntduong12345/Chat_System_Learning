import type {
  User,
  ChatSession,
  ChatSessionWithMessages,
  ChatMessage,
  LoginRequest,
  RegisterRequest,
  LoginResponse,
  CreateSessionRequest,
  SendMessageRequest,
  AssignSessionRequest,
} from "./types"

const API_BASE_URL = "https://localhost:7025/api"

class ApiService {
  private token: string | null = null

  constructor() {
    // Load token from localStorage on initialization
    if (typeof window !== "undefined") {
      this.token = localStorage.getItem("auth_token")
    }
  }

  setToken(token: string) {
    this.token = token
    if (typeof window !== "undefined") {
      localStorage.setItem("auth_token", token)
    }
  }

  clearToken() {
    this.token = null
    if (typeof window !== "undefined") {
      localStorage.removeItem("auth_token")
    }
  }

  private async request<T>(endpoint: string, options?: RequestInit): Promise<T> {
    try {
      const headers: Record<string, string> = {
        "Content-Type": "application/json",
      }

      if (this.token) {
        headers.Authorization = `Bearer ${this.token}`
      }

      const response = await fetch(`${API_BASE_URL}${endpoint}`, {
        headers: {
          ...headers,
          ...options?.headers,
        },
        ...options,
      })

      if (!response.ok) {
        if (response.status === 401) {
          this.clearToken()
          window.location.href = "/login"
          return Promise.reject(new Error("Unauthorized"))
        }

        const errorText = await response.text()
        console.error(`API Error ${response.status}:`, errorText)
        throw new Error(`API Error: ${response.status} ${response.statusText}`)
      }

      const data = await response.json()
      return data
    } catch (error) {
      console.error(`API Request failed for ${endpoint}:`, error)
      throw error
    }
  }

  // Authentication
  async login(request: LoginRequest): Promise<LoginResponse> {
    const response = await this.request<LoginResponse>("/auth/login", {
      method: "POST",
      body: JSON.stringify(request),
    })
    this.setToken(response.token)
    return response
  }

  async register(request: RegisterRequest): Promise<User> {
    return this.request<User>("/auth/register", {
      method: "POST",
      body: JSON.stringify(request),
    })
  }

  async logout(): Promise<void> {
    this.clearToken()
  }

  async getUser(id: string): Promise<User> {
    return this.request<User>(`/auth/user/${id}`)
  }

  async getAdmins(): Promise<User[]> {
    return this.request<User[]>("/auth/admins")
  }

  // Chat Sessions
  async createSession(request: CreateSessionRequest): Promise<ChatSession> {
    return this.request<ChatSession>("/chat/sessions", {
      method: "POST",
      body: JSON.stringify(request),
    })
  }

  async createSessionWithAdmin(adminId: string, initialMessage: string): Promise<ChatSession> {
    return this.request<ChatSession>("/chat/sessions/with-admin", {
      method: "POST",
      body: JSON.stringify({
        adminId,
        initialMessage,
        channelType: "Web",
      }),
    })
  }

  async getSession(sessionId: string): Promise<ChatSessionWithMessages> {
    return this.request<ChatSessionWithMessages>(`/chat/sessions/${sessionId}`)
  }

  async getCustomerSessions(): Promise<ChatSession[]> {
    return this.request<ChatSession[]>("/chat/sessions/customer")
  }

  async getAdminSessions(): Promise<ChatSession[]> {
    return this.request<ChatSession[]>("/chat/sessions/admin")
  }

  async getWaitingSessions(): Promise<ChatSession[]> {
    return this.request<ChatSession[]>("/chat/sessions/waiting")
  }

  async assignSession(sessionId: string, request: AssignSessionRequest): Promise<ChatSession> {
    return this.request<ChatSession>(`/chat/sessions/${sessionId}/assign`, {
      method: "POST",
      body: JSON.stringify(request),
    })
  }

  async assignAndJoinSession(sessionId: string): Promise<ChatSession> {
    return this.request<ChatSession>(`/chat/sessions/${sessionId}/assign-and-join`, {
      method: "POST",
    })
  }

  async closeSession(sessionId: string): Promise<ChatSession> {
    return this.request<ChatSession>(`/chat/sessions/${sessionId}/close`, {
      method: "POST",
    })
  }

  // Messages
  async sendMessage(sessionId: string, request: SendMessageRequest): Promise<ChatMessage> {
    return this.request<ChatMessage>(`/chat/sessions/${sessionId}/messages`, {
      method: "POST",
      body: JSON.stringify(request),
    })
  }

  async getSessionMessages(sessionId: string, page = 1, pageSize = 50): Promise<ChatMessage[]> {
    return this.request<ChatMessage[]>(`/chat/sessions/${sessionId}/messages?page=${page}&pageSize=${pageSize}`)
  }

  async updateMessageStatus(messageId: string, status: string): Promise<void> {
    return this.request<void>(`/chat/messages/${messageId}/status`, {
      method: "PUT",
      body: JSON.stringify(status),
    })
  }
}

export const apiService = new ApiService()
