<template>
  <div class="page-shell">
    <div class="dashboard-welcome">
      <div>
        <strong>Welcome back</strong>
        <span>Here's what's happening across your projects today.</span>
      </div>
      <a-space>
        <a-button type="primary" @click="router.push({ name: 'projects' })">+ New Project</a-button>
        <a-button>Customize</a-button>
      </a-space>
    </div>

    <a-spin :spinning="loading">
      <div class="metric-grid">
        <button class="metric-card metric-button" type="button" @click="router.push({ name: 'projects' })">
          <span class="metric-icon blue"><BankOutlined /></span>
          <span>
            <p class="metric-label">Active Projects</p>
            <strong class="metric-value">{{ projects.length }}</strong>
            <small class="metric-note positive">+2 this month</small>
          </span>
        </button>
        <button class="metric-card metric-button" type="button" @click="router.push({ name: 'tasks' })">
          <span class="metric-icon orange"><CheckSquareOutlined /></span>
          <span>
            <p class="metric-label">Open Tasks</p>
            <strong class="metric-value">{{ inFlightCount }}</strong>
            <small class="metric-note warning">+{{ dueSoonCount }} due soon</small>
          </span>
        </button>
        <button class="metric-card metric-button" type="button" @click="router.push({ name: 'variations' })">
          <span class="metric-icon orange"><FileDoneOutlined /></span>
          <span>
            <p class="metric-label">Pending Variations</p>
            <strong class="metric-value">{{ pendingVariationCount }}</strong>
            <small class="metric-note">{{ formatCurrency(pendingVariationValue) }}</small>
          </span>
        </button>
        <button class="metric-card metric-button" type="button" @click="router.push({ name: 'tasks' })">
          <span class="metric-icon red"><ClockCircleOutlined /></span>
          <span>
            <p class="metric-label">Overdue Items</p>
            <strong class="metric-value">{{ overdueTasks.length }}</strong>
            <small class="metric-note danger">needs attention</small>
          </span>
        </button>
        <div class="metric-card">
          <span class="metric-icon steel"><SafetyCertificateOutlined /></span>
          <span>
            <p class="metric-label">Safety Module</p>
            <strong class="metric-value">--</strong>
            <small class="metric-note">not connected</small>
          </span>
        </div>
      </div>

      <div class="dashboard-grid primary-grid">
        <a-card class="page-card project-table-card" :bordered="false">
          <template #title>Projects Overview</template>
          <template #extra><a @click="router.push({ name: 'projects' })">View all projects</a></template>
          <div class="overview-table">
            <div class="overview-row overview-head">
              <span>Project</span>
              <span>Location</span>
              <span>Progress</span>
              <span>Schedule</span>
              <span>Status</span>
            </div>
            <button
              v-for="project in projects.slice(0, 5)"
              :key="project.projectId"
              class="overview-row project-row"
              type="button"
              @click="openProject(project.projectId)"
            >
              <span>
                <strong>{{ project.name }}</strong>
                <small>{{ project.code }}</small>
              </span>
              <span>{{ project.address }}</span>
              <span>
                <i class="progress-track"><b :style="{ width: projectProgress(project.status) + '%' }"></b></i>
                <small>{{ projectProgress(project.status) }}%</small>
              </span>
              <span :class="scheduleClass(project.status)">
                <i class="status-dot"></i>{{ scheduleLabel(project.status) }}
              </span>
              <span><a-tag :color="projectStatusColor(project.status)">{{ projectStatusLabel(project.status) }}</a-tag></span>
            </button>
            <a-empty v-if="!projects.length" :description="t('noProjectsFound')" />
          </div>
        </a-card>

        <a-card class="page-card timeline-card" :bordered="false">
          <template #title>Project Timeline - {{ timelineProjectName }}</template>
          <template #extra>
            <a-space>
              <a-select
                v-if="projects.length"
                v-model:value="selectedTimelineProjectId"
                size="small"
                style="width: 180px"
              >
                <a-select-option v-for="project in projects" :key="project.projectId" :value="project.projectId">
                  {{ project.name }}
                </a-select-option>
              </a-select>
              <a @click="router.push({ name: 'progress' })">View full schedule</a>
            </a-space>
          </template>
          <div class="gantt-hint" v-if="timelineRows.length">Drag horizontally to inspect dates</div>
          <div
            v-if="timelineRows.length"
            ref="ganttScrollRef"
            class="dashboard-gantt-shell"
            :class="{ dragging: isDraggingGantt }"
            @mousedown="startGanttDrag"
            @mouseleave="stopGanttDrag"
            @mouseup="stopGanttDrag"
            @mousemove="moveGanttDrag"
          >
            <g-gantt-chart
              :chart-start="timelineChartStart"
              :chart-end="timelineChartEnd"
              precision="day"
              bar-start="start"
              bar-end="end"
              row-label-width="220"
              current-time
              :style="{ minWidth: `${timelineChartWidth}px` }"
              @click-bar="handleTimelineBarClick"
            >
              <g-gantt-row
                v-for="row in timelineRows"
                :key="row.rowId"
                :label="row.label"
                :bars="row.bars"
              />
            </g-gantt-chart>
          </div>
          <a-empty v-else description="No project dates or scheduled tasks yet">
            <template #description>
              <span>No project dates or scheduled tasks yet</span>
            </template>
            <a-button v-if="selectedTimelineProjectId" type="primary" @click="openTaskProject(selectedTimelineProjectId)">
              Open project tasks
            </a-button>
          </a-empty>
        </a-card>
      </div>

      <div class="dashboard-grid secondary-grid">
        <a-card class="page-card" :bordered="false">
          <template #title>My Tasks ({{ upcomingTasks.length }})</template>
          <template #extra><a @click="router.push({ name: 'tasks' })">View all tasks</a></template>
          <div class="compact-table">
            <div class="compact-row compact-head">
              <span>Task</span><span>Project</span><span>Due</span><span>Status</span>
            </div>
            <button v-for="task in upcomingTasks.slice(0, 6)" :key="task.taskItemId" class="compact-row" type="button" @click="openTaskProject(task.projectId)">
              <strong>{{ task.title }}</strong>
              <span>{{ task.projectName }}</span>
              <span>{{ formatShortDate(task.dueDate) }}</span>
              <span><a-tag :color="taskStatusColor(task.status)">{{ taskStatusLabel(task.status) }}</a-tag></span>
            </button>
            <a-empty v-if="!upcomingTasks.length" :description="t('dashboardNoUpcomingTasks')" />
          </div>
        </a-card>

        <a-card class="page-card" :bordered="false">
          <template #title>Pending Variations ({{ pendingVariations.length }})</template>
          <template #extra><a @click="router.push({ name: 'variations' })">View all</a></template>
          <div class="compact-table variation-table">
            <div class="compact-row compact-head">
              <span>Variation</span><span>Project</span><span>Value</span><span>Status</span>
            </div>
            <button v-for="variation in pendingVariations.slice(0, 6)" :key="variation.variationId" class="compact-row" type="button" @click="openVariationProject(variation.projectId)">
              <strong>{{ variation.title }}</strong>
              <span>{{ variation.projectName }}</span>
              <span>{{ formatCurrency(variation.amount) }}</span>
              <span><a-tag :color="variationStatusColor(variation.status)">{{ variationStatusLabel(variation.status) }}</a-tag></span>
            </button>
            <a-empty v-if="!pendingVariations.length" :description="t('dashboardNoVariations')" />
          </div>
        </a-card>

        <a-card class="page-card" :bordered="false">
          <template #title>Recent Attachments</template>
          <template #extra><a @click="router.push({ name: 'attachments' })">View all</a></template>
          <div class="attachment-list">
            <div v-for="file in recentFiles" :key="file.attachmentId" class="attachment-row">
              <span :class="['file-badge', fileBadgeClass(file)]">{{ fileExtension(file.fileName) }}</span>
              <strong>{{ file.fileName }}</strong>
              <small>{{ file.projectName }}</small>
              <small>{{ formatFileSize(file.fileSize) }}</small>
            </div>
            <a-empty v-if="!recentFiles.length" description="No attachments yet" />
          </div>
        </a-card>
      </div>

      <div class="dashboard-grid bottom-grid">
        <a-card class="page-card" :bordered="false">
          <template #title>Worker Tasks - Today</template>
          <template #extra><a @click="router.push({ name: 'tasks' })">View all workers</a></template>
          <div class="worker-strip">
            <article v-for="worker in workerCards" :key="worker.id" class="worker-card">
              <div class="worker-head">
                <span>{{ worker.initials }}</span>
                <div>
                  <strong>{{ worker.name }}</strong>
                  <small>{{ worker.trade }}</small>
                </div>
              </div>
              <h4>{{ worker.task }}</h4>
              <small>{{ worker.project }}</small>
              <i class="progress-track"><b :style="{ width: worker.progress + '%' }"></b></i>
              <em>{{ worker.progress }}%</em>
            </article>
            <a-empty v-if="!workerCards.length" description="No assigned open tasks" />
          </div>
        </a-card>

        <a-card class="page-card" :bordered="false">
          <template #title>User Management Preview</template>
          <template #extra><a @click="router.push({ name: 'users' })">View all users</a></template>
          <div class="compact-table users-preview">
            <div class="compact-row compact-head">
              <span>User</span><span>Role</span><span>Email</span><span>Status</span>
            </div>
            <div v-for="user in previewUsers" :key="user.userId" class="compact-row">
              <strong>{{ user.name }}</strong>
              <span>{{ user.role }}</span>
              <span>{{ user.email }}</span>
              <span :class="['active-status', { inactive: !user.isActive }]"><i class="status-dot"></i>{{ user.isActive ? 'Active' : 'Inactive' }}</span>
            </div>
            <a-empty v-if="!previewUsers.length" description="No users found" />
          </div>
        </a-card>
      </div>
    </a-spin>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import { useRouter } from 'vue-router'
