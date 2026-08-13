import { useState } from 'react'
import { Link } from 'react-router-dom'
import type { IngredientLine, Unit, CreateRecipeRequest } from '../types/recipe'

const unitOptions: Unit[] = [
  'Gram',
  'Kilogram',
  'Milliliter',
  'Liter',
  'Teaspoon',
  'Tablespoon',
  'Cup',
  'FluidOunce',
  'Ounce',
  'Pound',
  'Piece',
]

function createBlankIngredientLine(): IngredientLine {
  return {
    ingredientName: '',
    quantity: 0,
    unit: 'Cup',
  }
}

// Describes everything a parent component must supply to use RecipeForm.
interface RecipeFormProps {
  initialTitle: string
  initialInstructions: string
  initialServingSize: number
  initialIngredients: IngredientLine[]
  submitButtonLabel: string
  submittingButtonLabel: string
  cancelTo: string
  onSubmit: (formData: CreateRecipeRequest) => Promise<void>
}

function RecipeForm(props: RecipeFormProps) {
  const [title, setTitle] = useState<string>(props.initialTitle)
  const [instructions, setInstructions] = useState<string>(props.initialInstructions)
  const [servingSize, setServingSize] = useState<number>(props.initialServingSize)
  const [ingredients, setIngredients] = useState<IngredientLine[]>(props.initialIngredients)

  const [errorMessage, setErrorMessage] = useState<string>('')
  const [isSubmitting, setIsSubmitting] = useState<boolean>(false)

  function handleTitleChange(event: React.ChangeEvent<HTMLInputElement>) {
    setTitle(event.target.value)
  }

  function handleInstructionsChange(event: React.ChangeEvent<HTMLTextAreaElement>) {
    setInstructions(event.target.value)
  }

  function handleServingSizeChange(event: React.ChangeEvent<HTMLInputElement>) {
    setServingSize(Number(event.target.value))
  }

  function handleIngredientFieldChange(
    index: number,
    fieldName: 'ingredientName' | 'quantity' | 'unit',
    newValue: string | number
  ) {
    const updatedIngredients = ingredients.map(function (ingredient, currentIndex) {
      if (currentIndex !== index) {
        return ingredient
      }

      const updatedIngredient = {
        ingredientName: ingredient.ingredientName,
        quantity: ingredient.quantity,
        unit: ingredient.unit,
      }

      if (fieldName === 'ingredientName' && typeof newValue === 'string') {
        updatedIngredient.ingredientName = newValue
      }
      if (fieldName === 'quantity' && typeof newValue === 'number') {
        updatedIngredient.quantity = newValue
      }
      if (fieldName === 'unit' && typeof newValue === 'string') {
        updatedIngredient.unit = newValue as Unit
      }

      return updatedIngredient
    })

    setIngredients(updatedIngredients)
  }

  function handleAddIngredientClick() {
    const updatedIngredients = ingredients.concat([createBlankIngredientLine()])
    setIngredients(updatedIngredients)
  }

  function handleRemoveIngredientClick(index: number) {
    const updatedIngredients = ingredients.filter(function (_, currentIndex) {
      return currentIndex !== index
    })
    setIngredients(updatedIngredients)
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault()

    setErrorMessage('')
    setIsSubmitting(true)

    try {
      await props.onSubmit({
        title: title,
        instructions: instructions,
        servingSize: servingSize,
        ingredients: ingredients,
      })
    } catch {
      setErrorMessage('Something went wrong. Please check your entries and try again.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} className="bg-white p-6 rounded-lg shadow-sm border border-gray-200">
      <div className="mb-4">
        <label htmlFor="title" className="block text-sm font-medium text-gray-700 mb-1">
          Title
        </label>
        <input
          id="title"
          type="text"
          value={title}
          onChange={handleTitleChange}
          required
          className="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div className="mb-4">
        <label htmlFor="servingSize" className="block text-sm font-medium text-gray-700 mb-1">
          Serving Size
        </label>
        <input
          id="servingSize"
          type="number"
          min="1"
          value={servingSize}
          onChange={handleServingSizeChange}
          required
          className="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div className="mb-4">
        <label htmlFor="instructions" className="block text-sm font-medium text-gray-700 mb-1">
          Instructions
        </label>
        <textarea
          id="instructions"
          value={instructions}
          onChange={handleInstructionsChange}
          required
          rows={4}
          className="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div className="mb-4">
        <p className="block text-sm font-medium text-gray-700 mb-2">Ingredients</p>

        {ingredients.map(function (ingredient, index) {
          return (
            <div key={index} className="flex gap-2 mb-2">
              <input
                type="text"
                placeholder="Ingredient name"
                value={ingredient.ingredientName}
                onChange={function (event) {
                  handleIngredientFieldChange(index, 'ingredientName', event.target.value)
                }}
                required
                className="flex-1 border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />

              <input
                type="number"
                placeholder="Qty"
                min="0"
                step="0.01"
                value={ingredient.quantity}
                onChange={function (event) {
                  handleIngredientFieldChange(index, 'quantity', Number(event.target.value))
                }}
                required
                className="w-20 border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />

              <select
                value={ingredient.unit}
                onChange={function (event) {
                  handleIngredientFieldChange(index, 'unit', event.target.value)
                }}
                className="w-32 border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              >
                {unitOptions.map(function (unitOption) {
                  return (
                    <option key={unitOption} value={unitOption}>
                      {unitOption}
                    </option>
                  )
                })}
              </select>

              <button
                type="button"
                onClick={function () {
                  handleRemoveIngredientClick(index)
                }}
                disabled={ingredients.length === 1}
                className="text-red-600 px-2 disabled:text-gray-300"
              >
                Remove
              </button>
            </div>
          )
        })}

        <button
          type="button"
          onClick={handleAddIngredientClick}
          className="text-blue-600 text-sm hover:underline mt-1"
        >
          + Add Ingredient
        </button>
      </div>

      {errorMessage !== '' && (
        <p className="text-red-600 text-sm mb-4">{errorMessage}</p>
      )}

      <div className="flex gap-2">
        <button
          type="submit"
          disabled={isSubmitting}
          className="bg-blue-600 text-white px-4 py-2 rounded-md hover:bg-blue-700 disabled:bg-blue-300"
        >
          {isSubmitting ? props.submittingButtonLabel : props.submitButtonLabel}
        </button>

        <Link
          to={props.cancelTo}
          className="px-4 py-2 rounded-md text-gray-700 hover:bg-gray-100"
        >
          Cancel
        </Link>
      </div>
    </form>
  )
}

export default RecipeForm