<template>
  <div class="page-shell">
    <a-space direction="vertical" size="large" style="width: 100%">
      <a-card class="page-card dashboard-hero" :bordered="false" :loading="loading">
        <div class="dashboard-hero-copy">
          <span class="dashboard-eyebrow">{{ t('dashboardTitle') }}</span>
          <h2>{{ t('dashboardHeroTitle') }}</h2>
          <p>{{ t('dashboardHeroCopy') }}</p>
        </div>

        <div class="dashboard-stat-grid">
          <button class="dashboard-stat-card dashboard-stat-button" type="button" @click="router.push({ name: 'projects' })">
            <span>{{ t('dashboardProjects') }}</span>
            <strong>{{ projects.length }}</strong>
          </button>
          <button class="dashboard-stat-card dashboard-stat-button" type="button" @click="router.push({ name: 'tasks' })">
            <span>{{ t('dashboardTasksInFlight') }}</span>
            <strong>{{ inFlightCount }}</strong>
          </button>
          <button class="dashboard-stat-card dashboard-stat-button" type="button" @click="router.push({ name: 'tasks' })">
            <span>{{ t('dashboardDueSoon') }}</span>
            <strong>{{ dueSoonCount }}</strong>
          </button>
          <button class="dashboard-stat-card dashboard-stat-button" type="button" @click="router.push({ name: 'variations' })">
            <span>{{ t('dashboardPendingVariations') }}</span>
            <strong>{{ pendingVariationCount }}</strong>
          </button>
        </div>
      </a-card>

      <a-card class="page-card" :bordered="false" :loading="loading">
        <div class="focus-header">
          <div>
            <h3>{{ t('dashboardFocusTitle') }}</h3>
          </div>
        </div>

        <div class="focus-grid">
          <section class="focus-column">
            <div class="focus-column-head">
              <h4>{{ t('dashboardUpcomingTasks') }}</h4>
              <span>{{ upcomingTasks.length }}</span>
            </div>
            <div v-if="upcomingTasks.length" class="focus-list">
              <article v-for="task in upcomingTasks" :key="task.taskItemId" class="focus-item">
                <div class="focus-item-topline">
                  <strong>{{ task.title }}</strong>
                  <a-tag :color="taskStatusColor(task.status)">{{ taskStatusLabel(task.status) }}</a-tag>
                </div>
                <div class="focus-meta">{{ task.projectName }}</div>
                <div class="focus-copy">{{ dueLabel(task.dueDate) }} · {{ formatDate(task.dueDate) }}</div>
                <a-button class="focus-action" @click="openTaskProject(task.projectId)">{{ t('dashboardOpenTasks') }}</a-button>
              </article>
            </div>
            <a-empty v-else :description="t('dashboardNoUpcomingTasks')" />
          </section>

          <section class="focus-column">
            <div class="focus-column-head">
              <h4>{{ t('dashboardVariationQueue') }}</h4>
              <span>{{ pendingVariations.length }}</span>
            </div>
            <div v-if="pendingVariations.length" class="focus-list">
              <article v-for="variation in pendingVariations" :key="variation.variationId" class="focus-item">
                <div class="focus-item-topline">
                  <strong>{{ variation.title }}</strong>
                  <a-tag :color="variationStatusColor(variation.status)">{{ variationStatusLabel(variation.status) }}</a-tag>
                </div>
                <div class="focus-meta">{{ variation.projectName }}</div>
                <div class="focus-copy">{{ t('amount') }}: {{ formatCurrency(variation.amount) }}</div>
                <a-button class="focus-action" @click="openVariationProject(variation.projectId)">{{ t('dashboardOpenVariations') }}</a-button>
              </article>
            </div>
            <a-empty v-else :description="t('dashboardNoVariations')" />
          </section>

          <section class="focus-column">
            <div class="focus-column-head">
              <h4>{{ t('dashboardProjectAttention') }}</h4>
              <span>{{ attentionProjects.length }}</span>
            </div>
            <div v-if="attentionProjects.length" class="focus-list">
              <article v-for="project in attentionProjects" :key="project.projectId" class="focus-item">
                <div class="focus-item-topline">
                  <strong>{{ project.name }}</strong>
                  <a-tag :color="projectStatusColor(project.status)">{{ projectStatusLabel(project.status) }}</a-tag>
                </div>
                <div class="focus-meta">{{ project.address }}</div>
                <div class="focus-copy">{{ projectAttentionHint(project.status) }}</div>
                <a-button class="focus-action" @click="openProject(project.projectId)">{{ t('dashboardOpenProject') }}</a-button>
              </article>
            </div>
            <a-empty v-else :description="t('dashboardNoAttentionProjects')" />
          </section>
        </div>
      </a-card>

      <a-card class="page-card" :bordered="false" :loading="loading">
        <div class="focus-header">
          <div>
            <h3>{{ t('dashboardTaskHistory') }}</h3>
          </div>
        </div>

        <div class="history-grid">
          <section class="focus-column">
            <div class="focus-column-head">
              <h4>{{ t('dashboardCompletedTasks') }}</h4>
              <span>{{ completedTasks.length }}</span>
            </div>
            <div v-if="completedTasks.length" class="focus-list">
              <article v-for="task in completedTasks" :key="task.taskItemId" class="focus-item">
                <div class="focus-item-topline">
                  <strong>{{ task.title }}</strong>
                  <a-tag color="success">{{ taskStatusLabel(task.status) }}</a-tag>
                </div>
                <div class="focus-meta">{{ task.projectName }}</div>
                <div class="focus-copy">{{ t('dashboardCompletedSummary') }} · {{ t('dashboardDueLabel') }} {{ formatDate(task.dueDate) }}</div>
                <a-button class="focus-action" @click="openTaskProject(task.projectId)">{{ t('dashboardOpenTasks') }}</a-button>
              </article>
            </div>
            <a-empty v-else :description="t('dashboardNoCompletedTasks')" />
          </section>

          <section class="focus-column">
            <div class="focus-column-head">
              <h4>{{ t('dashboardOverdueTasks') }}</h4>
              <span>{{ overdueTasks.length }}</span>
            </div>
            <div v-if="overdueTasks.length" class="focus-list">
              <article v-for="task in overdueTasks" :key="task.taskItemId" class="focus-item">
                <div class="focus-item-topline">
                  <strong>{{ task.title }}</strong>
                  <a-tag color="error">{{ t('dashboardOverdue') }}</a-tag>
                </div>
                <div class="focus-meta">{{ task.projectName }}</div>
                <div class="focus-copy">{{ t('dashboardDueLabel') }} {{ formatDate(task.dueDate) }}</div>
                <a-button class="focus-action" @click="openTaskProject(task.projectId)">{{ t('dashboardOpenTasks') }}</a-button>
              </article>
            </div>
            <a-empty v-else :description="t('dashboardNoOverdueTasks')" />
          </section>
        </div>
      </a-card>
    </a-space>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import { useRouter } from 'vue-router'

