<template>
  <a-layout :class="['app-root', roleThemeClass]" style="min-height: 100vh">
    <a-layout-sider
      v-model:collapsed="collapsed"
      collapsible
      breakpoint="lg"
      collapsed-width="0"
      theme="dark"
      width="260"
      class="app-sider"
    >
      <div class="brand-block">
        <div class="brand-mark">
          <BuildOutlined />
        </div>
        <div v-if="!collapsed" class="brand-copy">
          <strong>BUILDMATE</strong>
          <span>CONSTRUCTION MANAGEMENT</span>
        </div>
      </div>

      <a-menu
        theme="dark"
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
            <FileTextOutlined />
          </template>
          Documents
        </a-menu-item>
        <a-menu-item v-if="isManagerView" key="/calendar" disabled>
          <template #icon>
            <CalendarOutlined />
          </template>
          Calendar
        </a-menu-item>
        <a-menu-item v-if="isManagerView" key="/reports" disabled>
          <template #icon>
            <AuditOutlined />
          </template>
          Site Reports
        </a-menu-item>
        <a-menu-item v-if="isManagerView" key="/drawings" disabled>
          <template #icon>
            <DeploymentUnitOutlined />
          </template>
          Drawings
        </a-menu-item>
        <a-menu-item v-if="isManagerView" key="/users">
          <template #icon>
            <TeamOutlined />
          </template>
          Teams
        </a-menu-item>
        <a-menu-item v-if="isManagerView" key="/settings" disabled>
          <template #icon>
            <SettingOutlined />
          </template>
          Settings
        </a-menu-item>
        <a-menu-item v-if="isWorkerView" key="/worker/dashboard">
          <template #icon>
            <HomeOutlined />
          </template>
          My Work
        </a-menu-item>
        <a-menu-item v-if="isWorkerView" key="/worker/tasks">
          <template #icon>
            <CheckSquareOutlined />
          </template>
          My Tasks
        </a-menu-item>
        <a-menu-item v-if="isWorkerView" key="/worker/report" disabled>
          <template #icon>
            <FileTextOutlined />
          </template>
          Submit Report
        </a-menu-item>
        <a-menu-item v-if="isWorkerView" key="/worker/safety" disabled>
          <template #icon>
            <SafetyCertificateOutlined />
          </template>
          Safety Training
        </a-menu-item>
        <a-menu-item v-if="isWorkerView" key="/worker/profile">
          <template #icon>
            <UserOutlined />
          </template>
          Personal Settings
        </a-menu-item>
      </a-menu>

      <div v-if="!collapsed && isManagerView" class="active-project">
        <span>ACTIVE PROJECT</span>
        <strong>{{ activeProject?.name ?? 'No active project' }}</strong>
        <div class="active-project-row">
          <small>Project Progress</small>
          <small>{{ activeProjectProgress }}%</small>
        </div>
        <div class="active-progress"><i :style="{ width: activeProjectProgress + '%' }"></i></div>
      </div>
      <div v-if="!collapsed && isWorkerView" class="worker-profile-card">
        <span class="worker-avatar">{{ currentUserLabel.slice(0, 1).toUpperCase() }}</span>
        <strong>{{ currentUserLabel }}</strong>
        <small>{{ roleLabel(currentUser?.role) || 'Contractor' }}</small>
        <em>Online</em>
      </div>
    </a-layout-sider>

    <a-layout>
      <a-layout-header class="app-header">
        <div class="header-intro">
          <div class="header-title">{{ pageTitle }}</div>
        </div>

        <div class="header-search">
          <SearchOutlined />
          <input placeholder="Search projects, tasks, docs, people..." />
        </div>

        <div class="header-actions">
          <button class="icon-button" type="button"><MessageOutlined /></button>
          <button class="icon-button with-badge" type="button" @click="notificationDrawerOpen = true">
            <BellOutlined />
            <span v-if="notificationSummary.unreadCount > 0">{{ notificationSummary.unreadCount }}</span>
          </button>
          <button class="icon-button" type="button"><QuestionCircleOutlined /></button>
          <LanguageSwitcher />
          <a-dropdown>
            <button class="profile-button" type="button">
              <span class="profile-avatar">{{ currentUserLabel.slice(0, 1).toUpperCase() }}</span>
              <span class="profile-copy">
                <strong>{{ currentUserLabel }}</strong>
                <small>{{ roleLabel(currentUser?.role) || t('unknown') }}</small>
              </span>
              <DownOutlined />
            </button>
            <template #overlay>
              <a-menu>
                <a-menu-item key="logout" @click="logout">{{ t('logout') }}</a-menu-item>
              </a-menu>
            </template>
          </a-dropdown>
        </div>
      </a-layout-header>

      <a-layout-content class="content-area">
        <RouterView />
      </a-layout-content>
    </a-layout>

    <a-drawer
      v-model:open="notificationDrawerOpen"
      title="Notification Center"
      placement="right"
      width="420"
      @after-open-change="handleNotificationDrawerChange"
    >
      <a-space direction="vertical" size="middle" style="width: 100%">
        <a-button block :disabled="notificationSummary.unreadCount === 0" @click="markAllNotificationsRead">
          Mark all as read
        </a-button>
        <a-empty v-if="notificationSummary.items.length === 0" description="No notifications yet" />
        <article
          v-for="item in notificationSummary.items"
          :key="item.notificationId"
          :class="['notification-item', { unread: !item.isRead }]"
          @click="openNotification(item)"
        >
          <div class="notification-topline">
            <a-tag :color="notificationTagColor(item.category)">{{ item.category }}</a-tag>
            <span>{{ formatNotificationTime(item.createdAt) }}</span>
          </div>
          <strong>{{ item.title }}</strong>
          <p>{{ item.message }}</p>
        </article>
      </a-space>
    </a-drawer>
  </a-layout>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter, RouterView } from 'vue-router'
