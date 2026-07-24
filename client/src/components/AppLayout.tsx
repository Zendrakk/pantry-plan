import { Outlet, Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'

function AppLayout() {
  const auth = useAuth()
  const navigate = useNavigate()

  async function handleLogoutClick() {
    await auth.logout()
    navigate('/login')
  }

  return (
    <div className="min-h-screen bg-gray-50">
      <header className="bg-white border-b border-gray-200">
        <div className="max-w-4xl mx-auto px-4 py-3 flex items-center justify-between">
          <Link to="/recipes" className="text-lg font-bold text-gray-900">
            Pantry &amp; Plan
          </Link>

          <nav className="flex items-center gap-4">
            <Link to="/recipes" className="text-sm text-gray-700 hover:text-blue-600">
              Recipes
            </Link>
            <Link to="/meal-plans" className="text-sm text-gray-700 hover:text-blue-600">
              Meal Plans
            </Link>
            <button
              onClick={handleLogoutClick}
              className="text-sm text-gray-700 hover:text-blue-600"
            >
              Log Out
            </button>
          </nav>
        </div>
      </header>

      <main className="max-w-4xl mx-auto px-4 py-6">
        <Outlet />
      </main>
    </div>
  )
}

export default AppLayout