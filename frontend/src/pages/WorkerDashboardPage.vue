<template>
  <div class="worker-page">
    <section class="worker-hero-bar">
      <BulbOutlined />
      <div>
        <strong>{{ greeting }}, {{ currentUserLabel }}!</strong>
        <span>{{ todayLabel }}</span>
      </div>
    </section>

    <a-spin :spinning="loading">
      <div class="worker-metrics">
        <article class="worker-metric">
          <span class="metric-symbol blue"><ClipboardText /></span>
          <div>
            <small>My Tasks</small>
            <strong>{{ openTasks.length }}</strong>
            <em>open assignments</em>
          </div>
        </article>
        <article class="worker-metric">
          <span class="metric-symbol green"><CheckCircleOutlined /></span>
          <div>
            <small>Due Today</small>
            <strong>{{ todayCount }}</strong>
            <em>finish before close</em>
          </div>
        </article>
        <article class="worker-metric">
          <span class="metric-symbol orange"><FileTextOutlined /></span>
          <div>
            <small>Blocked</small>
            <strong>{{ blockedCount }}</strong>
            <em>needs manager help</em>
          </div>
        </article>
        <article class="worker-metric">
          <span class="metric-symbol shield"><SafetyCertificateOutlined /></span>
          <div>
            <small>Completed</small>
            <strong>{{ doneCount }}</strong>
            <em>all time assigned</em>
          </div>
        </article>
      </div>

      <div class="worker-grid">
        <a-card class="page-card task-panel" :bordered="false">
          <template #title>My Tasks</template>
          <template #extra>
            <a-radio-group v-model:value="taskFilter" size="small">
              <a-radio-button value="open">In Progress</a-radio-button>
              <a-radio-button value="today">Today</a-radio-button>
              <a-radio-button value="done">Done</a-radio-button>
            </a-radio-group>
          </template>

          <div class="worker-task-list">
            <button
              v-for="task in filteredTasks"
              :key="task.taskItemId"
              class="worker-task-row"
              type="button"
              @click="openTask(task.taskItemId)"
            >
              <span :class="['task-icon', priorityClass(task.priority)]">
                <component :is="taskIcon(task)" />
              </span>
              <span class="task-main">
                <span class="task-title-line">
                  <a-tag :color="priorityColor(task.priority)">{{ task.priority }}</a-tag>
                  <strong>{{ task.title }}</strong>
                </span>
                <small>{{ task.projectName }} <i v-if="task.category">| {{ task.category }}</i></small>
                <small><CalendarOutlined /> Due: {{ formatDate(task.dueDate) }}</small>
              </span>
              <span class="task-progress">
                <small>{{ taskStatusLabel(task.status) }}</small>
                <i><b :style="{ width: taskProgress(task.status) + '%' }"></b></i>
                <em>{{ taskProgress(task.status) }}%</em>
              </span>
              <RightOutlined />
            </button>
            <a-empty v-if="!filteredTasks.length" description="No tasks in this view" />
          </div>
        </a-card>

        <aside class="worker-side">
          <a-card class="page-card quick-panel" :bordered="false">
            <template #title>Quick Actions</template>
            <div class="quick-actions">
              <button type="button" @click="router.push({ name: 'worker-tasks' })">
                <FileTextOutlined />
                <span>Submit Daily</span>
              </button>
              <button type="button" @click="openFirstTaskForUpload">
                <CameraOutlined />
                <span>Photo Upload</span>
              </button>
              <button type="button" @click="openFirstOpenTask">
                <AlertOutlined />
                <span>Report Issue</span>
              </button>
              <button type="button" @click="router.push({ name: 'worker-profile' })">
                <UserOutlined />
                <span>My Profile</span>
              </button>
            </div>
          </a-card>

          <a-card class="page-card report-panel" :bordered="false">
            <template #title>Task Reports</template>
            <template #extra><a @click="router.push({ name: 'worker-tasks' })">All</a></template>
            <div class="report-list">
              <div v-for="task in recentTasks" :key="task.taskItemId">
                <FileDoneOutlined />
                <strong>{{ task.title }}</strong>
                <small>{{ formatShortDate(task.dueDate) }}</small>
                <a-tag :color="statusColor(task.status)">{{ taskStatusLabel(task.status) }}</a-tag>
              </div>
              <a-empty v-if="!recentTasks.length" description="No recent task reports" />
            </div>
          </a-card>

          <a-card class="page-card notice-panel" :bordered="false">
            <template #title>Notices</template>
            <div class="notice-list">
              <div>
                <a-tag color="error">Important</a-tag>
                <strong>Review safety requirements before starting work</strong>
              </div>
              <div>
                <a-tag color="processing">Notice</a-tag>
                <strong>Upload site photos when work status changes</strong>
              </div>
              <div>
                <a-tag color="warning">Reminder</a-tag>
                <strong>Report blocked tasks before end of day</strong>
              </div>
            </div>
          </a-card>
        </aside>
      </div>
    </a-spin>
  </div>
</template>

