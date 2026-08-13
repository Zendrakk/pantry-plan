type ButtonVariant = 'primary' | 'secondary' | 'danger'

interface ButtonProps {
  type: 'button' | 'submit'
  variant: ButtonVariant
  disabled?: boolean
  onClick?: () => void
  children: React.ReactNode
}

function Button(props: ButtonProps) {
  let variantClasses = ''

  if (props.variant === 'primary') {
    variantClasses = 'bg-blue-600 text-white hover:bg-blue-700 disabled:bg-blue-300'
  }
  if (props.variant === 'secondary') {
    variantClasses = 'text-gray-700 hover:bg-gray-100'
  }
  if (props.variant === 'danger') {
    variantClasses = 'text-red-600 hover:underline disabled:text-gray-300'
  }

  return (
    <button
      type={props.type}
      disabled={props.disabled}
      onClick={props.onClick}
      className={'px-4 py-2 rounded-md text-sm ' + variantClasses}
    >
      {props.children}
    </button>
  )
}

export default Button