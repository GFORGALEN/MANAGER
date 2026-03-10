<template>
  <a-layout :class="roleThemeClass" style="min-height: 100vh">
    <a-layout-sider
      v-model:collapsed="collapsed"
      collapsible
      breakpoint="lg"
      collapsed-width="0"
      theme="light"
      width="240"
      style="border-inline-end: 1px solid #e5e7eb"
    >
      <div class="brand-block">
        <div class="brand-mark">CM</div>
        <div v-if="!collapsed" class="brand-copy">
          <strong>Manager</strong>
          <span>Construction Platform</span>
        </div>
      </div>

      <a-menu
        mode="inline"
        :selected-keys="selectedKeys"
        @click="handleMenuClick"
      >
        <a-menu-item v-if="isManagerView" key="/projects">
          <template #icon>
            <ProjectOutlined />
          </template>
          Projects
        </a-menu-item>
        <a-menu-item v-if="isManagerView" key="/tasks">
          <template #icon>
            <CheckSquareOutlined />
          </template>
          Tasks
        </a-menu-item>
        <a-menu-item v-if="isManagerView" key="/variations">
          <template #icon>
            <ProfileOutlined />
          </template>
          Variations
        </a-menu-item>
        <a-menu-item v-if="isManagerView" key="/attachments">
          <template #icon>
            <PaperClipOutlined />
          </template>
          Attachments
        </a-menu-item>
        <a-menu-item v-if="isManagerView" key="/users">
          <template #icon>
            <TeamOutlined />
          </template>
          Users
        </a-menu-item>
        <a-menu-item v-if="isWorkerView" key="/worker/dashboard">
          <template #icon>
            <AppstoreOutlined />
          </template>
          Dashboard
        </a-menu-item>
        <a-menu-item v-if="isWorkerView" key="/worker/tasks">
          <template #icon>
            <CheckSquareOutlined />
          </template>
          My Tasks
        </a-menu-item>
        <a-menu-item v-if="isWorkerView" key="/worker/profile">
          <template #icon>
            <UserOutlined />
          </template>
          Profile
        </a-menu-item>
      </a-menu>
    </a-layout-sider>

    <a-layout>
      <a-layout-header class="app-header">
        <div>
          <div class="header-title">{{ pageTitle }}</div>
          <div class="header-subtitle">Manage projects, tasks, variations and attachments.</div>
        </div>

        <a-space>
          <a-tag color="blue">{{ currentUser?.role ?? 'Unknown' }}</a-tag>
          <a-dropdown>
            <a-button>
              {{ currentUserLabel }}
              <DownOutlined />
            </a-button>
            <template #overlay>
              <a-menu>
                <a-menu-item key="logout" @click="logout">Logout</a-menu-item>
              </a-menu>
            </template>
          </a-dropdown>
        </a-space>
      </a-layout-header>

      <a-layout-content class="content-area">
        <RouterView />
      </a-layout-content>
    </a-layout>
  </a-layout>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { useRoute, useRouter, RouterView } from 'vue-router'
import {
  AppstoreOutlined,
  CheckSquareOutlined,
  DownOutlined,
  PaperClipOutlined,
  ProfileOutlined,
  ProjectOutlined,
  TeamOutlined,
  UserOutlined,
} from '@ant-design/icons-vue'

import { clearSession, getCurrentRole, getCurrentUser, getCurrentUserLabel } from '@/services/auth'

const route = useRoute()
const router = useRouter()
const collapsed = ref(false)

const currentUser = computed(() => getCurrentUser())
const currentUserLabel = computed(() => getCurrentUserLabel())
const currentRole = computed(() => getCurrentRole())
const isWorkerView = computed(() => getCurrentRole() === 'Contractor')
const isManagerView = computed(() => !isWorkerView.value)
const roleThemeClass = computed(() => `role-theme-${(currentRole.value ?? 'guest').toLowerCase()}`)
const selectedKeys = computed(() => {
  if (route.path.startsWith('/projects')) {
    return ['/projects']
  }

  if (route.path.startsWith('/tasks')) {
    return ['/tasks']
  }

  if (route.path.startsWith('/variations')) {
    return ['/variations']
  }

  if (route.path.startsWith('/attachments')) {
    return ['/attachments']
  }

  if (route.path.startsWith('/users')) {
    return ['/users']
  }

  if (route.path.startsWith('/worker/dashboard')) {
    return ['/worker/dashboard']
  }

  if (route.path.startsWith('/worker/tasks')) {
    return ['/worker/tasks']
  }

  if (route.path.startsWith('/worker/profile')) {
    return ['/worker/profile']
  }

  return []
})

const pageTitle = computed(() => String(route.meta.title ?? 'Dashboard'))

function handleMenuClick({ key }: { key: string }) {
  router.push(key)
}

function logout() {
  clearSession()
  router.push({ name: 'login' })
}
</script>

<style scoped>
.brand-block {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 20px 16px 12px;
}

.brand-mark {
  display: grid;
  place-items: center;
  width: 40px;
  height: 40px;
  border-radius: 14px;
  background: linear-gradient(135deg, var(--role-color-start, #1677ff), var(--role-color-end, #0f4fb8));
  color: #fff;
  font-weight: 700;
}

.brand-copy {
  display: flex;
  flex-direction: column;
  line-height: 1.1;
}

.brand-copy span {
  color: #64748b;
  font-size: 12px;
}

.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  height: auto;
  padding: 20px 24px;
  background: rgba(255, 255, 255, 0.72);
  backdrop-filter: blur(14px);
  border-bottom: 2px solid var(--role-border, #e5e7eb);
  box-shadow: inset 0 4px 0 0 var(--role-soft, transparent);
}

.header-title {
  font-size: 22px;
  font-weight: 700;
  color: #0f172a;
}

.header-subtitle {
  color: #64748b;
}

.content-area {
  padding: 0;
}

.role-theme-admin {
  --role-color-start: #b91c1c;
  --role-color-end: #f97316;
  --role-border: #fdba74;
  --role-soft: rgba(249, 115, 22, 0.15);
}

.role-theme-pm {
  --role-color-start: #0f766e;
  --role-color-end: #2563eb;
  --role-border: #93c5fd;
  --role-soft: rgba(37, 99, 235, 0.12);
}

.role-theme-contractor {
  --role-color-start: #15803d;
  --role-color-end: #65a30d;
  --role-border: #86efac;
  --role-soft: rgba(101, 163, 13, 0.14);
}

@media (max-width: 768px) {
  .app-header {
    padding: 16px;
    align-items: flex-start;
    flex-direction: column;
  }
}
</style>
