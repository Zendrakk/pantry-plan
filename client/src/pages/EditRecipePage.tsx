import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { getRecipe, updateRecipe } from '../api/recipes'
import RecipeForm from '../components/RecipeForm'
import type { Recipe, CreateRecipeRequest } from '../types/recipe'
import usePageTitle from '../hooks/usePageTitle'

function EditRecipePage() {
  const auth = useAuth()
  const navigate = useNavigate()
  const params = useParams()
  const recipeId = params.recipeId

  const [recipe, setRecipe] = useState<Recipe | null>(null)
  const [errorMessage, setErrorMessage] = useState<string>('')

  usePageTitle('Edit Recipe')

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

  async function handleFormSubmit(formData: CreateRecipeRequest) {
    if (auth.accessToken === null) {
      throw new Error('You must be logged in to edit a recipe.')
    }

    if (recipeId === undefined) {
      throw new Error('No recipe was specified.')
    }

    await updateRecipe(auth.accessToken, recipeId, formData)
    navigate('/recipes/' + recipeId)
  }

  if (errorMessage !== '') {
    return <p className="text-red-600">{errorMessage}</p>
  }

  if (recipe === null) {
    return <p className="text-gray-600">Loading recipe...</p>
  }

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">Edit Recipe</h1>

      <RecipeForm
        initialTitle={recipe.title}
        initialInstructions={recipe.instructions}
        initialServingSize={recipe.servingSize}
        initialIngredients={recipe.ingredients}
        submitButtonLabel="Save Changes"
        submittingButtonLabel="Saving..."
        onSubmit={handleFormSubmit}
      />
    </div>
  )
}

export default EditRecipePage