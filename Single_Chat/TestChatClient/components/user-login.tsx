"use client"

import type React from "react"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { User } from "lucide-react"
import { useChat } from "@/contexts/chat-context"

export function UserLogin() {
  const { setCurrentUser, connect } = useChat()
  const [userId, setUserId] = useState("")
  const [userName, setUserName] = useState("")
  const [isLoading, setIsLoading] = useState(false)

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!userId.trim() || !userName.trim()) return

    setIsLoading(true)
    try {
      setCurrentUser({ id: userId.trim(), name: userName.trim() })
      await connect()
    } catch (error) {
      console.error("Login failed:", error)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50">
      <Card className="w-full max-w-md">
        <CardHeader className="text-center">
          <div className="mx-auto w-12 h-12 bg-primary/10 rounded-full flex items-center justify-center mb-4">
            <User className="h-6 w-6 text-primary" />
          </div>
          <CardTitle>Welcome to Chat</CardTitle>
          <p className="text-sm text-muted-foreground">Enter your details to start chatting</p>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleLogin} className="space-y-4">
            <div>
              <label htmlFor="userId" className="block text-sm font-medium mb-1">
                User ID
              </label>
              <Input
                id="userId"
                value={userId}
                onChange={(e) => setUserId(e.target.value)}
                placeholder="Enter your user ID..."
                required
              />
            </div>
            <div>
              <label htmlFor="userName" className="block text-sm font-medium mb-1">
                Display Name
              </label>
              <Input
                id="userName"
                value={userName}
                onChange={(e) => setUserName(e.target.value)}
                placeholder="Enter your display name..."
                required
              />
            </div>
            <Button type="submit" className="w-full" disabled={!userId.trim() || !userName.trim() || isLoading}>
              {isLoading ? "Connecting..." : "Start Chatting"}
            </Button>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
