"use client"

import type React from "react"

import { useState, useRef, useEffect } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Badge } from "@/components/ui/badge"
import { Send, User } from "lucide-react"
import { useChat } from "@/contexts/chat-context"
import type { ChatMessage } from "@/lib/signalr-service"

export function ChatInterface() {
  const { currentSession, messages, sendMessage, isConnected } = useChat()
  const [newMessage, setNewMessage] = useState("")
  const messagesEndRef = useRef<HTMLDivElement>(null)

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" })
  }

  useEffect(() => {
    scrollToBottom()
  }, [messages])

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!newMessage.trim() || !isConnected) return

    await sendMessage(newMessage.trim())
    setNewMessage("")
  }

  if (!currentSession) {
    return (
      <Card className="flex-1">
        <CardContent className="flex items-center justify-center h-full">
          <div className="text-center text-muted-foreground">
            <h3 className="text-lg font-semibold mb-2">No Session Selected</h3>
            <p>Select a session from the sidebar to start chatting</p>
          </div>
        </CardContent>
      </Card>
    )
  }

  return (
    <Card className="flex-1 flex flex-col">
      <CardHeader className="border-b">
        <div className="flex items-center justify-between">
          <CardTitle className="flex items-center gap-2">
            {currentSession.sessionName}
            <Badge variant={currentSession.isActive ? "default" : "secondary"}>
              {currentSession.isActive ? "Active" : "Closed"}
            </Badge>
          </CardTitle>
          <Badge variant={isConnected ? "default" : "destructive"}>{isConnected ? "Connected" : "Disconnected"}</Badge>
        </div>
      </CardHeader>

      <CardContent className="flex-1 flex flex-col p-0">
        <ScrollArea className="flex-1 p-4">
          <div className="space-y-4">
            {messages.map((message) => (
              <MessageBubble key={message.id} message={message} />
            ))}
            <div ref={messagesEndRef} />
          </div>
        </ScrollArea>

        {currentSession.isActive && (
          <div className="border-t p-4">
            <form onSubmit={handleSendMessage} className="flex gap-2">
              <Input
                value={newMessage}
                onChange={(e) => setNewMessage(e.target.value)}
                placeholder="Type your message..."
                disabled={!isConnected}
                className="flex-1"
              />
              <Button type="submit" disabled={!newMessage.trim() || !isConnected} size="icon">
                <Send className="h-4 w-4" />
              </Button>
            </form>
          </div>
        )}
      </CardContent>
    </Card>
  )
}

function MessageBubble({ message }: { message: ChatMessage }) {
  const { currentUser } = useChat()
  const isOwnMessage = message.userId === currentUser?.id

  return (
    <div className={`flex ${isOwnMessage ? "justify-end" : "justify-start"}`}>
      <div className={`max-w-[70%] rounded-lg p-3 ${isOwnMessage ? "bg-primary text-primary-foreground" : "bg-muted"}`}>
        <div className="flex items-center gap-2 mb-1">
          <User className="h-3 w-3" />
          <span className="text-xs font-medium">{message.userName}</span>
          <span className="text-xs opacity-70">{new Date(message.timestamp).toLocaleTimeString()}</span>
        </div>
        <p className="text-sm">{message.content}</p>
      </div>
    </div>
  )
}
