import { useState, useEffect } from 'react'
import { useAuth } from '../auth/useAuth'
import { listMealPlans } from '../api/mealPlans'
import type { MealPlanSummary } from '../types/mealPlan'
import usePageTitle from '../hooks/usePageTitle'
import LinkButton from '../components/LinkButton'
import CardLink from '../components/CardLink'

function MealPlanListPage() {
  const auth = useAuth()

  const [mealPlans, setMealPlans] = useState<MealPlanSummary[] | null>(null)
  const [errorMessage, setErrorMessage] = useState<string>('')

  usePageTitle('Meal Plans')

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
        <LinkButton to="/meal-plans/new" variant="primary">
          New Meal Plan
        </LinkButton>
      </div>

      {mealPlans.length === 0 && (
        <p className="text-gray-600">You haven't created any meal plans yet.</p>
      )}

      <ul className="space-y-3">
        {mealPlans.map(function (mealPlan) {
          return (
            <li key={mealPlan.id}>
              <CardLink to={'/meal-plans/' + mealPlan.id}>
                <p className="font-semibold text-gray-900">Week of {mealPlan.weekStartDate}</p>
                <p className="text-sm text-gray-600">{mealPlan.entryCount} meals planned</p>
              </CardLink>
            </li>
          )
        })}
      </ul>
    </div>
  )
}

export default MealPlanListPage