<script setup lang="ts">
import { computed, h, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { message } from 'ant-design-vue'
import {
  AlertOutlined,
  BulbOutlined,
  CalendarOutlined,
  CameraOutlined,
  CheckCircleOutlined,
  FileDoneOutlined,
  FileTextOutlined,
  RightOutlined,
  SafetyCertificateOutlined,
  UserOutlined,
} from '@ant-design/icons-vue'

import api from '@/services/api'
import { getCurrentUserLabel } from '@/services/auth'
import type { PagedResult } from '@/types/common'
import type { TaskItem } from '@/types/task'

const ClipboardText = {
  render: () => h(FileTextOutlined),
}

const router = useRouter()
const loading = ref(false)
const tasks = ref<TaskItem[]>([])
const taskFilter = ref<'open' | 'today' | 'done'>('open')
const currentUserLabel = computed(() => getCurrentUserLabel())

const greeting = computed(() => {
  const hour = new Date().getHours()
  if (hour < 12) return 'Good morning'
  if (hour < 18) return 'Good afternoon'
  return 'Good evening'
})

const todayLabel = computed(() =>
  new Date().toLocaleDateString(undefined, {
    weekday: 'long',
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  }),
)

const openTasks = computed(() => tasks.value.filter((task) => task.status !== 'Done'))
const todayCount = computed(() => {
  const today = new Date().toDateString()
  return openTasks.value.filter((task) => new Date(task.dueDate).toDateString() === today).length
})
const blockedCount = computed(() => tasks.value.filter((task) => task.status === 'Blocked').length)
const doneCount = computed(() => tasks.value.filter((task) => task.status === 'Done').length)
const recentTasks = computed(() =>
  [...tasks.value]
    .sort((left, right) => new Date(right.createdAt).getTime() - new Date(left.createdAt).getTime())
    .slice(0, 4),
)

const filteredTasks = computed(() => {
  if (taskFilter.value === 'today') {
    const today = new Date().toDateString()
    return openTasks.value.filter((task) => new Date(task.dueDate).toDateString() === today)
  }

  if (taskFilter.value === 'done') {
    return tasks.value.filter((task) => task.status === 'Done')
  }

  return openTasks.value
    .sort((left, right) => new Date(left.dueDate).getTime() - new Date(right.dueDate).getTime())
    .slice(0, 6)
})

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

function openTask(taskId: string) {
  router.push({ name: 'worker-task-detail', params: { id: taskId } })
}

function openFirstOpenTask() {
  const task = openTasks.value[0]
  if (task) {
    openTask(task.taskItemId)
  } else {
    message.info('No open task available.')
  }
}

function openFirstTaskForUpload() {
  const task = openTasks.value[0]
  if (task) {
    openTask(task.taskItemId)
  } else {
    message.info('No task available for upload.')
  }
}

function taskStatusLabel(status: TaskItem['status']) {
  if (status === 'Draft') return 'Not Started'
  if (status === 'InProgress') return 'In Progress'
  if (status === 'Blocked') return 'Blocked'
  return 'Done'
}

function statusColor(status: TaskItem['status']) {
  return ({
    Draft: 'default',
    InProgress: 'processing',
    Blocked: 'warning',
    Done: 'success',
  } as Record<TaskItem['status'], string>)[status]
}

function taskProgress(status: TaskItem['status']) {
  return ({
    Draft: 0,
    InProgress: 60,
    Blocked: 35,
    Done: 100,
  } as Record<TaskItem['status'], number>)[status]
}

function priorityColor(priority: string) {
  if (priority === 'Critical' || priority === 'High') return 'error'
  if (priority === 'Medium') return 'processing'
  return 'success'
}

function priorityClass(priority: string) {
  if (priority === 'Critical' || priority === 'High') return 'high'
  if (priority === 'Medium') return 'medium'
  return 'low'
}

function taskIcon(task: TaskItem) {
  if (task.status === 'Done') return CheckCircleOutlined
  if (task.status === 'Blocked') return AlertOutlined
  return FileTextOutlined
}

function formatDate(value: string) {
  return new Date(value).toLocaleString(undefined, {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  })
}

function formatShortDate(value: string) {
  return new Date(value).toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
  })
}

onMounted(fetchTasks)
</script>

<style scoped>
.worker-page {
  min-height: calc(100vh - 68px);
  padding: 22px;
  background: #f4f7fb;
}

.worker-hero-bar {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 18px;
  padding: 0 8px;
}

.worker-hero-bar :deep(.anticon) {
  color: #ff8a00;
  font-size: 38px;
}

.worker-hero-bar strong,
.worker-hero-bar span {
  display: block;
}

.worker-hero-bar strong {
  color: #0f172a;
  font-size: 22px;
  font-weight: 900;
}

.worker-hero-bar span {
  color: #64748b;
  font-weight: 700;
}

.worker-metrics {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 14px;
  margin-bottom: 18px;
}

.worker-metric {
  display: flex;
  align-items: center;
  gap: 18px;
  min-height: 122px;
  padding: 22px;
  border: 1px solid var(--line);
  border-radius: 8px;
  background: #fff;
  box-shadow: var(--shadow-card);
}

