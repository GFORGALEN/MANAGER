<template>
  <div class="page-shell">
    <a-space direction="vertical" size="large" style="width: 100%">
      <section class="task-hub-head">
        <div>
          <span class="section-kicker">Cross-project control</span>
          <h1>Global Task Center</h1>
          <p>Track overdue, blocked, owner workload, priority, and SLA watch tasks across every active project.</p>
        </div>
        <a-button type="primary" @click="fetchTasks">Refresh</a-button>
      </section>

      <div class="task-metrics">
        <article class="metric-card">
          <div class="metric-icon red"><WarningOutlined /></div>
          <div>
            <p class="metric-label">Overdue</p>
            <div class="metric-value">{{ stats.overdue }}</div>
            <div class="metric-note">Open tasks past due</div>
          </div>
        </article>
        <article class="metric-card">
          <div class="metric-icon steel"><StopOutlined /></div>
          <div>
            <p class="metric-label">Blocked</p>
            <div class="metric-value">{{ stats.blocked }}</div>
            <div class="metric-note">Needs management action</div>
          </div>
        </article>
        <article class="metric-card">
          <div class="metric-icon orange"><FieldTimeOutlined /></div>
          <div>
            <p class="metric-label">SLA Watch</p>
            <div class="metric-value">{{ stats.slaWatch }}</div>
            <div class="metric-note">Due inside 72 hours</div>
          </div>
        </article>
        <article class="metric-card">
          <div class="metric-icon blue"><TeamOutlined /></div>
          <div>
            <p class="metric-label">Assigned</p>
            <div class="metric-value">{{ stats.assigned }}</div>
            <div class="metric-note">Tasks with owners</div>
          </div>
        </article>
      </div>

      <a-card class="page-card" :bordered="false">
        <template #title>Task Filters</template>
        <a-space wrap class="hub-toolbar">
          <a-select v-model:value="query.dueState" style="width: 150px" @change="handleFilterChange">
            <a-select-option value="">All work</a-select-option>
            <a-select-option value="overdue">Overdue</a-select-option>
            <a-select-option value="blocked">Blocked</a-select-option>
            <a-select-option value="slaWatch">SLA Watch</a-select-option>
          </a-select>
          <a-select v-model:value="query.status" style="width: 150px" @change="handleFilterChange">
            <a-select-option value="">All Statuses</a-select-option>
            <a-select-option value="Draft">Draft</a-select-option>
            <a-select-option value="InProgress">In Progress</a-select-option>
            <a-select-option value="Blocked">Blocked</a-select-option>
            <a-select-option value="Done">Done</a-select-option>
          </a-select>
          <a-select v-model:value="query.priority" style="width: 150px" @change="handleFilterChange">
            <a-select-option value="">All Priorities</a-select-option>
            <a-select-option value="Critical">Critical</a-select-option>
            <a-select-option value="High">High</a-select-option>
            <a-select-option value="Medium">Medium</a-select-option>
            <a-select-option value="Low">Low</a-select-option>
          </a-select>
          <a-select v-model:value="query.projectId" show-search option-filter-prop="label" style="width: 220px" @change="handleFilterChange">
            <a-select-option value="" label="All Projects">All Projects</a-select-option>
            <a-select-option v-for="project in projects" :key="project.projectId" :value="project.projectId" :label="project.name">
              {{ project.name }}
            </a-select-option>
          </a-select>
          <a-select v-model:value="query.assignedUserId" show-search option-filter-prop="label" style="width: 220px" @change="handleFilterChange">
            <a-select-option value="" label="All Owners">All Owners</a-select-option>
            <a-select-option v-for="user in users" :key="user.userId" :value="user.userId" :label="user.name">
              {{ user.name }}
            </a-select-option>
          </a-select>
        </a-space>
      </a-card>

      <a-card class="page-card" :bordered="false">
        <template #title>Management View</template>
        <a-table
          :columns="columns"
          :data-source="tasks"
          :loading="loading"
          row-key="taskItemId"
          :pagination="false"
          :scroll="{ x: 1120 }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'title'">
              <strong>{{ record.title }}</strong>
              <div class="muted">{{ record.projectName }}</div>
            </template>
            <template v-else-if="column.key === 'status'">
              <a-tag :color="taskStatusColor(record.status)">{{ statusLabel(record.status) }}</a-tag>
            </template>
            <template v-else-if="column.key === 'priority'">
              <a-tag :color="taskPriorityColor(record.priority)">{{ record.priority }}</a-tag>
            </template>
            <template v-else-if="column.key === 'assignedUsers'">
              <a-space wrap>
                <a-tag v-for="user in record.assignedUsers" :key="user.userId">{{ user.name }}</a-tag>
                <span v-if="record.assignedUsers.length === 0" class="muted">Unassigned</span>
              </a-space>
            </template>
            <template v-else-if="column.key === 'dueDate'">
              <span :class="{ danger: isOverdue(record), warning: isSlaWatch(record) }">{{ formatDate(record.dueDate) }}</span>
            </template>
            <template v-else-if="column.key === 'actions'">
              <a-space>
                <a-button size="small" @click="router.push({ name: 'project-detail', params: { projectId: record.projectId }, query: { tab: 'tasks' } })">
                  Project
                </a-button>
                <a-select :value="record.status" size="small" style="width: 128px" @change="(value) => updateTaskStatus(record.taskItemId, value)">
                  <a-select-option value="Draft">Draft</a-select-option>
                  <a-select-option value="InProgress">In Progress</a-select-option>
                  <a-select-option value="Blocked">Blocked</a-select-option>
                  <a-select-option value="Done">Done</a-select-option>
                </a-select>
              </a-space>
            </template>
          </template>
        </a-table>

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

      <a-card class="page-card" :bordered="false">
        <template #title>Recent Audit</template>
        <a-timeline>
          <a-timeline-item v-for="item in auditLogs" :key="item.auditLogId">
            <strong>{{ item.actorName }}</strong>
            <span> {{ item.action }} {{ item.entityType }}</span>
            <div class="muted">{{ item.summary || auditChangeText(item) }} / {{ formatDate(item.createdAt) }}</div>
          </a-timeline-item>
        </a-timeline>
      </a-card>
    </a-space>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { message } from 'ant-design-vue'
