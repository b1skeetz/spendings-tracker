<script setup lang="ts">
import { RouterLink, useRoute } from 'vue-router'

const route = useRoute()

interface NavItem {
  label: string
  icon: string
  to: string
  exact?: boolean
}

interface NavSection {
  label: string
  items: NavItem[]
}

// Добавляй новые разделы и пункты по мере роста проекта
const sections: NavSection[] = [
  {
    label: 'Главное',
    items: [{ label: 'Дашборд', icon: 'pi pi-home', to: '/', exact: true }],
  },
  {
    label: 'Аккаунт',
    items: [{ label: 'Профиль', icon: 'pi pi-user', to: '/profile' }],
  },
]

function isActive(item: NavItem): boolean {
  if (item.exact) return route.path === item.to
  return route.path === item.to || route.path.startsWith(item.to + '/')
}
</script>

<template>
  <aside class="layout-sidebar">
    <nav class="layout-menu">
      <template v-for="section in sections" :key="section.label">
        <div class="layout-menu-section">{{ section.label }}</div>
        <RouterLink
          v-for="item in section.items"
          :key="item.to"
          :to="item.to"
          class="layout-menu-item"
          active-class=""
          exact-active-class=""
          :class="{ 'router-link-active': isActive(item) }"
        >
          <i :class="['layout-menu-item-icon', item.icon]" />
          <span class="layout-menu-item-label">{{ item.label }}</span>
        </RouterLink>
      </template>
    </nav>
  </aside>
</template>