import {
  AppstoreOutlined,
  AuditOutlined,
  BellOutlined,
  BuildOutlined,
  CalendarOutlined,
  CheckSquareOutlined,
  DeploymentUnitOutlined,
  DownOutlined,
  FileTextOutlined,
  HomeOutlined,
  MessageOutlined,
  ProfileOutlined,
  ProjectOutlined,
  QuestionCircleOutlined,
  SearchOutlined,
  SafetyCertificateOutlined,
  SettingOutlined,
  TeamOutlined,
  UserOutlined,
} from '@ant-design/icons-vue'

import LanguageSwitcher from '@/components/LanguageSwitcher.vue'
import { clearSession, getCurrentRole, getCurrentUser, getCurrentUserLabel } from '@/services/auth'
import api from '@/services/api'
import { useI18n } from '@/services/i18n'
import type { PagedResult } from '@/types/common'
import type { ProjectListItem } from '@/types/project'

interface NotificationItem {
  notificationId: string
  title: string
  message: string
  category: string
  entityType?: string | null
  entityId?: string | null
  isRead: boolean
  createdAt: string
}

interface NotificationSummary {
  unreadCount: number
  items: NotificationItem[]
}

const route = useRoute()
const router = useRouter()
const collapsed = ref(false)
const sidebarProjects = ref<ProjectListItem[]>([])
const notificationDrawerOpen = ref(false)
const notificationSummary = ref<NotificationSummary>({ unreadCount: 0, items: [] })

const currentUser = computed(() => getCurrentUser())
const currentUserLabel = computed(() => getCurrentUserLabel())
const currentRole = computed(() => getCurrentRole())
const isWorkerView = computed(() => getCurrentRole() === 'Contractor')
const isManagerView = computed(() => !isWorkerView.value)
const roleThemeClass = computed(() => `role-theme-${(currentRole.value ?? 'guest').toLowerCase()}`)
const { t, roleLabel } = useI18n()
const activeProject = computed(() => sidebarProjects.value.find((project) => project.status === 'Active') ?? sidebarProjects.value[0] ?? null)
const activeProjectProgress = computed(() => {
  if (!activeProject.value) return 0

  return ({
    Planning: 12,
    Active: 68,
    OnHold: 45,
    Completed: 100,
  } as Record<string, number>)[activeProject.value.status] ?? 30
})
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

async function fetchSidebarProjects() {
  if (!isManagerView.value) return

  try {
    const { data } = await api.get<PagedResult<ProjectListItem>>('/projects', {
      params: {
        pageNumber: 1,
        pageSize: 20,
      },
    })

    sidebarProjects.value = data.items
  } catch {
    sidebarProjects.value = []
  }
}

async function fetchNotificationSummary() {
  try {
    const { data } = await api.get<NotificationSummary>('/notifications/summary')
    notificationSummary.value = data
  } catch {
    notificationSummary.value = { unreadCount: 0, items: [] }
  }
}

