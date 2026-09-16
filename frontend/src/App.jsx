import { useState } from 'react'
import { Navigate, Route, Routes, useNavigate } from 'react-router'
import ProtectedRoute from './components/ProtectedRoute.jsx'
import AppShell from './components/AppShell.jsx'
import CatalogPage from './pages/CatalogPage.jsx'
import DashboardPage from './pages/DashboardPage.jsx'
import LoginPage from './pages/LoginPage.jsx'
import RolesPage from './pages/RolesPage.jsx'
import UsersPage from './pages/UsersPage.jsx'
import SalesPage from './pages/SalesPage.jsx'
import CashPage from './pages/CashPage.jsx'
import ServiceMaintenancePage from './pages/ServiceMaintenancePage.jsx'

const storageKey = 'cotelnet.session'

function WithPermission({ session, permission, children }) {
  return (session?.permissions ?? []).includes(permission) ? children : <Navigate to="/" replace />
}

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
      <Route path="/" element={<ProtectedRoute session={session}><AppShell session={session} onLogout={logout} /></ProtectedRoute>}>
        <Route index element={<DashboardPage session={session} />} />
        <Route path="ventas" element={<WithPermission session={session} permission="ventas.access"><SalesPage session={session} /></WithPermission>} />
        <Route path="caja" element={<WithPermission session={session} permission="caja.access"><CashPage session={session} /></WithPermission>} />
        <Route path="administracion/usuarios" element={<WithPermission session={session} permission="users.manage"><UsersPage session={session} /></WithPermission>} />
        <Route path="administracion/roles" element={<WithPermission session={session} permission="roles.manage"><RolesPage session={session} /></WithPermission>} />
        <Route path="administracion/estafetas" element={<WithPermission session={session} permission="estafetas.manage"><CatalogPage kind="estafetas" session={session} /></WithPermission>} />
        <Route path="administracion/terminales" element={<WithPermission session={session} permission="terminals.manage"><CatalogPage kind="terminals" session={session} /></WithPermission>} />
        <Route path="administracion/servicios" element={<WithPermission session={session} permission="services.manage"><ServiceMaintenancePage session={session} /></WithPermission>} />
      </Route>
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
