import { defineStore } from 'pinia'
import { ref } from 'vue'
import { jwtDecode } from 'jwt-decode'
import jwtService from '@/services/jwtService'
import type { JwtPayload } from '@/types/auth'

export const useUserStore = defineStore('user', () => {
  const userName = ref<string | null>(null)
  const email = ref<string | null>(null)
  const role = ref<string | null>(null)

  function loadFromToken() {
    const token = jwtService.getToken()
    if (!token) return

    const decoded = jwtDecode<JwtPayload>(token)
    userName.value = decoded.sub
    email.value = decoded.email
    role.value = decoded.role
  }

  function clear() {
    userName.value = null
    email.value = null
    role.value = null
  }

  return { userName, email, role, loadFromToken, clear }
})
