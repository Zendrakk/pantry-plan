import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from './useAuth'

function ProtectedRoute() {
  const auth = useAuth()

  // While we're still trying to silently refresh the access token on
  // initial app load, we don't yet know whether the user is really
  // logged in or not. Showing a loading state here avoids a flash of
  // redirecting to the login page for a split second before the silent
  // refresh finishes.
  if (auth.isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <p className="text-gray-600">Loading...</p>
      </div>
    )
  }

  if (auth.accessToken === null) {
    return <Navigate to="/login" replace />
  }

  // Outlet renders whichever child route actually matched.
  return <Outlet />
}

export default ProtectedRoute