# Frontend — Template Project

Полное описание структуры и конвенций фронтенда. Прочитав этот файл, любой AI-агент
сможет воспроизвести фронтенд с нуля или добавить новую функциональность корректно.

---

## Стек

| Технология | Версия | Роль |
|---|---|---|
| Vue 3 | ^3.5 | UI-фреймворк, Composition API + `<script setup>` |
| TypeScript | ^5.7 | Строгая типизация (`strict: true`) |
| Vite | ^6.0 | Сборщик, dev-сервер |
| Vue Router | ^4.5 | SPA-роутинг |
| unplugin-vue-router | ^0.12 | Авто-генерация маршрутов из `src/pages/` |
| Pinia | ^3.0 | Глобальный стейт |
| PrimeVue | ^4.3 | UI-библиотека компонентов |
| @primeuix/themes | ^1.0 | Тема Aura для PrimeVue |
| Tailwind CSS | ^4.0 | Утилитарные стили (совместно с `@tailwindcss/vite`) |
| Axios | ^1.8 | HTTP-клиент |
| jwt-decode | ^4.0 | Декодирование JWT-токена на клиенте |
| mitt | ^3.0 | Глобальный event emitter (для logout и т.п.) |
| zod | ^3.24 | Схемная валидация форм |
| @primevue/forms | ^1.0 | Интеграция форм PrimeVue с резолверами (Zod) |

---

## Структура файлов

```
frontend/
├── FRONTEND.md              ← этот файл
└── app/                     ← корень приложения (npm install запускать здесь)
    ├── index.html
    ├── vite.config.ts
    ├── tsconfig.json
    ├── tsconfig.app.json
    ├── tsconfig.node.json
    ├── package.json
    ├── eslint.config.ts
    ├── .prettierrc
    ├── .gitignore
    └── src/
        ├── env.d.ts          ← vite/client типы
        ├── main.ts           ← точка входа, монтирование Vue
        ├── App.vue           ← корневой компонент, auth guard, layout switch
        │
        ├── assets/
        │   ├── main.css      ← @import tailwindcss + layer order
        │   └── layout.css    ← CSS-переменные и классы layout (sidebar, topbar, main)
        │
        ├── layouts/
        │   ├── AppLayout.vue   ← обёртка: sidebar + topbar + router-view
        │   ├── AppSidebar.vue  ← левая навигация
        │   └── AppTopbar.vue   ← верхняя панель с кнопкой меню и слотом #end
        │
        ├── pages/            ← авто-маршруты (unplugin-vue-router)
        │   ├── index.vue        → /
        │   ├── login/
        │   │   └── index.vue    → /login
        │   └── sign-up/
        │       └── index.vue    → /sign-up
        │
        ├── services/
        │   ├── emitter.ts       ← mitt: Events = { logoutRequired }
        │   ├── jwtService.ts    ← read/write/clear JWT из localStorage
        │   └── http/
        │       ├── httpClient.ts      ← Axios singleton, interceptors, token refresh
        │       ├── httpResponses.ts   ← TypeScript-типы ответов API
        │       └── authHttpService.ts ← login / signUp
        │
        ├── stores/
        │   └── user.ts     ← Pinia store: userName, email, role из JWT
        │
        └── types/
            └── auth.ts     ← interface JwtPayload
```

---

## Ключевые архитектурные решения

### Авто-маршруты

Используется `unplugin-vue-router`. Каждый файл `src/pages/**/*.vue` становится маршрутом:

| Файл | Маршрут |
|---|---|
| `src/pages/index.vue` | `/` |
| `src/pages/login/index.vue` | `/login` |
| `src/pages/sign-up/index.vue` | `/sign-up` |
| `src/pages/users/index.vue` | `/users` |
| `src/pages/users/[id].vue` | `/users/:id` |

Плагин генерирует `src/typed-router.d.ts` (в `.gitignore`). `main.ts` импортирует
`routes` из `vue-router/auto-routes`.

**Как добавить новую страницу:**
1. Создать файл `src/pages/<раздел>/index.vue`.
2. Добавить пункт в `AppSidebar.vue`.
3. Больше ничего не требуется.

---

### Auth Guard (`App.vue`)

```
router.beforeEach:
  /login  или  /sign-up  → пропустить без проверки
  нет токена             → redirect /login
  есть токен             → isAuthorized = true, продолжить
```

`AppLayout` показывается только если `isAuthorized && !isPublicRoute`.
На публичных маршрутах рендерится просто `<RouterView />` (без sidebar/topbar).

При получении события `logoutRequired` от emitter:
- токен очищается
- `isAuthorized = false`
- redirect `/login`

Кнопка «Выйти» (`logout` в `App.vue`) сначала вызывает `authHttpService.logout()`
(`POST /api/connect/logout` отзывает refresh-токен на сервере), затем делает то же самое локально.

---

### HTTP-клиент (`httpClient.ts`)

Singleton-класс `HttpClient` вокруг Axios:

