"use client"

import type React from "react"

import { useState, useEffect } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog"
import { Plus, MessageCircle, X, RotateCcw } from "lucide-react"
import { useChat } from "@/contexts/chat-context"
import type { ChatSession } from "@/lib/signalr-service"

export function SessionSidebar() {
  const {
    sessions,
    currentSession,
    createSession,
    joinSession,
    closeSession,
    reactivateSession,
    loadUserSessions,
    isLoading,
  } = useChat()

  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false)
  const [newSessionName, setNewSessionName] = useState("")

  useEffect(() => {
    loadUserSessions()
  }, [])

  const handleCreateSession = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!newSessionName.trim()) return

    console.log("Creating session:", newSessionName.trim())

    try {
      await createSession(newSessionName.trim())
      setNewSessionName("")
      setIsCreateDialogOpen(false)
      console.log("Session created successfully")
    } catch (error) {
      console.error("Failed to create session:", error)
    }
  }

  const handleJoinSession = async (sessionId: number) => {
    console.log("Joining session:", sessionId)
    try {
      await joinSession(sessionId)
      console.log("Joined session successfully")
    } catch (error) {
      console.error("Failed to join session:", error)
    }
  }

  const handleCloseSession = async (sessionId: number, e: React.MouseEvent) => {
    e.stopPropagation()
    await closeSession(sessionId)
  }

  const handleReactivateSession = async (sessionId: number, e: React.MouseEvent) => {
    e.stopPropagation()
    await reactivateSession(sessionId)
  }

  return (
    <Card className="w-80 flex flex-col">
      <CardHeader className="border-b">
        <div className="flex items-center justify-between">
          <CardTitle className="flex items-center gap-2">
            <MessageCircle className="h-5 w-5" />
            Chat Sessions
          </CardTitle>
          <Dialog open={isCreateDialogOpen} onOpenChange={setIsCreateDialogOpen}>
            <DialogTrigger asChild>
              <Button size="sm" variant="outline">
                <Plus className="h-4 w-4" />
              </Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Create New Session</DialogTitle>
              </DialogHeader>
              <form onSubmit={handleCreateSession} className="space-y-4">
                <Input
                  value={newSessionName}
                  onChange={(e) => setNewSessionName(e.target.value)}
                  placeholder="Enter session name..."
                  autoFocus
                />
                <div className="flex justify-end gap-2">
                  <Button type="button" variant="outline" onClick={() => setIsCreateDialogOpen(false)}>
                    Cancel
                  </Button>
                  <Button type="submit" disabled={!newSessionName.trim() || isLoading}>
                    Create
                  </Button>
                </div>
              </form>
            </DialogContent>
          </Dialog>
        </div>
      </CardHeader>

      <CardContent className="flex-1 p-0">
        <ScrollArea className="h-full">
          <div className="p-4 space-y-2">
            {sessions.length === 0 ? (
              <div className="text-center text-muted-foreground py-8">
                <MessageCircle className="h-8 w-8 mx-auto mb-2 opacity-50" />
                <p className="text-sm">No sessions yet</p>
                <p className="text-xs">Create your first session to get started</p>
              </div>
            ) : (
              sessions.map((session) => (
                <SessionItem
                  key={session.id}
                  session={session}
                  isActive={currentSession?.id === session.id}
                  onJoin={() => handleJoinSession(session.id)}
                  onClose={(e) => handleCloseSession(session.id, e)}
                  onReactivate={(e) => handleReactivateSession(session.id, e)}
                />
              ))
            )}
          </div>
        </ScrollArea>
      </CardContent>
    </Card>
  )
}

interface SessionItemProps {
  session: ChatSession
  isActive: boolean
  onJoin: () => void
  onClose: (e: React.MouseEvent) => void
  onReactivate: (e: React.MouseEvent) => void
}

function SessionItem({ session, isActive, onJoin, onClose, onReactivate }: SessionItemProps) {
  return (
    <div
      className={`p-3 rounded-lg border cursor-pointer transition-colors ${
        isActive ? "bg-primary/10 border-primary" : "hover:bg-muted border-border"
      }`}
      onClick={onJoin}
    >
      <div className="flex items-start justify-between">
        <div className="flex-1 min-w-0">
          <h4 className="font-medium truncate">{session.sessionName}</h4>
          <p className="text-xs text-muted-foreground">Created {new Date(session.createdAt).toLocaleDateString()}</p>
          {session.closedAt && (
            <p className="text-xs text-muted-foreground">Closed {new Date(session.closedAt).toLocaleDateString()}</p>
          )}
        </div>
        <div className="flex items-center gap-1 ml-2">
          <Badge variant={session.isActive ? "default" : "secondary"} className="text-xs">
            {session.isActive ? "Active" : "Closed"}
          </Badge>
          {session.isActive ? (
            <Button
              size="sm"
              variant="ghost"
              onClick={onClose}
              className="h-6 w-6 p-0 hover:bg-destructive hover:text-destructive-foreground"
            >
              <X className="h-3 w-3" />
            </Button>
          ) : (
            <Button
              size="sm"
              variant="ghost"
              onClick={onReactivate}
              className="h-6 w-6 p-0 hover:bg-green-100 hover:text-green-700"
            >
              <RotateCcw className="h-3 w-3" />
            </Button>
          )}
        </div>
      </div>
    </div>
  )
}
