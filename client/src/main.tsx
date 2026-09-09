import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { BrowserRouter } from 'react-router-dom'
import { AuthProvider } from './auth/AuthContext'
import { ToastProvider } from './toast/ToastContext'
import ToastContainer from './toast/ToastContainer'
import { ConfirmProvider } from './confirm/ConfirmContext'
import './index.css'
import App from './App.tsx'

const rootElement = document.getElementById('root')

createRoot(rootElement!).render(
  <StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <ToastProvider>
          <ConfirmProvider>
            <App />
            <ToastContainer />
          </ConfirmProvider>
        </ToastProvider>
      </AuthProvider>
    </BrowserRouter>
  </StrictMode>,
)
