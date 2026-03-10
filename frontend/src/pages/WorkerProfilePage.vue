<template>
  <div class="page-shell">
    <a-card class="page-card" :bordered="false" :loading="loading">
      <template #title>My Profile</template>
      <template #extra>
        <a-button danger @click="logout">Logout</a-button>
      </template>

      <a-descriptions v-if="currentUser" :column="1" bordered>
        <a-descriptions-item label="Name">{{ currentUser.name }}</a-descriptions-item>
        <a-descriptions-item label="Email">{{ currentUser.email }}</a-descriptions-item>
        <a-descriptions-item label="Phone Number">{{ currentUser.phoneNumber || 'Not provided' }}</a-descriptions-item>
        <a-descriptions-item label="Role">{{ currentUser.role }}</a-descriptions-item>
        <a-descriptions-item label="Active">{{ currentUser.isActive ? 'Yes' : 'No' }}</a-descriptions-item>
      </a-descriptions>
    </a-card>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { message } from 'ant-design-vue'

import api from '@/services/api'
import { clearSession, getCurrentUser, setCurrentUser } from '@/services/auth'
import type { CurrentUser } from '@/types/auth'

const router = useRouter()
const currentUser = ref<CurrentUser | null>(getCurrentUser())
const loading = ref(false)

async function fetchProfile() {
  loading.value = true
  try {
    const { data } = await api.get<CurrentUser>('/auth/me')
    currentUser.value = data
    setCurrentUser(data)
  } catch {
    message.error('Failed to load profile.')
  } finally {
    loading.value = false
  }
}

function logout() {
  clearSession()
  router.push({ name: 'login' })
}

onMounted(fetchProfile)
</script>
