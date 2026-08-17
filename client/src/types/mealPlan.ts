import type { Unit } from './recipe'

export type MealType = 'Breakfast' | 'Lunch' | 'Dinner' | 'Snack'

export interface MealPlanEntry {
  id: string
  recipeId: string
  recipeTitle: string
  date: string
  mealType: MealType
  isLeftover: boolean
}

export interface MealPlan {
  id: string
  weekStartDate: string
  entries: MealPlanEntry[]
}

export interface MealPlanSummary {
  id: string
  weekStartDate: string
  entryCount: number
}

export interface ShoppingListQuantity {
  quantity: number
  unit: Unit
}

export interface ShoppingListItem {
  ingredientName: string
  category: string | null
  quantities: ShoppingListQuantity[]
}

export interface ShoppingList {
  mealPlanId: string
  items: ShoppingListItem[]
}