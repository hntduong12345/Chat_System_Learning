"use client";

import { useState } from "react";
import "@/src/styles/LoginPage.css";

function LoginPage({ onLogin, onNavigateToSignUp }) {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const handleSignIn = (e) => {
    e.preventDefault();
    if (email && password) {
      onLogin(email);
    }
  };

  return (
    <div className="login-container">
      <div className="login-card">
        <h1 className="login-title">ChatFlow</h1>
        <p className="login-subtitle">Welcome back</p>

        <form onSubmit={handleSignIn}>
          <div className="form-group">
            <label htmlFor="email">Email</label>
            <input
              id="email"
              type="email"
              placeholder="Enter your email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="form-input"
            />
          </div>

          <div className="form-group">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              type="password"
              placeholder="Enter your password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="form-input"
            />
          </div>

          <button type="submit" className="btn-signin">
            Sign In
          </button>
        </form>

        <p className="login-footer">
          Don't have an account?{" "}
          <button onClick={onNavigateToSignUp} className="signup-link">
            Sign up
          </button>
        </p>
      </div>
    </div>
  );
}

export default LoginPage;
