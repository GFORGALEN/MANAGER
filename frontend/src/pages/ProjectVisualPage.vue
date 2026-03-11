<template>
  <div class="page-shell">
    <a-card class="page-card" :bordered="false">
      <template #title>{{ t('progressTitle') }}</template>
      <p class="page-copy">{{ t('progressCopy') }}</p>

      <a-tabs v-model:activeKey="activeView">
        <a-tab-pane key="board" :tab="t('progressBoard')">
          <div class="board-grid">
            <div v-for="column in boardColumns" :key="column.status" class="board-column">
              <div class="board-column-head">
                <div>
                  <div class="board-column-title">{{ column.label }}</div>
                  <div class="board-column-count">{{ column.items.length }}</div>
                </div>
                <a-tag :color="projectStatusColor(column.status)">{{ column.label }}</a-tag>
              </div>

              <a-space direction="vertical" style="width: 100%">
                <a-card v-for="item in column.items" :key="item.projectId" size="small" class="board-card" @click="openProject(item.projectId)">
                  <a-space direction="vertical" style="width: 100%">
                    <div class="card-head">
                      <div>
                        <strong>{{ item.name }}</strong>
                        <div class="muted">#{{ item.code }}</div>
                      </div>
                      <span class="percent">{{ item.progress }}%</span>
                    </div>

                    <a-progress :percent="item.progress" :show-info="false" />

                    <a-space wrap>
                      <a-tag>{{ t('todo') }} {{ item.todoCount }}</a-tag>
                      <a-tag color="processing">{{ t('doing') }} {{ item.doingCount }}</a-tag>
                      <a-tag color="success">{{ t('done') }} {{ item.doneCount }}</a-tag>
                    </a-space>

                    <div class="muted small">{{ item.address }}</div>
                    <div class="muted small">{{ t('progressVariations') }}: {{ item.variationCount }} | {{ t('progressAttachments') }}: {{ item.attachmentCount }}</div>

                    <a-space wrap>
                      <a-button size="small" type="primary" @click.stop="openProject(item.projectId)">{{ t('progressOpenProject') }}</a-button>
                      <a-button size="small" @click.stop="openTasks(item.projectId)">{{ t('progressOpenTasks') }}</a-button>
                    </a-space>
                  </a-space>
                </a-card>
              </a-space>
            </div>
          </div>
        </a-tab-pane>

        <a-tab-pane key="roadmap" :tab="t('progressRoadmap')">
          <a-space direction="vertical" style="width: 100%" size="middle">
            <a-alert :message="t('progressNeedsDates')" type="info" show-icon />
            <a-alert :message="t('progressTaskTimingNote')" type="warning" show-icon />

            <div v-if="roadmapRows.length" class="roadmap-shell">
              <g-gantt-chart
                :chart-start="chartStart"
                :chart-end="chartEnd"
                precision="day"
                bar-start="start"
                bar-end="end"
                row-label-width="220"
                current-time
                @click-bar="handleRoadmapBarClick"
              >
                <g-gantt-row
                  v-for="row in roadmapRows"
                  :key="row.rowId"
                  :label="row.label"
                  :bars="row.bars"
                />
              </g-gantt-chart>
            </div>

            <a-empty v-else :description="t('progressNoRoadmap')" />
          </a-space>
        </a-tab-pane>
      </a-tabs>
    </a-card>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import { useRouter } from 'vue-router'

import api from '@/services/api'
import { useI18n } from '@/services/i18n'
import type { Attachment } from '@/types/attachment'
import type { PagedResult } from '@/types/common'
import type { ProjectListItem } from '@/types/project'
import type { TaskItem } from '@/types/task'
import type { Variation } from '@/types/variation'

type ProgressCard = ProjectListItem & {
  startDate?: string | null
  endDate?: string | null
  taskCount: number
  todoCount: number
  doingCount: number
  doneCount: number
  variationCount: number
  approvedVariationCount: number
  attachmentCount: number
  progress: number
}

