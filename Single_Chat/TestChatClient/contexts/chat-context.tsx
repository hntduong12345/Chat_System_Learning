"use client"

import type React from "react"
import { createContext, useContext, useReducer, useEffect } from "react"
import { type ChatSession, type ChatMessage, signalRService } from "@/lib/signalr-service"
import { apiService } from "@/lib/api-service"

interface ChatState {
  sessions: ChatSession[]
  currentSession: ChatSession | null
  messages: ChatMessage[]
  isConnected: boolean
  isLoading: boolean
  error: string | null
  currentUser: { id: string; name: string } | null
}

type ChatAction =
  | { type: "SET_LOADING"; payload: boolean }
  | { type: "SET_ERROR"; payload: string | null }
  | { type: "SET_CONNECTED"; payload: boolean }
  | { type: "SET_SESSIONS"; payload: ChatSession[] }
  | { type: "SET_CURRENT_SESSION"; payload: ChatSession | null }
  | { type: "SET_MESSAGES"; payload: ChatMessage[] }
  | { type: "ADD_MESSAGE"; payload: ChatMessage }
  | { type: "SET_CURRENT_USER"; payload: { id: string; name: string } }
  | { type: "UPDATE_SESSION"; payload: ChatSession }

const initialState: ChatState = {
  sessions: [],
  currentSession: null,
  messages: [],
  isConnected: false,
  isLoading: false,
  error: null,
  currentUser: null,
}

function chatReducer(state: ChatState, action: ChatAction): ChatState {
  switch (action.type) {
    case "SET_LOADING":
      return { ...state, isLoading: action.payload }
    case "SET_ERROR":
      return { ...state, error: action.payload }
    case "SET_CONNECTED":
      return { ...state, isConnected: action.payload }
    case "SET_SESSIONS":
      return { ...state, sessions: action.payload }
    case "SET_CURRENT_SESSION":
      return { ...state, currentSession: action.payload }
    case "SET_MESSAGES":
      return { ...state, messages: action.payload }
    case "ADD_MESSAGE":
      return { ...state, messages: [...state.messages, action.payload] }
    case "SET_CURRENT_USER":
      return { ...state, currentUser: action.payload }
    case "UPDATE_SESSION":
      return {
        ...state,
        sessions: state.sessions.map((s) => (s.id === action.payload.id ? action.payload : s)),
        currentSession: state.currentSession?.id === action.payload.id ? action.payload : state.currentSession,
      }
    default:
      return state
  }
}

interface ChatContextType extends ChatState {
  connect: () => Promise<void>
  disconnect: () => Promise<void>
  createSession: (sessionName: string) => Promise<void>
  joinSession: (sessionId: number) => Promise<void>
  leaveSession: () => Promise<void>
  sendMessage: (message: string) => Promise<void>
  closeSession: (sessionId: number) => Promise<void>
  reactivateSession: (sessionId: number) => Promise<void>
  loadUserSessions: () => Promise<void>
  setCurrentUser: (user: { id: string; name: string }) => void
}

const ChatContext = createContext<ChatContextType | undefined>(undefined)

