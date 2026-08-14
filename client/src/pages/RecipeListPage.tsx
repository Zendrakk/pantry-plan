import { useState, useEffect } from 'react'
import { useAuth } from '../auth/useAuth'
import { listRecipes } from '../api/recipes'
import type { RecipeSummary } from '../types/recipe'
import usePageTitle from '../hooks/usePageTitle'
import LinkButton from '../components/LinkButton'
import CardLink from '../components/CardLink'
import Spinner from '../components/Spinner'

function RecipeListPage() {
  const auth = useAuth()
  
  usePageTitle('Recipes')

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
    return <Spinner />
  }

  return (
    <div>
      <div className="flex items-center justify-between mb-6">
        <h1 className="text-2xl font-bold text-gray-900">Recipes</h1>
        <LinkButton to="/recipes/new" variant="primary">
          New Recipe
        </LinkButton>
      </div>

      {recipes.length === 0 && (
        <p className="text-gray-600">You haven't added any recipes yet.</p>
      )}

      <ul className="space-y-3">
        {recipes.map(function (recipe) {
          return (
            <li key={recipe.id}>
              <CardLink to={'/recipes/' + recipe.id}>
                <p className="font-semibold text-gray-900">{recipe.title}</p>
                <p className="text-sm text-gray-600">
                  Serves {recipe.servingSize} &middot; {recipe.ingredientCount} ingredients
                </p>
              </CardLink>
            </li>
          )
        })}
      </ul>
    </div>
  )
}

export default RecipeListPage