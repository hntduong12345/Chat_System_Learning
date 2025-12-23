"use client"

import { useState, useEffect } from "react"
import { Input } from "@/components/ui/input"
import { Card, CardContent } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { MessageCircle, Clock, Users, Search, Star, Shield } from "lucide-react"
import { useChat } from "@/contexts/chat-context"
import { useAuth } from "@/contexts/auth-context"
import { apiService } from "@/lib/api-service"
import { formatDistanceToNow } from "date-fns"
import type { User as UserType } from "@/lib/types"

export function SessionSidebar() {
  const { sessions, waitingSessions, currentSession, createSession, joinSession, assignSession } = useChat()
  const { user } = useAuth()
  const [availableAdmins, setAvailableAdmins] = useState<UserType[]>([])
  const [isLoadingAdmins, setIsLoadingAdmins] = useState(false)
  const [searchQuery, setSearchQuery] = useState("")

  // Load available admins for customers
  useEffect(() => {
    if (user?.roleName === "Customer") {
      loadAvailableAdmins()
    }
  }, [user])

  const loadAvailableAdmins = async () => {
    try {
      setIsLoadingAdmins(true)
      const admins = await apiService.getAdmins()
      setAvailableAdmins(admins)
    } catch (error) {
      console.error("Failed to load admins:", error)
    } finally {
      setIsLoadingAdmins(false)
    }
  }

  const handleStartChatWithAdmin = async (admin: UserType) => {
    // Check if there's already an active session with this admin
    const existingSession = sessions.find((s) => s.admin?.id === admin.id && s.status !== "Closed")

    if (existingSession) {
      // Join existing session
      await joinSession(existingSession.id)
    } else {
      // Create new session with this admin
      try {
        await createSession({
          initialMessage: `Hi ${admin.fullName}, I need assistance.`,
          channelType: "Web",
        })
      } catch (error) {
        console.error("Failed to create session:", error)
      }
    }
  }

  const handleJoinWaitingSession = async (sessionId: string) => {
    // For admin: clicking on waiting session automatically assigns it to them and joins
    if (user?.roleName === "Admin") {
      await assignSession(sessionId, user.id)
    }
    await joinSession(sessionId)
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

  const getInitials = (name: string) => {
    return name
      .split(" ")
      .map((n) => n[0])
      .join("")
      .toUpperCase()
      .slice(0, 2)
  }

  // Filter admins based on search query
  const filteredAdmins = availableAdmins.filter(
    (admin) =>
      admin.fullName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      admin.email.toLowerCase().includes(searchQuery.toLowerCase()),
  )

  // For customers: Show available admins to chat with
  if (user?.roleName === "Customer") {
    return (
      <div className="w-80 bg-white border-r flex flex-col">
        {/* Header */}
        <div className="p-4 border-b">
          <h2 className="text-lg font-semibold mb-3">Support Agents</h2>

          {/* Search */}
          <div className="relative">
            <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
            <Input
              placeholder="Search agents..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-10"
            />
          </div>
        </div>

        {/* Available Admins */}
        <div className="flex-1 flex flex-col">
          <div className="p-4 pb-2">
            <div className="flex items-center space-x-2">
              <Users className="h-4 w-4 text-gray-500" />
              <h3 className="text-sm font-medium text-gray-700">Available Support</h3>
              <Badge variant="outline" className="text-xs">
                {filteredAdmins.length}
              </Badge>
            </div>
          </div>

          <ScrollArea className="flex-1 px-4">
            <div className="space-y-2 pb-4">
              {isLoadingAdmins ? (
                <div className="flex items-center justify-center py-8">
                  <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-primary"></div>
                </div>
              ) : (
                filteredAdmins.map((admin) => {
                  const existingSession = sessions.find((s) => s.admin?.id === admin.id && s.status !== "Closed")
                  const hasActiveChat = !!existingSession

                  return (
                    <Card
                      key={admin.id}
                      className={`cursor-pointer transition-colors ${
                        currentSession?.admin?.id === admin.id
                          ? "bg-blue-50 border-blue-200 shadow-sm"
                          : "hover:bg-gray-50"
                      }`}
                      onClick={() => handleStartChatWithAdmin(admin)}
                    >
                      <CardContent className="p-4">
                        <div className="flex items-center space-x-3">
                          <div className="relative">
                            <Avatar className="h-12 w-12">
                              <AvatarFallback className="bg-blue-100 text-blue-600 font-medium">
                                {getInitials(admin.fullName)}
                              </AvatarFallback>
                            </Avatar>
                            {/* Online indicator */}
                            <div className="absolute -bottom-1 -right-1 h-4 w-4 bg-green-500 border-2 border-white rounded-full"></div>
                          </div>

                          <div className="flex-1 min-w-0">
                            <div className="flex items-center space-x-2 mb-1">
                              <span className="font-medium text-sm truncate">{admin.fullName}</span>
                              <Shield className="h-3 w-3 text-blue-500" />
                            </div>

                            <div className="flex items-center space-x-2 mb-2">
                              <Badge variant="outline" className="text-xs">
                                Support Agent
                              </Badge>
                              {hasActiveChat && (
                                <Badge className="text-xs bg-green-100 text-green-800">Active Chat</Badge>
                              )}
                            </div>

                            <div className="flex items-center space-x-1 text-xs text-gray-500">
                              <Star className="h-3 w-3 fill-yellow-400 text-yellow-400" />
                              <span>4.9</span>
                              <span>•</span>
                              <span>Online</span>
                            </div>

                            {existingSession?.lastMessage && (
                              <p className="text-xs text-gray-600 truncate mt-2">
                                <span className="font-medium">
                                  {existingSession.lastMessage.sender.id === user?.id ? "You" : "Agent"}:
                                </span>{" "}
                                {existingSession.lastMessage.content}
                              </p>
                            )}
                          </div>

                          <div className="text-right">
                            {existingSession?.unreadCount && existingSession.unreadCount > 0 && (
                              <Badge
                                variant="destructive"
                                className="text-xs h-5 w-5 p-0 flex items-center justify-center mb-1"
                              >
                                {existingSession.unreadCount}
                              </Badge>
                            )}
                            {existingSession && (
                              <div className="text-xs text-gray-500">
                                {formatDistanceToNow(new Date(existingSession.lastActiveAt), { addSuffix: true })}
                              </div>
                            )}
                          </div>
                        </div>
                      </CardContent>
                    </Card>
                  )
                })
              )}

              {!isLoadingAdmins && filteredAdmins.length === 0 && (
                <div className="text-center py-12">
                  <Users className="h-12 w-12 text-gray-400 mx-auto mb-4" />
                  <h3 className="text-sm font-medium text-gray-900 mb-2">
                    {searchQuery ? "No agents found" : "No support agents available"}
                  </h3>
                  <p className="text-xs text-gray-500">
                    {searchQuery ? "Try adjusting your search" : "Please check back later"}
                  </p>
                </div>
              )}
            </div>
          </ScrollArea>
        </div>

        {/* Recent Chats Section */}
        {sessions.length > 0 && (
          <div className="border-t">
            <div className="p-4 pb-2">
              <h3 className="text-sm font-medium text-gray-700">Recent Conversations</h3>
            </div>
            <ScrollArea className="max-h-32 px-4">
              <div className="space-y-1 pb-4">
                {sessions.slice(0, 3).map((session) => (
                  <div
                    key={session.id}
                    className="flex items-center space-x-2 p-2 rounded hover:bg-gray-50 cursor-pointer text-xs"
                    onClick={() => joinSession(session.id)}
                  >
                    <Avatar className="h-6 w-6">
                      <AvatarFallback className="bg-gray-100 text-gray-600 text-xs">
                        {getInitials(session.admin?.fullName || "Support")}
                      </AvatarFallback>
                    </Avatar>
                    <div className="flex-1 min-w-0">
                      <div className="truncate font-medium">{session.admin?.fullName || "Support"}</div>
                      <div className="text-gray-500 truncate">{session.lastMessage?.content || "No messages yet"}</div>
                    </div>
                  </div>
                ))}
              </div>
            </ScrollArea>
          </div>
        )}
      </div>
    )
  }

  // For admins: Show waiting sessions and active chats
  return (
    <div className="w-80 bg-white border-r flex flex-col">
      {/* Header */}
      <div className="p-4 border-b">
        <h2 className="text-lg font-semibold">Chat Management</h2>
      </div>

      {/* Waiting Sessions (Admin only) */}
      {waitingSessions.length > 0 && (
        <div className="p-4 border-b bg-yellow-50">
          <div className="flex items-center space-x-2 mb-3">
            <div className="h-2 w-2 bg-yellow-500 rounded-full animate-pulse"></div>
            <h3 className="text-sm font-medium text-yellow-800">New Customer Requests</h3>
            <Badge variant="secondary" className="text-xs">
              {waitingSessions.length}
            </Badge>
          </div>
          <ScrollArea className="max-h-48">
            <div className="space-y-2">
              {waitingSessions.map((session) => (
                <Card
                  key={session.id}
                  className="cursor-pointer hover:bg-yellow-100 transition-colors border-yellow-200"
                  onClick={() => handleJoinWaitingSession(session.id)}
                >
                  <CardContent className="p-3">
                    <div className="flex items-center space-x-3">
                      <Avatar className="h-8 w-8">
                        <AvatarFallback className="bg-yellow-200 text-yellow-800 text-xs">
                          {getInitials(session.customer.fullName)}
                        </AvatarFallback>
                      </Avatar>
                      <div className="flex-1 min-w-0">
                        <div className="font-medium text-sm truncate">{session.customer.fullName}</div>
                        <div className="flex items-center space-x-1 text-xs text-gray-500">
                          <Clock className="h-3 w-3" />
                          <span>{formatDistanceToNow(new Date(session.createAt), { addSuffix: true })}</span>
                        </div>
                        {session.lastMessage && (
                          <p className="text-xs text-gray-600 truncate mt-1">{session.lastMessage.content}</p>
                        )}
                      </div>
                      <div className="text-xs text-yellow-600 font-medium">Click to Accept</div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          </ScrollArea>
        </div>
      )}

      {/* Active Sessions */}
      <div className="flex-1 flex flex-col">
        <div className="p-4 pb-2">
          <h3 className="text-sm font-medium text-gray-700">Active Chats</h3>
        </div>
        <ScrollArea className="flex-1 px-4">
          <div className="space-y-2 pb-4">
            {sessions.map((session) => (
              <Card
                key={session.id}
                className={`cursor-pointer transition-colors ${
                  currentSession?.id === session.id ? "bg-blue-50 border-blue-200 shadow-sm" : "hover:bg-gray-50"
                }`}
                onClick={() => joinSession(session.id)}
              >
                <CardContent className="p-4">
                  <div className="flex items-start space-x-3">
                    <Avatar className="h-10 w-10">
                      <AvatarFallback className="bg-gray-100 text-gray-600">
                        {getInitials(session.customer.fullName)}
                      </AvatarFallback>
                    </Avatar>
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center justify-between mb-1">
                        <span className="text-sm font-medium truncate">{session.customer.fullName}</span>
                        {session.unreadCount > 0 && (
                          <Badge variant="destructive" className="text-xs h-5 w-5 p-0 flex items-center justify-center">
                            {session.unreadCount}
                          </Badge>
                        )}
                      </div>

                      <div className="flex items-center space-x-2 mb-2">
                        <Badge className={`${getStatusColor(session.status)} text-xs`}>{session.status}</Badge>
                        <div className="flex items-center space-x-1 text-xs text-gray-500">
                          <Clock className="h-3 w-3" />
                          <span>{formatDistanceToNow(new Date(session.lastActiveAt), { addSuffix: true })}</span>
                        </div>
                      </div>

                      {session.lastMessage && (
                        <p className="text-xs text-gray-600 truncate">
                          <span className="font-medium">
                            {session.lastMessage.sender.id === user?.id ? "You" : session.lastMessage.sender.fullName}:
                          </span>{" "}
                          {session.lastMessage.content}
                        </p>
                      )}
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}

            {sessions.length === 0 && (
              <div className="text-center py-12">
                <MessageCircle className="h-12 w-12 text-gray-400 mx-auto mb-4" />
                <h3 className="text-sm font-medium text-gray-900 mb-2">No active chats</h3>
                <p className="text-xs text-gray-500">Waiting sessions will appear above when customers need help</p>
              </div>
            )}
          </div>
        </ScrollArea>
      </div>
    </div>
  )
}
