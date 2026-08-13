interface CardProps {
  children: React.ReactNode
}

function Card(props: CardProps) {
  return (
    <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
      {props.children}
    </div>
  )
}

export default Card