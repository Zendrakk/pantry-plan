import { createContext, useState } from 'react'
import type { ReactNode } from 'react'
import Button from '../components/Button'

interface ConfirmState {
  message: string
  resolve: (result: boolean) => void
}

interface ConfirmContextValue {
  confirm: (message: string) => Promise<boolean>
}

export const ConfirmContext = createContext<ConfirmContextValue | undefined>(undefined)

interface ConfirmProviderProps {
  children: ReactNode
}

export function ConfirmProvider(props: ConfirmProviderProps) {
  const [confirmState, setConfirmState] = useState<ConfirmState | null>(null)

  function confirm(message: string): Promise<boolean> {
    return new Promise(function (resolve) {
      setConfirmState({
        message: message,
        resolve: resolve,
      })
    })
  }

  function handleConfirmClick() {
    if (confirmState !== null) {
      confirmState.resolve(true)
      setConfirmState(null)
    }
  }

  function handleCancelClick() {
    if (confirmState !== null) {
      confirmState.resolve(false)
      setConfirmState(null)
    }
  }

  const contextValue: ConfirmContextValue = {
    confirm: confirm,
  }

  return (
    <ConfirmContext.Provider value={contextValue}>
      {props.children}

      {confirmState !== null && (
        <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50">
          <div className="bg-white p-6 rounded-lg shadow-md max-w-sm w-full mx-4">
            <p className="text-gray-900 mb-6">{confirmState.message}</p>

            <div className="flex gap-2 justify-end">
              <Button type="button" variant="secondary" onClick={handleCancelClick}>
                Cancel
              </Button>
              <Button type="button" variant="danger" onClick={handleConfirmClick}>
                Confirm
              </Button>
            </div>
          </div>
        </div>
      )}
    </ConfirmContext.Provider>
  )
}