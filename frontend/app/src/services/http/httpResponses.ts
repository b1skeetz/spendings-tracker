// Ответы от POST /api/v1/auth/login
export type LoginResponse = {
  userName: string
  token: string
  status: string
}

// Ответ от POST /api/v1/auth/sign-up
export type SignUpResponse = {
  data: {
    message: string
  }
  status: string
}

// Ответ от POST /api/connect/logout
export type LogoutResponse = {
  status: string
}

// Общий формат ошибки API
export type ApiError = {
  message: string
  code?: string
}
