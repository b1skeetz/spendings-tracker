<script setup lang="ts">
import { computed, ref } from 'vue'
import { RouterView } from 'vue-router'
import AppSidebar from './AppSidebar.vue'
import AppTopbar from './AppTopbar.vue'

const props = defineProps<{ onLogout: () => void }>()

const sidebarOpen = ref(true)
const mobileSidebarActive = ref(false)

const containerClass = computed(() => ({
  'layout-static-inactive': !sidebarOpen.value,
  'layout-mobile-active': mobileSidebarActive.value,
}))

function toggleMenu(): void {
  if (window.innerWidth <= 991) {
    mobileSidebarActive.value = !mobileSidebarActive.value
  } else {
    sidebarOpen.value = !sidebarOpen.value
  }
}

function hideMobileSidebar(): void {
  mobileSidebarActive.value = false
}
</script>

<template>
  <div class="layout-wrapper" :class="containerClass">
    <AppTopbar @toggle-menu="toggleMenu">
      <template #end>
        <Button
          icon="pi pi-sign-out"
          severity="secondary"
          text
          rounded
          title="Выйти"
          @click="props.onLogout()"
        />
      </template>
    </AppTopbar>
    <AppSidebar />
    <div class="layout-main-container">
      <div class="layout-main">
        <RouterView :key="$route.fullPath" v-slot="{ Component }">
          <template v-if="Component">
            <Suspense timeout="0">
              <component :is="Component" />
              <template #fallback>
                <div class="flex items-center justify-center p-8">
                  <ProgressSpinner />
                </div>
              </template>
            </Suspense>
          </template>
        </RouterView>
      </div>
    </div>
    <div class="layout-mask" @click="hideMobileSidebar" />
  </div>
</template>
