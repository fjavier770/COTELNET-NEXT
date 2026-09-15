import { useState } from 'react'
import { Navigate, Route, Routes, useNavigate } from 'react-router'
import ProtectedRoute from './components/ProtectedRoute.jsx'
import DashboardPage from './pages/DashboardPage.jsx'
import LoginPage from './pages/LoginPage.jsx'

const storageKey = 'cotelnet.session'

function loadSession() {
  try {
    return JSON.parse(sessionStorage.getItem(storageKey))
  } catch {
    return null
  }
}

export default function App() {
  const [session, setSession] = useState(loadSession)
  const navigate = useNavigate()

  function authenticate(value) {
    sessionStorage.setItem(storageKey, JSON.stringify(value))
    setSession(value)
    navigate('/')
  }

  function logout() {
    sessionStorage.removeItem(storageKey)
    setSession(null)
    navigate('/login')
  }

  return (
    <Routes>
      <Route path="/login" element={session ? <Navigate to="/" replace /> : <LoginPage onAuthenticated={authenticate} />} />
      <Route path="/" element={<ProtectedRoute session={session}><DashboardPage session={session} onLogout={logout} /></ProtectedRoute>} />
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

