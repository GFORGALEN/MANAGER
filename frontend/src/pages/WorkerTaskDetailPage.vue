<template>
  <div class="page-shell">
    <a-space direction="vertical" size="large" style="width: 100%">
      <a-card class="page-card" :bordered="false" :loading="loading">
        <template #title>Task Detail</template>
        <template #extra>
          <a-button @click="router.push({ name: 'worker-tasks' })">Back</a-button>
        </template>

        <a-descriptions v-if="task" :column="1" bordered>
          <a-descriptions-item label="Title">{{ task.title }}</a-descriptions-item>
          <a-descriptions-item label="Project">{{ task.projectName }}</a-descriptions-item>
          <a-descriptions-item label="Project Address">{{ task.projectAddress }}</a-descriptions-item>
          <a-descriptions-item label="Description">{{ task.description || 'No description' }}</a-descriptions-item>
          <a-descriptions-item label="Due Date">{{ formatDate(task.dueDate) }}</a-descriptions-item>
          <a-descriptions-item label="Status">
            <a-space>
              <a-select v-model:value="selectedStatus" style="width: 160px">
                <a-select-option value="Todo">Todo</a-select-option>
                <a-select-option value="Doing">Doing</a-select-option>
                <a-select-option value="Done">Done</a-select-option>
              </a-select>
              <a-button type="primary" :loading="savingStatus" @click="updateStatus">Update</a-button>
            </a-space>
          </a-descriptions-item>
        </a-descriptions>
      </a-card>

      <a-card class="page-card" :bordered="false">
        <template #title>Attachments</template>

        <a-list v-if="task?.attachments.length" :data-source="task.attachments" item-layout="horizontal">
          <template #renderItem="{ item }">
            <a-list-item>
              <a-list-item-meta :title="item.fileName" :description="item.filePath" />
            </a-list-item>
          </template>
        </a-list>

        <a-empty v-else description="No attachments available." />
      </a-card>
    </a-space>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { message } from 'ant-design-vue'

import api from '@/services/api'
import type { TaskDetail, UpdateTaskStatusPayload } from '@/types/task'

const route = useRoute()
const router = useRouter()

const task = ref<TaskDetail | null>(null)
const selectedStatus = ref<UpdateTaskStatusPayload['status']>('Todo')
const loading = ref(false)
const savingStatus = ref(false)

async function fetchTask() {
  loading.value = true
  try {
    const { data } = await api.get<TaskDetail>(`/my/tasks/${route.params.id}`)
    task.value = data
    selectedStatus.value = data.status
  } catch {
    message.error('Failed to load task detail.')
  } finally {
    loading.value = false
  }
}

async function updateStatus() {
  savingStatus.value = true
  try {
    const { data } = await api.patch<TaskDetail>(`/my/tasks/${route.params.id}/status`, {
      status: selectedStatus.value,
    })
    task.value = data
    message.success('Task status updated.')
  } catch {
    message.error('Failed to update task status.')
  } finally {
    savingStatus.value = false
  }
}

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}

onMounted(fetchTask)
</script>
