const TOKEN_KEY = 'token'

class JwtService {
  private token: string | null = null

  public saveToken = (token: string) => {
    localStorage.setItem(TOKEN_KEY, token)
    this.token = token
  }

  public getToken = (): string | null => {
    if (this.token != null) return this.token
    this.token = localStorage.getItem(TOKEN_KEY)
    return this.token
  }

  public clear = () => {
    this.token = null
    localStorage.removeItem(TOKEN_KEY)
  }
}

export default new JwtService()
