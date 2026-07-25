import { useState, useEffect } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { listRecipes } from '../api/recipes'
import type { RecipeSummary } from '../types/recipe'

function RecipeListPage() {
  const auth = useAuth()

  // recipes starts as null, meaning "we haven't loaded anything yet."
  // Once loading finishes, it becomes either a real array (possibly
  // empty) or stays null if the request failed.
  const [recipes, setRecipes] = useState<RecipeSummary[] | null>(null)
  const [errorMessage, setErrorMessage] = useState<string>('')

  useEffect(function () {

    async function loadRecipes() {
      if (auth.accessToken === null) {
        return
      }

      try {
        const result = await listRecipes(auth.accessToken)
        setRecipes(result)
      } catch {
        setErrorMessage('Failed to load recipes. Please try again.')
      }
    }

    loadRecipes()

  }, [auth.accessToken])

  if (errorMessage !== '') {
    return <p className="text-red-600">{errorMessage}</p>
  }

  if (recipes === null) {
    return <p className="text-gray-600">Loading recipes...</p>
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Recipes</h1>
        <Link
          to="/recipes/new"
          className="bg-blue-600 text-white px-4 py-2 rounded-md text-sm hover:bg-blue-700"
        >
          New Recipe
        </Link>
      </div>

      {recipes.length === 0 && (
        <p className="text-gray-600">You haven't added any recipes yet.</p>
      )}

      <ul className="space-y-3">
        {recipes.map(function (recipe) {
          return (
            <li key={recipe.id}>
              <Link
                to={'/recipes/' + recipe.id}
                className="block bg-white p-4 rounded-lg shadow-sm hover:shadow-md border border-gray-200"
              >
                <p className="font-semibold text-gray-900">{recipe.title}</p>
                <p className="text-sm text-gray-600">
                  Serves {recipe.servingSize} &middot; {recipe.ingredientCount} ingredients
                </p>
              </Link>
            </li>
          )
        })}
      </ul>
    </div>
  )
}

export default RecipeListPage