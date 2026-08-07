import { apiRequest } from './client'
import type { MealPlan, MealPlanSummary, ShoppingList } from '../types/mealPlan'

export async function listMealPlans(accessToken: string): Promise<MealPlanSummary[]> {
  const result = await apiRequest<MealPlanSummary[]>('/mealplans', {
    method: 'GET',
    accessToken: accessToken,
  })

  return result
}

export async function getMealPlan(accessToken: string, mealPlanId: string): Promise<MealPlan> {
  const result = await apiRequest<MealPlan>('/mealplans/' + mealPlanId, {
    method: 'GET',
    accessToken: accessToken,
  })

  return result
}

export async function createMealPlan(accessToken: string, weekStartDate: string): Promise<MealPlan> {
  const result = await apiRequest<MealPlan>('/mealplans', {
    method: 'POST',
    body: { weekStartDate: weekStartDate },
    accessToken: accessToken,
  })

  return result
}

export async function deleteMealPlan(accessToken: string, mealPlanId: string): Promise<void> {
  await apiRequest<void>('/mealplans/' + mealPlanId, {
    method: 'DELETE',
    accessToken: accessToken,
  })
}

export async function addMealPlanEntry(
  accessToken: string,
  mealPlanId: string,
  recipeId: string,
  date: string,
  mealType: string
): Promise<MealPlan> {
  const result = await apiRequest<MealPlan>('/mealplans/' + mealPlanId + '/entries', {
    method: 'POST',
    body: { recipeId: recipeId, date: date, mealType: mealType },
    accessToken: accessToken,
  })

  return result
}

export async function removeMealPlanEntry(
  accessToken: string,
  mealPlanId: string,
  entryId: string
): Promise<MealPlan> {
  const result = await apiRequest<MealPlan>('/mealplans/' + mealPlanId + '/entries/' + entryId, {
    method: 'DELETE',
    accessToken: accessToken,
  })

  return result
}

export async function getShoppingList(accessToken: string, mealPlanId: string): Promise<ShoppingList> {
  const result = await apiRequest<ShoppingList>('/mealplans/' + mealPlanId + '/shopping-list', {
    method: 'GET',
    accessToken: accessToken,
  })

  return result
}