<script setup lang="ts">
import { onMounted } from 'vue'
import { useUserStore } from '@/stores/user'

const userStore = useUserStore()

onMounted(() => {
  userStore.loadFromToken()
})
</script>

<template>
  <section class="flex flex-col gap-4 w-full">
    <div class="flex flex-col gap-1">
      <h1 class="text-2xl font-bold">Дашборд</h1>
      <p class="text-sm" style="color: var(--p-text-muted-color)">
        Добро пожаловать,
        <strong>{{ userStore.email ?? 'пользователь' }}</strong>
      </p>
    </div>

    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
      <Card>
        <template #title>Template Project</template>
        <template #content>
          <p style="color: var(--p-text-muted-color)">
            Стартовый шаблон. Добавляйте свои страницы в
            <code>src/pages/</code> — они подхватятся автоматически.
          </p>
        </template>
      </Card>
    </div>
  </section>
</template>
