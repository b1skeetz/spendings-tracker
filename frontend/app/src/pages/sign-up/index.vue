<script setup lang="ts">
import { ref } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import authHttpService from '@/services/http/authHttpService'

const router = useRouter()
const toast = useToast()

const name = ref('')
const email = ref('')
const phoneNumber = ref('')
const age = ref<number | null>(null)
const password = ref('')
const loading = ref(false)
const error = ref('')

const handleSignUp = async () => {
  loading.value = true
  error.value = ''
  try {
    const response = await authHttpService.signUp({
      name: name.value,
      email: email.value,
      phoneNumber: phoneNumber.value,
      age: age.value ?? 0,
      password: password.value,
    })
    if (response.status !== 200) {
      error.value = 'Ошибка при регистрации. Проверьте данные и попробуйте снова.'
      return
    }
    toast.add({ severity: 'success', summary: 'Готово', detail: 'Аккаунт создан', life: 3000 })
    await router.push('/login')
  } catch {
    error.value = 'Произошла ошибка при регистрации'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="login-page">
    <div class="login-card">
      <div class="login-title">Регистрация</div>
      <div class="login-subtitle">Template Project</div>

      <form class="login-form" @submit.prevent="handleSignUp">
        <div class="p-field">
          <label>Имя</label>
          <InputText v-model="name" placeholder="Иван Иванов" style="width: 100%" autofocus />
        </div>
        <div class="p-field">
          <label>Email</label>
          <InputText v-model="email" type="email" placeholder="email@example.com" style="width: 100%" />
        </div>
        <div class="p-field">
          <label>Телефон</label>
          <InputText v-model="phoneNumber" placeholder="+7 777 000 0000" style="width: 100%" />
        </div>
        <div class="p-field">
          <label>Возраст</label>
          <InputNumber v-model="age" placeholder="25" style="width: 100%" :min="1" :max="120" />
        </div>
        <div class="p-field">
          <label>Пароль</label>
          <Password v-model="password" placeholder="••••••••" style="width: 100%" toggle-mask />
        </div>
        <Message v-if="error" severity="error" :closable="false">{{ error }}</Message>
        <Button
          type="submit"
          label="Зарегистрироваться"
          icon="pi pi-user-plus"
          :loading="loading"
          style="width: 100%"
        />
        <div class="text-center text-sm" style="color: var(--p-text-muted-color)">
          Уже есть аккаунт?
          <RouterLink to="/login" class="text-primary-500 hover:underline">Войти</RouterLink>
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
  width: 24rem;
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
