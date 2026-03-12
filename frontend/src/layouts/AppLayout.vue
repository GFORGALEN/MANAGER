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
        <div class="brand-mark">
          <span>CM</span>
        </div>
        <div v-if="!collapsed" class="brand-copy">
          <strong>{{ t('brandTitle') }}</strong>
          <span>{{ t('brandSubtitle') }}</span>
        </div>
      </div>
      <div v-if="!collapsed" class="sider-context">
        <div class="context-label">{{ roleLabel(currentUser?.role) || t('workspace') }}</div>
        <strong>{{ pageTitle }}</strong>
        <span>{{ t('headerSubtitle') }}</span>
      </div>

      <a-menu
        mode="inline"
        :selected-keys="selectedKeys"
        @click="handleMenuClick"
      >
        <a-menu-item v-if="isManagerView" key="/dashboard">
          <template #icon>
            <AppstoreOutlined />
          </template>
          {{ t('menuManagerDashboard') }}
        </a-menu-item>
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
        <div class="header-intro">
          <div class="header-title-row">
            <div class="header-title">{{ pageTitle }}</div>
            <div class="header-title-glow"></div>
          </div>
          <div class="header-subtitle">{{ t('headerSubtitle') }}</div>
        </div>

        <a-space class="header-actions">
          <div class="header-command-bar">
            <LanguageSwitcher />
            <a-tag class="role-pill" color="blue">{{ roleLabel(currentUser?.role) || t('unknown') }}</a-tag>
            <a-dropdown>
              <a-button class="profile-button">
                <span class="profile-avatar">{{ currentUserLabel.slice(0, 1).toUpperCase() }}</span>
                <span>{{ currentUserLabel }}</span>
                <DownOutlined />
              </a-button>
              <template #overlay>
                <a-menu>
                  <a-menu-item key="logout" @click="logout">{{ t('logout') }}</a-menu-item>
                </a-menu>
              </template>
            </a-dropdown>
          </div>
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

  if (route.path.startsWith('/dashboard')) {
    return ['/dashboard']
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
  gap: 14px;
  padding: 18px 16px 14px;
}

.app-sider {
  border-inline-end: 1px solid rgba(226, 232, 240, 0.8);
  background:
    linear-gradient(180deg, rgba(255, 255, 255, 0.62), rgba(247, 242, 235, 0.94) 18%, rgba(236, 229, 218, 0.98)) !important;
  box-shadow:
    inset -1px 0 0 rgba(255, 255, 255, 0.55),
    inset 0 1px 0 rgba(255, 255, 255, 0.65);
}

.brand-mark {
  display: grid;
  place-items: center;
  width: 38px;
  height: 38px;
  border-radius: 14px;
  background: linear-gradient(135deg, #7c2d12, #b91c1c);
  color: #fff;
  font-weight: 800;
  letter-spacing: -0.04em;
  box-shadow: 0 12px 28px rgba(124, 45, 18, 0.24);
}

.brand-mark span {
  transform: translateY(-0.5px);
}

.brand-copy {
  display: flex;
  flex-direction: column;
  gap: 2px;
  line-height: 1.05;
}

.brand-copy strong {
  font-size: 24px;
  font-weight: 700;
  letter-spacing: -0.06em;
  color: #111827;
}

.brand-copy span {
  color: #8e7f6d;
  font-size: 11px;
  letter-spacing: 0.02em;
}

.sider-context {
  margin: 2px 16px 12px;
  padding: 14px;
  border-radius: 18px;
  background: linear-gradient(135deg, rgba(15, 23, 42, 0.92), rgba(30, 41, 59, 0.92));
  color: #fff;
  box-shadow: 0 14px 28px rgba(15, 23, 42, 0.14);
}

.sider-context strong {
  display: block;
  margin: 6px 0;
  font-size: 15px;
}

.sider-context span {
  display: block;
  color: rgba(226, 232, 240, 0.72);
  font-size: 12px;
  line-height: 1.45;
}

.context-label {
  display: inline-flex;
  padding: 4px 8px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.1);
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.06em;
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
  background: linear-gradient(135deg, rgba(180, 83, 9, 0.16), rgba(120, 113, 108, 0.12));
}

