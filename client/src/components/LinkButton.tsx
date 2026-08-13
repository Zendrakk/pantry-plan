import { Link } from 'react-router-dom'

type LinkButtonVariant = 'primary' | 'secondary'

interface LinkButtonProps {
  to: string
  variant: LinkButtonVariant
  children: React.ReactNode
}

function LinkButton(props: LinkButtonProps) {
  let variantClasses = ''

  if (props.variant === 'primary') {
    variantClasses = 'bg-blue-600 text-white hover:bg-blue-700'
  }
  if (props.variant === 'secondary') {
    variantClasses = 'text-gray-700 hover:bg-gray-100'
  }

  return (
    <Link to={props.to} className={'px-4 py-2 rounded-md text-sm inline-block ' + variantClasses}>
      {props.children}
    </Link>
  )
}

export default LinkButton