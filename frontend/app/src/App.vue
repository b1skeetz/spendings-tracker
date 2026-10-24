<script setup lang="ts">
import { ref } from 'vue'
import { RouterView, useRouter } from 'vue-router'
import jwtService from '@/services/jwtService'
import emitter from '@/services/emitter'
import authHttpService from '@/services/http/authHttpService'
import AppLayout from '@/layouts/AppLayout.vue'

const router = useRouter()
const isAuthorized = ref(false)

const isPublicRoute = (path: string) => path === '/login' || path === '/sign-up'

const showLayout = () => !isPublicRoute(router.currentRoute.value.path) && isAuthorized.value

router.beforeEach((to, _from, next) => {
  if (isPublicRoute(to.path)) {
    next()
    return
  }

  const token = jwtService.getToken()
  if (!token) {
    router.push('/login')
    return
  }

  isAuthorized.value = true
  next()
})

const logout = async () => {
  try {
    await authHttpService.logout()
  } catch {
    // Локальный выход выполняем в любом случае
  }
  jwtService.clear()
  isAuthorized.value = false
  router.push('/login')
}

emitter.on('logoutRequired', () => {
  jwtService.clear()
  isAuthorized.value = false
  router.push('/login')
})
</script>

<template>
  <AppLayout v-if="showLayout()" :on-logout="logout" />
  <RouterView v-else />
  <Toast />
</template>