import {
  BankOutlined,
  CheckSquareOutlined,
  ClockCircleOutlined,
  FileDoneOutlined,
  SafetyCertificateOutlined,
} from '@ant-design/icons-vue'

import api from '@/services/api'
import { useI18n } from '@/services/i18n'
import type { PagedResult } from '@/types/common'
import type { ProjectDetail, ProjectListItem } from '@/types/project'
import type { TaskItem } from '@/types/task'
import type { Variation } from '@/types/variation'
import type { Attachment } from '@/types/attachment'
import type { UserSummary } from '@/types/user'

type VariationWithProject = Variation & { projectName: string }
type AttachmentWithProject = Attachment & { projectName: string }
type TimelineRow = {
  rowId: string
  label: string
  bars: Array<{
    start: string
    end: string
    ganttBarConfig: {
      id: string
      label: string
      style: Record<string, string>
    }
  }>
}

const router = useRouter()
const { t, projectStatusLabel, variationStatusLabel } = useI18n()

const loading = ref(false)
const projects = ref<ProjectListItem[]>([])
const tasks = ref<TaskItem[]>([])
const variations = ref<VariationWithProject[]>([])
const attachments = ref<AttachmentWithProject[]>([])
const users = ref<UserSummary[]>([])
const projectDetails = ref<Record<string, ProjectDetail>>({})
const selectedTimelineProjectId = ref<string | null>(null)
const ganttScrollRef = ref<HTMLDivElement | null>(null)
const isDraggingGantt = ref(false)
const ganttDragStartX = ref(0)
const ganttDragStartScrollLeft = ref(0)

