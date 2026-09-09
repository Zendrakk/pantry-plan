import { useContext } from 'react'
import { ConfirmContext } from './ConfirmContext'

export function useConfirm() {
  const context = useContext(ConfirmContext)

  if (context === undefined) {
    throw new Error('useConfirm must be used inside a ConfirmProvider')
  }

  return context
}