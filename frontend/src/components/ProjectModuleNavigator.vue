<template>
  <div class="page-shell">
    <a-space direction="vertical" size="large" style="width: 100%">
      <a-card class="page-card" :bordered="false">
        <div class="hub-header">
          <div>
            <h3 class="hub-title">{{ title }}</h3>
            <p class="hub-copy">{{ description }}</p>
          </div>

          <a-space>
            <a-input-search
              v-model:value="keyword"
              :placeholder="searchPlaceholder"
              style="width: 280px"
              @search="fetchProjects"
            />
            <a-button @click="fetchProjects">Refresh</a-button>
          </a-space>
        </div>

        <a-table
          :columns="columns"
          :data-source="projects"
          :loading="loading"
          :pagination="false"
          row-key="projectId"
        >
          <template #bodyCell="{ column, record }">
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

.hub-title {
  margin: 0 0 6px;
  font-size: 20px;
}

.hub-copy {
  margin: 0;
  color: #64748b;
}

.hub-footer {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}
</style>
