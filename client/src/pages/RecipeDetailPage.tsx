import { useState, useEffect } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { getRecipe } from '../api/recipes'
import type { Recipe } from '../types/recipe'
import { useNavigate } from 'react-router-dom'
import { deleteRecipe } from '../api/recipes'
import { ApiError } from '../api/client'

function RecipeDetailPage() {
  const auth = useAuth()
  const navigate = useNavigate()
  const params = useParams()
  const recipeId = params.recipeId

  const [recipe, setRecipe] = useState<Recipe | null>(null)
  const [errorMessage, setErrorMessage] = useState<string>('')

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

  if (errorMessage !== '') {
    return <p className="text-red-600">{errorMessage}</p>
  }

  if (recipe === null) {
    return <p className="text-gray-600">Loading recipe...</p>
  }

  async function handleDeleteClick() {
    if (auth.accessToken === null || recipeId === undefined) {
      return
    }

    const confirmed = window.confirm('Are you sure you want to delete this recipe?')
    if (!confirmed) {
      return
    }

    try {
      await deleteRecipe(auth.accessToken, recipeId)
      navigate('/recipes')
    } catch (error) {
      if (error instanceof ApiError && error.status === 409) {
        setErrorMessage('This recipe is used in a meal plan and cannot be deleted.')
      } else {
        setErrorMessage('Failed to delete this recipe. Please try again.')
      }
    }
  }

  return (
    <div>
      <Link to="/recipes" className="text-sm text-blue-600 hover:underline">
        &larr; Back to Recipes
      </Link>

      <Link to={'/recipes/' + recipe.id + '/edit'} className="text-sm text-blue-600 hover:underline ml-4">
        Edit
      </Link>

      <button
        onClick={handleDeleteClick}
        className="text-sm text-red-600 hover:underline ml-4"
      >
        Delete
      </button>

      <div className="bg-white p-6 rounded-lg shadow-sm border border-gray-200 mt-4">
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
        <p className="text-gray-700 whitespace-pre-wrap">{recipe.instructions}</p>
      </div>
    </div>
  )
}

export default RecipeDetailPage