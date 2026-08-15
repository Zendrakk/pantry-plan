import { useToast } from './useToast'

function ToastContainer() {
  const toast = useToast()

  return (
    <div className="fixed bottom-4 right-4 flex flex-col gap-2 z-50">
      {toast.toasts.map(function (singleToast) {
        let toastClasses = ''

        if (singleToast.type === 'success') {
          toastClasses = 'bg-green-600 text-white'
        }
        if (singleToast.type === 'error') {
          toastClasses = 'bg-red-600 text-white'
        }

        return (
          <div
            key={singleToast.id}
            className={'px-4 py-3 rounded-md shadow-md ' + toastClasses}
          >
            {singleToast.message}
          </div>
        )
      })}
    </div>
  )
}

export default ToastContainer