const inFlightCount = computed(() => tasks.value.filter((task) => task.status !== 'Done').length)
const dueSoonCount = computed(() => tasks.value.filter((task) => isDueSoon(task) && task.status !== 'Done').length)
const pendingVariationCount = computed(() => variations.value.filter((variation) => isPendingVariation(variation.status)).length)
const pendingVariationValue = computed(() =>
  variations.value
    .filter((variation) => isPendingVariation(variation.status))
    .reduce((total, variation) => total + variation.amount, 0),
)

const upcomingTasks = computed(() =>
  [...tasks.value]
    .filter((task) => task.status !== 'Done' && !isOverdue(task))
    .sort((left, right) => new Date(left.dueDate).getTime() - new Date(right.dueDate).getTime())
    .slice(0, 8),
)

const pendingVariations = computed(() =>
  variations.value
    .filter((variation) => isPendingVariation(variation.status))
    .slice(0, 8),
)

const overdueTasks = computed(() =>
  [...tasks.value]
    .filter((task) => isOverdue(task))
    .sort((left, right) => new Date(left.dueDate).getTime() - new Date(right.dueDate).getTime())
    .slice(0, 8),
)

const defaultTimelineProject = computed(() => projects.value.find((project) => project.status === 'Active') ?? projects.value[0] ?? null)
const activeProject = computed(() =>
  projects.value.find((project) => project.projectId === selectedTimelineProjectId.value)
  ?? defaultTimelineProject.value
  ?? null,
)
const activeProjectDetail = computed(() => activeProject.value ? projectDetails.value[activeProject.value.projectId] : null)
const timelineProjectName = computed(() => activeProject.value?.name ?? 'No active project')
const timelineTasks = computed(() =>
  tasks.value
    .filter((task) => !activeProject.value || task.projectId === activeProject.value.projectId)
    .sort((left, right) => new Date(left.startDate ?? left.createdAt).getTime() - new Date(right.startDate ?? right.createdAt).getTime())
    .slice(0, 8),
)
const timelineRows = computed<TimelineRow[]>(() =>
  [
    ...(activeProjectDetail.value?.startDate && activeProjectDetail.value?.endDate
      ? [{
          rowId: `project-${activeProjectDetail.value.projectId}`,
          label: `${activeProjectDetail.value.code} · ${activeProjectDetail.value.name}`,
          bars: [
            {
              start: toGanttDate(activeProjectDetail.value.startDate),
              end: toGanttDate(activeProjectDetail.value.endDate),
              ganttBarConfig: {
                id: `project:${activeProjectDetail.value.projectId}`,
                label: `${activeProjectDetail.value.status} · ${projectProgress(activeProjectDetail.value.status)}%`,
                style: {
                  background: projectStatusGradient(activeProjectDetail.value.status),
                  color: '#ffffff',
                  borderRadius: '999px',
                  boxShadow: '0 6px 16px rgba(15, 23, 42, 0.18)',
                  fontWeight: '800',
                },
              },
            },
          ],
        }]
      : []),
    ...timelineTasks.value
    .filter((task) => task.dueDate)
    .map((task) => ({
      rowId: task.taskItemId,
      label: task.title,
      bars: [
        {
          start: toGanttDate(getTaskStart(task)),
          end: toGanttDate(getTaskEnd(task)),
          ganttBarConfig: {
            id: `task:${task.taskItemId}:${task.projectId}`,
            label: `${task.assignedUsers?.map((user) => user.name).join(', ') || task.assignedUserName || 'Unassigned'} · ${task.status}`,
            style: {
              background: taskStatusGradient(task.status),
              color: '#ffffff',
              borderRadius: '999px',
              boxShadow: '0 5px 12px rgba(15, 23, 42, 0.16)',
              fontWeight: '700',
            },
          },
        },
      ],
    })),
  ],
)
const timelineChartStart = computed(() => {
  const dates = timelineRows.value.flatMap((row) => row.bars.map((bar) => new Date(bar.start).getTime()))
  if (!dates.length) return toGanttDate(new Date().toISOString())

  const start = new Date(Math.min(...dates))
  start.setDate(start.getDate() - 5)
  return toGanttDate(start.toISOString())
})
const timelineChartEnd = computed(() => {
  const dates = timelineRows.value.flatMap((row) => row.bars.map((bar) => new Date(bar.end).getTime()))
  if (!dates.length) {
    const fallback = new Date()
    fallback.setDate(fallback.getDate() + 30)
    return toGanttDate(fallback.toISOString())
  }

  const end = new Date(Math.max(...dates))
  end.setDate(end.getDate() + 7)
  return toGanttDate(end.toISOString())
})
const timelineChartWidth = computed(() => {
  const start = new Date(timelineChartStart.value).getTime()
  const end = new Date(timelineChartEnd.value).getTime()
  const days = Math.max(1, Math.ceil((end - start) / (1000 * 60 * 60 * 24)))
  const rowLabelWidth = 220
  const dayWidth = 42

  return Math.max(900, rowLabelWidth + days * dayWidth)
})
const recentFiles = computed(() =>
  [...attachments.value]
    .sort((left, right) => new Date(right.uploadedAt).getTime() - new Date(left.uploadedAt).getTime())
    .slice(0, 5),
)
const workerCards = computed(() =>
  tasks.value
    .filter((task) => task.status !== 'Done')
    .flatMap((task) => {
      const assignees = task.assignedUsers?.length
        ? task.assignedUsers
        : task.assignedUserName
          ? [{ userId: task.assignedUserId ?? task.taskItemId, name: task.assignedUserName, email: '', role: 'Contractor' }]
          : []

      return assignees.map((assignee) => ({
        id: `${task.taskItemId}-${assignee.userId}`,
        initials: initialsFor(assignee.name),
        name: assignee.name,
        trade: assignee.role,
        task: task.title,
        project: task.projectName,
        progress: taskProgress(task.status),
      }))
    })
    .slice(0, 4),
)
const previewUsers = computed(() => users.value.slice(0, 5))

