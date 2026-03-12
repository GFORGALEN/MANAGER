<template>
  <div class="page-shell">
    <a-space direction="vertical" size="large" style="width: 100%">
      <a-card class="page-card" :bordered="false">
        <section class="hub-hero">
          <div class="hub-hero-copy">
            <span class="hub-eyebrow">{{ heroLabel }}</span>
            <h3 class="hub-title">{{ resolvedTitle }}</h3>
            <p class="hub-copy">{{ resolvedDescription }}</p>
          </div>
          <div class="hero-stats">
            <div class="hero-stat-card">
              <span>{{ t('hubTotalProjects') }}</span>
              <strong>{{ totalCount }}</strong>
              <small>{{ t('hubLiveModuleIndex') }}</small>
            </div>
          </div>
        </section>

        <div class="hub-header">
          <div class="hub-header-meta">
            <span class="hub-section-label">{{ t('projectDirectory') }}</span>
            <span class="hub-section-line"></span>
          </div>
          <a-space class="hub-toolbar">
            <a-input-search
              v-model:value="keyword"
              :placeholder="searchPlaceholder"
              class="hub-search"
              size="large"
              @search="fetchProjects"
            />
            <a-button class="hub-refresh" @click="fetchProjects">{{ t('refresh') }}</a-button>
          </a-space>
        </div>

        <a-table
          class="desktop-table project-table"
          :columns="columns"
          :data-source="projects"
          :loading="loading"
          :pagination="false"
          row-key="projectId"
          :scroll="{ x: 980 }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'name'">
              <div class="project-cell">
                <strong>{{ record.name }}</strong>
                <div class="muted">#{{ record.code }}</div>
              </div>
            </template>
            <template v-else-if="column.key === 'clientName'">
              <span class="cell-secondary">{{ record.clientName || t('hubUnassignedClient') }}</span>
            </template>
            <template v-else-if="column.key === 'status'">
              <a-tag class="status-pill" :color="projectStatusColor(record.status)">{{ projectStatusLabel(record.status) }}</a-tag>
            </template>
            <template v-if="column.key === 'createdAt'">
              <span class="cell-secondary">{{ formatDate(record.createdAt) }}</span>
            </template>
            <template v-else-if="column.key === 'address'">
              <span class="cell-secondary">{{ record.address || t('hubAddressPending') }}</span>
            </template>
            <template v-else-if="column.key === 'actions'">
              <a-space class="table-actions">
                <a-button type="primary" class="open-module-button" @click="openModule(record.projectId)">
                  {{ t('open') }} {{ resolvedCtaLabel }}
                </a-button>
                <a-button @click="openProject(record.projectId)">{{ t('hubProjectDetail') }}</a-button>
              </a-space>
            </template>
          </template>
        </a-table>

        <div class="mobile-projects">
          <a-space v-if="projects.length" direction="vertical" size="middle" style="width: 100%">
            <a-card v-for="project in projects" :key="project.projectId" size="small" class="mobile-project-card">
              <a-space direction="vertical" style="width: 100%">
                <div class="mobile-card-title">
                  <div>
                    <strong>{{ project.name }}</strong>
                    <div class="muted">#{{ project.code }}</div>
                  </div>
                  <a-tag :color="projectStatusColor(project.status)">{{ projectStatusLabel(project.status) }}</a-tag>
                </div>
                <span>{{ project.address }}</span>
                <span class="muted">{{ t('client') }}: {{ project.clientName || t('notSet') }}</span>
                <span class="muted">{{ t('created') }}: {{ formatDate(project.createdAt) }}</span>
                <a-space wrap>
                  <a-button size="small" type="primary" @click="openModule(project.projectId)">
                  {{ t('open') }} {{ resolvedCtaLabel }}
                  </a-button>
                  <a-button size="small" @click="openProject(project.projectId)">{{ t('hubProjectDetail') }}</a-button>
                </a-space>
              </a-space>
            </a-card>
          </a-space>
          <a-empty v-else :description="t('noProjectsFound')" />
        </div>

        <div class="hub-footer">
          <div class="hub-footer-copy">
            <strong>{{ projects.length }}</strong>
            <span>{{ t('hubProjectsInView') }}</span>
          </div>
          <a-pagination
            :current="pageNumber"
            :page-size="pageSize"
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
import { computed, onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import { useRouter } from 'vue-router'

import api from '@/services/api'
import { useI18n } from '@/services/i18n'
import type { PagedResult } from '@/types/common'
import type { ProjectListItem } from '@/types/project'

interface Props {
  title: string
  description: string
  tabKey: 'tasks' | 'variations' | 'attachments'
  ctaLabel: string
  searchPlaceholder?: string
}

const props = withDefaults(defineProps<Props>(), {
  searchPlaceholder: 'Search projects by name or address',
})

const router = useRouter()
const { t, projectStatusLabel } = useI18n()

const projects = ref<ProjectListItem[]>([])
const loading = ref(false)
const keyword = ref('')
const pageNumber = ref(1)
const pageSize = ref(10)
const totalCount = ref(0)
const resolvedTitle = computed(() => props.title)
const resolvedDescription = computed(() => props.description)
const resolvedCtaLabel = computed(() => props.ctaLabel)
const heroLabel = computed(() => `${resolvedCtaLabel.value} ${t('hubWorkspace')}`)

const columns = [
  {
    title: t('projectName'),
    dataIndex: 'name',
    key: 'name',
  },
  {
    title: t('client'),
    dataIndex: 'clientName',
    key: 'clientName',
  },
  {
    title: t('status'),
    dataIndex: 'status',
    key: 'status',
    width: 120,
  },
  {
    title: t('address'),
    dataIndex: 'address',
    key: 'address',
  },
  {
    title: t('created'),
    dataIndex: 'createdAt',
    key: 'createdAt',
    width: 220,
  },
  {
    title: t('actions'),
    key: 'actions',
    width: 280,
  },
]

async function fetchProjects() {
  loading.value = true

  try {
    const { data } = await api.get<PagedResult<ProjectListItem>>('/projects', {
      params: {
        keyword: keyword.value || undefined,
        pageNumber: pageNumber.value,
        pageSize: pageSize.value,
      },
    })

    projects.value = data.items
    totalCount.value = data.totalCount
  } catch {
    message.error(t('hubLoadFailed'))
  } finally {
    loading.value = false
  }
}

function handlePageChange(page: number, size: number) {
  pageNumber.value = page
  pageSize.value = size
  fetchProjects()
}

function openModule(projectId: string) {
  router.push({
    name: 'project-detail',
    params: { projectId },
    query: { tab: props.tabKey },
  })
}

function openProject(projectId: string) {
  router.push({
    name: 'project-detail',
    params: { projectId },
  })
}

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}

