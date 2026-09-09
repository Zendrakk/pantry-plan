import { useState, useEffect } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { getRecipe } from '../api/recipes'
import { useNavigate } from 'react-router-dom'
import { deleteRecipe } from '../api/recipes'
import { ApiError, extractConflictingMealPlans } from '../api/client'
import { useToast } from '../toast/useToast'
import type { Recipe, ConflictingMealPlan } from '../types/recipe'
import usePageTitle from '../hooks/usePageTitle'
import Button from '../components/Button'
import LinkButton from '../components/LinkButton'
import Card from '../components/Card'
import Spinner from '../components/Spinner'

function RecipeDetailPage() {
  const auth = useAuth()
  const navigate = useNavigate()
  const params = useParams()
  const toast = useToast()
  const recipeId = params.recipeId

  const [recipe, setRecipe] = useState<Recipe | null>(null)
  const [errorMessage, setErrorMessage] = useState<string>('')
  const [conflictingMealPlans, setConflictingMealPlans] = useState<ConflictingMealPlan[]>([])

  useEffect(function () {

    async function loadRecipe() {
      if (auth.accessToken === null) {
        return
      }

      if (recipeId === undefined) {
        setErrorMessage('No recipe was specified.')
        return
      }

      try {
        const result = await getRecipe(auth.accessToken, recipeId)
        setRecipe(result)
      } catch {
        setErrorMessage('Failed to load this recipe. It may not exist.')
      }
    }

    loadRecipe()

  }, [auth.accessToken, recipeId])

  usePageTitle(recipe === null ? 'Recipe' : recipe.title)

  if (errorMessage !== '') {
    return <p className="text-red-600">{errorMessage}</p>
  }

  if (recipe === null) {
    return <Spinner />
  }

  async function handleDeleteClick() {
    if (auth.accessToken === null || recipeId === undefined) {
      return
    }

    const confirmed = window.confirm('Are you sure you want to delete this recipe?')
    if (!confirmed) {
      return
    }

    setConflictingMealPlans([])

    try {
      await deleteRecipe(auth.accessToken, recipeId)
      toast.showToast('Recipe deleted.', 'success')
      navigate('/recipes')
    } catch (error) {
      if (error instanceof ApiError && error.status === 409) {
        toast.showToast('This recipe is used in a meal plan and cannot be deleted.', 'error')
        setConflictingMealPlans(extractConflictingMealPlans(error))
      } else {
        toast.showToast('Failed to delete this recipe. Please try again.', 'error')
      }
    }
  }

  return (
    <div>
      <Link to="/recipes" className="text-sm text-blue-600 hover:underline">
        &larr; Back to Recipes
      </Link>

      <LinkButton to={'/recipes/' + recipe.id + '/edit'} variant="secondary">
        Edit
      </LinkButton>

      <Button type="button" variant="danger" onClick={handleDeleteClick}>
        Delete
      </Button>

      {conflictingMealPlans.length > 0 && (
        <div className="mt-2">
          <p className="text-sm text-gray-700">This recipe is used in:</p>
          <ul className="ml-4 list-disc">
            {conflictingMealPlans.map(function (mealPlan) {
              return (
                <li key={mealPlan.id}>
                  <Link to={'/meal-plans/' + mealPlan.id} className="text-sm text-blue-600 hover:underline">
                    Week of {mealPlan.weekStartDate}
                  </Link>
                </li>
              )
            })}
          </ul>
        </div>
      )}

      <div className="mt-4">
        <Card>
          <h1 className="text-2xl font-bold text-gray-900">{recipe.title}</h1>
          <p className="text-sm text-gray-600 mb-4">Serves {recipe.servingSize}</p>

          <h2 className="text-lg font-semibold text-gray-900 mb-2">Ingredients</h2>
          <ul className="list-disc list-inside mb-4">
            {recipe.ingredients.map(function (ingredientLine, index) {
              return (
                <li key={index} className="text-gray-700">
                  {ingredientLine.quantity} {ingredientLine.unit} {ingredientLine.ingredientName}
                </li>
              )
            })}
          </ul>

          <h2 className="text-lg font-semibold text-gray-900 mb-2">Instructions</h2>
          <p className="text-gray-700 whitespace-pre-wrap break-words">{recipe.instructions}</p>
        </Card>
      </div>
    </div>
  )
}

export default RecipeDetailPage