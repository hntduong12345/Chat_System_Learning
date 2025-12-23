"use client"

import type React from "react"
import { useState, useRef, useEffect } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Badge } from "@/components/ui/badge"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Send, X, Clock, User } from "lucide-react"
import { useChat } from "@/contexts/chat-context"
import { useAuth } from "@/contexts/auth-context"
import { formatDistanceToNow } from "date-fns"

export function ChatInterface() {
  const { currentSession, sendMessage, leaveSession, closeSession } = useChat()
  const { user } = useAuth()
  const [message, setMessage] = useState("")
  const messagesEndRef = useRef<HTMLDivElement>(null)

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" })
  }

  useEffect(() => {
    scrollToBottom()
  }, [currentSession?.messages])

  const handleSendMessage = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!message.trim()) return

    await sendMessage(message.trim())
    setMessage("")
  }

  const handleCloseSession = async () => {
    if (currentSession) {
      await closeSession(currentSession.id)
    }
  }

  if (!currentSession) {
    return (
      <div className="flex-1 flex items-center justify-center bg-gray-50">
        <div className="text-center">
          <User className="h-12 w-12 text-gray-400 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">No chat selected</h3>
          <p className="text-gray-500">Select a chat session to start messaging</p>
        </div>
      </div>
    )
  }

  const getStatusColor = (status: string) => {
    switch (status) {
      case "Waiting":
        return "bg-yellow-100 text-yellow-800"
      case "Active":
        return "bg-green-100 text-green-800"
      case "Closed":
        return "bg-gray-100 text-gray-800"
      default:
        return "bg-blue-100 text-blue-800"
    }
  }

  return (
    <div className="flex-1 flex flex-col">
      {/* Chat Header */}
      <div className="bg-white border-b px-6 py-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center space-x-4">
            <div>
              <h2 className="text-lg font-semibold">
                Chat with{" "}
                {user?.roleName === "Customer"
                  ? currentSession.admin?.fullName || "Support"
                  : currentSession.customer.fullName}
              </h2>
              <div className="flex items-center space-x-2 text-sm text-gray-500">
                <Badge className={getStatusColor(currentSession.status)}>{currentSession.status}</Badge>
                <span>•</span>
                <Clock className="h-4 w-4" />
                <span>Created {formatDistanceToNow(new Date(currentSession.createAt), { addSuffix: true })}</span>
              </div>
            </div>
          </div>
          <div className="flex items-center space-x-2">
            {currentSession.status !== "Closed" && (
              <Button variant="outline" size="sm" onClick={handleCloseSession}>
                <X className="h-4 w-4 mr-2" />
                Close Chat
              </Button>
            )}
            <Button variant="ghost" size="sm" onClick={leaveSession}>
              <X className="h-4 w-4" />
            </Button>
          </div>
        </div>
      </div>

      {/* Messages */}
      <ScrollArea className="flex-1 p-6">
        <div className="space-y-4">
          {currentSession.messages.map((msg) => {
            const isOwnMessage = msg.sender.id === user?.id
            return (
              <div key={msg.id} className={`flex ${isOwnMessage ? "justify-end" : "justify-start"}`}>
                <div
                  className={`max-w-xs lg:max-w-md px-4 py-2 rounded-lg ${
                    isOwnMessage ? "bg-blue-500 text-white" : "bg-gray-100 text-gray-900"
                  }`}
                >
                  <div className="text-sm font-medium mb-1">{msg.sender.fullName}</div>
                  <div className="text-sm">{msg.content}</div>
                  <div className={`text-xs mt-1 ${isOwnMessage ? "text-blue-100" : "text-gray-500"}`}>
                    {formatDistanceToNow(new Date(msg.sendAt), { addSuffix: true })}
                  </div>
                </div>
              </div>
            )
          })}
          <div ref={messagesEndRef} />
        </div>
      </ScrollArea>

      {/* Message Input */}
      {currentSession.status !== "Closed" && (
        <div className="bg-white border-t p-4">
          <form onSubmit={handleSendMessage} className="flex space-x-2">
            <Input
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              placeholder="Type your message..."
              className="flex-1"
            />
            <Button type="submit" disabled={!message.trim()}>
              <Send className="h-4 w-4" />
            </Button>
          </form>
        </div>
      )}
    </div>
  )
}
