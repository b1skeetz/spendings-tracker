import 'primeicons/primeicons.css'
import './assets/main.css'
import './assets/layout.css'

import { createApp } from 'vue'
import { createPinia } from 'pinia'
import PrimeVue from 'primevue/config'
import ToastService from 'primevue/toastservice'
import Aura from '@primeuix/themes/aura'
import App from './App.vue'
import { createRouter, createWebHistory } from 'vue-router'
import { routes, handleHotUpdate } from 'vue-router/auto-routes'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

createApp(App)
  .use(router)
  .use(PrimeVue, {
    theme: {
      preset: Aura,
      options: {
        darkModeSelector: 'none',
        cssLayer: {
          name: 'primevue',
          order: 'base, primevue',
        },
      },
    },
  })
  .use(ToastService)
  .use(createPinia())
  .mount('#app')

if (import.meta.hot) {
  handleHotUpdate(router)
}
