import { Link } from 'react-router-dom'

interface CardLinkProps {
  to: string
  children: React.ReactNode
}

function CardLink(props: CardLinkProps) {
  return (
    <Link
      to={props.to}
      className="block bg-white p-4 rounded-lg shadow-sm hover:shadow-md border border-gray-200"
    >
      {props.children}
    </Link>
  )
}

export default CardLink