async function markAllNotificationsRead() {
  await api.post('/notifications/read-all')
  await fetchNotificationSummary()
}

async function openNotification(item: NotificationItem) {
  if (!item.isRead) {
    await api.patch(`/notifications/${item.notificationId}/read`)
    await fetchNotificationSummary()
  }

  if (item.entityType === 'Task') {
    router.push({ name: 'tasks' })
    notificationDrawerOpen.value = false
  } else if (item.entityType === 'Variation') {
    router.push({ name: 'variations' })
    notificationDrawerOpen.value = false
  }
}

function handleNotificationDrawerChange(open: boolean) {
  if (open) {
    fetchNotificationSummary()
  }
}

function notificationTagColor(category: string) {
  return ({ Task: 'blue', Variation: 'orange', System: 'default' } as Record<string, string>)[category] ?? 'default'
}

function formatNotificationTime(value: string) {
  return new Date(value).toLocaleString()
}

onMounted(() => {
  fetchSidebarProjects()
  fetchNotificationSummary()
})
</script>

<style scoped>
.app-root {
  background: #f4f7fb;
}

.brand-block {
  display: flex;
  align-items: center;
  gap: 12px;
  height: 76px;
  padding: 18px 20px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.app-sider {
  position: relative;
  overflow: hidden;
  background:
    linear-gradient(rgba(255, 255, 255, 0.025) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255, 255, 255, 0.025) 1px, transparent 1px),
    linear-gradient(180deg, #07192b 0%, #092238 48%, #071522 100%) !important;
  background-size: 22px 22px, 22px 22px, auto;
  box-shadow: inset -1px 0 0 rgba(255, 255, 255, 0.08);
}

.brand-mark {
  display: grid;
  place-items: center;
  width: 42px;
  height: 42px;
  border-radius: 8px;
  background: linear-gradient(145deg, #ff8a00, #f05a00);
  color: #fff;
  font-size: 24px;
  box-shadow: 0 14px 24px rgba(249, 115, 22, 0.24);
}

.brand-copy {
  display: flex;
  flex-direction: column;
  gap: 1px;
  line-height: 1;
}

.brand-copy strong {
  font-size: 20px;
  font-weight: 900;
  letter-spacing: 0.02em;
  color: #f8fafc;
}

.brand-copy span {
  color: rgba(226, 232, 240, 0.76);
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.08em;
}

.active-project {
  position: absolute;
  right: 14px;
  bottom: 18px;
  left: 14px;
  padding: 14px;
  border-radius: 8px;
  background: rgba(15, 48, 74, 0.78);
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.active-project span {
  color: rgba(203, 213, 225, 0.68);
  font-size: 10px;
  font-weight: 900;
  letter-spacing: 0.06em;
}

.active-project strong {
  display: block;
  margin: 8px 0 14px;
  color: #fff;
  font-size: 13px;
}

.active-project-row {
  display: flex;
  justify-content: space-between;
  color: #cbd5e1;
  font-size: 12px;
}

.active-progress {
  height: 6px;
  margin-top: 8px;
  border-radius: 999px;
  background: rgba(148, 163, 184, 0.24);
  overflow: hidden;
}

.active-progress i {
  display: block;
  width: 66%;
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, #0f62d6, #1d9bf0);
}

.worker-profile-card {
  position: absolute;
  right: 14px;
  bottom: 18px;
  left: 14px;
  display: grid;
  gap: 5px;
  padding: 14px;
  border-radius: 8px;
  color: #fff;
  background: rgba(15, 48, 74, 0.78);
  border: 1px solid rgba(255, 255, 255, 0.08);
}

.worker-avatar {
  display: grid;
  place-items: center;
  width: 52px;
  height: 52px;
  margin-bottom: 8px;
  border-radius: 50%;
  background: linear-gradient(145deg, #ff8a00, #0f62d6);
  font-size: 20px;
  font-weight: 900;
}

.worker-profile-card strong {
  font-size: 15px;
}

.worker-profile-card small {
  color: rgba(226, 232, 240, 0.78);
}

.worker-profile-card em {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  margin-top: 4px;
  color: #22c55e;
  font-style: normal;
  font-size: 12px;
  font-weight: 900;
}

.worker-profile-card em::before {
  content: '';
  width: 8px;
  height: 8px;
  border-radius: 999px;
  background: currentColor;
}

:deep(.ant-menu) {
  background: transparent;
  border-inline-end: 0 !important;
  padding: 12px 10px 170px;
}

:deep(.ant-menu-dark .ant-menu-item) {
  margin-inline: 0;
  margin-block: 4px;
  height: 44px;
  line-height: 44px;
  border-radius: 6px;
  color: rgba(226, 232, 240, 0.9);
  font-weight: 700;
}

:deep(.ant-menu-dark .ant-menu-item-selected) {
  background: linear-gradient(90deg, rgba(249, 115, 22, 0.72), rgba(249, 115, 22, 0.42));
  color: #fff;
}

:deep(.ant-menu-item-disabled) {
  opacity: 0.62;
}

:deep(.ant-layout-sider-trigger) {
  display: grid;
  place-items: center;
  background: rgba(7, 25, 43, 0.95);
  border-top: 1px solid rgba(255, 255, 255, 0.08);
}

.app-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 18px;
  height: 68px;
  padding: 0 24px;
  background: rgba(255, 255, 255, 0.96);
  border-bottom: 1px solid rgba(15, 23, 42, 0.1);
  box-shadow: 0 1px 2px rgba(15, 23, 42, 0.04);
}

.header-intro {
  min-width: 0;
  flex: 0 0 auto;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 10px;
  flex: 0 0 auto;
}

.header-search {
  display: flex;
  align-items: center;
  gap: 10px;
  width: min(420px, 38vw);
  height: 42px;
  padding: 0 14px;
  border-radius: 6px;
  border: 1px solid rgba(15, 23, 42, 0.12);
  background: #fff;
  color: #64748b;
}

.header-search input {
  width: 100%;
  border: 0;
  outline: 0;
  color: #0f172a;
  background: transparent;
  font: inherit;
}

.header-title {
  color: #111827;
  font-size: 22px;
  font-weight: 900;
}

.icon-button {
  position: relative;
  display: grid;
  place-items: center;
  width: 38px;
  height: 38px;
  border: 0;
  border-radius: 6px;
  background: transparent;
  color: #334155;
  cursor: pointer;
}

.icon-button:hover {
  background: #f1f5f9;
}

.with-badge span {
  position: absolute;
  top: 2px;
  right: 2px;
  min-width: 16px;
  height: 16px;
  padding: 0 4px;
  border-radius: 999px;
  background: #ef4444;
  color: #fff;
  font-size: 10px;
  line-height: 16px;
  font-weight: 700;
}

.notification-item {
  display: grid;
  gap: 8px;
  padding: 14px;
  border: 1px solid rgba(15, 23, 42, 0.1);
  border-radius: 8px;
  background: #fff;
  cursor: pointer;
}

.notification-item.unread {
  border-color: rgba(15, 98, 214, 0.34);
  background: #f8fbff;
}

.notification-item strong {
  color: #0f172a;
  font-size: 14px;
}

.notification-item p {
  margin: 0;
  color: #64748b;
  line-height: 1.5;
}

.notification-topline {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
}

.notification-topline span {
  color: #94a3b8;
  font-size: 11px;
  font-weight: 700;
}

.profile-button {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 184px;
  height: 44px;
  padding: 0 8px 0 6px;
  border: 0;
  border-radius: 6px;
  background: #fff;
  color: #0f172a;
  cursor: pointer;
}

.profile-avatar {
  display: grid;
  place-items: center;
  width: 34px;
  height: 34px;
  border-radius: 50%;
  background: linear-gradient(145deg, #f59e0b, #0f62d6);
  color: #fff;
  font-size: 14px;
  font-weight: 900;
}

.profile-copy {
  display: flex;
  flex: 1;
  min-width: 0;
  flex-direction: column;
  align-items: flex-start;
  line-height: 1.1;
}

.profile-copy strong {
  max-width: 110px;
  overflow: hidden;
  font-size: 13px;
  font-weight: 900;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.profile-copy small {
  color: #64748b;
  font-size: 11px;
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
  .app-header {
    height: auto;
    padding: 14px;
    flex-direction: column;
    align-items: stretch;
  }

  .header-search,
  .header-actions {
    width: 100%;
    justify-content: space-between;
  }

  .profile-button {
    min-width: 0;
  }
}
</style>