.metric-symbol {
  display: grid;
  place-items: center;
  width: 56px;
  height: 56px;
  border-radius: 12px;
  color: #fff;
  font-size: 28px;
}

.metric-symbol.blue { background: linear-gradient(145deg, #1769e0, #0f62d6); }
.metric-symbol.green { background: linear-gradient(145deg, #22c55e, #059669); }
.metric-symbol.orange { background: linear-gradient(145deg, #ff8a00, #f05a00); }
.metric-symbol.shield { background: linear-gradient(145deg, #2563eb, #1d4ed8); }

.worker-metric small,
.worker-metric em {
  display: block;
  color: #64748b;
  font-style: normal;
  font-weight: 700;
}

.worker-metric small {
  font-size: 15px;
}

.worker-metric strong {
  display: block;
  color: #0f172a;
  font-size: 34px;
  line-height: 1.1;
  font-weight: 900;
}

.worker-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.55fr) minmax(360px, 0.95fr);
  gap: 18px;
}

.worker-task-list {
  display: grid;
  border: 1px solid var(--line);
  border-radius: 8px;
  overflow: hidden;
}

.worker-task-row {
  display: grid;
  grid-template-columns: 60px minmax(0, 1fr) 140px 24px;
  gap: 16px;
  align-items: center;
  min-height: 112px;
  padding: 18px;
  border: 0;
  border-bottom: 1px solid var(--line);
  background: #fff;
  text-align: left;
  cursor: pointer;
}

.worker-task-row:hover {
  background: #f8fbff;
}

.task-icon {
  display: grid;
  place-items: center;
  width: 56px;
  height: 56px;
  border-radius: 12px;
  color: #fff;
  font-size: 28px;
}

.task-icon.high { background: linear-gradient(145deg, #ff8a00, #f05a00); }
.task-icon.medium { background: linear-gradient(145deg, #1769e0, #0f62d6); }
.task-icon.low { background: linear-gradient(145deg, #22c55e, #059669); }

.task-main {
  display: grid;
  gap: 7px;
  min-width: 0;
}

.task-title-line {
  display: flex;
  align-items: center;
  gap: 10px;
}

.task-title-line strong {
  color: #0f172a;
  font-size: 17px;
  font-weight: 900;
}

.task-main small {
  color: #64748b;
  font-size: 13px;
  font-weight: 700;
}

.task-main small :deep(.anticon) {
  margin-right: 5px;
}

.task-progress {
  display: grid;
  gap: 5px;
  color: #334155;
  font-size: 12px;
  font-weight: 800;
}

.task-progress i {
  display: block;
  height: 6px;
  border-radius: 999px;
  background: #e2e8f0;
  overflow: hidden;
}

.task-progress b {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: #1769e0;
}

.task-progress em {
  font-style: normal;
}

.worker-side {
  display: grid;
  gap: 18px;
  align-content: start;
}

.quick-actions {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 14px;
}

.quick-actions button {
  display: grid;
  gap: 10px;
  place-items: center;
  min-height: 86px;
  border: 0;
  border-radius: 8px;
  background: #f8fafc;
  color: #0f172a;
  font-weight: 900;
  cursor: pointer;
}

.quick-actions button:hover {
  background: #eef6ff;
}

.quick-actions :deep(.anticon) {
  display: grid;
  place-items: center;
  width: 42px;
  height: 42px;
  border-radius: 10px;
  background: linear-gradient(145deg, #1769e0, #0f62d6);
  color: #fff;
  font-size: 22px;
}

.quick-actions button:nth-child(2) :deep(.anticon) {
  background: linear-gradient(145deg, #22c55e, #059669);
}

.quick-actions button:nth-child(3) :deep(.anticon) {
  background: linear-gradient(145deg, #ff8a00, #f05a00);
}

.quick-actions button:nth-child(4) :deep(.anticon) {
  background: linear-gradient(145deg, #8b5cf6, #7c3aed);
}

.report-list,
.notice-list {
  display: grid;
  gap: 12px;
}

.report-list > div {
  display: grid;
  grid-template-columns: 24px minmax(0, 1fr) 80px auto;
  gap: 10px;
  align-items: center;
  padding-bottom: 10px;
  border-bottom: 1px solid var(--line);
}

.report-list strong,
.notice-list strong {
  color: #0f172a;
  font-size: 13px;
}

.report-list small {
  color: #64748b;
  font-weight: 700;
}

.notice-list > div {
  display: grid;
  grid-template-columns: 78px minmax(0, 1fr);
  gap: 10px;
  align-items: center;
  padding-bottom: 12px;
  border-bottom: 1px solid var(--line);
}

@media (max-width: 1280px) {
  .worker-grid,
  .worker-metrics {
    grid-template-columns: 1fr 1fr;
  }
}

@media (max-width: 768px) {
  .worker-page {
    padding: 14px;
  }

  .worker-grid,
  .worker-metrics,
  .quick-actions {
    grid-template-columns: 1fr;
  }

  .worker-task-row,
  .report-list > div,
  .notice-list > div {
    grid-template-columns: 1fr;
  }
}
</style>
