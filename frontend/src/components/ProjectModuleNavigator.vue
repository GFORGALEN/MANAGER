<template>
  <div class="page-shell">
    <a-space direction="vertical" size="large" style="width: 100%">
      <a-card class="page-card" :bordered="false">
        <div class="hub-header">
          <div>
            <h3 class="hub-title">{{ title }}</h3>
            <p class="hub-copy">{{ description }}</p>
          </div>

          <a-space class="hub-toolbar">
            <a-input-search
              v-model:value="keyword"
              :placeholder="searchPlaceholder"
              class="hub-search"
              @search="fetchProjects"
            />
            <a-button @click="fetchProjects">Refresh</a-button>
          </a-space>
        </div>

        <a-table
          class="desktop-table"
          :columns="columns"
          :data-source="projects"
          :loading="loading"
          :pagination="false"
          row-key="projectId"
          :scroll="{ x: 980 }"
        >
          <template #bodyCell="{ column, record }">
            <template v-if="column.key === 'name'">
              <div>
                <strong>{{ record.name }}</strong>
                <div class="muted">#{{ record.code }}</div>
              </div>
            </template>
            <template v-else-if="column.key === 'status'">
              <a-tag :color="projectStatusColor(record.status)">{{ record.status }}</a-tag>
            </template>
            <template v-if="column.key === 'createdAt'">
              {{ formatDate(record.createdAt) }}
            </template>
            <template v-else-if="column.key === 'actions'">
              <a-space>
                <a-button type="primary" @click="openModule(record.projectId)">
                  Open {{ ctaLabel }}
                </a-button>
                <a-button @click="openProject(record.projectId)">Project Detail</a-button>
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
                  <a-tag :color="projectStatusColor(project.status)">{{ project.status }}</a-tag>
                </div>
                <span>{{ project.address }}</span>
                <span class="muted">Client: {{ project.clientName || 'Not set' }}</span>
                <span class="muted">Created: {{ formatDate(project.createdAt) }}</span>
                <a-space wrap>
                  <a-button size="small" type="primary" @click="openModule(project.projectId)">
                    Open {{ ctaLabel }}
                  </a-button>
                  <a-button size="small" @click="openProject(project.projectId)">Project Detail</a-button>
                </a-space>
              </a-space>
            </a-card>
          </a-space>
          <a-empty v-else description="No projects found." />
        </div>

        <div class="hub-footer">
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
import { onMounted, ref } from 'vue'
import { message } from 'ant-design-vue'
import { useRouter } from 'vue-router'

import api from '@/services/api'
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

const projects = ref<ProjectListItem[]>([])
const loading = ref(false)
const keyword = ref('')
const pageNumber = ref(1)
const pageSize = ref(10)
const totalCount = ref(0)

const columns = [
  {
    title: 'Project Name',
    dataIndex: 'name',
    key: 'name',
  },
  {
    title: 'Client',
    dataIndex: 'clientName',
    key: 'clientName',
  },
  {
    title: 'Status',
    dataIndex: 'status',
    key: 'status',
    width: 120,
  },
  {
    title: 'Address',
    dataIndex: 'address',
    key: 'address',
  },
  {
    title: 'Created',
    dataIndex: 'createdAt',
    key: 'createdAt',
    width: 220,
  },
  {
    title: 'Actions',
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
    message.error(`Failed to load projects for ${props.title.toLowerCase()}.`)
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
.hub-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 16px;
  margin-bottom: 20px;
}

.hub-toolbar {
  flex-wrap: wrap;
  justify-content: flex-end;
}

.hub-search {
  width: 280px;
}

.hub-title {
  margin: 0 0 6px;
  font-size: 20px;
}

.hub-copy {
  margin: 0;
  color: #64748b;
}

.desktop-table {
  display: block;
}

.mobile-projects {
  display: none;
}

.mobile-project-card {
  border-radius: 16px;
}

.mobile-card-title {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 12px;
}

.muted {
  color: #64748b;
}

.hub-footer {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}

@media (max-width: 768px) {
  .hub-search {
    width: 100%;
  }

  .desktop-table {
    display: none;
  }

  .mobile-projects {
    display: block;
    margin-top: 8px;
  }

  .hub-footer {
    justify-content: center;
  }
}
</style>
