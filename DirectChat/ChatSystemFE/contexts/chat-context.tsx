"use client"

import type React from "react"
import { createContext, useContext, useReducer, useEffect } from "react"
import { apiService } from "@/lib/api-service"
import { signalRService } from "@/lib/signalr-service"
import { useAuth } from "./auth-context"
import type { ChatSession, ChatSessionWithMessages, ChatMessage, CreateSessionRequest } from "@/lib/types"

interface ChatState {
  sessions: ChatSession[]
  currentSession: ChatSessionWithMessages | null
  waitingSessions: ChatSession[]
  isLoading: boolean
  error: string | null
}

type ChatAction =
  | { type: "SET_LOADING"; payload: boolean }
  | { type: "SET_ERROR"; payload: string | null }
  | { type: "SET_SESSIONS"; payload: ChatSession[] }
  | { type: "SET_CURRENT_SESSION"; payload: ChatSessionWithMessages | null }
  | { type: "SET_WAITING_SESSIONS"; payload: ChatSession[] }
  | { type: "ADD_MESSAGE"; payload: ChatMessage }
  | { type: "UPDATE_SESSION"; payload: ChatSession }
  | { type: "REMOVE_WAITING_SESSION"; payload: string }

const initialState: ChatState = {
  sessions: [],
  currentSession: null,
  waitingSessions: [],
  isLoading: false,
  error: null,
}

function chatReducer(state: ChatState, action: ChatAction): ChatState {
  switch (action.type) {
    case "SET_LOADING":
      return { ...state, isLoading: action.payload }
    case "SET_ERROR":
      return { ...state, error: action.payload }
    case "SET_SESSIONS":
      return { ...state, sessions: action.payload }
    case "SET_CURRENT_SESSION":
      return { ...state, currentSession: action.payload }
    case "SET_WAITING_SESSIONS":
      return { ...state, waitingSessions: action.payload }
    case "ADD_MESSAGE":
      if (state.currentSession && state.currentSession.id === action.payload.chatSessionId) {
        return {
          ...state,
          currentSession: {
            ...state.currentSession,
            messages: [...state.currentSession.messages, action.payload],
          },
        }
      }
      return state
    case "UPDATE_SESSION":
      return {
        ...state,
        sessions: state.sessions.map((s) => (s.id === action.payload.id ? action.payload : s)),
        waitingSessions: state.waitingSessions.map((s) => (s.id === action.payload.id ? action.payload : s)),
      }
    case "REMOVE_WAITING_SESSION":
      return {
        ...state,
        waitingSessions: state.waitingSessions.filter((s) => s.id !== action.payload),
      }
    default:
      return state
  }
}

interface ChatContextType extends ChatState {
  loadSessions: () => Promise<void>
  loadWaitingSessions: () => Promise<void>
  createSession: (request: CreateSessionRequest) => Promise<ChatSession>
  createSessionWithAdmin: (adminId: string, initialMessage: string) => Promise<ChatSession>
  joinSession: (sessionId: string) => Promise<void>
  leaveSession: () => Promise<void>
  sendMessage: (content: string) => Promise<void>
  assignSession: (sessionId: string, adminId: string) => Promise<void>
  closeSession: (sessionId: string) => Promise<void>
}

const ChatContext = createContext<ChatContextType | undefined>(undefined)