import { FieldTimeOutlined, StopOutlined, TeamOutlined, WarningOutlined } from '@ant-design/icons-vue'

import api from '@/services/api'
import type { PagedResult } from '@/types/common'
import type { ProjectListItem } from '@/types/project'
import type { TaskItem, UpdateTaskStatusPayload } from '@/types/task'
import type { UserSummary } from '@/types/user'

interface AuditLog {
  auditLogId: string
  actorName: string
  entityType: string
  action: string
  fieldName?: string | null
  oldValue?: string | null
  newValue?: string | null
  summary?: string | null
  createdAt: string
}

const router = useRouter()
const tasks = ref<TaskItem[]>([])
const projects = ref<ProjectListItem[]>([])
const users = ref<UserSummary[]>([])
const auditLogs = ref<AuditLog[]>([])
const totalCount = ref(0)
const loading = ref(false)

const query = reactive({
  pageNumber: 1,
  pageSize: 10,
  status: '',
  priority: '',
  projectId: '',
  assignedUserId: '',
  dueState: '',
  sortBy: 'dueDate',
  sortOrder: 'asc',
})

const columns = [
  { title: 'Task', dataIndex: 'title', key: 'title', width: 280 },
  { title: 'Status', dataIndex: 'status', key: 'status', width: 140 },
  { title: 'Priority', dataIndex: 'priority', key: 'priority', width: 120 },
  { title: 'Owners', dataIndex: 'assignedUsers', key: 'assignedUsers', width: 240 },
  { title: 'Due', dataIndex: 'dueDate', key: 'dueDate', width: 180 },
  { title: 'Actions', key: 'actions', width: 260 },
]

const stats = computed(() => ({
  overdue: tasks.value.filter(isOverdue).length,
  blocked: tasks.value.filter((task) => task.status === 'Blocked').length,
  slaWatch: tasks.value.filter(isSlaWatch).length,
  assigned: tasks.value.filter((task) => task.assignedUsers.length > 0).length,
}))

