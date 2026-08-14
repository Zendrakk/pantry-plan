import { useState, useEffect } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useAuth } from '../auth/useAuth'
import { getShoppingList } from '../api/mealPlans'
import type { ShoppingList } from '../types/mealPlan'
import usePageTitle from '../hooks/usePageTitle'
import Card from '../components/Card'

function ShoppingListPage() {
  const auth = useAuth()
  const params = useParams()
  const mealPlanId = params.mealPlanId

  const [shoppingList, setShoppingList] = useState<ShoppingList | null>(null)
  const [errorMessage, setErrorMessage] = useState<string>('')

  usePageTitle('Shopping List')

  useEffect(function () {

    async function loadShoppingList() {
      if (auth.accessToken === null) {
        return
      }

      if (mealPlanId === undefined) {
        setErrorMessage('No meal plan was specified.')
        return
      }

      try {
        const result = await getShoppingList(auth.accessToken, mealPlanId)
        setShoppingList(result)
      } catch {
        setErrorMessage('Failed to load the shopping list. Please try again.')
      }
    }

    loadShoppingList()

  }, [auth.accessToken, mealPlanId])

  if (errorMessage !== '') {
    return <p className="text-red-600">{errorMessage}</p>
  }

  if (shoppingList === null) {
    return <p className="text-gray-600">Loading shopping list...</p>
  }

  return (
    <div>
      <Link to={'/meal-plans/' + mealPlanId} className="text-sm text-blue-600 hover:underline">
        &larr; Back to Meal Plan
      </Link>

      <h1 className="text-2xl font-bold text-gray-900 mt-4 mb-6">Shopping List</h1>

      {shoppingList.items.length === 0 && (
        <p className="text-gray-600">
          No meals are planned yet, so there is nothing to shop for.
        </p>
      )}

      <Card>
        <div className="divide-y divide-gray-100 -m-6">
          {shoppingList.items.map(function (item) {
            return (
              <div key={item.ingredientName} className="p-4">
                <p className="font-semibold text-gray-900">{item.ingredientName}</p>

                {item.category !== null && (
                  <p className="text-xs text-gray-500 uppercase tracking-wide">{item.category}</p>
                )}

                <ul className="mt-1">
                  {item.quantities.map(function (quantityLine, index) {
                    return (
                      <li key={index} className="text-sm text-gray-700">
                        {quantityLine.quantity} {quantityLine.unit}
                      </li>
                    )
                  })}
                </ul>
              </div>
            )
          })}
        </div>
      </Card>
    </div>
  )
}

export default ShoppingListPage