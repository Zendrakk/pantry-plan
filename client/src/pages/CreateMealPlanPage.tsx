import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { createMealPlan } from '../api/mealPlans'
import usePageTitle from '../hooks/usePageTitle'
import Button from '../components/Button'
import LinkButton from '../components/LinkButton'

function CreateMealPlanPage() {
  const auth = useAuth()
  const navigate = useNavigate()

  const [weekStartDate, setWeekStartDate] = useState<string>('')
  const [errorMessage, setErrorMessage] = useState<string>('')
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false)

  usePageTitle('New Meal Plan')

  function handleDateChange(event: React.ChangeEvent<HTMLInputElement>) {
    setWeekStartDate(event.target.value)
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()

    setErrorMessage('')
    setIsSubmitting(true)

    if (auth.accessToken === null) {
      setErrorMessage('You must be logged in to create a meal plan.')
      setIsSubmitting(false)
      return
    }

    try {
      const createdMealPlan = await createMealPlan(auth.accessToken, weekStartDate)
      navigate('/meal-plans/' + createdMealPlan.id)
    } catch {
      setErrorMessage('Failed to create meal plan. Please try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">New Meal Plan</h1>

      <form onSubmit={handleSubmit} className="bg-white p-6 rounded-lg shadow-sm border border-gray-200 max-w-sm">
        <div className="mb-4">
          <label htmlFor="weekStartDate" className="block text-sm font-medium text-gray-700 mb-1">
            Week Start Date
          </label>
          <input
            id="weekStartDate"
            type="date"
            value={weekStartDate}
            onChange={handleDateChange}
            required
            className="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>

        {errorMessage !== '' && (
          <p className="text-red-600 text-sm mb-4">{errorMessage}</p>
        )}

        <div className="flex gap-2">
          <Button type="submit" variant="primary" disabled={isSubmitting}>
            {isSubmitting ? 'Creating...' : 'Create Meal Plan'}
          </Button>

          <LinkButton to="/meal-plans" variant="secondary">
            Cancel
          </LinkButton>
        </div>
      </form>
    </div>
  )
}

export default CreateMealPlanPage