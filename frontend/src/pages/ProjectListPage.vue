<template>
  <div class="page-shell">
    <a-card class="page-card" :bordered="false">
      <template #title>Projects</template>
      <template #extra>
        <a-space>
          <a-input-search
            v-model:value="keyword"
            placeholder="Search by name or address"
            style="width: 260px"
            @search="fetchProjects"
          />
          <a-button type="primary" @click="openCreateModal">New Project</a-button>
          <a-button @click="fetchProjects">Refresh</a-button>
        </a-space>
      </template>

      <a-table
        :columns="columns"
        :data-source="projects"
        :pagination="false"
        :loading="loading"
        row-key="projectId"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'name'">
            <a @click="goToProject(record.projectId)">{{ record.name }}</a>
          </template>
          <template v-else-if="column.key === 'createdAt'">
            {{ formatDate(record.createdAt) }}
          </template>
          <template v-else-if="column.key === 'actions'">
            <a-space>
              <a-button size="small" @click="goToProject(record.projectId)">Open</a-button>
              <a-popconfirm
                title="Delete this project?"
                ok-text="Delete"
                cancel-text="Cancel"
                @confirm="deleteProject(record.projectId)"
              >
                <a-button size="small" danger>Delete</a-button>
              </a-popconfirm>
            </a-space>
          </template>
        </template>
      </a-table>

      <div class="table-footer">
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

    <a-modal
      v-model:open="createModalOpen"
      title="Create Project"
      ok-text="Create"
      :confirm-loading="savingProject"
      @ok="submitCreateProject"
    >
      <a-form layout="vertical">
        <a-form-item label="Project Name" required>
          <a-input v-model:value="projectForm.name" placeholder="Enter project name" />
        </a-form-item>
        <a-form-item label="Address" required>
          <a-input v-model:value="projectForm.address" placeholder="Enter project address" />
        </a-form-item>
      </a-form>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue'
import { message } from 'ant-design-vue'
import { useRouter } from 'vue-router'

import api from '@/services/api'
import type { PagedResult } from '@/types/common'
import type { CreateProjectPayload, ProjectListItem } from '@/types/project'

const router = useRouter()

const projects = ref<ProjectListItem[]>([])
const loading = ref(false)
const savingProject = ref(false)
const createModalOpen = ref(false)
const keyword = ref('')
const pageNumber = ref(1)
const pageSize = ref(10)
const totalCount = ref(0)

const projectForm = reactive<CreateProjectPayload>({
  name: '',
  address: '',
})

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
  },
  {
    title: 'Actions',
    key: 'actions',
    width: 180,
  },
]

async function fetchProjects() {
  loading.value = true

  try {
    const { data } = await api.get<PagedResult<ProjectListItem>>('/projects', {
      params: {
        pageNumber: pageNumber.value,
        pageSize: pageSize.value,
        keyword: keyword.value || undefined,
      },
    })

    projects.value = data.items
    totalCount.value = data.totalCount
  } catch {
    message.error('Failed to load projects.')
  } finally {
    loading.value = false
  }
}

function openCreateModal() {
  projectForm.name = ''
  projectForm.address = ''
  createModalOpen.value = true
}

async function submitCreateProject() {
  if (!projectForm.name.trim() || !projectForm.address.trim()) {
    message.warning('Project name and address are required.')
    return
  }

  savingProject.value = true

  try {
    await api.post('/projects', {
      name: projectForm.name,
      address: projectForm.address,
    })
    message.success('Project created.')
    createModalOpen.value = false
    await fetchProjects()
  } catch {
    message.error('Failed to create project.')
  } finally {
    savingProject.value = false
  }
}

async function deleteProject(projectId: string) {
  try {
    await api.delete(`/projects/${projectId}`)
    message.success('Project deleted.')
    await fetchProjects()
  } catch {
    message.error('Failed to delete project.')
  }
}

function handlePageChange(page: number, size: number) {
  pageNumber.value = page
  pageSize.value = size
  fetchProjects()
}

function goToProject(projectId: string) {
  router.push({ name: 'project-detail', params: { projectId } })
}

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}

onMounted(fetchProjects)
</script>

<style scoped>
.table-footer {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}
</style>