:deep(.ant-layout-sider-trigger) {
  display: grid;
  place-items: center;
  width: 42px !important;
  height: 42px !important;
  right: -21px;
  bottom: auto;
  top: 92px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.88);
  color: #4b3f31;
  border: 1px solid rgba(191, 168, 138, 0.3);
  box-shadow:
    0 10px 28px rgba(17, 24, 39, 0.08),
    inset 0 1px 0 rgba(255, 255, 255, 0.9);
  z-index: 3;
}

.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 24px;
  height: auto;
  padding: 18px 26px 20px;
  background:
    linear-gradient(180deg, rgba(255, 255, 255, 0.4), rgba(255, 255, 255, 0.18)),
    rgba(247, 242, 235, 0.62);
  backdrop-filter: blur(24px) saturate(135%);
  border-bottom: 1px solid rgba(255, 255, 255, 0.55);
  box-shadow:
    inset 0 1px 0 rgba(255, 255, 255, 0.7),
    inset 0 -1px 0 rgba(120, 109, 96, 0.08);
}

.header-intro {
  min-width: 0;
}

.header-title-row {
  position: relative;
  display: flex;
  align-items: center;
  gap: 14px;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}

.header-title {
  position: relative;
  z-index: 1;
  font-size: 30px;
  font-weight: 700;
  color: #111827;
  letter-spacing: -0.04em;
}

.header-title-glow {
  width: 110px;
  height: 24px;
  border-radius: 999px;
  background: linear-gradient(90deg, var(--role-soft, rgba(59, 130, 246, 0.15)), transparent);
  filter: blur(10px);
}

.header-subtitle {
  margin-top: 6px;
  color: #7c6f61;
}

.header-command-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 8px 10px;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.46);
  border: 1px solid rgba(255, 255, 255, 0.64);
  box-shadow:
    0 12px 30px rgba(17, 24, 39, 0.08),
    inset 0 1px 0 rgba(255, 255, 255, 0.75);
}

.role-pill {
  margin-inline: 2px;
  padding-inline: 10px;
  border-radius: 999px;
  font-weight: 700;
}

.profile-button {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  min-width: 138px;
  height: 42px;
  justify-content: space-between;
  border-radius: 999px !important;
  background: rgba(255, 255, 255, 0.82);
  border-color: rgba(17, 24, 39, 0.08);
}

.profile-avatar {
  display: grid;
  place-items: center;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--role-color-start, #1677ff), var(--role-color-end, #0f4fb8));
  color: #fff;
  font-size: 12px;
  font-weight: 700;
}

.content-area {
  padding: 0;
}

.role-theme-admin {
  --role-color-start: #9a3412;
  --role-color-end: #b91c1c;
  --role-border: #f59e0b;
  --role-soft: rgba(180, 83, 9, 0.16);
}

.role-theme-pm {
  --role-color-start: #1f2937;
  --role-color-end: #0f766e;
  --role-border: #14b8a6;
  --role-soft: rgba(15, 118, 110, 0.12);
}

.role-theme-contractor {
  --role-color-start: #365314;
  --role-color-end: #4d7c0f;
  --role-border: #84cc16;
  --role-soft: rgba(77, 124, 15, 0.14);
}

@media (max-width: 768px) {
  :deep(.ant-layout-sider-trigger) {
    top: 84px;
  }

  .app-header {
    padding: 16px;
    align-items: flex-start;
    flex-direction: column;
  }

  .header-actions {
    width: 100%;
    justify-content: flex-start;
  }

  .header-title {
    font-size: 24px;
  }

  .header-command-bar {
    width: 100%;
    justify-content: space-between;
    flex-wrap: wrap;
  }
}
</style>
