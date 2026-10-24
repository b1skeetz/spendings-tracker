<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import authHttpService from '@/services/http/authHttpService'
import jwtService from '@/services/jwtService'

const router = useRouter()
const login = ref('')
const password = ref('')
const loading = ref(false)
const error = ref('')

const handleLogin = async () => {
  loading.value = true
  error.value = ''
  try {
    const response = await authHttpService.login(login.value, password.value)
    if (response.status !== 200) {
      error.value = 'Неверный логин или пароль'
      return
    }
    jwtService.saveToken(response.data.token)
    await router.push('/')
  } catch {
    error.value = 'Произошла ошибка при входе'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <div class="login-card">
      <div class="login-title">Template Project</div>
      <div class="login-subtitle">Вход в систему</div>

      <form class="login-form" @submit.prevent="handleLogin">
        <div class="p-field">
          <label>Логин / Email</label>
          <InputText
            v-model="login"
            placeholder="email@example.com"
            style="width: 100%"
            autofocus
          />
        </div>
        <div class="p-field">
          <label>Пароль</label>
          <Password
            v-model="password"
            placeholder="••••••••"
            style="width: 100%"
            :feedback="false"
            toggle-mask
          />
        </div>
        <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
        <Button
          type="submit"
          label="Войти"
          icon="pi pi-sign-in"
          :loading="loading"
          style="width: 100%"
        />
        <div class="text-center text-sm" style="color: var(--p-text-muted-color)">
          Нет аккаунта?
          <RouterLink to="/sign-up" class="text-primary-500 hover:underline"
            >Зарегистрироваться</RouterLink
          >
        </div>
      </form>
    </div>
  </div>
</template>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--p-surface-50);
}
.login-card {
  width: 22rem;
  padding: 2.5rem 2rem;
  background: var(--p-content-background);
  border: 1px solid var(--p-content-border-color);
  border-radius: var(--p-border-radius-xl, 12px);
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
.login-title {
  font-size: 1.5rem;
  font-weight: 700;
  letter-spacing: -0.02em;
  color: var(--p-text-color);
}
.login-subtitle {
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
  margin-top: -0.5rem;
}
.login-form {
  display: flex;
  flex-direction: column;
  gap: 1rem;
  margin-top: 0.5rem;
}
.p-field {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
}
.p-field label {
  font-size: 0.85rem;
  font-weight: 500;
  color: var(--p-text-color);
}
</style>
