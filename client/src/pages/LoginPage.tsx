import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { ApiError } from '../api/client'

function LoginPage() {
  const auth = useAuth()
  const navigate = useNavigate()

  // These two pieces of state hold what the user has typed into each field.
  const [email, setEmail] = useState<string>('')
  const [password, setPassword] = useState<string>('')

  // Holds an error message to display, or an empty string if there is none.
  const [errorMessage, setErrorMessage] = useState<string>('')

  // Tracks whether a login request is currently in progress, so we can
  // disable the submit button and avoid double-submissions.
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false)

  function handleEmailChange(event: React.ChangeEvent<HTMLInputElement>) {
    setEmail(event.target.value)
  }

  function handlePasswordChange(event: React.ChangeEvent<HTMLInputElement>) {
    setPassword(event.target.value)
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    // Prevent the browser's default behavior, which is to reload the
    // whole page when a form is submitted. We want to handle the
    // submission ourselves with JavaScript instead.
    event.preventDefault()

    setErrorMessage('')
    setIsSubmitting(true)

    try {
      await auth.login(email, password)
      navigate('/recipes')
    } catch (error) {
      if (error instanceof ApiError && error.status === 401) {
        setErrorMessage('Incorrect email or password.')
      } else {
        setErrorMessage('Something went wrong. Please try again.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center">
      <div className="bg-white p-8 rounded-lg shadow-md w-full max-w-sm">
        <h1 className="text-2xl font-bold text-gray-900 mb-6">Log In</h1>

        <form onSubmit={handleSubmit}>
          <div className="mb-4">
            <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1">
              Email
            </label>
            <input
              id="email"
              type="email"
              value={email}
              onChange={handleEmailChange}
              required
              className="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          <div className="mb-4">
            <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1">
              Password
            </label>
            <input
              id="password"
              type="password"
              value={password}
              onChange={handlePasswordChange}
              required
              className="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
          </div>

          {errorMessage !== '' && (
            <p className="text-red-600 text-sm mb-4">{errorMessage}</p>
          )}

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full bg-blue-600 text-white py-2 rounded-md hover:bg-blue-700 disabled:bg-blue-300"
          >
            {isSubmitting ? 'Logging in...' : 'Log In'}
          </button>
        </form>
        <p className="text-sm text-gray-600 mt-4 text-center">
          Don't have an account?{' '}
          <Link to="/register" className="text-blue-600 hover:underline">
            Sign up
          </Link>
        </p>
      </div>
    </div>
  )
}

export default LoginPage