import api from '@/services/api'
import { useI18n } from '@/services/i18n'
import type { PagedResult } from '@/types/common'
import type { ProjectListItem } from '@/types/project'
import type { TaskItem } from '@/types/task'
import type { Variation } from '@/types/variation'

type VariationWithProject = Variation & { projectName: string }

const router = useRouter()
const { t, projectStatusLabel, variationStatusLabel } = useI18n()

const loading = ref(false)
const projects = ref<ProjectListItem[]>([])
const tasks = ref<TaskItem[]>([])
const variations = ref<VariationWithProject[]>([])

const inFlightCount = computed(() => tasks.value.filter((task) => task.status !== 'Done').length)
const dueSoonCount = computed(() => tasks.value.filter((task) => isDueSoon(task) && task.status !== 'Done').length)
const pendingVariationCount = computed(() => variations.value.filter((variation) => isPendingVariation(variation.status)).length)

const upcomingTasks = computed(() =>
  [...tasks.value]
    .filter((task) => task.status !== 'Done' && !isOverdue(task))
    .sort((left, right) => new Date(left.dueDate).getTime() - new Date(right.dueDate).getTime())
    .slice(0, 5),
)

const pendingVariations = computed(() =>
  variations.value
    .filter((variation) => isPendingVariation(variation.status))
    .slice(0, 5),
)

const attentionProjects = computed(() =>
  projects.value
    .filter((project) => project.status === 'OnHold' || project.status === 'Planning')
    .slice(0, 5),
)

const completedTasks = computed(() =>
  [...tasks.value]
    .filter((task) => task.status === 'Done')
    .sort((left, right) => new Date(right.dueDate).getTime() - new Date(left.dueDate).getTime())
    .slice(0, 6),
)

const overdueTasks = computed(() =>
  [...tasks.value]
    .filter((task) => isOverdue(task))
    .sort((left, right) => new Date(left.dueDate).getTime() - new Date(right.dueDate).getTime())
    .slice(0, 6),
)

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

    const taskResults = await Promise.all(
      projects.value.map(async (project) => {
        const response = await api.get<PagedResult<TaskItem>>(`/projects/${project.projectId}/tasks`, {
          params: {
            pageNumber: 1,
            pageSize: 100,
          },
        })

        return response.data.items
      }),
    )

    const variationResults = await Promise.all(
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
    )

    tasks.value = taskResults.flat()
    variations.value = variationResults.flat()
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
  if (status === 'Todo') return t('todo')
  if (status === 'Doing') return t('doing')
  return t('done')
}

