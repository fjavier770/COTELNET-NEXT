import { Navigate } from 'react-router'

export default function ProtectedRoute({ session, children }) {
  return session ? children : <Navigate to="/login" replace />
}

