"use client"
import { ChatProvider, useChat } from "@/contexts/chat-context"
import { UserLogin } from "@/components/user-login"
import { SessionSidebar } from "@/components/session-sidebar"
import { ChatInterface } from "@/components/chat-interface"
import { Alert, AlertDescription } from "@/components/ui/alert"
import { AlertCircle } from "lucide-react"

function ChatApp() {
  const { currentUser, error, isConnected } = useChat()

  if (!currentUser) {
    return <UserLogin />
  }

  return (
    <div className="h-screen flex flex-col bg-gray-50">
      {error && (
        <Alert variant="destructive" className="m-4">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      <div className="flex-1 flex gap-4 p-4 min-h-0">
        <SessionSidebar />
        <ChatInterface />
      </div>

      <div className="border-t bg-white px-4 py-2">
        <div className="flex items-center justify-between text-sm text-muted-foreground">
          <span>
            Logged in as: {currentUser.name} ({currentUser.id})
          </span>
          <span className={`flex items-center gap-1 ${isConnected ? "text-green-600" : "text-red-600"}`}>
            <div className={`w-2 h-2 rounded-full ${isConnected ? "bg-green-600" : "bg-red-600"}`} />
            {isConnected ? "Connected" : "Disconnected"}
          </span>
        </div>
      </div>
    </div>
  )
}

export default function Home() {
  return (
    <ChatProvider>
      <ChatApp />
    </ChatProvider>
  )
}