function taskStatusColor(status: TaskItem['status']) {
  return ({
    Todo: 'default',
    Doing: 'processing',
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

function dueLabel(value: string) {
  const now = new Date()
  const dueDate = new Date(value)
  const diffDays = Math.ceil((dueDate.getTime() - now.getTime()) / (1000 * 60 * 60 * 24))

  if (diffDays < 0) return t('dashboardOverdue')
  if (diffDays === 0) return t('dashboardDueToday')
  if (diffDays === 1) return t('dashboardDueTomorrow')
  return `${diffDays}d`
}

function projectAttentionHint(status: string) {
  if (status === 'Planning') return t('dashboardPlanningHint')
  if (status === 'OnHold') return t('dashboardOnHoldHint')
  return t('dashboardActiveHint')
}

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat(undefined, {
    style: 'currency',
    currency: 'USD',
    maximumFractionDigits: 0,
  }).format(value)
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

onMounted(fetchDashboard)
</script>

<style scoped>
.dashboard-hero {
  overflow: hidden;
}

.dashboard-hero-copy h2 {
  margin: 12px 0 8px;
  font-size: clamp(30px, 5vw, 46px);
  line-height: 0.98;
  letter-spacing: -0.06em;
}

.dashboard-hero-copy p {
  max-width: 680px;
  margin: 0;
  color: var(--text-soft);
}

.dashboard-eyebrow {
  display: inline-flex;
  padding: 6px 11px;
  border-radius: 999px;
  background: rgba(17, 24, 39, 0.06);
  color: #6b7280;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.16em;
  text-transform: uppercase;
}

.dashboard-stat-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 14px;
  margin-top: 22px;
}

.dashboard-stat-card {
  padding: 18px;
  border-radius: 22px;
  background: rgba(255, 255, 255, 0.54);
  border: 1px solid rgba(255, 255, 255, 0.68);
}

.dashboard-stat-button {
  width: 100%;
  text-align: left;
  cursor: pointer;
  transition:
    transform 0.22s ease,
    box-shadow 0.22s ease,
    border-color 0.22s ease,
    background 0.22s ease;
}

.dashboard-stat-button:hover {
  transform: translateY(-2px);
  border-color: rgba(59, 130, 246, 0.2);
  background: rgba(255, 255, 255, 0.72);
  box-shadow: 0 18px 32px rgba(15, 23, 42, 0.08);
}

.dashboard-stat-button:focus-visible {
  outline: 2px solid rgba(59, 130, 246, 0.45);
  outline-offset: 3px;
}

.dashboard-stat-card span {
  color: var(--text-muted);
  font-size: 12px;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.dashboard-stat-card strong {
  display: block;
  margin-top: 12px;
  font-size: 34px;
  line-height: 1;
  letter-spacing: -0.05em;
}

.focus-header h3 {
  margin: 0;
  font-size: 24px;
  letter-spacing: -0.04em;
}

.focus-grid,
.history-grid {
  display: grid;
  gap: 16px;
  margin-top: 22px;
}

.focus-grid {
  grid-template-columns: repeat(3, minmax(0, 1fr));
}

.history-grid {
  grid-template-columns: repeat(2, minmax(0, 1fr));
}

.focus-column {
  min-width: 0;
}

.focus-column-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.focus-column-head h4 {
  margin: 0;
  font-size: 16px;
}

.focus-column-head span {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 28px;
  height: 28px;
  padding-inline: 8px;
  border-radius: 999px;
  background: rgba(17, 24, 39, 0.06);
  color: #475569;
  font-size: 12px;
  font-weight: 700;
}

.focus-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.focus-item {
  padding: 16px;
  border-radius: 22px;
  background: rgba(255, 255, 255, 0.5);
  border: 1px solid rgba(255, 255, 255, 0.72);
}

.focus-item-topline {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
}

.focus-item-topline strong {
  line-height: 1.35;
}

.focus-meta {
  margin-top: 8px;
  color: #475569;
  font-weight: 600;
}

.focus-copy {
  margin-top: 6px;
  color: var(--text-soft);
  font-size: 14px;
}

.focus-action {
  margin-top: 14px;
}

@media (max-width: 1080px) {
  .dashboard-stat-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .focus-grid,
  .history-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .dashboard-stat-grid {
    grid-template-columns: 1fr;
  }
}
</style>
