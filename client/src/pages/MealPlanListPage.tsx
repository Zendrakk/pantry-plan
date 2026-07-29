import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { listMealPlans } from '../api/mealPlans'
import type { MealPlanSummary } from '../types/mealPlan'

function MealPlanListPage() {
  const auth = useAuth()

  const [mealPlans, setMealPlans] = useState<MealPlanSummary[] | null>(null)
  const [errorMessage, setErrorMessage] = useState<string>('')

  useEffect(function () {

    async function loadMealPlans() {
      if (auth.accessToken === null) {
        return
      }

      try {
        const result = await listMealPlans(auth.accessToken)
        setMealPlans(result)
      } catch {
        setErrorMessage('Failed to load meal plans. Please try again.')
      }
    }

    loadMealPlans()

  }, [auth.accessToken])

  if (errorMessage !== '') {
    return <p className="text-red-600">{errorMessage}</p>
  }

  if (mealPlans === null) {
    return <p className="text-gray-600">Loading meal plans...</p>
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Meal Plans</h1>
        <Link
          to="/meal-plans/new"
          className="bg-blue-600 text-white px-4 py-2 rounded-md text-sm hover:bg-blue-700"
        >
          New Meal Plan
        </Link>
      </div>

      {mealPlans.length === 0 && (
        <p className="text-gray-600">You haven't created any meal plans yet.</p>
      )}

      <ul className="space-y-3">
        {mealPlans.map(function (mealPlan) {
          return (
            <li key={mealPlan.id}>
              <Link
                to={'/meal-plans/' + mealPlan.id}
                className="block bg-white p-4 rounded-lg shadow-sm hover:shadow-md border border-gray-200"
              >
                <p className="font-semibold text-gray-900">Week of {mealPlan.weekStartDate}</p>
                <p className="text-sm text-gray-600">{mealPlan.entryCount} meals planned</p>
              </Link>
            </li>
          )
        })}
      </ul>
    </div>
  )
}

export default MealPlanListPage