async function fetchDashboard() {
  loading.value = true

  try {
    const { data } = await api.get<PagedResult<ProjectListItem>>('/projects', {
      params: {
        pageNumber: 1,
        pageSize: 100,
      },
    })

    projects.value = data.items

    if (!selectedTimelineProjectId.value && projects.value.length) {
      selectedTimelineProjectId.value = defaultTimelineProject.value?.projectId ?? projects.value[0].projectId
    }

    const [detailResults, taskResults, variationResults, attachmentResults, userResult] = await Promise.all([
      Promise.all(
        projects.value.map(async (project) => {
          const response = await api.get<ProjectDetail>(`/projects/${project.projectId}`)
          return response.data
        }),
      ),
      Promise.all(
      projects.value.map(async (project) => {
        const response = await api.get<PagedResult<TaskItem>>(`/projects/${project.projectId}/tasks`, {
          params: {
            pageNumber: 1,
            pageSize: 100,
          },
        })

        return response.data.items
      }),
      ),
      Promise.all(
        projects.value.map(async (project) => {
          const response = await api.get<PagedResult<Variation>>(`/projects/${project.projectId}/variations`, {
            params: {
              pageNumber: 1,
              pageSize: 100,
            },
          })

          return response.data.items.map((variation) => ({
            ...variation,
            projectName: project.name,
          }))
        }),
      ),
      Promise.all(
        projects.value.map(async (project) => {
          const response = await api.get<PagedResult<Attachment>>(`/projects/${project.projectId}/attachments`, {
            params: {
              pageNumber: 1,
              pageSize: 20,
            },
          })

          return response.data.items.map((attachment) => ({
            ...attachment,
            projectName: project.name,
          }))
        }),
      ),
      api.get<PagedResult<UserSummary>>('/users', {
        params: {
          pageNumber: 1,
          pageSize: 20,
        },
      }),
    ])

    projectDetails.value = Object.fromEntries(detailResults.map((project) => [project.projectId, project]))
    tasks.value = taskResults.flat()
    variations.value = variationResults.flat()
    attachments.value = attachmentResults.flat()
    users.value = userResult.data.items
  } catch {
    message.error(t('dashboardDataFailed'))
  } finally {
    loading.value = false
  }
}

