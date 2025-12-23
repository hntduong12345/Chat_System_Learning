"use client";

import { useState } from "react";
import Sidebar from "@/src/components/Sidebar";
import ChatPanel from "@/src/components/ChatPanel";
import ChatDetailsPanel from "@/src/components/ChatDetailsPanel";
import "@/src/styles/ChatPage.css";

function ChatPage({
  onNavigate,
  isDarkMode,
  toggleDarkMode,
  currentUser,
  onLogout,
}) {
  const [selectedChat, setSelectedChat] = useState({
    id: 1,
    name: "Sarah Wilson",
    avatar:
      "https://hebbkx1anhila5yf.public.blob.vercel-storage.com/image-RtuIyR3MZmEGNotmIS0dv2iHAHRfLE.png",
    status: "Active now",
    email: "sarah@example.com",
    phone: "+1 234 567 8900",
  });
  const [showChatDetails, setShowChatDetails] = useState(false);
  const [detailsTab, setDetailsTab] = useState("info");

  const chats = [
    {
      id: 1,
      name: "Sarah Wilson",
      avatar:
        "https://hebbkx1anhila5yf.public.blob.vercel-storage.com/image-RtuIyR3MZmEGNotmIS0dv2iHAHRfLE.png",
      lastMessage: "Hey! How are you doing?",
      timestamp: "2m",
      unread: 2,
      status: "Active now",
      email: "sarah@example.com",
      phone: "+1 234 567 8900",
    },
    {
      id: 2,
      name: "Team Project",
      avatar:
        "https://hebbkx1anhila5yf.public.blob.vercel-storage.com/image-RtuIyR3MZmEGNotmIS0dv2iHAHRfLE.png",
      lastMessage: "Alice: The presentation looks great!",
      timestamp: "1h",
      unread: 0,
      status: "Group",
      email: "team@example.com",
      phone: "+1 234 567 8901",
    },
    {
      id: 3,
      name: "Michael Rodriguez",
      avatar:
        "https://hebbkx1anhila5yf.public.blob.vercel-storage.com/image-RtuIyR3MZmEGNotmIS0dv2iHAHRfLE.png",
      lastMessage: "Sounds good to me!",
      timestamp: "3h",
      unread: 0,
      status: "Online",
      email: "michael@example.com",
      phone: "+1 234 567 8902",
    },
  ];

  const messages = [
    {
      id: 1,
      sender: "Sarah Wilson",
      text: "Hey there! How has your day been?",
      timestamp: "10:30 AM",
      isOwn: false,
      reactions: ["👍"],
    },
    {
      id: 2,
      sender: "You",
      text: "Pretty good! Just working on some projects. How about you?",
      timestamp: "10:32 AM",
      isOwn: true,
      reactions: [],
    },
    {
      id: 3,
      sender: "Sarah Wilson",
      text: "Same here! Want to grab coffee later?",
      timestamp: "10:35 AM",
      isOwn: false,
      reactions: [],
    },
    {
      id: 4,
      sender: "Sarah Wilson",
      text: "Hey! How are you doing?",
      timestamp: "2m",
      isOwn: false,
      reactions: [],
    },
  ];

  const handleLogout = () => {
    onLogout();
  };

  return (
    <div className="chat-page">
      <Sidebar
        chats={chats}
        selectedChat={selectedChat}
        onSelectChat={setSelectedChat}
        onNavigate={onNavigate}
        isDarkMode={isDarkMode}
        toggleDarkMode={toggleDarkMode}
        currentUser={currentUser}
        onLogout={handleLogout}
      />
      <ChatPanel
        selectedChat={selectedChat}
        messages={messages}
        onShowDetails={() => setShowChatDetails(true)}
        isDarkMode={isDarkMode}
      />
      {showChatDetails && (
        <ChatDetailsPanel
          chat={selectedChat}
          onClose={() => setShowChatDetails(false)}
          activeTab={detailsTab}
          onTabChange={setDetailsTab}
          isDarkMode={isDarkMode}
        />
      )}
    </div>
  );
}

export default ChatPage;
