"use client";

import { useState } from "react";
import "@/src/styles/ProfilePage.css";

function ProfilePage({ onNavigate, isDarkMode, currentUser }) {
  const [name, setName] = useState(currentUser?.name || "12131");
  const [email, setEmail] = useState(currentUser?.email || "12@21");
  const [activeTab, setActiveTab] = useState("profile");

  const handleSaveChanges = () => {
    alert("Changes saved!");
  };

  return (
    <div className="profile-page">
      <div className="profile-header">
        <button className="back-btn" onClick={() => onNavigate("chat")}>
          ←
        </button>
        <h1 className="profile-title">ChatFlow</h1>
        <div className="header-icons">
          <button className="icon-btn">🌙</button>
          <button className="icon-btn">↗</button>
        </div>
      </div>

      <div className="profile-tabs">
        <button
          className={`tab-btn ${activeTab === "profile" ? "active" : ""}`}
          onClick={() => setActiveTab("profile")}
        >
          👤 Profile
        </button>
        <button
          className={`tab-btn ${activeTab === "settings" ? "active" : ""}`}
          onClick={() => setActiveTab("settings")}
        >
          ⚙️ Settings
        </button>
      </div>

      {activeTab === "profile" && (
        <div className="profile-content">
          <div className="profile-section">
            <h2>Profile Information</h2>

            <div className="avatar-section">
              <img
                src="https://hebbkx1anhila5yf.public.blob.vercel-storage.com/image-RtuIyR3MZmEGNotmIS0dv2iHAHRfLE.png"
                alt="Avatar"
                className="avatar-large"
              />
              <button className="btn-change-avatar">Change Avatar</button>
            </div>

            <div className="form-group">
              <label>Name</label>
              <input
                type="text"
                value={name}
                onChange={(e) => setName(e.target.value)}
                className="form-input"
              />
            </div>

            <div className="form-group">
              <label>Email</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                className="form-input"
              />
            </div>

            <button className="btn-save" onClick={handleSaveChanges}>
              Save Changes
            </button>
          </div>
        </div>
      )}
    </div>
  );
}

export default ProfilePage;
