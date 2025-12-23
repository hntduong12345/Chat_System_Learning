import { useState } from "react";
import LoginPage from "@/src/pages/LoginPage";
import ChatPage from "@/src/pages/ChatPage";
import ProfilePage from "@/src/pages/ProfilePage";
import SettingsPage from "@/src/pages/SettingsPage";
import "./App.css";

function App() {
  const [currentPage, setCurrentPage] = useState("login");
  const [isDarkMode, setIsDarkMode] = useState(false);
  const [currentUser, setCurrentUser] = useState(null);

  const handleLogin = (email) => {
    setCurrentUser({
      email,
      name: "12131",
      avatar:
        "https://hebbkx1anhila5yf.public.blob.vercel-storage.com/image-RtuIyR3MZmEGNotmIS0dv2iHAHRfLE.png",
    });
    setCurrentPage("chat");
  };

  const handleLogout = () => {
    setCurrentUser(null);
    setCurrentPage("login");
  };

  const toggleDarkMode = () => {
    setIsDarkMode(!isDarkMode);
  };

  return (
    <div className={`app ${isDarkMode ? "dark-mode" : "light-mode"}`}>
      {currentPage === "login" && <LoginPage onLogin={handleLogin} />}
      {currentPage === "chat" && (
        <ChatPage
          onNavigate={setCurrentPage}
          isDarkMode={isDarkMode}
          toggleDarkMode={toggleDarkMode}
          currentUser={currentUser}
          onLogout={handleLogout}
        />
      )}
      {currentPage === "profile" && (
        <ProfilePage
          onNavigate={setCurrentPage}
          isDarkMode={isDarkMode}
          currentUser={currentUser}
        />
      )}
      {currentPage === "settings" && (
        <SettingsPage
          onNavigate={setCurrentPage}
          isDarkMode={isDarkMode}
          toggleDarkMode={toggleDarkMode}
        />
      )}
    </div>
  );
}

export default App;