```
validateStatus: 200-499 — не бросать исключение (4xx обрабатываются в коде)
Request interceptor: добавляет Authorization: Bearer <token>
Response interceptor:
  status === 401 → handleUnauthorizedAttempt()
    → POST /api/connect/token (refresh; refresh-токен браузер отправляет сам из HttpOnly-cookie)
    → повтор исходного запроса с новым access-токеном
    → очередь параллельных запросов (failedQueue)
    → при ошибке refresh: emit('logoutRequired')
  401 от /api/v1/auth/* (вход, регистрация) → refresh НЕ запускается,
    ответ возвращается странице как есть (неверный логин или пароль)
```

**Публичные методы:**

```typescript
httpClient.get<T>(url, queryParams?, config?) → Promise<T>
httpClient.post<T>(url, data?, config?)       → Promise<AxiosResponse<T>>
httpClient.put<T>(url, data?, config?)        → Promise<AxiosResponse<T>>
httpClient.delete<T>(url, config?)            → Promise<AxiosResponse<T>>
```

`get` возвращает `T` (распакованный `.data`).
`post`/`put`/`delete` возвращают полный `AxiosResponse<T>` — чтобы проверять `.status`.

---

### Сервисы API

Каждый domain-сервис — отдельный файл `src/services/http/<name>HttpService.ts`.
Импортирует `httpClient` и экспортирует объект с методами:

```typescript
// Пример: новый сервис
import httpClient from './httpClient'
import type { ProductsResponse } from './httpResponses'

const productsHttpService = {
  getAll() {
    return httpClient.get<ProductsResponse>('/api/v1/products')
  },
  create(data: CreateProductDto) {
    return httpClient.post<{ id: string }>('/api/v1/products', data)
  },
}

export default productsHttpService
```

Все ответные типы описываются в `httpResponses.ts`.

---

### Layout

Layout активируется только для авторизованных не-публичных маршрутов.

```
AppLayout
├── AppTopbar  (position: fixed, top: 0, z-index: 1000)
│   ├── кнопка-гамбургер → toggleMenu()
│   ├── RouterLink "Template Project" → /
│   └── slot #end  (кнопка выхода)
├── AppSidebar (position: fixed, left: 0, top: topbar-height, z-index: 999)
│   └── nav.layout-menu
│       └── RouterLink × N  (секции + пункты)
└── .layout-main-container
    └── .layout-main
        └── RouterView (с Suspense + ProgressSpinner fallback)
```

**CSS-переменные layout (layout.css):**

```css
--layout-sidebar-width: calc(15rem + 25px);
--layout-topbar-height: 4rem;
```

**Классы-состояния на `.layout-wrapper`:**

| Класс | Эффект |
|---|---|
| `layout-static-inactive` | Sidebar скрыт (translateX), main без margin |
| `layout-mobile-active` | Sidebar виден поверх контента, затемняющая маска |

На мобильных (≤ 991px) `margin-left: 0` на `.layout-main-container` всегда.

---

### Стили

Два слоя, порядок задан в `main.css`:

```css
@import "tailwindcss";
@layer base, primevue;
```

**Tailwind** — утилитарные классы в шаблонах: `flex`, `gap-4`, `grid`, `text-sm` и т.д.

**PrimeVue** — компоненты автоматически резолвятся через `unplugin-vue-components`
с `PrimeVueResolver`. Импортировать компоненты в `<script>` не нужно.

**PrimeVue CSS-переменные** (тема Aura) — использовать для цветов в кастомных стилях:

```css
var(--p-text-color)            /* основной текст */
var(--p-text-muted-color)      /* серый текст */
var(--p-content-background)    /* фон карточек */
var(--p-content-border-color)  /* бордер */
var(--p-surface-50)            /* фон страниц */
var(--p-primary-color)         /* акцентный цвет */
```

---

### Страничные компоненты — конвенции

```vue
<script setup lang="ts">
// 1. Импорты сервисов и сторов
// 2. ref/reactive состояние
// 3. Загрузка данных в onMounted
// 4. Обработчики событий
</script>

<template>
  <section class="flex flex-col gap-4 w-full">
    <!-- Заголовок -->
    <!-- Блок фильтров (если нужен) -->
    <!-- DataTable или контент -->
  </section>
</template>
```

**DataTable — всегда добавлять `#empty`:**

```vue
<DataTable :value="items">
  <!-- колонки -->
  <template #empty>
    <div class="flex flex-col items-center gap-2 py-6 text-gray-400">
      <i class="pi pi-inbox text-4xl" />
      <span>Данные отсутствуют</span>
    </div>
  </template>
</DataTable>
```

---

### Pinia Store

```typescript
// Паттерн: setup store
export const useXxxStore = defineStore('xxx', () => {
  const data = ref<Type | null>(null)

  async function load() { ... }
  function clear() { data.value = null }

  return { data, load, clear }
})
```

---

### Валидация форм

Используется `zod` + `@primevue/forms` resolver:

```typescript
import { z } from 'zod'
import { zodResolver } from '@primevue/forms/resolvers/zod'

const schema = z.object({
  email: z.string().email('Некорректный email'),
  password: z.string().min(6, 'Минимум 6 символов'),
})
const resolver = zodResolver(schema)
```

