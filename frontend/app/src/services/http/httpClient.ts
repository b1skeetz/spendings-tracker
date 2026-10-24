import axios, {
  AxiosError,
  type AxiosInstance,
  type AxiosRequestConfig,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from 'axios'

import jwtService from '@/services/jwtService'
import emitter from '@/services/emitter'

interface TokenRefreshResponse {
  access_token: string
  expires_in: number
  token_type: string
}

type InternalAxiosRequestConfigWithRetry = InternalAxiosRequestConfig & { _retry?: boolean }

// 401 от эндпоинтов входа/регистрации означает неверные данные, а не истёкший токен
const isAuthEndpoint = (url?: string) => !!url?.includes('/api/v1/auth/')

class HttpClient {
  private readonly instance: AxiosInstance
  private isRefreshing = false
  private failedQueue: Array<{
    resolve: (value: unknown) => void
    reject: (reason?: unknown) => void
  }> = []

  constructor(baseURL: string = '') {
    this.instance = axios.create({
      baseURL,
      headers: { 'Content-Type': 'application/json' },
      // 4xx — обрабатываем в коде, не бросаем исключение
      validateStatus: (status: number) => status >= 200 && status < 500,
    })

    this.instance.interceptors.request.use(
      (config: InternalAxiosRequestConfig) => {
        if (!config.url?.includes('/api/connect/token') && !config.headers['Authorization']) {
          const token = jwtService.getToken()
          if (token) {
            config.headers['Authorization'] = `Bearer ${token}`
          }
        }
        return config
      },
      (error) => Promise.reject(error),
    )

    this.instance.interceptors.response.use(
      async (response: AxiosResponse) => {
        if (response.status === 401 && !isAuthEndpoint(response.config.url)) {
          try {
            return await this.handleUnauthorizedAttempt(
              response.config as InternalAxiosRequestConfigWithRetry,
            )
          } catch (refreshError) {
            return Promise.reject(refreshError)
          }
        }
        return response
      },
      (error: AxiosError) => {
        console.error('HTTP error (status >= 500 or network error):', error)
        return Promise.reject(error)
      },
    )
  }

  private async handleUnauthorizedAttempt(
    originalRequest: InternalAxiosRequestConfigWithRetry,
  ): Promise<AxiosResponse> {
    if (originalRequest._retry) {
      throw new Error('Token refresh failed after retry.')
    }

    if (this.isRefreshing) {
      return new Promise((resolve, reject) => {
        this.failedQueue.push({ resolve, reject })
      }).then(async (token) => {
        if (originalRequest.headers) {
          originalRequest.headers['Authorization'] = 'Bearer ' + token
        }
        return await this.instance(originalRequest)
      })
    }

    originalRequest._retry = true
    this.isRefreshing = true

    try {
      const response = await axios.post<TokenRefreshResponse>(
        `${this.instance.defaults.baseURL || ''}/api/connect/token`,
        new URLSearchParams({ grant_type: 'refresh_token', client_id: 'spa_app' }),
        {
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
          withCredentials: true,
        },
      )

      const newToken = response.data.access_token
      jwtService.saveToken(newToken)

      if (originalRequest.headers) {
        originalRequest.headers['Authorization'] = `Bearer ${newToken}`
      }

      this.processQueue(null, newToken)
      this.isRefreshing = false
      return await this.instance(originalRequest)
    } catch (refreshError) {
      jwtService.clear()
      this.processQueue(refreshError, null)
      emitter.emit('logoutRequired', { reason: 'refreshFailed' })
      this.isRefreshing = false
      throw refreshError
    }
  }

  private processQueue(error: unknown | null, token: string | null = null): void {
    this.failedQueue.forEach((prom) => {
      if (error) prom.reject(error)
      else prom.resolve(token)
    })
    this.failedQueue = []
  }

  public async get<T = unknown>(
    url: string,
    queryParams?: Record<string, unknown>,
    config?: AxiosRequestConfig,
  ): Promise<T> {
    const response: AxiosResponse<T> = await this.instance.get<T>(url, {
      ...config,
      params: queryParams,
      paramsSerializer: (params) => {
        const sp = new URLSearchParams()
        for (const key in params) {
          if (!Object.prototype.hasOwnProperty.call(params, key)) continue
          const value = params[key]
          if (Array.isArray(value)) value.forEach((v) => sp.append(key, String(v)))
          else if (value !== null && value !== undefined) sp.append(key, String(value))
        }
        return sp.toString()
      },
    })
    return response.data
  }

  public async post<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.instance.post<T>(url, data, config)
  }

  public async put<T = unknown>(
    url: string,
    data?: unknown,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.instance.put<T>(url, data, config)
  }

  public async delete<T = unknown>(
    url: string,
    config?: AxiosRequestConfig,
  ): Promise<AxiosResponse<T>> {
    return this.instance.delete<T>(url, config)
  }
}

const httpClient = new HttpClient()
export default httpClient
