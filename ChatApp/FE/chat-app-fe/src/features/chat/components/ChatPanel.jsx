"use client";

import { useState } from "react";
import "@/src/styles/ChatPanel.css";

function ChatPanel({ selectedChat, messages, onShowDetails, isDarkMode }) {
  const [messageText, setMessageText] = useState("");

  const handleSendMessage = () => {
    if (messageText.trim()) {
      console.log("Message sent:", messageText);
      setMessageText("");
    }
  };

  return (
    <div className="chat-panel">
      <div className="chat-header">
        <div className="chat-header-info">
          <img
            src={selectedChat.avatar || "/placeholder.svg"}
            alt={selectedChat.name}
            className="header-avatar"
          />
          <div>
            <h2 className="chat-header-name">{selectedChat.name}</h2>
            <p className="chat-header-status">{selectedChat.status}</p>
          </div>
        </div>
        <div className="chat-header-actions">
          <button className="icon-btn">🔍</button>
          <button className="icon-btn" onClick={onShowDetails}>
            ℹ️
          </button>
        </div>
      </div>

      <div className="messages-container">
        {messages.map((msg) => (
          <div
            key={msg.id}
            className={`message ${msg.isOwn ? "own" : "other"}`}
          >
            <div className="message-bubble">
              <p className="message-text">{msg.text}</p>
              <span className="message-time">{msg.timestamp}</span>
            </div>
            {msg.reactions.length > 0 && (
              <div className="message-reactions">
                {msg.reactions.map((reaction, idx) => (
                  <span key={idx} className="reaction">
                    {reaction}
                  </span>
                ))}
              </div>
            )}
          </div>
        ))}
      </div>

      <div className="message-input-area">
        <button className="input-icon-btn">📎</button>
        <input
          type="text"
          placeholder="Type a message..."
          value={messageText}
          onChange={(e) => setMessageText(e.target.value)}
          onKeyPress={(e) => e.key === "Enter" && handleSendMessage()}
          className="message-input"
        />
        <button className="input-icon-btn">😊</button>
        <button className="send-btn" onClick={handleSendMessage}>
          ✈️
        </button>
      </div>
    </div>
  );
}

export default ChatPanel;
