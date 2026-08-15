import { useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { createRecipe } from '../api/recipes'
import { useToast } from '../toast/useToast'
import RecipeForm from '../components/RecipeForm'
import type { CreateRecipeRequest, IngredientLine } from '../types/recipe'
import usePageTitle from '../hooks/usePageTitle'

function createBlankIngredientLine(): IngredientLine {
  return {
    ingredientName: '',
    quantity: 0,
    unit: 'Cup',
  }
}

function CreateRecipePage() {
  const auth = useAuth()
  const toast = useToast()
  const navigate = useNavigate()
  
  usePageTitle('New Recipe')

  async function handleFormSubmit(formData: CreateRecipeRequest) {
    if (auth.accessToken === null) {
      throw new Error('You must be logged in to create a recipe.')
    }

    const createdRecipe = await createRecipe(auth.accessToken, formData)
    toast.showToast('Recipe created.', 'success')
    navigate('/recipes/' + createdRecipe.id)
  }

  return (
    <div>
      <h1 className="text-2xl font-bold text-gray-900 mb-6">New Recipe</h1>

      <RecipeForm
        initialTitle=""
        initialInstructions=""
        initialServingSize={1}
        initialIngredients={[createBlankIngredientLine()]}
        submitButtonLabel="Create Recipe"
        submittingButtonLabel="Creating..."
        cancelTo="/recipes"
        onSubmit={handleFormSubmit}
      />
    </div>
  )
}

export default CreateRecipePage