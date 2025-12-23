export interface User {
  id: string
  email: string
  fullName: string
  phoneNumber?: string
  roleName: string
  createAt: string
}

export interface ChatSession {
  id: string
  customer: User
  admin?: User
  status: string
  createAt: string
  closeAt?: string
  lastActiveAt: string
  connectionState?: string
  inactivityTimeout: number
  channelType: string
  unreadCount: number
  lastMessage?: ChatMessage
}

export interface ChatSessionWithMessages extends ChatSession {
  messages: ChatMessage[]
}

export interface ChatMessage {
  id: string
  chatSessionId: string
  sender: User
  content: string
  sendAt: string
  deliveryStatus: string
  sourcePlatform: string
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  fullName: string
  password: string
  phoneNumber?: string
  roleName: string
}

export interface LoginResponse {
  user: User
  token: string
  expiresAt: string
}

export interface CreateSessionRequest {
  initialMessage: string
  channelType: string
}

export interface SendMessageRequest {
  content: string
  sourcePlatform: string
}

export interface AssignSessionRequest {
  adminId: string
}
