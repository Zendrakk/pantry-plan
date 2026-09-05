import { render, screen } from '@testing-library/react'
import { MemoryRouter, Routes, Route } from 'react-router-dom'
import GuestRoute from './GuestRoute'
import * as useAuthModule from './useAuth'

describe('GuestRoute', function () {

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
      <MemoryRouter initialEntries={['/login']}>
        <Routes>
          <Route element={<GuestRoute />}>
            <Route path="/login" element={<div>Login Page</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByTestId('spinner')).toBeInTheDocument()
  })

  test('redirects to recipes when the user is already authenticated', function () {
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
      <MemoryRouter initialEntries={['/login']}>
        <Routes>
          <Route path="/recipes" element={<div>Recipes Page</div>} />
          <Route element={<GuestRoute />}>
            <Route path="/login" element={<div>Login Page</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText('Recipes Page')).toBeInTheDocument()
    expect(screen.queryByText('Login Page')).not.toBeInTheDocument()
  })

  test('renders the login page when the user is not authenticated', function () {
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
      <MemoryRouter initialEntries={['/login']}>
        <Routes>
          <Route path="/recipes" element={<div>Recipes Page</div>} />
          <Route element={<GuestRoute />}>
            <Route path="/login" element={<div>Login Page</div>} />
          </Route>
        </Routes>
      </MemoryRouter>
    )

    expect(screen.getByText('Login Page')).toBeInTheDocument()
  })

})