function isDueSoon(task: TaskItem) {
  const now = new Date()
  const dueDate = new Date(task.dueDate)
  const diff = dueDate.getTime() - now.getTime()
  return diff >= 0 && diff <= 1000 * 60 * 60 * 48
}

function isOverdue(task: TaskItem) {
  return task.status !== 'Done' && new Date(task.dueDate).getTime() < Date.now()
}

function isPendingVariation(status: Variation['status']) {
  return status === 'Submitted' || status === 'NeedInfo'
}

function taskStatusLabel(status: TaskItem['status']) {
  if (status === 'Draft') return t('todo')
  if (status === 'InProgress') return t('doing')
  if (status === 'Blocked') return t('blocked')
  return t('done')
}

function taskStatusColor(status: TaskItem['status']) {
  return ({
    Draft: 'default',
    InProgress: 'processing',
    Blocked: 'warning',
    Done: 'success',
  } as Record<TaskItem['status'], string>)[status]
}

function variationStatusColor(status: Variation['status']) {
  return ({
    Submitted: 'processing',
    NeedInfo: 'warning',
    Approved: 'success',
    Rejected: 'error',
    Draft: 'default',
  } as Record<Variation['status'], string>)[status] ?? 'default'
}

function projectStatusColor(status: string) {
  return ({
    Planning: 'default',
    Active: 'processing',
    OnHold: 'warning',
    Completed: 'success',
  } as Record<string, string>)[status] ?? 'default'
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat(undefined, {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: 0,
  }).format(value)
}

