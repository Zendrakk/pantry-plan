import { pluralize } from './pluralize'

describe('pluralize', function () {

  test('returns the singular form when count is exactly 1', function () {
    const result = pluralize(1, 'ingredient', 'ingredients')

    expect(result).toBe('ingredient')
  })

  test('returns the plural form when count is 0', function () {
    const result = pluralize(0, 'ingredient', 'ingredients')

    expect(result).toBe('ingredients')
  })

  test('returns the plural form when count is greater than 1', function () {
    const result = pluralize(5, 'ingredient', 'ingredients')

    expect(result).toBe('ingredients')
  })

  test('works correctly with different word pairs', function () {
    const result = pluralize(1, 'meal planned', 'meals planned')

    expect(result).toBe('meal planned')
  })

})