export function ChatProvider({ children }: { children: React.ReactNode }) {
  const [state, dispatch] = useReducer(chatReducer, initialState)

  useEffect(() => {
    // Set up SignalR event listeners
    const handleReceiveMessage = (message: ChatMessage) => {
      dispatch({ type: "ADD_MESSAGE", payload: message })
    }

    const handleMessageHistory = (messages: ChatMessage[]) => {
      dispatch({ type: "SET_MESSAGES", payload: messages })
    }

    const handleError = (error: string) => {
      dispatch({ type: "SET_ERROR", payload: error })
    }

    signalRService.on("ReceiveMessage", handleReceiveMessage)
    signalRService.on("MessageHistory", handleMessageHistory)
    signalRService.on("Error", handleError)

    return () => {
      signalRService.off("ReceiveMessage", handleReceiveMessage)
      signalRService.off("MessageHistory", handleMessageHistory)
      signalRService.off("Error", handleError)
    }
  }, [])

  const connect = async () => {
    try {
      dispatch({ type: "SET_LOADING", payload: true })
      await signalRService.connect()
      dispatch({ type: "SET_CONNECTED", payload: true })
      dispatch({ type: "SET_ERROR", payload: null })
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Failed to connect to chat server" })
    } finally {
      dispatch({ type: "SET_LOADING", payload: false })
    }
  }

  const disconnect = async () => {
    await signalRService.disconnect()
    dispatch({ type: "SET_CONNECTED", payload: false })
  }

  const createSession = async (sessionName: string) => {
    if (!state.currentUser) return

    try {
      dispatch({ type: "SET_LOADING", payload: true })
      const session = await apiService.createSession({
        sessionName,
        userId: state.currentUser.id,
      })

      // Reload sessions to get the updated list
      await loadUserSessions()

      // Automatically join the newly created session
      await joinSession(session.id)

      dispatch({ type: "SET_ERROR", payload: null })
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Failed to create session" })
    } finally {
      dispatch({ type: "SET_LOADING", payload: false })
    }
  }

  const joinSession = async (sessionId: number) => {
    if (!state.currentUser || !state.isConnected) {
      dispatch({ type: "SET_ERROR", payload: "Not connected to chat server" })
      return
    }

    try {
      dispatch({ type: "SET_LOADING", payload: true })

      // Leave current session if any
      if (state.currentSession) {
        await signalRService.leaveSession(state.currentSession.id)
      }

      // Get session details From API first
      const session = await apiService.getSession(sessionId)

      if (!session) {
        dispatch({ type: "SET_ERROR", payload: "Session not found" })
        return
      }

      dispatch({ type: "SET_CURRENT_SESSION", payload: session })

      // Join SignalR group
      await signalRService.joinSession(sessionId, state.currentUser.id, state.currentUser.name)

      dispatch({ type: "SET_ERROR", payload: null })
    } catch (error) {
      console.error("Failed to join session:", error)
      dispatch({ type: "SET_ERROR", payload: "Failed to join session" })
    } finally {
      dispatch({ type: "SET_LOADING", payload: false })
    }
  }

  const leaveSession = async () => {
    if (state.currentSession) {
      await signalRService.leaveSession(state.currentSession.id)
      dispatch({ type: "SET_CURRENT_SESSION", payload: null })
      dispatch({ type: "SET_MESSAGES", payload: [] })
    }
  }

  const sendMessage = async (message: string) => {
    if (!state.currentSession || !state.currentUser || !state.isConnected) return

    try {
      await signalRService.sendMessage(state.currentSession.id, state.currentUser.id, state.currentUser.name, message)
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Failed to send message" })
    }
  }

  const closeSession = async (sessionId: number) => {
    if (!state.currentUser) return

    try {
      const updatedSession = await apiService.closeSession(sessionId, state.currentUser.id)
      dispatch({ type: "UPDATE_SESSION", payload: updatedSession })

      if (state.currentSession?.id === sessionId) {
        await leaveSession()
      }
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Failed to close session" })
    }
  }

  const reactivateSession = async (sessionId: number) => {
    if (!state.currentUser) return

    try {
      const updatedSession = await apiService.reactivateSession(sessionId, state.currentUser.id)
      dispatch({ type: "UPDATE_SESSION", payload: updatedSession })
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Failed to reactivate session" })
    }
  }

  const loadUserSessions = async () => {
    if (!state.currentUser) return

    try {
      const sessions = await apiService.getUserSessions(state.currentUser.id)
      dispatch({ type: "SET_SESSIONS", payload: sessions })
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Failed to load sessions" })
    }
  }

  const setCurrentUser = (user: { id: string; name: string }) => {
    dispatch({ type: "SET_CURRENT_USER", payload: user })
  }

  return (
    <ChatContext.Provider
      value={{
        ...state,
        connect,
        disconnect,
        createSession,
        joinSession,
        leaveSession,
        sendMessage,
        closeSession,
        reactivateSession,
        loadUserSessions,
        setCurrentUser,
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
