function Spinner() {
  return (
    <div className="flex items-center justify-center py-8" data-testid="spinner">
      <div className="h-8 w-8 border-4 border-gray-200 border-t-blue-600 rounded-full animate-spin"></div>
    </div>
  )
}

export default Spinner