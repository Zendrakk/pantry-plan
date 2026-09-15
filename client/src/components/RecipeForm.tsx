import { useState } from 'react'
import Button from './Button'
import LinkButton from './LinkButton'
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
          minLength={3}
          maxLength={200}
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
          minLength={10}
          maxLength={5000}
          rows={4}
          className="w-full border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500 break-words"
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
                minLength={2}
                maxLength={100}
                className="flex-1 border border-gray-300 rounded-md px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              />

              <input
                type="number"
                placeholder="Qty"
                min="0.01"
                step="1"
                value={ingredient.quantity === 0 ? '' : ingredient.quantity}
                onChange={function (event) {
                  var rawValue = event.target.value

                  if (rawValue === '') {
                    handleIngredientFieldChange(index, 'quantity', 0)
                  } else {
                    handleIngredientFieldChange(index, 'quantity', Number(rawValue))
                  }
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

              <Button
                type="button"
                variant="danger"
                disabled={ingredients.length === 1}
                onClick={function () {
                  handleRemoveIngredientClick(index)
                }}
              >
                Remove
              </Button>
            </div>
          )
        })}

        <Button type="button" variant="secondary" onClick={handleAddIngredientClick}>
          + Add Ingredient
        </Button>
      </div>

      {errorMessage !== '' && (
        <p className="text-red-600 text-sm mb-4">{errorMessage}</p>
      )}

      <div className="flex gap-2">
        <Button type="submit" variant="primary" disabled={isSubmitting}>
          {isSubmitting ? props.submittingButtonLabel : props.submitButtonLabel}
        </Button>

        <LinkButton to={props.cancelTo} variant="secondary">
          Cancel
        </LinkButton>
      </div>
    </form>
  )
}

export default RecipeForm