import { apiRequest } from './client'
import type { LoginRequest, RegisterRequest, LoginResponse } from '../types/auth'

// Calls POST /api/auth/register.
// Returns nothing meaningful on success - the backend responds with 201
// Created and no body we need, but throws an ApiError on failure
// (e.g. duplicate email, weak password).
export async function register(email: string, password: string): Promise<void> {
  const requestBody: RegisterRequest = {
    email: email,
    password: password,
  }

  await apiRequest<void>('/auth/register', {
    method: 'POST',
    body: requestBody,
  })
}

// Calls POST /api/auth/login.
// Returns the access token and user info on success.
// The refresh token cookie is set automatically by the browser -
// our code never sees or touches it directly.
export async function login(email: string, password: string): Promise<LoginResponse> {
  const requestBody: LoginRequest = {
    email: email,
    password: password,
  }

  const response = await apiRequest<LoginResponse>('/auth/login', {
    method: 'POST',
    body: requestBody,
  })

  return response
}

// Calls POST /api/auth/refresh.
// The browser automatically sends the HttpOnly refresh token cookie -
// we don't pass anything explicitly.
// Returns a new access token on success.
export async function refresh(): Promise<{ accessToken: string }> {
  const response = await apiRequest<{ accessToken: string }>('/auth/refresh', {
    method: 'POST',
  })

  return response
}

// Calls POST /api/auth/logout.
// Revokes the refresh token on the server and clears the cookie.
export async function logout(): Promise<void> {
  await apiRequest<void>('/auth/logout', {
    method: 'POST',
  })
}