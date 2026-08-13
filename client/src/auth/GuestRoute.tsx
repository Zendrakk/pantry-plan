import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from './useAuth'

function GuestRoute() {
  const auth = useAuth()

  // Same reasoning as ProtectedRoute: don't make a decision until we
  // actually know whether the silent refresh succeeded or not.
  if (auth.isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <p className="text-gray-600">Loading...</p>
      </div>
    )
  }

  // Only show the guest content if the user is not authenticated
  if (auth.accessToken !== null) {
    return <Navigate to="/recipes" replace />
  }

  return <Outlet />
}

export default GuestRoute