function formatShortDate(value: string) {
  return new Date(value).toLocaleDateString(undefined, {
    month: 'short',
    day: 'numeric',
  })
}

function formatFileSize(value: number) {
  if (value >= 1024 * 1024) {
    return `${(value / 1024 / 1024).toFixed(1)} MB`
  }

  if (value >= 1024) {
    return `${Math.round(value / 1024)} KB`
  }

  return `${value} B`
}

function openProject(projectId: string) {
  router.push({ name: 'project-detail', params: { projectId } })
}

function openTaskProject(projectId: string) {
  router.push({ name: 'project-detail', params: { projectId }, query: { tab: 'tasks' } })
}

function openVariationProject(projectId: string) {
  router.push({ name: 'project-detail', params: { projectId }, query: { tab: 'variations' } })
}

function projectProgress(status: string) {
  return ({
    Planning: 12,
    Active: 68,
    OnHold: 45,
    Completed: 100,
  } as Record<string, number>)[status] ?? 30
}

function projectStatusGradient(status: string) {
  return ({
    Planning: 'linear-gradient(135deg, #64748b, #94a3b8)',
    Active: 'linear-gradient(135deg, #1769e0, #0f62d6)',
    OnHold: 'linear-gradient(135deg, #f59e0b, #ea580c)',
    Completed: 'linear-gradient(135deg, #16a34a, #059669)',
  } as Record<string, string>)[status] ?? 'linear-gradient(135deg, #64748b, #94a3b8)'
}

function taskProgress(status: TaskItem['status']) {
  return ({
    Draft: 10,
    InProgress: 55,
    Blocked: 35,
    Done: 100,
  } as Record<TaskItem['status'], number>)[status]
}

function initialsFor(name: string) {
  return name
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase())
    .join('') || 'U'
}

function fileExtension(fileName: string) {
  const extension = fileName.split('.').pop()?.toLowerCase()
  return extension && extension !== fileName.toLowerCase() ? extension.slice(0, 3) : 'file'
}

function fileBadgeClass(file: Attachment) {
  const extension = fileExtension(file.fileName)
  if (extension === 'pdf') return 'pdf'
  if (extension === 'xls' || extension === 'xlsx') return 'xls'
  if (extension === 'dwg') return 'dwg'
  return 'file'
}

function taskStatusGradient(status: TaskItem['status']) {
  return ({
    Draft: 'linear-gradient(135deg, #64748b, #94a3b8)',
    InProgress: 'linear-gradient(135deg, #1769e0, #0f62d6)',
    Blocked: 'linear-gradient(135deg, #f59e0b, #ea580c)',
    Done: 'linear-gradient(135deg, #16a34a, #059669)',
  } as Record<TaskItem['status'], string>)[status]
}

function getTaskStart(task: TaskItem) {
  return task.startDate ?? task.createdAt
}

function getTaskEnd(task: TaskItem) {
  const start = new Date(getTaskStart(task))
  const due = new Date(task.dueDate)

  if (due <= start) {
    const fallback = new Date(start)
    fallback.setDate(fallback.getDate() + 1)
    return fallback.toISOString()
  }

  return task.dueDate
}