export function ChatProvider({ children }: { children: React.ReactNode }) {
  const [state, dispatch] = useReducer(chatReducer, initialState)
  const { user, isAuthenticated } = useAuth()

  useEffect(() => {
    if (isAuthenticated && user) {
      // Set up SignalR event listeners
      const handleReceiveMessage = (message: ChatMessage) => {
        dispatch({ type: "ADD_MESSAGE", payload: message })
      }

      const handleSessionAssigned = (session: ChatSession) => {
        dispatch({ type: "UPDATE_SESSION", payload: session })
        // Remove from waiting sessions when assigned
        dispatch({ type: "REMOVE_WAITING_SESSION", payload: session.id })
      }

      const handleError = (error: string) => {
        dispatch({ type: "SET_ERROR", payload: error })
      }

      signalRService.on("ReceiveMessage", handleReceiveMessage)
      signalRService.on("SessionAssigned", handleSessionAssigned)
      signalRService.on("Error", handleError)

      // Load initial data
      loadSessions()
      if (user.roleName === "Admin") {
        loadWaitingSessions()
        // Refresh waiting sessions every 30 seconds for admins
        const interval = setInterval(loadWaitingSessions, 30000)
        return () => {
          clearInterval(interval)
          signalRService.off("ReceiveMessage", handleReceiveMessage)
          signalRService.off("SessionAssigned", handleSessionAssigned)
          signalRService.off("Error", handleError)
        }
      }

      return () => {
        signalRService.off("ReceiveMessage", handleReceiveMessage)
        signalRService.off("SessionAssigned", handleSessionAssigned)
        signalRService.off("Error", handleError)
      }
    }
  }, [isAuthenticated, user])

  const loadSessions = async () => {
    if (!user) return

    try {
      dispatch({ type: "SET_LOADING", payload: true })
      let sessions: ChatSession[]

      if (user.roleName === "Customer") {
        sessions = await apiService.getCustomerSessions()
      } else {
        sessions = await apiService.getAdminSessions()
      }

      dispatch({ type: "SET_SESSIONS", payload: sessions })
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Failed to load sessions" })
    } finally {
      dispatch({ type: "SET_LOADING", payload: false })
    }
  }

  const loadWaitingSessions = async () => {
    try {
      const sessions = await apiService.getWaitingSessions()
      dispatch({ type: "SET_WAITING_SESSIONS", payload: sessions })
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Failed to load waiting sessions" })
    }
  }

  const createSession = async (request: CreateSessionRequest): Promise<ChatSession> => {
    try {
      dispatch({ type: "SET_LOADING", payload: true })
      const session = await apiService.createSession(request)

      // Automatically join the created session
      await joinSession(session.id)

      // Reload sessions
      await loadSessions()

      return session
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Failed to create session" })
      throw error
    } finally {
      dispatch({ type: "SET_LOADING", payload: false })
    }
  }

  const createSessionWithAdmin = async (adminId: string, initialMessage: string): Promise<ChatSession> => {
    try {
      dispatch({ type: "SET_LOADING", payload: true })
      const session = await apiService.createSessionWithAdmin(adminId, initialMessage)

      // Automatically join the created session
      await joinSession(session.id)

      // Reload sessions
      await loadSessions()

      return session
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Failed to create session" })
      throw error
    } finally {
      dispatch({ type: "SET_LOADING", payload: false })
    }
  }

  const joinSession = async (sessionId: string) => {
    try {
      dispatch({ type: "SET_LOADING", payload: true })

      // Leave current session if any
      if (state.currentSession) {
        await signalRService.leaveSession(state.currentSession.id)
      }

      // Get session details
      const session = await apiService.getSession(sessionId)
      dispatch({ type: "SET_CURRENT_SESSION", payload: session })

      // Join SignalR group
      await signalRService.joinSession(sessionId)
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Failed to join session" })
    } finally {
      dispatch({ type: "SET_LOADING", payload: false })
    }
  }

  const leaveSession = async () => {
    if (state.currentSession) {
      await signalRService.leaveSession(state.currentSession.id)
      dispatch({ type: "SET_CURRENT_SESSION", payload: null })
    }
  }

  const sendMessage = async (content: string) => {
    if (!state.currentSession) return

    try {
      await signalRService.sendMessage(state.currentSession.id, content)
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Failed to send message" })
    }
  }

  const assignSession = async (sessionId: string, adminId: string) => {
    try {
      await apiService.assignAndJoinSession(sessionId)
      // The session will be updated via SignalR event
      await loadSessions()
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Failed to assign session" })
    }
  }

  const closeSession = async (sessionId: string) => {
    try {
      await apiService.closeSession(sessionId)
      await loadSessions()
      if (state.currentSession?.id === sessionId) {
        dispatch({ type: "SET_CURRENT_SESSION", payload: null })
      }
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Failed to close session" })
    }
  }

  return (
    <ChatContext.Provider
      value={{
        ...state,
        loadSessions,
        loadWaitingSessions,
        createSession,
        createSessionWithAdmin,
        joinSession,
        leaveSession,
        sendMessage,
        assignSession,
        closeSession,
      }}
    >
      {children}
    </ChatContext.Provider>
  )
}

export function useChat() {
  const context = useContext(ChatContext)
  if (context === undefined) {
    throw new Error("useChat must be used within a ChatProvider")
  }
  return context
}