type RoadmapRow = {
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
const { t, projectStatusLabel } = useI18n()
const activeView = ref('board')
const projects = ref<ProjectListItem[]>([])
const projectDetails = ref<Record<string, { startDate?: string | null; endDate?: string | null }>>({})
const taskMap = ref<Record<string, TaskItem[]>>({})
const variationMap = ref<Record<string, Variation[]>>({})
const attachmentMap = ref<Record<string, Attachment[]>>({})

const cards = computed<ProgressCard[]>(() =>
  projects.value.map((project) => {
    const tasks = taskMap.value[project.projectId] ?? []
    const variations = variationMap.value[project.projectId] ?? []
    const attachments = attachmentMap.value[project.projectId] ?? []
    const todoCount = tasks.filter((task) => task.status === 'Todo').length
    const doingCount = tasks.filter((task) => task.status === 'Doing').length
    const doneCount = tasks.filter((task) => task.status === 'Done').length
    const progress = tasks.length > 0
      ? Math.round((doneCount / tasks.length) * 100)
      : project.status === 'Completed'
        ? 100
        : project.status === 'Active'
          ? 55
          : project.status === 'OnHold'
            ? 40
            : 15

    return {
      ...project,
      startDate: projectDetails.value[project.projectId]?.startDate,
      endDate: projectDetails.value[project.projectId]?.endDate,
      taskCount: tasks.length,
      todoCount,
      doingCount,
      doneCount,
      variationCount: variations.length,
      approvedVariationCount: variations.filter((item) => item.status === 'Approved').length,
      attachmentCount: attachments.length,
      progress,
    }
  }),
)

const boardColumns = computed(() => ([
  { status: 'Planning', label: projectStatusLabel('Planning'), items: cards.value.filter((item) => item.status === 'Planning') },
  { status: 'Active', label: projectStatusLabel('Active'), items: cards.value.filter((item) => item.status === 'Active') },
  { status: 'OnHold', label: projectStatusLabel('OnHold'), items: cards.value.filter((item) => item.status === 'OnHold') },
  { status: 'Completed', label: projectStatusLabel('Completed'), items: cards.value.filter((item) => item.status === 'Completed') },
]))

const roadmapRows = computed<RoadmapRow[]>(() =>
  cards.value
    .filter((item) => item.startDate && item.endDate)
    .flatMap((item) => {
      const projectRow: RoadmapRow = {
        rowId: `project-${item.projectId}`,
        label: `${item.code} · ${item.name}`,
        bars: [
          {
            start: toGanttDate(item.startDate!),
            end: toGanttDate(item.endDate!),
            ganttBarConfig: {
              id: item.projectId,
              label: `${projectStatusLabel(item.status)} · ${item.progress}%`,
              style: {
                background: statusGradient(item.status),
                color: '#ffffff',
                borderRadius: '999px',
                boxShadow: '0 6px 20px rgba(15, 23, 42, 0.18)',
              },
            },
          },
        ],
      }

      const taskRows: RoadmapRow[] = (taskMap.value[item.projectId] ?? [])
        .filter((task) => task.dueDate)
        .map((task) => {
          const taskStart = getTaskStart(task, item.startDate!)
          const taskEnd = getTaskEnd(task)

          return {
            rowId: `task-${task.taskItemId}`,
            label: `  > ${task.title}`,
            bars: [
              {
                start: toGanttDate(taskStart),
                end: toGanttDate(taskEnd),
                ganttBarConfig: {
                  id: `task:${task.taskItemId}:${item.projectId}`,
                    label: `${task.assignedUsers?.map((user) => user.name).join(', ') || task.assignedUserName || 'Unassigned'} · ${task.status}`,
                  style: {
                    background: taskStatusColor(task.status),
                    color: '#ffffff',
                    borderRadius: '10px',
                    boxShadow: '0 4px 12px rgba(15, 23, 42, 0.12)',
                  },
                },
              },
            ],
          }
        })

      return [projectRow, ...taskRows]
    }),
)

const chartStart = computed(() => {
  const dates = roadmapRows.value.flatMap((row) => row.bars.map((bar) => new Date(bar.start).getTime()))
  if (!dates.length) {
    return toGanttDate(new Date().toISOString())
  }

  const earliest = new Date(Math.min(...dates))
  earliest.setDate(earliest.getDate() - 7)
  return toGanttDate(earliest.toISOString())
})

const chartEnd = computed(() => {
  const dates = roadmapRows.value.flatMap((row) => row.bars.map((bar) => new Date(bar.end).getTime()))
  if (!dates.length) {
    const future = new Date()
    future.setDate(future.getDate() + 30)
    return toGanttDate(future.toISOString())
  }

  const latest = new Date(Math.max(...dates))
  latest.setDate(latest.getDate() + 7)
  return toGanttDate(latest.toISOString())
})

async function fetchProjects() {
  const { data } = await api.get<PagedResult<ProjectListItem>>('/projects', {
    params: { pageNumber: 1, pageSize: 100 },
  })
  projects.value = data.items
}

async function fetchChildren() {
  await Promise.all(projects.value.map(async (project) => {
    const [projectRes, taskRes, variationRes, attachmentRes] = await Promise.all([
      api.get(`/projects/${project.projectId}`),
      api.get<PagedResult<TaskItem>>(`/projects/${project.projectId}/tasks`, { params: { pageNumber: 1, pageSize: 100 } }),
      api.get<PagedResult<Variation>>(`/projects/${project.projectId}/variations`, { params: { pageNumber: 1, pageSize: 100 } }),
      api.get<PagedResult<Attachment>>(`/projects/${project.projectId}/attachments`, { params: { pageNumber: 1, pageSize: 100 } }),
    ])

    projectDetails.value[project.projectId] = {
      startDate: projectRes.data.startDate,
      endDate: projectRes.data.endDate,
    }
    taskMap.value[project.projectId] = taskRes.data.items
    variationMap.value[project.projectId] = variationRes.data.items
    attachmentMap.value[project.projectId] = attachmentRes.data.items
  }))
}

async function load() {
  try {
    await fetchProjects()
    await fetchChildren()
  } catch {
    message.error('Failed to load project progress.')
  }
}

function openProject(projectId: string) {
  router.push({ name: 'project-detail', params: { projectId } })
}

function openTasks(projectId: string) {
  router.push({ name: 'project-detail', params: { projectId }, query: { tab: 'tasks' } })
}

function handleRoadmapBarClick(payload: { bar: { ganttBarConfig: { id: string } } }) {
  const id = payload.bar.ganttBarConfig.id

  if (id.startsWith('task:')) {
    const [, , projectId] = id.split(':')
    openTasks(projectId)
    return
  }

  openProject(id)
}

function projectStatusColor(status: string) {
  return ({
    Planning: 'default',
    Active: 'processing',
    OnHold: 'warning',
    Completed: 'success',
  } as Record<string, string>)[status] ?? 'default'
}

function statusGradient(status: string) {
  return ({
    Planning: 'linear-gradient(135deg, #64748b, #94a3b8)',
    Active: 'linear-gradient(135deg, #2563eb, #06b6d4)',
    OnHold: 'linear-gradient(135deg, #d97706, #f59e0b)',
    Completed: 'linear-gradient(135deg, #16a34a, #22c55e)',
  } as Record<string, string>)[status] ?? 'linear-gradient(135deg, #64748b, #94a3b8)'
}

function taskStatusColor(status: TaskItem['status']) {
  return ({
    Todo: 'linear-gradient(135deg, #64748b, #94a3b8)',
    Doing: 'linear-gradient(135deg, #2563eb, #06b6d4)',
    Done: 'linear-gradient(135deg, #16a34a, #22c55e)',
  } as Record<TaskItem['status'], string>)[status]
}

function getTaskStart(task: TaskItem, projectStart: string) {
  const createdAt = new Date(task.startDate ?? task.createdAt)
  const projectBegin = new Date(projectStart)
  return createdAt < projectBegin ? projectBegin.toISOString() : createdAt.toISOString()
}

function getTaskEnd(task: TaskItem) {
  const dueDate = new Date(task.dueDate)
  const createdAt = new Date(task.createdAt)

  if (dueDate <= createdAt) {
    const fallback = new Date(createdAt)
    fallback.setDate(fallback.getDate() + 1)
    return fallback.toISOString()
  }

  return dueDate.toISOString()
}

function toGanttDate(value: string) {
  const date = new Date(value)
  return `${date.getFullYear()}-${`${date.getMonth() + 1}`.padStart(2, '0')}-${`${date.getDate()}`.padStart(2, '0')} 00:00`
}

onMounted(load)
</script>

<style scoped>
.page-copy {
  margin: 0 0 20px;
  color: #64748b;
}

.board-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
  align-items: start;
}

