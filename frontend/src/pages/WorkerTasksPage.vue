<template>
  <div class="page-shell">
    <a-space direction="vertical" size="large" style="width: 100%">
      <a-card class="page-card" :bordered="false">
        <template #title>我的任务</template>
        <template #extra>
          <a-space wrap>
            <a-select v-model:value="query.status" style="width: 180px" @change="fetchTasks">
              <a-select-option value="">全部状态</a-select-option>
              <a-select-option value="Draft">草稿</a-select-option>
              <a-select-option value="InProgress">进行中</a-select-option>
              <a-select-option value="Blocked">阻塞</a-select-option>
              <a-select-option value="Done">已完成</a-select-option>
            </a-select>
            <a-button @click="fetchTasks">刷新</a-button>
          </a-space>
        </template>

        <a-space v-if="tasks.length" direction="vertical" size="middle" style="width: 100%">
          <a-card v-for="task in tasks" :key="task.taskItemId" size="small" class="worker-task-card">
            <a-space direction="vertical" style="width: 100%">
              <div class="task-topline">
                <strong>{{ task.title }}</strong>
                <a-tag :color="statusColorMap[task.status]">{{ statusLabelMap[task.status] }}</a-tag>
              </div>
              <span class="muted">{{ task.projectName }}</span>
              <a-space wrap size="small">
                <a-tag>{{ task.priority }}</a-tag>
                <a-tag v-if="task.category">{{ task.category }}</a-tag>
                <a-tag v-if="task.estimatedHours" color="gold">{{ task.estimatedHours }}h</a-tag>
              </a-space>
              <span v-if="task.description">{{ task.description }}</span>
              <span class="muted">截止：{{ formatDate(task.dueDate) }}</span>
              <div class="task-actions">
                <a-button v-if="task.status === 'Draft'" type="primary" @click="setTaskStatus(task.taskItemId, 'InProgress')">开始</a-button>
                <a-button v-if="task.status === 'InProgress'" @click="setTaskStatus(task.taskItemId, 'Blocked')">标记阻塞</a-button>
                <a-button v-if="task.status !== 'Done'" @click="setTaskStatus(task.taskItemId, 'Done')">完成</a-button>
                <a-button @click="router.push({ name: 'worker-task-detail', params: { id: task.taskItemId } })">打开详情</a-button>
              </div>
            </a-space>
          </a-card>
        </a-space>

        <a-empty v-else description="当前没有分配给你的任务。" />

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
import type { TaskItem, UpdateTaskStatusPayload } from '@/types/task'

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
  Draft: 'default',
  InProgress: 'processing',
  Blocked: 'warning',
  Done: 'success',
}

const statusLabelMap: Record<TaskItem['status'], string> = {
  Draft: '草稿',
  InProgress: '进行中',
  Blocked: '阻塞',
  Done: '已完成',
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
    message.error('加载工人任务失败。')
  } finally {
    loading.value = false
  }
}

async function setTaskStatus(taskId: string, status: UpdateTaskStatusPayload['status']) {
  try {
    await api.patch(`/my/tasks/${taskId}/status`, { status })
    message.success(
      status === 'Done'
        ? '任务已完成。'
        : status === 'Blocked'
          ? '任务已标记为阻塞。'
          : '任务已开始。',
    )
    await fetchTasks()
  } catch {
    message.error('更新任务状态失败。')
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
.worker-task-card {
  border-radius: 18px;
}

.task-topline {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
}

.task-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
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