async function fetchTasks() {
  loading.value = true
  try {
    const { data } = await api.get<PagedResult<TaskItem>>('/tasks', {
      params: {
        ...query,
        status: query.status || undefined,
        priority: query.priority || undefined,
        projectId: query.projectId || undefined,
        assignedUserId: query.assignedUserId || undefined,
        dueState: query.dueState || undefined,
      },
    })
    tasks.value = data.items
    totalCount.value = data.totalCount
  } catch {
    message.error('Failed to load global tasks.')
  } finally {
    loading.value = false
  }
}

async function fetchLookups() {
  const [projectResponse, userResponse] = await Promise.all([
    api.get<PagedResult<ProjectListItem>>('/projects', { params: { pageNumber: 1, pageSize: 100 } }),
    api.get<PagedResult<UserSummary>>('/users', { params: { pageNumber: 1, pageSize: 100, isActive: true } }),
  ])
  projects.value = projectResponse.data.items
  users.value = userResponse.data.items
}

async function fetchAuditLogs() {
  try {
    const { data } = await api.get<PagedResult<AuditLog>>('/audit-logs', {
      params: { pageNumber: 1, pageSize: 8 },
    })
    auditLogs.value = data.items
  } catch {
    auditLogs.value = []
  }
}

async function updateTaskStatus(taskId: string, status: UpdateTaskStatusPayload['status']) {
  try {
    await api.patch(`/tasks/${taskId}/status`, { status })
    message.success('Task status updated.')
    await Promise.all([fetchTasks(), fetchAuditLogs()])
  } catch {
    message.error('Failed to update task status.')
  }
}

function handleFilterChange() {
  query.pageNumber = 1
  fetchTasks()
}

function handlePageChange(page: number, pageSize: number) {
  query.pageNumber = page
  query.pageSize = pageSize
  fetchTasks()
}

function isOverdue(task: TaskItem) {
  return task.status !== 'Done' && new Date(task.dueDate).getTime() < Date.now()
}

function isSlaWatch(task: TaskItem) {
  const due = new Date(task.dueDate).getTime()
  return task.status !== 'Done' && due >= Date.now() && due <= Date.now() + 72 * 60 * 60 * 1000
}

function taskStatusColor(status: string) {
  return ({ Draft: 'default', InProgress: 'processing', Blocked: 'error', Done: 'success' } as Record<string, string>)[status] ?? 'default'
}

function statusLabel(status: string) {
  return ({ InProgress: 'In Progress' } as Record<string, string>)[status] ?? status
}

function taskPriorityColor(priority: string) {
  return ({ Low: 'default', Medium: 'blue', High: 'orange', Critical: 'red' } as Record<string, string>)[priority] ?? 'default'
}

function auditChangeText(item: AuditLog) {
  if (!item.fieldName) return item.action
  return `${item.fieldName}: ${item.oldValue || '-'} -> ${item.newValue || '-'}`
}

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}

onMounted(async () => {
  await Promise.all([fetchLookups(), fetchTasks(), fetchAuditLogs()])
})
</script>

<style scoped>
.task-hub-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 18px;
}

.section-kicker {
  color: #64748b;
  font-size: 11px;
  font-weight: 900;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.task-hub-head h1 {
  margin: 6px 0 6px;
  font-size: 32px;
  line-height: 1;
}

.task-hub-head p {
  max-width: 740px;
  margin: 0;
  color: #64748b;
  font-weight: 600;
}

.task-metrics {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 14px;
}

.hub-toolbar {
  align-items: center;
}

.table-footer {
  display: flex;
  justify-content: flex-end;
  margin-top: 16px;
}

.muted {
  color: #64748b;
}

.danger {
  color: #dc2626;
  font-weight: 800;
}

.warning {
  color: #d97706;
  font-weight: 800;
}

@media (max-width: 900px) {
  .task-hub-head {
    flex-direction: column;
  }

  .task-metrics {
    grid-template-columns: 1fr;
  }
}
</style>
