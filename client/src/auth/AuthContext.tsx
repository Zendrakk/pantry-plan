import { createContext, useState, useEffect } from 'react'
import type { ReactNode } from 'react'
import * as authApi from '../api/auth'

// This describes everything that will be available to any component that reads from AuthContext.
interface AuthContextValue {
  accessToken: string | null
  userId: string | null
  email: string | null
  isLoading: boolean
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

// Create the actual Context object. We give it a default value of undefined, and we will check for
// that later to catch a mistake (using the context outside of the Provider).
export const AuthContext = createContext<AuthContextValue | undefined>(undefined)

// AuthProvider is a component that wraps around the rest of the app.
// Any component rendered inside it will be able to read AuthContext.
interface AuthProviderProps {
  children: ReactNode
}

export function AuthProvider(props: AuthProviderProps) {

  // useState gives us a piece of state that React tracks, and a function to update it. Whenever
  // we call the update function, React re-renders any component that reads this state, with the new value.
  const [accessToken, setAccessToken] = useState<string | null>(null)
  const [userId, setUserId] = useState<string | null>(null)
  const [email, setEmail] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState<boolean>(true)

  // useEffect lets us run code in response to the component being
  // rendered, rather than in response to a user action like a button
  // click. Here, we want to run some code exactly once, when the app
  // first loads: try to silently refresh the access token using the
  // refresh token cookie, in case the user already has a valid session
  // from a previous visit.
  useEffect(function () {

    async function attemptSilentRefresh() {
      try {
        const result = await authApi.refresh()
        setAccessToken(result.accessToken)
      } catch {
        // No valid refresh token cookie, or it was rejected.
        // That's fine - the user just isn't logged in.
        setAccessToken(null)
      } finally {
        setIsLoading(false)
      }
    }

    attemptSilentRefresh()

  }, [])

  async function login(emailValue: string, passwordValue: string): Promise<void> {
    const result = await authApi.login(emailValue, passwordValue)
    setAccessToken(result.accessToken)
    setUserId(result.userId)
    setEmail(result.email)
  }

  async function register(emailValue: string, passwordValue: string): Promise<void> {
    await authApi.register(emailValue, passwordValue)
  }

  async function logout(): Promise<void> {
    await authApi.logout()
    setAccessToken(null)
    setUserId(null)
    setEmail(null)
  }

  const contextValue: AuthContextValue = {
    accessToken: accessToken,
    userId: userId,
    email: email,
    isLoading: isLoading,
    login: login,
    register: register,
    logout: logout,
  }

  return (
    <AuthContext.Provider value={contextValue}>
      {props.children}
    </AuthContext.Provider>
  )
}