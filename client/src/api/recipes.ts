import { apiRequest } from './client'
import type { Recipe, RecipeSummary, CreateRecipeRequest } from '../types/recipe'

export async function listRecipes(accessToken: string): Promise<RecipeSummary[]> {
  const result = await apiRequest<RecipeSummary[]>('/recipes', {
    method: 'GET',
    accessToken: accessToken,
  })

  return result
}

export async function getRecipe(accessToken: string, recipeId: string): Promise<Recipe> {
  const result = await apiRequest<Recipe>('/recipes/' + recipeId, {
    method: 'GET',
    accessToken: accessToken,
  })

  return result
}

export async function createRecipe(accessToken: string, request: CreateRecipeRequest): Promise<Recipe> {
  const result = await apiRequest<Recipe>('/recipes', {
    method: 'POST',
    body: request,
    accessToken: accessToken,
  })

  return result
}

export async function updateRecipe(accessToken: string, recipeId: string, request: CreateRecipeRequest): Promise<Recipe> {
  const result = await apiRequest<Recipe>('/recipes/' + recipeId, {
    method: 'PUT',
    body: request,
    accessToken: accessToken,
  })

  return result
}

export async function deleteRecipe(accessToken: string, recipeId: string): Promise<void> {
  await apiRequest<void>('/recipes/' + recipeId, {
    method: 'DELETE',
    accessToken: accessToken,
  })
}