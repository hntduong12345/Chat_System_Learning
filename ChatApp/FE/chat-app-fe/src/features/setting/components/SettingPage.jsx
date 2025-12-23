"use client";

import { useState } from "react";
import "@/src/styles/SettingsPage.css";

function SettingsPage({ onNavigate, isDarkMode, toggleDarkMode }) {
  const [onlineStatus, setOnlineStatus] = useState(true);
  const [readReceipts, setReadReceipts] = useState(true);

  return (
    <div className="settings-page">
      <div className="settings-header">
        <button className="back-btn" onClick={() => onNavigate("chat")}>
          ←
        </button>
        <h1 className="settings-title">ChatFlow</h1>
        <div className="header-icons">
          <button className="icon-btn">🌙</button>
          <button className="icon-btn">↗</button>
        </div>
      </div>

      <div className="settings-tabs">
        <button className="tab-btn">👤 Profile</button>
        <button className="tab-btn active">⚙️ Settings</button>
      </div>

      <div className="settings-content">
        <div className="settings-section">
          <h2>Appearance</h2>
          <div className="setting-item">
            <div className="setting-info">
              <h3>Dark Mode</h3>
              <p>Switch between light and dark themes</p>
            </div>
            <label className="toggle-switch">
              <input
                type="checkbox"
                checked={isDarkMode}
                onChange={toggleDarkMode}
              />
              <span className="toggle-slider"></span>
            </label>
          </div>
        </div>

        <div className="settings-section">
          <h2>Privacy</h2>
          <div className="setting-item">
            <div className="setting-info">
              <h3>Online Status</h3>
              <p>Show when you're online</p>
            </div>
            <label className="checkbox">
              <input
                type="checkbox"
                checked={onlineStatus}
                onChange={(e) => setOnlineStatus(e.target.checked)}
              />
              <span className="checkmark"></span>
            </label>
          </div>

          <div className="setting-item">
            <div className="setting-info">
              <h3>Read Receipts</h3>
              <p>Let others see when you've read messages</p>
            </div>
            <label className="checkbox">
              <input
                type="checkbox"
                checked={readReceipts}
                onChange={(e) => setReadReceipts(e.target.checked)}
              />
              <span className="checkmark"></span>
            </label>
          </div>
        </div>
      </div>
    </div>
  );
}

export default SettingsPage;
