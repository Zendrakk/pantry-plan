import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import RecipeForm from './RecipeForm'

function renderRecipeForm(onSubmit: (formData: unknown) => Promise<void>) {
  render(
    <MemoryRouter>
      <RecipeForm
        initialTitle=""
        initialInstructions=""
        initialServingSize={1}
        initialIngredients={[{ ingredientName: '', quantity: 0, unit: 'Cup' }]}
        submitButtonLabel="Create Recipe"
        submittingButtonLabel="Creating..."
        cancelTo="/recipes"
        onSubmit={onSubmit}
      />
    </MemoryRouter>
  )
}

describe('RecipeForm', function () {

  test('starts with exactly one ingredient row', function () {
    renderRecipeForm(vi.fn())

    const ingredientNameInputs = screen.getAllByPlaceholderText('Ingredient name')
    expect(ingredientNameInputs).toHaveLength(1)
  })

  test('adds a new ingredient row when "Add Ingredient" is clicked', async function () {
    const user = userEvent.setup()
    renderRecipeForm(vi.fn())

    const addButton = screen.getByText('+ Add Ingredient')
    await user.click(addButton)

    const ingredientNameInputs = screen.getAllByPlaceholderText('Ingredient name')
    expect(ingredientNameInputs).toHaveLength(2)
  })

  test('removes an ingredient row when its Remove button is clicked', async function () {
    const user = userEvent.setup()
    renderRecipeForm(vi.fn())

    // Add a second row first, so there are two - the Remove button is
    // disabled when only one ingredient row remains, since a recipe
    // must always have at least one ingredient.
    await user.click(screen.getByText('+ Add Ingredient'))
    expect(screen.getAllByPlaceholderText('Ingredient name')).toHaveLength(2)

    const removeButtons = screen.getAllByText('Remove')
    await user.click(removeButtons[0])

    expect(screen.getAllByPlaceholderText('Ingredient name')).toHaveLength(1)
  })

  test('does not allow removing the last remaining ingredient row', function () {
    renderRecipeForm(vi.fn())

    const removeButton = screen.getByText('Remove')
    expect(removeButton).toBeDisabled()
  })

  test('calls onSubmit with the entered form data', async function () {
    const user = userEvent.setup()
    const handleSubmit = vi.fn().mockResolvedValue(undefined)
    renderRecipeForm(handleSubmit)

    await user.type(screen.getByLabelText('Title'), 'Pancakes')
    await user.type(screen.getByLabelText('Instructions'), 'Mix and cook')
    await user.type(screen.getByPlaceholderText('Ingredient name'), 'Flour')

    const quantityInput = screen.getByPlaceholderText('Qty')
    await user.type(quantityInput, '2')

    await user.click(screen.getByText('Create Recipe'))

    expect(handleSubmit).toHaveBeenCalledWith({
      title: 'Pancakes',
      instructions: 'Mix and cook',
      servingSize: 1,
      ingredients: [{ ingredientName: 'Flour', quantity: 2, unit: 'Cup' }],
    })
  })

  test('shows an error message when onSubmit fails', async function () {
    const user = userEvent.setup()
    const handleSubmit = vi.fn().mockRejectedValue(new Error('Server error'))
    renderRecipeForm(handleSubmit)

    await user.type(screen.getByLabelText('Title'), 'Pancakes')
    await user.type(screen.getByLabelText('Instructions'), 'Mix and cook')
    await user.type(screen.getByPlaceholderText('Ingredient name'), 'Flour')

    const quantityInput = screen.getByPlaceholderText('Qty')
    await user.type(quantityInput, '2')

    await user.click(screen.getByText('Create Recipe'))

    expect(await screen.findByText('Something went wrong. Please check your entries and try again.')).toBeInTheDocument()
    })

})