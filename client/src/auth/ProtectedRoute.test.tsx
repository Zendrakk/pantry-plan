import { render, screen } from '@testing-library/react'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import ProtectedRoute from './ProtectedRoute'
import * as useAuthModule from './useAuth'

describe('ProtectedRoute', function () {

  afterEach(function () {
    vi.restoreAllMocks()
  })

  test('shows a loading indicator while auth state is still loading', function () {
    vi.spyOn(useAuthModule, 'useAuth').mockReturnValue({
      accessToken: null,
      userId: null,
      email: null,
      isLoading: true,
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
    })

    render(
      <MemoryRouter initialEntries={['/recipes']}>
        <Routes>
          <Route element={<ProtectedRoute />}>
            <Route path="/recipes" element={<div>Protected Content</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByTestId('spinner')).toBeInTheDocument()
  })

  test('redirects to login when the user is not authenticated', function () {
    vi.spyOn(useAuthModule, 'useAuth').mockReturnValue({
      accessToken: null,
      userId: null,
      email: null,
      isLoading: false,
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
    })

    render(
      <MemoryRouter initialEntries={['/recipes']}>
        <Routes>
          <Route path="/login" element={<div>Login Page</div>} />
          <Route element={<ProtectedRoute />}>
            <Route path="/recipes" element={<div>Protected Content</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText('Login Page')).toBeInTheDocument()
    expect(screen.queryByText('Protected Content')).not.toBeInTheDocument()
  })

  test('renders the protected content when the user is authenticated', function () {
    vi.spyOn(useAuthModule, 'useAuth').mockReturnValue({
      accessToken: 'fake-token',
      userId: 'user-1',
      email: 'test@example.com',
      isLoading: false,
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
    })

    render(
      <MemoryRouter initialEntries={['/recipes']}>
        <Routes>
          <Route path="/login" element={<div>Login Page</div>} />
          <Route element={<ProtectedRoute />}>
            <Route path="/recipes" element={<div>Protected Content</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText('Protected Content')).toBeInTheDocument()
  })

})