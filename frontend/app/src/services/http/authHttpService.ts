import httpClient from './httpClient'
import type { LoginResponse, LogoutResponse, SignUpResponse } from './httpResponses'

const authHttpService = {
  login(login: string, password: string) {
    return httpClient.post<LoginResponse>('/api/v1/auth/login', { login, password })
  },

  signUp(data: {
    email: string
    password: string
    phoneNumber: string
    name: string
    age: number
  }) {
    return httpClient.post<SignUpResponse>('/api/v1/auth/sign-up', data)
  },

  // Отзывает refresh-токен на сервере и удаляет его HttpOnly-cookie
  logout() {
    return httpClient.post<LogoutResponse>('/api/connect/logout')
  },
}

export default authHttpService
