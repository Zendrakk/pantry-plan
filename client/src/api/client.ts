import type { ConflictingMealPlan } from "../types/recipe"

const BASE_URL = '/api'

export class ApiError extends Error {
  status: number
  body: unknown

  constructor(status: number, body: unknown) {
    super('API request failed with status ' + status)
    this.status = status
    this.body = body
  }
}

interface RequestOptions {
  method?: string
  body?: unknown
  accessToken?: string | null
}

export async function apiRequest<TResponse>(
  path: string,
  options: RequestOptions = {}
): Promise<TResponse> {

  // Pull values out of the options object, with a default for method.
  const method = options.method ?? 'GET'
  const body = options.body
  const accessToken = options.accessToken

  // Build up the request headers.
  const headers: Record<string, string> = {}

  if (body !== undefined) {
    headers['Content-Type'] = 'application/json'
  }

  if (accessToken) {
    headers['Authorization'] = `Bearer ${accessToken}`
  }

  // Convert the request body to a JSON string, if we have one.
  let requestBody: string | undefined = undefined
  if (body !== undefined) {
    requestBody = JSON.stringify(body)
  }

  // Send the actual HTTP request.
  const response = await fetch(BASE_URL + path, {
    method: method,
    headers: headers,
    body: requestBody,
    credentials: 'include', // send cookies (needed for the refresh token)
  })

  // If the server responded with an error status code, throw an ApiError
  // so calling code can use a normal try/catch.
  if (!response.ok) {
    let errorBody: unknown = null

    try {
      errorBody = await response.json()
    } catch {
      // response had no JSON body - that's fine, errorBody stays null
    }

    throw new ApiError(response.status, errorBody)
  }

  // A 204 No Content response (e.g. after a successful delete) has no
  // body to parse, so return early.
  if (response.status === 204) {
    return undefined as TResponse
  }

  // Some successful responses (like our logout endpoint) return a 200
  // status but an empty body. Read the raw text first, and only attempt
  // to parse it as JSON if there is actually something there.
  const responseText = await response.text()

  if (responseText === '') {
    return undefined as TResponse
  }

  const data = JSON.parse(responseText)
  return data as TResponse
}

// Safely extracts a "mealPlans" array from an ApiError's body, if present.
// Used specifically for the 409 conflict response when deleting a recipe
// that's still referenced by one or more meal plans.
export function extractConflictingMealPlans(error: ApiError): ConflictingMealPlan[] {
  const body = error.body

  if (body === null || typeof body !== 'object') {
    return []
  }

  if (!('mealPlans' in body)) {
    return []
  }

  const mealPlans = (body as { mealPlans: unknown }).mealPlans

  if (!Array.isArray(mealPlans)) {
    return []
  }

  return mealPlans as ConflictingMealPlan[]
}