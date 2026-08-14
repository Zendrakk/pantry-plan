import { useState, useEffect } from 'react'
import { useParams, Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { getMealPlan, addMealPlanEntry, removeMealPlanEntry, deleteMealPlan } from '../api/mealPlans'
import { listRecipes } from '../api/recipes'
import type { MealPlan, MealType } from '../types/mealPlan'
import type { RecipeSummary } from '../types/recipe'
import usePageTitle from '../hooks/usePageTitle'
import Button from '../components/Button'
import LinkButton from '../components/LinkButton'
import Card from '../components/Card'

const mealTypeOptions: MealType[] = ['Breakfast', 'Lunch', 'Dinner', 'Snack']

function MealPlanDetailPage() {
  const auth = useAuth()
  const params = useParams()
  const navigate = useNavigate()
  const mealPlanId = params.mealPlanId

  const [mealPlan, setMealPlan] = useState<MealPlan | null>(null)
  const [recipes, setRecipes] = useState<RecipeSummary[] | null>(null)
  const [errorMessage, setErrorMessage] = useState<string>('')

  // Fields for the "add entry" form.
  const [selectedRecipeId, setSelectedRecipeId] = useState<string>('')
  const [entryDate, setEntryDate] = useState<string>('')
  const [selectedMealType, setSelectedMealType] = useState<MealType>('Dinner')
  const [isAddingEntry, setIsAddingEntry] = useState<boolean>(false)

  useEffect(function () {

    async function loadData() {
      if (auth.accessToken === null) {
        return
      }

      if (mealPlanId === undefined) {
        setErrorMessage('No meal plan was specified.')
        return
      }

      try {
        const mealPlanResult = await getMealPlan(auth.accessToken, mealPlanId)
        setMealPlan(mealPlanResult)

        const recipesResult = await listRecipes(auth.accessToken)
        setRecipes(recipesResult)

        if (recipesResult.length > 0) {
          setSelectedRecipeId(recipesResult[0].id)
        }
      } catch {
        setErrorMessage('Failed to load this meal plan. It may not exist.')
      }
    }

    loadData()

  }, [auth.accessToken, mealPlanId])

  usePageTitle(mealPlan === null ? 'Meal Plan' : 'Week of ' + mealPlan.weekStartDate)

  async function handleAddEntrySubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()

    if (auth.accessToken === null || mealPlanId === undefined) {
      return
    }

    setIsAddingEntry(true)

    try {
      const updatedMealPlan = await addMealPlanEntry(
        auth.accessToken,
        mealPlanId,
        selectedRecipeId,
        entryDate,
        selectedMealType
      )
      setMealPlan(updatedMealPlan)
      setEntryDate('')
    } catch {
      setErrorMessage('Failed to add that meal. Please try again.')
    } finally {
      setIsAddingEntry(false)
    }
  }

  async function handleRemoveEntryClick(entryId: string) {
    if (auth.accessToken === null || mealPlanId === undefined) {
      return
    }

    try {
      const updatedMealPlan = await removeMealPlanEntry(auth.accessToken, mealPlanId, entryId)
      setMealPlan(updatedMealPlan)
    } catch {
      setErrorMessage('Failed to remove that meal. Please try again.')
    }
  }

  async function handleDeleteMealPlanClick() {
    if (auth.accessToken === null || mealPlanId === undefined) {
      return
    }

    const confirmed = window.confirm('Are you sure you want to delete this meal plan? This cannot be undone.')
    if (!confirmed) {
      return
    }

    try {
      await deleteMealPlan(auth.accessToken, mealPlanId)
      navigate('/meal-plans')
    } catch {
      setErrorMessage('Failed to delete this meal plan. Please try again.')
    }
  }

  if (errorMessage !== '') {
    return <p className="text-red-600">{errorMessage}</p>
  }

  if (mealPlan === null || recipes === null) {
    return <p className="text-gray-600">Loading meal plan...</p>
  }

  return (
    <div>
      <Link to="/meal-plans" className="text-sm text-blue-600 hover:underline">
        &larr; Back to Meal Plans
      </Link>

      <div className="flex items-center justify-between mt-4 mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Week of {mealPlan.weekStartDate}</h1>
        <div className="flex gap-2">
          <LinkButton to={'/meal-plans/' + mealPlan.id + '/shopping-list'} variant="primary">
            View Shopping List
          </LinkButton>
          <Button type="button" variant="danger" onClick={handleDeleteMealPlanClick}>
            Delete
          </Button>
        </div>
      </div>

      <div className="mb-6">
        <Card>
          <h2 className="text-lg font-semibold text-gray-900 mb-3">Planned Meals</h2>

          {mealPlan.entries.length === 0 && (
            <p className="text-gray-600 mb-3">No meals planned yet.</p>
          )}

          <ul className="space-y-2 mb-4">
            {mealPlan.entries.map(function (entry) {
              return (
                <li key={entry.id} className="flex items-center justify-between border-b border-gray-100 pb-2">
                  <span className="text-gray-700">
                    {entry.date} &middot; {entry.mealType} &middot; {entry.recipeTitle}
                  </span>
                  <Button
                    type="button"
                    variant="danger"
                    onClick={function () {
                      handleRemoveEntryClick(entry.id)
                    }}
                  >
                    Remove
                  </Button>
                </li>
              )
            })}
          </ul>

          {recipes.length === 0 && (
            <p className="text-gray-600 text-sm">
              You don't have any recipes yet. Create one before planning meals.
            </p>
          )}

          {recipes.length > 0 && (
            <form onSubmit={handleAddEntrySubmit} className="flex gap-2 items-end flex-wrap">
              <div>
                <label htmlFor="recipeSelect" className="block text-sm font-medium text-gray-700 mb-1">
                  Recipe
                </label>
                <select
                  id="recipeSelect"
                  value={selectedRecipeId}
                  onChange={function (event) {
                    setSelectedRecipeId(event.target.value)
                  }}
                  className="border border-gray-300 rounded-md px-3 py-2"
                >
                  {recipes.map(function (recipeSummary) {
                    return (
                      <option key={recipeSummary.id} value={recipeSummary.id}>
                        {recipeSummary.title}
                      </option>
                    )
                  })}
                </select>
              </div>

              <div>
                <label htmlFor="entryDate" className="block text-sm font-medium text-gray-700 mb-1">
                  Date
                </label>
                <input
                  id="entryDate"
                  type="date"
                  value={entryDate}
                  onChange={function (event) {
                    setEntryDate(event.target.value)
                  }}
                  required
                  className="border border-gray-300 rounded-md px-3 py-2"
                />
              </div>

              <div>
                <label htmlFor="mealTypeSelect" className="block text-sm font-medium text-gray-700 mb-1">
                  Meal
                </label>
                <select
                  id="mealTypeSelect"
                  value={selectedMealType}
                  onChange={function (event) {
                    setSelectedMealType(event.target.value as MealType)
                  }}
                  className="border border-gray-300 rounded-md px-3 py-2"
                >
                  {mealTypeOptions.map(function (mealTypeOption) {
                    return (
                      <option key={mealTypeOption} value={mealTypeOption}>
                        {mealTypeOption}
                      </option>
                    )
                  })}
                </select>
              </div>

              <Button type="submit" variant="primary" disabled={isAddingEntry}>
                {isAddingEntry ? 'Adding...' : 'Add Meal'}
              </Button>
            </form>
          )}
        </Card>
      </div>
    </div>
  )
}

export default MealPlanDetailPage