.board-column {
  padding: 14px;
  border-radius: 18px;
  background: linear-gradient(180deg, rgba(248, 250, 252, 0.96), rgba(241, 245, 249, 0.94));
  border: 1px solid rgba(226, 232, 240, 0.9);
}

.board-column-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 14px;
}

.board-column-title {
  font-weight: 700;
}

.board-column-count {
  color: #64748b;
  font-size: 12px;
}

.board-card {
  border-radius: 16px;
  cursor: pointer;
  border: 1px solid rgba(226, 232, 240, 0.86);
  transition:
    transform 0.18s ease,
    box-shadow 0.18s ease,
    border-color 0.18s ease;
}

.board-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 16px 32px rgba(15, 23, 42, 0.1);
  border-color: rgba(96, 165, 250, 0.28);
}

.card-head {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  align-items: flex-start;
}

.percent {
  font-weight: 700;
  color: #2563eb;
}

.muted {
  color: #64748b;
}

.small {
  font-size: 12px;
}

.roadmap-shell {
  overflow-x: auto;
  padding-bottom: 8px;
}

:deep(.g-gantt-chart) {
  min-width: 980px;
  border: 1px solid rgba(226, 232, 240, 0.9);
  border-radius: 18px;
  overflow: hidden;
  background: rgba(255, 255, 255, 0.94);
  box-shadow: 0 16px 36px rgba(15, 23, 42, 0.08);
}

:deep(.g-gantt-bar) {
  cursor: pointer;
}

@media (max-width: 1200px) {
  .board-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 768px) {
  .board-grid {
    grid-template-columns: 1fr;
  }
}
</style>
