"use client";
import "@/src/styles/ChatDetailsPanel.css";

function ChatDetailsPanel({
  chat,
  onClose,
  activeTab,
  onTabChange,
  isDarkMode,
}) {
  return (
    <div className="chat-details-overlay">
      <div className="chat-details-panel">
        <div className="details-header">
          <h2>Chat Details</h2>
          <button className="close-btn" onClick={onClose}>
            ✕
          </button>
        </div>

        <div className="details-tabs">
          <button
            className={`details-tab ${activeTab === "info" ? "active" : ""}`}
            onClick={() => onTabChange("info")}
          >
            ℹ️ Info
          </button>
          <button
            className={`details-tab ${activeTab === "media" ? "active" : ""}`}
            onClick={() => onTabChange("media")}
          >
            📷 Media
          </button>
          <button
            className={`details-tab ${
              activeTab === "settings" ? "active" : ""
            }`}
            onClick={() => onTabChange("settings")}
          >
            ⚙️ Settings
          </button>
        </div>

        {activeTab === "info" && (
          <div className="details-content">
            <div className="info-section">
              <img
                src={chat.avatar || "/placeholder.svg"}
                alt={chat.name}
                className="info-avatar"
              />
              <h3>{chat.name}</h3>
              <p className="info-status">{chat.status}</p>
            </div>

            <div className="info-item">
              <span className="info-label">Email</span>
              <span className="info-value">{chat.email}</span>
            </div>

            <div className="info-item">
              <span className="info-label">Phone</span>
              <span className="info-value">{chat.phone}</span>
            </div>
          </div>
        )}

        {activeTab === "media" && (
          <div className="details-content">
            <p className="empty-message">No media shared yet</p>
          </div>
        )}

        {activeTab === "settings" && (
          <div className="details-content">
            <div className="settings-option">
              <span>🔕 Mute Notifications</span>
            </div>
            <div className="settings-option">
              <span>🔍 Search in Chat</span>
            </div>
            <div className="settings-option delete">
              <span>🗑️ Delete Chat</span>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default ChatDetailsPanel;
