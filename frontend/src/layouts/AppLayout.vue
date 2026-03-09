<template>
  <a-layout style="min-height: 100vh">
    <a-layout-sider
      v-model:collapsed="collapsed"
      collapsible
      breakpoint="lg"
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
        <a-menu-item key="/projects">
          <template #icon>
            <ProjectOutlined />
          </template>
          Projects
        </a-menu-item>
        <a-menu-item key="/tasks">
          <template #icon>
            <CheckSquareOutlined />
          </template>
          Tasks
        </a-menu-item>
        <a-menu-item key="/variations">
          <template #icon>
            <ProfileOutlined />
          </template>
          Variations
        </a-menu-item>
        <a-menu-item key="/attachments">
          <template #icon>
            <PaperClipOutlined />
          </template>
          Attachments
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
          <a-tag color="blue">{{ currentUser.role }}</a-tag>
          <a-dropdown>
            <a-button>
              {{ currentUser.username }}
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
  CheckSquareOutlined,
  DownOutlined,
  PaperClipOutlined,
  ProfileOutlined,
  ProjectOutlined,
} from '@ant-design/icons-vue'

import { clearSession, getCurrentUser } from '@/services/auth'

const route = useRoute()
const router = useRouter()
const collapsed = ref(false)

const currentUser = computed(() => getCurrentUser())
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
  background: linear-gradient(135deg, #1677ff, #0f4fb8);
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
  border-bottom: 1px solid #e5e7eb;
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
</style>