function toGanttDate(value: string) {
  const date = new Date(value)
  return `${date.getFullYear()}-${`${date.getMonth() + 1}`.padStart(2, '0')}-${`${date.getDate()}`.padStart(2, '0')} 00:00`
}

function handleTimelineBarClick(payload: { bar: { ganttBarConfig: { id: string } } }) {
  const [type, firstId, projectIdFromTask] = payload.bar.ganttBarConfig.id.split(':')
  const projectId = type === 'project' ? firstId : projectIdFromTask
  if (projectId) {
    openTaskProject(projectId)
  }
}

function startGanttDrag(event: MouseEvent) {
  if (!ganttScrollRef.value) return

  isDraggingGantt.value = true
  ganttDragStartX.value = event.pageX - ganttScrollRef.value.offsetLeft
  ganttDragStartScrollLeft.value = ganttScrollRef.value.scrollLeft
}

function stopGanttDrag() {
  isDraggingGantt.value = false
}

function moveGanttDrag(event: MouseEvent) {
  if (!isDraggingGantt.value || !ganttScrollRef.value) return

  event.preventDefault()
  const x = event.pageX - ganttScrollRef.value.offsetLeft
  const walk = x - ganttDragStartX.value
  ganttScrollRef.value.scrollLeft = ganttDragStartScrollLeft.value - walk
}

function scheduleLabel(status: string) {
  if (status === 'OnHold') return 'At Risk'
  if (status === 'Completed') return 'Complete'
  if (status === 'Planning') return 'Planning'
  return 'On Track'
}

function scheduleClass(status: string) {
  return {
    'schedule-risk': status === 'OnHold',
    'schedule-ok': status === 'Active' || status === 'Completed',
    'schedule-plan': status === 'Planning',
  }
}

onMounted(fetchDashboard)
</script>

<style scoped>
.dashboard-welcome {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  margin-bottom: 20px;
}

.dashboard-welcome strong {
  display: block;
  color: #0f172a;
  font-size: 15px;
  font-weight: 900;
}

.dashboard-welcome span {
  color: #64748b;
  font-size: 13px;
}

.metric-grid {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 16px;
  margin-bottom: 18px;
}

.metric-button {
  width: 100%;
  border: 1px solid var(--line);
  cursor: pointer;
  text-align: left;
}

.metric-button:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-hover);
}

.metric-note.positive {
  color: #047857;
}

.metric-note.warning {
  color: #ea580c;
}

.metric-note.danger {
  color: #dc2626;
}

.dashboard-grid {
  display: grid;
  gap: 18px;
  margin-top: 18px;
}

.primary-grid {
  grid-template-columns: minmax(460px, 1.05fr) minmax(520px, 1.15fr);
}

.secondary-grid {
  grid-template-columns: 1fr 1fr 1fr;
}

.bottom-grid {
  grid-template-columns: minmax(520px, 1.6fr) minmax(380px, 1fr);
}

.overview-table,
.compact-table {
  display: grid;
}

.overview-row {
  display: grid;
  grid-template-columns: 1.4fr 1fr 1fr 0.9fr 0.8fr;
  gap: 12px;
  align-items: center;
  min-height: 52px;
  padding: 10px 2px;
  border: 0;
  border-bottom: 1px solid var(--line);
  background: transparent;
  text-align: left;
}

.overview-head,
.compact-head {
  min-height: 34px;
  color: #475569;
  font-size: 11px;
  font-weight: 900;
  text-transform: uppercase;
}

.project-row {
  cursor: pointer;
}

.project-row:hover {
  background: #f8fbff;
}

.overview-row strong,
.compact-row strong {
  display: block;
  color: #0f172a;
  font-size: 13px;
}

.overview-row small,
.compact-row small {
  display: block;
  color: #64748b;
  font-size: 11px;
}

.progress-track {
  display: block;
  height: 5px;
  margin-bottom: 5px;
  overflow: hidden;
  border-radius: 999px;
  background: #e2e8f0;
}

.progress-track b {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, #0f62d6, #1769e0);
}

.schedule-ok {
  color: #047857;
}

.schedule-risk {
  color: #ea580c;
}