```vue
<Form :resolver="resolver" @submit="onSubmit">
  <FormField name="email" v-slot="{ field, error }">
    <InputText v-bind="field" />
    <small v-if="error" class="text-red-500">{{ error.message }}</small>
  </FormField>
</Form>
```

---

## Backend API (project-sample)

Базовый URL: `http://localhost:5000` (в dev через Vite-proxy `/api → http://localhost:5000`).

### Эндпоинты

#### POST `/api/v1/auth/login`

**Request:**
```json
{ "login": "string", "password": "string" }
```

**Response 200** (плюс HttpOnly-cookie `refresh_token`, `Path=/api/connect`):
```json
{ "userName": "<id пользователя>", "token": "<jwt>", "status": "Ok" }
```

**Response 400** — пустые логин или пароль; **401** — неверный логин или пароль (тело пустое).

#### POST `/api/v1/auth/sign-up`

**Request:**
```json
{
  "email": "string",
  "password": "string",
  "phoneNumber": "string",
  "name": "string",
  "age": 0
}
```

**Response 200:**
```json
{ "data": { "message": "Sign up successful" }, "status": "Ok" }
```

**Response 400** — ошибка валидации или email уже занят.

#### POST `/api/connect/token`

Обновление access-токена. Тело `application/x-www-form-urlencoded`:
`grant_type=refresh_token&client_id=spa_app`. Refresh-токен берётся из cookie.

**Response 200** (плюс новая cookie `refresh_token`: токен одноразовый, ротируется):
```json
{ "access_token": "<jwt>", "token_type": "Bearer", "expires_in": 7200 }
```

**Response 400:** `{ "error": "invalid_grant" }` — refresh-токена нет, он просрочен или отозван.

#### POST `/api/connect/logout`

Отзывает refresh-токен из cookie и удаляет cookie. **Response 200:** `{ "status": "Ok" }`.

### Аутентификация

JWT Bearer. Access-токен хранится в `localStorage` под ключом `token`.
После логина `jwtService.saveToken(token)` — токен автоматически подставляется
в каждый запрос через request interceptor httpClient.

Payload токена: `sub` (Id пользователя), `email`, `role`, `iat`, `exp`, `iss`, `aud`.

Refresh-токен хранится только в HttpOnly-cookie (JavaScript его не видит) и уходит
лишь на `/api/connect/*`. Cookie работает, когда фронтенд и API на одном origin
(в dev — через Vite-proxy).

---

## Запуск и сборка

```bash
cd frontend/app

# Установка зависимостей (npm или pnpm — оба работают)
npm install
# или
pnpm install

# Dev-сервер → http://localhost:5173
npm run dev

# Production-сборка → dist/
npm run build

# Линтинг
npm run lint:check
npm run format:check
```

**Требования:** Node.js ≥ 20, npm ≥ 10 (или pnpm ≥ 9).

> Если Node.js не установлен: скачать с https://nodejs.org/ (LTS-версию).

---

## Как добавить новую фичу — чек-лист

### Новая страница

- [ ] Создать `src/pages/<раздел>/index.vue`
- [ ] Добавить пункт в `AppSidebar.vue` → `sections`
- [ ] Добавить ответные типы в `httpResponses.ts`
- [ ] Создать `src/services/http/<name>HttpService.ts`

### Новый HTTP-сервис

- [ ] Добавить типы в `httpResponses.ts`
- [ ] Создать `src/services/http/<name>HttpService.ts`
- [ ] Использовать `httpClient.get/post/put/delete`

### Новый Pinia store

- [ ] Создать `src/stores/<name>.ts`
- [ ] Экспортировать `useXxxStore` (setup-стиль)

---

## Prettier / ESLint

`.prettierrc`:
```json
{
  "semi": false,
  "singleQuote": true,
  "printWidth": 100,
  "endOfLine": "lf",
  "trailingComma": "all"
}
```

ESLint: `eslint.config.ts` — flat config, `eslint-plugin-vue` + TypeScript.
`vue/multi-word-component-names` отключён (разрешает `index.vue` и т.п.).

**Важно:** никогда не писать несколько выражений в inline `@click` — Prettier ломает
Vue template compiler. Всегда выносить в метод:
```vue
<!-- Неправильно -->
@click="a = ''; b()"

<!-- Правильно -->
@click="onAction"
```

---

## Переменные окружения

| Переменная | Назначение | По умолчанию |
|---|---|---|
| `VITE_API_PROXY_URL` | URL бэкенда для Vite-proxy | `http://localhost:5000` |

Создать `.env.local` для локальных переопределений (в `.gitignore`).

---

## Добавление ролей (расширение)

Если бэкенд вернёт роль в JWT (`decoded.role`), расширить:

1. `src/types/auth.ts` — добавить поле `role`
2. `src/stores/user.ts` — сохранять роль
3. `App.vue` — добавить проверку `allowedRoles` в `router.beforeEach`
4. `AppSidebar.vue` — рендерить разные секции в зависимости от роли
5. `AppLayout.vue` — передавать `role` как prop в `AppSidebar`
