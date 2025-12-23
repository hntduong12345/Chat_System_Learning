"use client";
import "@/src/styles/Sidebar.css";

function Sidebar({
  chats,
  selectedChat,
  onSelectChat,
  onNavigate,
  isDarkMode,
  toggleDarkMode,
  currentUser,
  onLogout,
}) {
  return (
    <div className="sidebar">
      <div className="sidebar-header">
        <h1 className="sidebar-title">ChatFlow</h1>
        <button className="icon-btn" onClick={toggleDarkMode}>
          🌙
        </button>
      </div>

      <div className="chats-list">
        {chats.map((chat) => (
          <div
            key={chat.id}
            className={`chat-item ${
              selectedChat?.id === chat.id ? "active" : ""
            }`}
            onClick={() => onSelectChat(chat)}
          >
            <img
              src={chat.avatar || "/placeholder.svg"}
              alt={chat.name}
              className="chat-avatar"
            />
            <div className="chat-info">
              <h3 className="chat-name">{chat.name}</h3>
              <p className="chat-preview">{chat.lastMessage}</p>
            </div>
            <div className="chat-meta">
              <span className="chat-time">{chat.timestamp}</span>
              {chat.unread > 0 && (
                <span className="unread-badge">{chat.unread}</span>
              )}
            </div>
          </div>
        ))}
      </div>

      <div className="sidebar-footer">
        <img
          src={currentUser?.avatar || "/placeholder.svg"}
          alt="User"
          className="user-avatar"
        />
        <div className="user-info">
          <p className="user-name">{currentUser?.name}</p>
          <p className="user-status">Online</p>
        </div>
        <button className="arrow-btn" onClick={() => onNavigate("profile")}>
          →
        </button>
      </div>
    </div>
  );
}

export default Sidebar;
