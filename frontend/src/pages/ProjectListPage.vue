<template>
  <div class="page-shell">
    <a-card class="page-card" :bordered="false">
      <template #title>Projects</template>
      <template #extra>
        <a-space class="toolbar-wrap">
          <a-input-search
            v-model:value="keyword"
            placeholder="Search by name or address"
            class="toolbar-search"
            @search="fetchProjects"
          />
          <a-button type="primary" @click="openCreateModal">New Project</a-button>
          <a-button @click="fetchProjects">Refresh</a-button>
        </a-space>
      </template>

      <a-table
        class="desktop-table"
        :columns="columns"
        :data-source="projects"
        :pagination="false"
        :loading="loading"
        row-key="projectId"
        :scroll="{ x: 980 }"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'name'">
            <a @click="goToProject(record.projectId)">{{ record.name }}</a>
          </template>
          <template v-else-if="column.key === 'status'">
            <a-tag :color="projectStatusColor(record.status)">{{ record.status }}</a-tag>
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
                <a-button size="small" type="primary" @click="goToProject(project.projectId)">Open</a-button>
                <a-popconfirm
                  title="Delete this project?"
                  ok-text="Delete"
                  cancel-text="Cancel"
                  @confirm="deleteProject(project.projectId)"
                >
                  <a-button size="small" danger>Delete</a-button>
                </a-popconfirm>
              </a-space>
            </a-space>
          </a-card>
        </a-space>
        <a-empty v-else description="No projects found." />
      </div>

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
        <a-form-item label="Project Code" required>
          <a-input v-model:value="projectForm.code" placeholder="Enter project code" />
        </a-form-item>
        <a-form-item label="Project Name" required>
          <a-input v-model:value="projectForm.name" placeholder="Enter project name" />
        </a-form-item>
        <a-form-item label="Address" required>
          <a-input v-model:value="projectForm.address" placeholder="Enter project address" />
        </a-form-item>
        <a-form-item label="Client Name">
          <a-input v-model:value="projectForm.clientName" placeholder="Enter client name" />
        </a-form-item>
        <a-form-item label="Status" required>
          <a-select v-model:value="projectForm.status">
            <a-select-option value="Planning">Planning</a-select-option>
            <a-select-option value="Active">Active</a-select-option>
            <a-select-option value="OnHold">On Hold</a-select-option>
            <a-select-option value="Completed">Completed</a-select-option>
          </a-select>
        </a-form-item>
        <a-form-item label="Budget">
          <a-input-number v-model:value="projectForm.budget" style="width: 100%" :min="0" />
        </a-form-item>
        <a-form-item label="Start Date">
          <a-input v-model:value="projectForm.startDate" type="date" />
        </a-form-item>
        <a-form-item label="End Date">
          <a-input v-model:value="projectForm.endDate" type="date" />
        </a-form-item>
        <a-form-item label="Description">
          <a-textarea v-model:value="projectForm.description" :rows="3" placeholder="Project summary" />
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
  code: '',
  name: '',
  address: '',
  description: '',
  clientName: '',
  status: 'Planning',
  budget: null,
  startDate: '',
  endDate: '',
})

const columns = [
  {
    title: 'Code',
    dataIndex: 'code',
    key: 'code',
    width: 120,
  },
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
  projectForm.code = ''
  projectForm.name = ''
  projectForm.address = ''
  projectForm.description = ''
  projectForm.clientName = ''
  projectForm.status = 'Planning'
  projectForm.budget = null
  projectForm.startDate = ''
  projectForm.endDate = ''
  createModalOpen.value = true
}

async function submitCreateProject() {
  if (!projectForm.code.trim() || !projectForm.name.trim() || !projectForm.address.trim()) {
    message.warning('Project code, name, and address are required.')
    return
  }

  savingProject.value = true

  try {
    await api.post('/projects', {
      code: projectForm.code,
      name: projectForm.name,
      address: projectForm.address,
      description: projectForm.description || null,
      clientName: projectForm.clientName || null,
      status: projectForm.status,
      budget: projectForm.budget ?? null,
      startDate: projectForm.startDate ? new Date(projectForm.startDate).toISOString() : null,
      endDate: projectForm.endDate ? new Date(projectForm.endDate).toISOString() : null,
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
.toolbar-wrap {
  flex-wrap: wrap;
  justify-content: flex-end;
}

.toolbar-search {
  width: 260px;
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

.table-footer {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}

@media (max-width: 768px) {
  .toolbar-search {
    width: 100%;
  }

  .desktop-table {
    display: none;
  }

  .mobile-projects {
    display: block;
    margin-top: 8px;
  }

  .table-footer {
    justify-content: center;
  }
}
</style>
