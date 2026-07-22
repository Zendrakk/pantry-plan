export type Unit =
  | 'Gram'
  | 'Kilogram'
  | 'Milliliter'
  | 'Liter'
  | 'Teaspoon'
  | 'Tablespoon'
  | 'Cup'
  | 'FluidOunce'
  | 'Ounce'
  | 'Pound'
  | 'Piece'

export interface IngredientLine {
  ingredientName: string
  quantity: number
  unit: Unit
}

export interface Recipe {
  id: string
  title: string
  instructions: string
  servingSize: number
  ingredients: IngredientLine[]
}

export interface RecipeSummary {
  id: string
  title: string
  servingSize: number
  ingredientCount: number
}

export interface CreateRecipeRequest {
  title: string
  instructions: string
  servingSize: number
  ingredients: IngredientLine[]
}