.schedule-plan {
  color: #64748b;
}

.dashboard-gantt-shell {
  overflow-x: auto;
  padding-bottom: 8px;
  cursor: grab;
  user-select: none;
  scrollbar-color: #94a3b8 #e2e8f0;
  scrollbar-width: thin;
}

.dashboard-gantt-shell.dragging {
  cursor: grabbing;
}

.dashboard-gantt-shell::-webkit-scrollbar {
  height: 10px;
}

.dashboard-gantt-shell::-webkit-scrollbar-track {
  border-radius: 999px;
  background: #e2e8f0;
}

.dashboard-gantt-shell::-webkit-scrollbar-thumb {
  border-radius: 999px;
  background: #94a3b8;
}

.gantt-hint {
  margin: -2px 0 8px;
  color: #64748b;
  font-size: 12px;
  font-weight: 700;
}

:deep(.g-gantt-chart) {
  height: 300px;
  overflow: hidden;
  border: 1px solid var(--line);
  border-radius: 8px;
  background: #fff;
}

:deep(.g-gantt-row-label) {
  font-size: 12px;
  font-weight: 800;
}

:deep(.g-gantt-bar) {
  cursor: pointer;
}

.compact-row {
  display: grid;
  grid-template-columns: 1.35fr 1fr 0.8fr 0.8fr;
  gap: 12px;
  align-items: center;
  min-height: 42px;
  padding: 8px 0;
  border: 0;
  border-bottom: 1px solid var(--line);
  background: transparent;
  color: #334155;
  text-align: left;
}

button.compact-row {
  cursor: pointer;
}

button.compact-row:hover {
  background: #f8fbff;
}

.attachment-list {
  display: grid;
  gap: 10px;
}

.attachment-row {
  display: grid;
  grid-template-columns: 38px minmax(0, 1.4fr) 0.8fr 0.45fr;
  gap: 10px;
  align-items: center;
  padding-bottom: 10px;
  border-bottom: 1px solid var(--line);
}

.attachment-row strong {
  overflow: hidden;
  color: #0f172a;
  font-size: 12px;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.attachment-row small {
  color: #64748b;
  font-size: 11px;
}

.file-badge {
  display: grid;
  place-items: center;
  width: 28px;
  height: 28px;
  border-radius: 6px;
  color: #fff;
  font-size: 9px;
  font-weight: 900;
  text-transform: uppercase;
}

.file-badge.pdf { background: #dc2626; }
.file-badge.xls { background: #16a34a; }
.file-badge.dwg { background: #2563eb; }

.worker-strip {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 14px;
}

.worker-card {
  position: relative;
  padding: 14px;
  border: 1px solid var(--line);
  border-radius: 8px;
  background: #fff;
}

.worker-head {
  display: flex;
  align-items: center;
  gap: 10px;
}

.worker-head > span {
  display: grid;
  place-items: center;
  width: 42px;
  height: 42px;
  border-radius: 8px;
  background: linear-gradient(145deg, #f59e0b, #0f62d6);
  color: #fff;
  font-weight: 900;
}

.worker-head strong,
.worker-card h4 {
  margin: 0;
  color: #0f172a;
}

.worker-head small,
.worker-card small {
  color: #64748b;
  font-size: 11px;
}

.worker-card h4 {
  margin-top: 14px;
  font-size: 14px;
}

.worker-card .progress-track {
  margin-top: 14px;
}

.worker-card em {
  color: #334155;
  font-style: normal;
  font-size: 12px;
  font-weight: 800;
}

.active-status {
  color: #047857;
  font-weight: 800;
}

.active-status.inactive {
  color: #64748b;
}

@media (max-width: 1280px) {
  .metric-grid,
  .secondary-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .primary-grid,
  .bottom-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .dashboard-welcome {
    align-items: flex-start;
    flex-direction: column;
  }

  .metric-grid,
  .secondary-grid,
  .worker-strip {
    grid-template-columns: 1fr;
  }

  .overview-row,
  .compact-row,
  .attachment-row {
    grid-template-columns: 1fr;
  }

  .timeline-board {
    overflow-x: auto;
  }
}
</style>
