import { createContext, useState } from 'react'
import type { ReactNode } from 'react'

export type ToastType = 'success' | 'error'

export interface Toast {
  id: string
  message: string
  type: ToastType
}

interface ToastContextValue {
  toasts: Toast[]
  showToast: (message: string, type: ToastType) => void
  dismissToast: (id: string) => void
}

export const ToastContext = createContext<ToastContextValue | undefined>(undefined)

interface ToastProviderProps {
  children: ReactNode
}

export function ToastProvider(props: ToastProviderProps) {
  const [toasts, setToasts] = useState<Toast[]>([])

  function showToast(message: string, type: ToastType) {
    const newToast: Toast = {
      id: crypto.randomUUID(),
      message: message,
      type: type,
    }

    const updatedToasts = toasts.concat([newToast])
    setToasts(updatedToasts)

    // Automatically remove this toast after 4 seconds.
    setTimeout(function () {
      dismissToast(newToast.id)
    }, 4000)
  }

  function dismissToast(id: string) {
    const remainingToasts = toasts.filter(function (toast) {
      return toast.id !== id
    })
    setToasts(remainingToasts)
  }

  const contextValue: ToastContextValue = {
    toasts: toasts,
    showToast: showToast,
    dismissToast: dismissToast,
  }

  return (
    <ToastContext.Provider value={contextValue}>
      {props.children}
    </ToastContext.Provider>
  )
}