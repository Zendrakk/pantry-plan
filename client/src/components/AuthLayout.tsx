import type { ReactNode } from 'react'

interface AuthLayoutProps {
  children: ReactNode
}

function AuthLayout(props: AuthLayoutProps) {
  return (
    <div className="min-h-screen bg-gray-50 flex flex-col items-center justify-center px-4">
      <div className="flex items-center gap-2 mb-8">
        <img src="/logo.svg" alt="Pantry & Plan logo" className="h-10 w-10" />
        <span className="text-2xl font-bold text-gray-900">Pantry &amp; Plan</span>
      </div>

      {props.children}
    </div>
  )
}

export default AuthLayout