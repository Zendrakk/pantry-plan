import { apiRequest, ApiError, extractConflictingMealPlans } from './client'

describe('apiRequest', function () {

  afterEach(function () {
    vi.restoreAllMocks()
  })

  test('returns parsed JSON on a successful response', async function () {
    const fakeResponse = new Response(JSON.stringify({ title: 'Test Recipe' }), {
      status: 200,
      headers: { 'Content-Type': 'application/json' },
    })

    vi.spyOn(globalThis, 'fetch').mockResolvedValue(fakeResponse)

    const result = await apiRequest<{ title: string }>('/recipes/123')

    expect(result.title).toBe('Test Recipe')
  })

  test('returns undefined for a 204 No Content response', async function () {
    const fakeResponse = new Response(null, { status: 204 })

    vi.spyOn(globalThis, 'fetch').mockResolvedValue(fakeResponse)

    const result = await apiRequest<void>('/recipes/123', { method: 'DELETE' })

    expect(result).toBeUndefined()
  })

  test('returns undefined for a 200 response with an empty body', async function () {
    // This is the exact bug found and fixed during development -
    // a success response with no body at all should not throw when
    // we try to parse it as JSON.
    const fakeResponse = new Response('', { status: 200 })

    vi.spyOn(globalThis, 'fetch').mockResolvedValue(fakeResponse)

    const result = await apiRequest<void>('/auth/logout', { method: 'POST' })

    expect(result).toBeUndefined()
  })

  test('throws an ApiError with the correct status on a failed request', async function () {
    const fakeResponse = new Response(JSON.stringify({ error: 'Not found' }), {
      status: 404,
      headers: { 'Content-Type': 'application/json' },
    })

    vi.spyOn(globalThis, 'fetch').mockResolvedValue(fakeResponse)

    await expect(apiRequest('/recipes/does-not-exist')).rejects.toThrow(ApiError)
  })

  test('includes the parsed error body on a failed request', async function () {
    const fakeResponse = new Response(JSON.stringify({ error: 'Not found' }), {
      status: 404,
      headers: { 'Content-Type': 'application/json' },
    })

    vi.spyOn(globalThis, 'fetch').mockResolvedValue(fakeResponse)

    try {
      await apiRequest('/recipes/does-not-exist')
      // If we reach this line, apiRequest did not throw, which is wrong.
      expect.fail('Expected apiRequest to throw an ApiError')
    } catch (error) {
      expect(error).toBeInstanceOf(ApiError)
      if (error instanceof ApiError) {
        expect(error.status).toBe(404)
        expect(error.body).toEqual({ error: 'Not found' })
      }
    }
  })

})

describe('extractConflictingMealPlans', function () {

  test('returns the meal plans array when present and well-formed', function () {
    const error = new ApiError(409, {
      title: 'Recipe is in use',
      mealPlans: [{ id: 'abc-123', weekStartDate: '2026-07-20' }],
    })

    const result = extractConflictingMealPlans(error)

    expect(result).toEqual([{ id: 'abc-123', weekStartDate: '2026-07-20' }])
  })

  test('returns an empty array when the body has no mealPlans property', function () {
    const error = new ApiError(409, { title: 'Some other conflict' })

    const result = extractConflictingMealPlans(error)

    expect(result).toEqual([])
  })

  test('returns an empty array when the body is null', function () {
    const error = new ApiError(500, null)

    const result = extractConflictingMealPlans(error)

    expect(result).toEqual([])
  })

  test('returns an empty array when mealPlans is not actually an array', function () {
    const error = new ApiError(409, { mealPlans: 'not an array' })

    const result = extractConflictingMealPlans(error)

    expect(result).toEqual([])
  })

})