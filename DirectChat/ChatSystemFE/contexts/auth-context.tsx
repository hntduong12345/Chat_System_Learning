"use client"

import type React from "react"
import { createContext, useContext, useReducer, useEffect } from "react"
import { apiService } from "@/lib/api-service"
import { signalRService } from "@/lib/signalr-service"
import type { User, LoginRequest, RegisterRequest } from "@/lib/types"

interface AuthState {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null
}

type AuthAction =
  | { type: "SET_LOADING"; payload: boolean }
  | { type: "SET_ERROR"; payload: string | null }
  | { type: "SET_USER"; payload: User | null }
  | { type: "LOGOUT" }

const initialState: AuthState = {
  user: null,
  isAuthenticated: false,
  isLoading: true,
  error: null,
}

function authReducer(state: AuthState, action: AuthAction): AuthState {
  switch (action.type) {
    case "SET_LOADING":
      return { ...state, isLoading: action.payload }
    case "SET_ERROR":
      return { ...state, error: action.payload }
    case "SET_USER":
      return {
        ...state,
        user: action.payload,
        isAuthenticated: !!action.payload,
        error: null,
      }
    case "LOGOUT":
      return {
        ...state,
        user: null,
        isAuthenticated: false,
        error: null,
      }
    default:
      return state
  }
}

interface AuthContextType extends AuthState {
  login: (request: LoginRequest) => Promise<void>
  register: (request: RegisterRequest) => Promise<void>
  logout: () => Promise<void>
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [state, dispatch] = useReducer(authReducer, initialState)

  useEffect(() => {
    // Check if user is already logged in
    const token = localStorage.getItem("auth_token")
    const userData = localStorage.getItem("user_data")

    if (token && userData) {
      try {
        const user = JSON.parse(userData)
        dispatch({ type: "SET_USER", payload: user })
        // Connect to SignalR
        signalRService.connect(token).catch(console.error)
      } catch (error) {
        console.error("Failed to parse user data:", error)
        localStorage.removeItem("auth_token")
        localStorage.removeItem("user_data")
      }
    }

    dispatch({ type: "SET_LOADING", payload: false })
  }, [])

  const login = async (request: LoginRequest) => {
    try {
      dispatch({ type: "SET_LOADING", payload: true })
      const response = await apiService.login(request)

      // Store user data
      localStorage.setItem("user_data", JSON.stringify(response.user))

      dispatch({ type: "SET_USER", payload: response.user })

      // Connect to SignalR
      await signalRService.connect(response.token)

      dispatch({ type: "SET_ERROR", payload: null })
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Login failed. Please check your credentials." })
      throw error
    } finally {
      dispatch({ type: "SET_LOADING", payload: false })
    }
  }

  const register = async (request: RegisterRequest) => {
    try {
      dispatch({ type: "SET_LOADING", payload: true })
      await apiService.register(request)
      dispatch({ type: "SET_ERROR", payload: null })
    } catch (error) {
      dispatch({ type: "SET_ERROR", payload: "Registration failed. Please try again." })
      throw error
    } finally {
      dispatch({ type: "SET_LOADING", payload: false })
    }
  }

  const logout = async () => {
    try {
      await signalRService.disconnect()
      await apiService.logout()
      localStorage.removeItem("user_data")
      dispatch({ type: "LOGOUT" })
    } catch (error) {
      console.error("Logout error:", error)
      // Even if there's an error, still clear the local state
      localStorage.removeItem("user_data")
      dispatch({ type: "LOGOUT" })
    }
  }

  return (
    <AuthContext.Provider
      value={{
        ...state,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (context === undefined) {
    throw new Error("useAuth must be used within an AuthProvider")
  }
  return context
}