function projectStatusColor(status: string) {
  return ({
    Planning: 'default',
    Active: 'processing',
    OnHold: 'warning',
    Completed: 'success',
  } as Record<string, string>)[status] ?? 'default'
}

onMounted(fetchProjects)
</script>

<style scoped>
.hub-hero {
  display: grid;
  grid-template-columns: minmax(0, 1.7fr) minmax(240px, 0.5fr);
  gap: 18px;
  margin-bottom: 22px;
}

.hub-hero-copy,
.hero-stat-card {
  position: relative;
  overflow: hidden;
  border-radius: 24px;
  border: 1px solid rgba(255, 255, 255, 0.72);
  background: var(--hero-gradient);
  box-shadow: 0 18px 40px rgba(15, 23, 42, 0.06);
}

.hub-hero-copy {
  padding: 26px 26px 30px;
}

.hub-hero-copy::after,
.hero-stat-card::after {
  content: '';
  position: absolute;
  inset: auto -20px -36px auto;
  width: 140px;
  height: 140px;
  border-radius: 50%;
  background: radial-gradient(circle, rgba(59, 130, 246, 0.12), transparent 64%);
  pointer-events: none;
}

.hub-eyebrow {
  display: inline-flex;
  align-items: center;
  padding: 6px 11px;
  border-radius: 999px;
  background: rgba(17, 24, 39, 0.06);
  color: #6b7280;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.16em;
  text-transform: uppercase;
}

.hero-stats {
  display: block;
}

.hero-stat-card {
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  min-height: 148px;
  padding: 20px;
}

.hero-stat-card span {
  color: var(--text-muted);
  font-size: 12px;
  text-transform: uppercase;
  letter-spacing: 0.14em;
}

.hero-stat-card strong {
  margin-top: 16px;
  font-size: 34px;
  line-height: 1;
  letter-spacing: -0.05em;
}

.hero-stat-card small {
  margin-top: 8px;
  color: var(--text-soft);
  font-size: 13px;
}

.hub-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
  margin-bottom: 18px;
}

.hub-header-meta {
  display: flex;
  align-items: center;
  gap: 14px;
}

.hub-section-label {
  color: #5f564d;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.16em;
  text-transform: uppercase;
}

.hub-section-line {
  width: 120px;
  height: 1px;
  background: linear-gradient(90deg, rgba(17, 24, 39, 0.18), transparent);
}

.hub-toolbar {
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 10px;
}

.hub-search {
  width: 340px;
}

.hub-title {
  margin: 14px 0 8px;
  font-size: clamp(28px, 4vw, 42px);
  line-height: 0.98;
  letter-spacing: -0.06em;
}

.hub-copy {
  margin: 0;
  max-width: 640px;
  color: #7c6f61;
  font-size: 15px;
}

.hub-refresh {
  background: rgba(255, 255, 255, 0.66);
}

.project-table :deep(.ant-table-container) {
  border: 1px solid rgba(120, 109, 96, 0.08);
  border-radius: 24px;
  overflow: hidden;
}

.desktop-table {
  display: block;
}

.project-cell {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.project-cell strong {
  font-size: 15px;
}

.cell-secondary {
  color: #5f564d;
}

.status-pill {
  border-radius: 999px;
  font-weight: 600;
  padding-inline: 10px;
}

.table-actions {
  gap: 10px;
}

.open-module-button {
  min-width: 162px;
}

.mobile-projects {
  display: none;
}

.mobile-project-card {
  border-radius: 22px;
  background: rgba(255, 255, 255, 0.5);
  border: 1px solid rgba(255, 255, 255, 0.64);
}

.mobile-card-title {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 12px;
}

.muted {
  color: #8e7f6d;
}

.hub-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 14px;
  margin-top: 22px;
}

.hub-footer-copy {
  display: inline-flex;
  align-items: baseline;
  gap: 10px;
  color: var(--text-muted);
}

.hub-footer-copy strong {
  font-size: 22px;
  color: #111827;
  letter-spacing: -0.04em;
}

.hub-footer-copy span {
  font-size: 13px;
  text-transform: lowercase;
}

@media (max-width: 1080px) {
  .hub-hero {
    grid-template-columns: 1fr;
  }

  .hero-stats {
    display: block;
  }
}

@media (max-width: 768px) {
  .hub-header {
    align-items: flex-start;
    flex-direction: column;
  }

  .hub-search {
    width: 100%;
  }

  .hub-hero-copy {
    padding: 20px;
  }

  .hub-title {
    font-size: 30px;
  }

  .desktop-table {
    display: none;
  }

  .mobile-projects {
    display: block;
    margin-top: 8px;
  }

  .hub-footer {
    flex-direction: column;
    justify-content: center;
  }
}
</style>
