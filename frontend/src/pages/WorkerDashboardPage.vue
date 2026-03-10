<template>
  <div class="page-shell">
    <a-space direction="vertical" size="large" style="width: 100%">
      <a-card class="page-card" :bordered="false" :loading="loading">
        <template #title>Today at a glance</template>

        <a-row :gutter="[16, 16]">
          <a-col :xs="24" :sm="8">
            <a-card size="small">
              <a-statistic title="Due Today" :value="todayCount" />
            </a-card>
          </a-col>
          <a-col :xs="24" :sm="8">
            <a-card size="small">
              <a-statistic title="Doing" :value="doingCount" />
            </a-card>
          </a-col>
          <a-col :xs="24" :sm="8">
            <a-card size="small">
              <a-statistic title="Done" :value="doneCount" />
            </a-card>
          </a-col>
        </a-row>
      </a-card>

      <a-card class="page-card" :bordered="false">
        <template #title>Quick Actions</template>

        <a-space direction="vertical" style="width: 100%">
          <a-button type="primary" block size="large" @click="router.push({ name: 'worker-tasks' })">
            Open My Tasks
          </a-button>
          <a-button block size="large" @click="router.push({ name: 'worker-profile' })">
            View My Profile
          </a-button>
        </a-space>
      </a-card>
    </a-space>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { message } from 'ant-design-vue'

import api from '@/services/api'
import type { PagedResult } from '@/types/common'
import type { TaskItem } from '@/types/task'

const router = useRouter()
const loading = ref(false)
const tasks = ref<TaskItem[]>([])

const todayCount = computed(() => {
  const today = new Date().toDateString()
  return tasks.value.filter((task) => new Date(task.dueDate).toDateString() === today).length
})

const doingCount = computed(() => tasks.value.filter((task) => task.status === 'Doing').length)
const doneCount = computed(() => tasks.value.filter((task) => task.status === 'Done').length)

async function fetchTasks() {
  loading.value = true
  try {
    const { data } = await api.get<PagedResult<TaskItem>>('/my/tasks', {
      params: {
        pageNumber: 1,
        pageSize: 100,
      },
    })
    tasks.value = data.items
  } catch {
    message.error('Failed to load worker dashboard.')
  } finally {
    loading.value = false
  }
}

onMounted(fetchTasks)
</script>
