import { Routes, Route } from 'react-router-dom'
import RegisterPage from './pages/RegisterPage'
import LoginPage from './pages/LoginPage'
import ProtectedRoute from './auth/ProtectedRoute'
import AppLayout from './components/AppLayout'
import RecipeListPage from './pages/RecipeListPage'

function App() {
  return (
    <Routes>
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/login" element={<LoginPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route path="/recipes" element={<RecipeListPage />} />
          <Route path="/meal-plans" element={<div>Meal plans page coming soon</div>} />
        </Route>
      </Route>

      <Route path="*" element={<div>Page not found.</div>} />
    </Routes>
  )
}

export default App
