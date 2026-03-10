<template>
  <div class="page-shell">
    <a-space direction="vertical" size="large" style="width: 100%">
      <a-card class="page-card" :bordered="false">
        <template #title>My Tasks</template>
        <template #extra>
          <a-space>
            <a-select v-model:value="query.status" style="width: 160px" @change="fetchTasks">
              <a-select-option value="">All Statuses</a-select-option>
              <a-select-option value="Todo">Todo</a-select-option>
              <a-select-option value="Doing">Doing</a-select-option>
              <a-select-option value="Done">Done</a-select-option>
            </a-select>
            <a-button @click="fetchTasks">Refresh</a-button>
          </a-space>
        </template>

        <a-space v-if="tasks.length" direction="vertical" size="middle" style="width: 100%">
          <a-card v-for="task in tasks" :key="task.taskItemId" size="small">
            <a-space direction="vertical" style="width: 100%">
              <div class="task-topline">
                <strong>{{ task.title }}</strong>
                <a-tag :color="statusColorMap[task.status]">{{ task.status }}</a-tag>
              </div>
              <span class="muted">{{ task.projectName }}</span>
              <span v-if="task.description">{{ task.description }}</span>
              <span class="muted">Due: {{ formatDate(task.dueDate) }}</span>
              <a-button type="primary" block @click="router.push({ name: 'worker-task-detail', params: { id: task.taskItemId } })">
                Open Task
              </a-button>
            </a-space>
          </a-card>
        </a-space>

        <a-empty v-else description="No assigned tasks found." />

        <div class="table-footer">
          <a-pagination
            :current="query.pageNumber"
            :page-size="query.pageSize"
            :total="totalCount"
            show-size-changer
            @change="handlePageChange"
            @showSizeChange="handlePageChange"
          />
        </div>
      </a-card>
    </a-space>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { message } from 'ant-design-vue'

import api from '@/services/api'
import type { PagedResult } from '@/types/common'
import type { TaskItem } from '@/types/task'

const router = useRouter()

const tasks = ref<TaskItem[]>([])
const totalCount = ref(0)
const loading = ref(false)

const query = reactive({
  status: '',
  pageNumber: 1,
  pageSize: 10,
})

const statusColorMap: Record<TaskItem['status'], string> = {
  Todo: 'default',
  Doing: 'processing',
  Done: 'success',
}

async function fetchTasks() {
  loading.value = true
  try {
    const { data } = await api.get<PagedResult<TaskItem>>('/my/tasks', {
      params: {
        status: query.status || undefined,
        pageNumber: query.pageNumber,
        pageSize: query.pageSize,
      },
    })

    tasks.value = data.items
    totalCount.value = data.totalCount
  } catch {
    message.error('Failed to load assigned tasks.')
  } finally {
    loading.value = false
  }
}

function handlePageChange(page: number, pageSize: number) {
  query.pageNumber = page
  query.pageSize = pageSize
  fetchTasks()
}

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}

onMounted(fetchTasks)
</script>

<style scoped>
.task-topline {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
}

.muted {
  color: #64748b;
}

.table-footer {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}
</style>
