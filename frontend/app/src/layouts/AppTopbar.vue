<script setup lang="ts">
import { RouterLink } from 'vue-router'

defineEmits<{ 'toggle-menu': [] }>()
</script>

<template>
  <header class="layout-topbar">
    <div class="layout-topbar-start">
      <Button icon="pi pi-bars" severity="secondary" text rounded @click="$emit('toggle-menu')" />
      <RouterLink to="/" class="layout-topbar-brand">Template Project</RouterLink>
    </div>
    <div class="layout-topbar-end">
      <slot name="end" />
    </div>
  </header>
</template>
