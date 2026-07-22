import { useAuth } from './auth/useAuth'

function App() {
  const auth = useAuth()

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center">
      <div className="text-center">
        <h1 className="text-3xl font-bold text-gray-900">Pantry & Plan</h1>
        <p className="mt-4 text-gray-600">
          Loading: {auth.isLoading ? 'true' : 'false'}
        </p>
        <p className="text-gray-600">
          Access token: {auth.accessToken ? 'present' : 'none'}
        </p>
      </div>
    </div>
  )
}

export default App
