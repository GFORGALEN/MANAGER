<template>
  <a-layout :class="roleThemeClass" style="min-height: 100vh">
    <a-layout-sider
      v-model:collapsed="collapsed"
      collapsible
      breakpoint="lg"
      collapsed-width="0"
      theme="light"
      width="240"
      class="app-sider"
    >
      <div class="brand-block">
        <div class="brand-mark">CM</div>
        <div v-if="!collapsed" class="brand-copy">
          <strong>{{ t('brandTitle') }}</strong>
          <span>{{ t('brandSubtitle') }}</span>
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
          {{ t('menuProjects') }}
        </a-menu-item>
        <a-menu-item v-if="isManagerView" key="/progress">
          <template #icon>
            <AppstoreOutlined />
          </template>
          {{ t('menuProgress') }}
        </a-menu-item>
        <a-menu-item v-if="isManagerView" key="/tasks">
          <template #icon>
            <CheckSquareOutlined />
          </template>
          {{ t('menuTasks') }}
        </a-menu-item>
        <a-menu-item v-if="isManagerView" key="/variations">
          <template #icon>
            <ProfileOutlined />
          </template>
          {{ t('menuVariations') }}
        </a-menu-item>
        <a-menu-item v-if="isManagerView" key="/attachments">
          <template #icon>
            <PaperClipOutlined />
          </template>
          {{ t('menuAttachments') }}
        </a-menu-item>
        <a-menu-item v-if="isManagerView" key="/users">
          <template #icon>
            <TeamOutlined />
          </template>
          {{ t('menuUsers') }}
        </a-menu-item>
        <a-menu-item v-if="isWorkerView" key="/worker/dashboard">
          <template #icon>
            <AppstoreOutlined />
          </template>
          {{ t('menuDashboard') }}
        </a-menu-item>
        <a-menu-item v-if="isWorkerView" key="/worker/tasks">
          <template #icon>
            <CheckSquareOutlined />
          </template>
          {{ t('menuMyTasks') }}
        </a-menu-item>
        <a-menu-item v-if="isWorkerView" key="/worker/profile">
          <template #icon>
            <UserOutlined />
          </template>
          {{ t('menuProfile') }}
        </a-menu-item>
      </a-menu>
    </a-layout-sider>

    <a-layout>
      <a-layout-header class="app-header">
        <div>
          <div class="header-title">{{ pageTitle }}</div>
          <div class="header-subtitle">{{ t('headerSubtitle') }}</div>
        </div>

        <a-space class="header-actions">
          <LanguageSwitcher />
          <a-tag class="role-pill" color="blue">{{ roleLabel(currentUser?.role) || 'Unknown' }}</a-tag>
          <a-dropdown>
            <a-button class="profile-button">
              {{ currentUserLabel }}
              <DownOutlined />
            </a-button>
            <template #overlay>
              <a-menu>
                <a-menu-item key="logout" @click="logout">{{ t('logout') }}</a-menu-item>
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

import LanguageSwitcher from '@/components/LanguageSwitcher.vue'
import { clearSession, getCurrentRole, getCurrentUser, getCurrentUserLabel } from '@/services/auth'
import { useI18n } from '@/services/i18n'

const route = useRoute()
const router = useRouter()
const collapsed = ref(false)

const currentUser = computed(() => getCurrentUser())
const currentUserLabel = computed(() => getCurrentUserLabel())
const currentRole = computed(() => getCurrentRole())
const isWorkerView = computed(() => getCurrentRole() === 'Contractor')
const isManagerView = computed(() => !isWorkerView.value)
const roleThemeClass = computed(() => `role-theme-${(currentRole.value ?? 'guest').toLowerCase()}`)
const { t, roleLabel } = useI18n()
const selectedKeys = computed(() => {
  if (route.path.startsWith('/projects')) {
    return ['/projects']
  }

  if (route.path.startsWith('/progress')) {
    return ['/progress']
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
  padding: 22px 18px 16px;
}

.app-sider {
  border-inline-end: 1px solid rgba(226, 232, 240, 0.8);
  background: linear-gradient(180deg, rgba(255, 255, 255, 0.9), rgba(248, 250, 252, 0.94)) !important;
  box-shadow: inset -1px 0 0 rgba(255, 255, 255, 0.55);
}

.brand-mark {
  display: grid;
  place-items: center;
  width: 46px;
  height: 46px;
  border-radius: 16px;
  background: linear-gradient(135deg, var(--role-color-start, #1677ff), var(--role-color-end, #0f4fb8));
  color: #fff;
  font-weight: 700;
  box-shadow: 0 10px 24px rgba(37, 99, 235, 0.28);
}

.brand-copy {
  display: flex;
  flex-direction: column;
  line-height: 1.1;
}

.brand-copy strong {
  font-size: 18px;
  letter-spacing: -0.02em;
}

.brand-copy span {
  color: #64748b;
  font-size: 12px;
}

:deep(.ant-menu) {
  background: transparent;
  border-inline-end: 0 !important;
  padding: 0 12px 12px;
}

:deep(.ant-menu-light .ant-menu-item) {
  margin-inline: 0;
  margin-block: 6px;
  height: 44px;
  line-height: 44px;
  border-radius: 12px;
  font-weight: 600;
}

:deep(.ant-menu-light .ant-menu-item-selected) {
  background: linear-gradient(135deg, rgba(59, 130, 246, 0.14), rgba(14, 165, 233, 0.1));
}

:deep(.ant-layout-sider-trigger) {
  background: rgba(248, 250, 252, 0.96);
  color: #334155;
  border-top: 1px solid rgba(226, 232, 240, 0.9);
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

.header-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}

.header-title {
  font-size: 22px;
  font-weight: 700;
  color: #0f172a;
  letter-spacing: -0.02em;
}

.header-subtitle {
  color: #64748b;
}

.role-pill {
  padding-inline: 10px;
  border-radius: 999px;
  font-weight: 700;
}

.profile-button {
  min-width: 112px;
  justify-content: space-between;
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

  .header-actions {
    width: 100%;
    justify-content: space-between;
  }
}
</style>
