<template>
  <div class="page-shell">
    <a-space direction="vertical" size="large" style="width: 100%">
      <a-card class="page-card" :bordered="false" :loading="projectLoading">
        <template #title>Project Detail</template>
        <template #extra>
          <a-space class="detail-actions">
            <a-button @click="openEditProjectModal" :disabled="!project">Edit Project</a-button>
            <a-popconfirm title="Delete this project?" ok-text="Delete" cancel-text="Cancel" @confirm="deleteProject">
              <a-button danger :disabled="!project">Delete</a-button>
            </a-popconfirm>
          </a-space>
        </template>

        <a-descriptions v-if="project" class="project-desktop" :column="1" bordered>
          <a-descriptions-item label="Project ID">{{ project.projectId }}</a-descriptions-item>
          <a-descriptions-item label="Code">{{ project.code }}</a-descriptions-item>
          <a-descriptions-item label="Name">{{ project.name }}</a-descriptions-item>
          <a-descriptions-item label="Address">{{ project.address }}</a-descriptions-item>
          <a-descriptions-item label="Client">{{ project.clientName || 'Not set' }}</a-descriptions-item>
          <a-descriptions-item label="Status">
            <a-tag :color="projectStatusColor(project.status)">{{ project.status }}</a-tag>
          </a-descriptions-item>
          <a-descriptions-item label="Budget">{{ formatCurrency(project.budget ?? 0) }}</a-descriptions-item>
          <a-descriptions-item label="Start Date">{{ formatOptionalDate(project.startDate) }}</a-descriptions-item>
          <a-descriptions-item label="End Date">{{ formatOptionalDate(project.endDate) }}</a-descriptions-item>
          <a-descriptions-item label="Description">{{ project.description || 'No description' }}</a-descriptions-item>
          <a-descriptions-item label="Created At">{{ formatDate(project.createdAt) }}</a-descriptions-item>
        </a-descriptions>

        <div v-if="project" class="project-mobile">
          <a-space direction="vertical" style="width: 100%">
            <a-card size="small">
              <div class="mobile-project-header">
                <div>
                  <strong>{{ project.name }}</strong>
                  <div class="muted">#{{ project.code }}</div>
                </div>
                <a-tag :color="projectStatusColor(project.status)">{{ project.status }}</a-tag>
              </div>
            </a-card>
            <a-card size="small">
              <a-space direction="vertical" style="width: 100%">
                <span><strong>Address:</strong> {{ project.address }}</span>
                <span><strong>Client:</strong> {{ project.clientName || 'Not set' }}</span>
                <span><strong>Budget:</strong> {{ formatCurrency(project.budget ?? 0) }}</span>
                <span><strong>Start:</strong> {{ formatOptionalDate(project.startDate) }}</span>
                <span><strong>End:</strong> {{ formatOptionalDate(project.endDate) }}</span>
                <span><strong>Description:</strong> {{ project.description || 'No description' }}</span>
              </a-space>
            </a-card>
          </a-space>
        </div>

        <a-empty v-else description="Project not found" />
      </a-card>

      <a-card class="page-card" :bordered="false">
        <a-tabs v-model:activeKey="activeTab">
          <a-tab-pane key="tasks" tab="Tasks">
            <a-space direction="vertical" size="middle" style="width: 100%">
              <a-space wrap>
                <a-select v-model:value="taskQuery.status" style="width: 160px" @change="fetchTasks">
                  <a-select-option value="">All Statuses</a-select-option>
                  <a-select-option value="Todo">Todo</a-select-option>
                  <a-select-option value="Doing">Doing</a-select-option>
                  <a-select-option value="Done">Done</a-select-option>
                </a-select>
                <a-button type="primary" @click="openCreateTaskModal">New Task</a-button>
                <a-button @click="fetchTasks">Refresh</a-button>
              </a-space>

              <a-table :columns="taskColumns" :data-source="tasks" row-key="taskItemId" :loading="tasksLoading" :pagination="false" :scroll="{ x: 980 }">
                <template #bodyCell="{ column, record }">
                  <template v-if="column.key === 'dueDate'">
                    {{ formatDate(record.dueDate) }}
                  </template>
                  <template v-else-if="column.key === 'status'">
                    <a-select :value="record.status" style="width: 120px" @change="(value) => updateTaskStatus(record.taskItemId, value)">
                      <a-select-option value="Todo">Todo</a-select-option>
                      <a-select-option value="Doing">Doing</a-select-option>
                      <a-select-option value="Done">Done</a-select-option>
                    </a-select>
                  </template>
                  <template v-else-if="column.key === 'actions'">
                    <a-button size="small" @click="openEditTaskModal(record)">Edit</a-button>
                  </template>
                </template>
              </a-table>

              <a-pagination
                :current="taskQuery.pageNumber"
                :page-size="taskQuery.pageSize"
                :total="taskTotal"
                show-size-changer
                @change="handleTaskPageChange"
                @showSizeChange="handleTaskPageChange"
              />
            </a-space>
          </a-tab-pane>

          <a-tab-pane key="variations" tab="Variations">
            <a-space direction="vertical" size="middle" style="width: 100%">
              <a-space wrap>
                <a-select v-model:value="variationQuery.status" style="width: 180px" @change="fetchVariations">
                  <a-select-option value="">All Statuses</a-select-option>
                  <a-select-option value="Draft">Draft</a-select-option>
                  <a-select-option value="Submitted">Submitted</a-select-option>
                  <a-select-option value="Approved">Approved</a-select-option>
                  <a-select-option value="Rejected">Rejected</a-select-option>
                  <a-select-option value="NeedInfo">NeedInfo</a-select-option>
                </a-select>
                <a-button type="primary" @click="openCreateVariationModal">New Variation</a-button>
                <a-button @click="fetchVariations">Refresh</a-button>
              </a-space>

              <a-table :columns="variationColumns" :data-source="variations" row-key="variationId" :loading="variationsLoading" :pagination="false" :scroll="{ x: 900 }">
                <template #bodyCell="{ column, record }">
                  <template v-if="column.key === 'amount'">
                    {{ formatCurrency(record.amount) }}
                  </template>
                  <template v-else-if="column.key === 'status'">
                    <a-select :value="record.status" style="width: 140px" @change="(value) => updateVariationStatus(record.variationId, value)">
                      <a-select-option value="Draft">Draft</a-select-option>
                      <a-select-option value="Submitted">Submitted</a-select-option>
                      <a-select-option value="Approved">Approved</a-select-option>
                      <a-select-option value="Rejected">Rejected</a-select-option>
                      <a-select-option value="NeedInfo">NeedInfo</a-select-option>
                    </a-select>
                  </template>
                  <template v-else-if="column.key === 'actions'">
                    <a-button size="small" @click="openEditVariationModal(record)">Edit</a-button>
                  </template>
                </template>
              </a-table>

              <a-pagination
                :current="variationQuery.pageNumber"
                :page-size="variationQuery.pageSize"
                :total="variationTotal"
                show-size-changer
                @change="handleVariationPageChange"
                @showSizeChange="handleVariationPageChange"
              />
            </a-space>
          </a-tab-pane>

          <a-tab-pane key="attachments" tab="Attachments">
            <a-space direction="vertical" size="middle" style="width: 100%">
              <a-space wrap>
                <a-button type="primary" @click="openCreateAttachmentModal">New Attachment Metadata</a-button>
                <a-button @click="attachmentUploadOpen = true">Upload File</a-button>
                <a-button @click="fetchAttachments">Refresh</a-button>
              </a-space>

              <a-table :columns="attachmentColumns" :data-source="attachments" row-key="attachmentId" :loading="attachmentsLoading" :pagination="false" :scroll="{ x: 900 }">
                <template #bodyCell="{ column, record }">
                  <template v-if="column.key === 'fileSize'">
                    {{ formatFileSize(record.fileSize) }}
                  </template>
                  <template v-else-if="column.key === 'uploadedAt'">
                    {{ formatDate(record.uploadedAt) }}
                  </template>
                  <template v-else-if="column.key === 'actions'">
                    <a-space>
                      <a-button size="small" @click="openEditAttachmentModal(record)">Edit</a-button>
                      <a-popconfirm title="Delete this attachment?" ok-text="Delete" cancel-text="Cancel" @confirm="deleteAttachment(record.attachmentId)">
                        <a-button size="small" danger>Delete</a-button>
                      </a-popconfirm>
                    </a-space>
                  </template>
                </template>
              </a-table>

              <a-pagination
                :current="attachmentQuery.pageNumber"
                :page-size="attachmentQuery.pageSize"
                :total="attachmentTotal"
                show-size-changer
                @change="handleAttachmentPageChange"
                @showSizeChange="handleAttachmentPageChange"
              />
            </a-space>
          </a-tab-pane>
        </a-tabs>
      </a-card>
    </a-space>

    <a-modal v-model:open="projectModalOpen" title="Edit Project" :confirm-loading="projectSaving" @ok="submitProject">
      <a-form layout="vertical">
        <a-form-item label="Project Code" required>
          <a-input v-model:value="projectForm.code" />
        </a-form-item>
        <a-form-item label="Project Name" required>
          <a-input v-model:value="projectForm.name" />
        </a-form-item>
        <a-form-item label="Address" required>
          <a-input v-model:value="projectForm.address" />
        </a-form-item>
        <a-form-item label="Client Name">
          <a-input v-model:value="projectForm.clientName" />
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
          <a-textarea v-model:value="projectForm.description" :rows="3" />
        </a-form-item>
      </a-form>
    </a-modal>

    <a-modal
      v-model:open="taskModalOpen"
      :title="taskModalMode === 'create' ? 'Create Task' : 'Edit Task'"
      :confirm-loading="taskSaving"
      @ok="submitTask"
    >
      <a-form layout="vertical">
        <a-form-item label="Title" required>
          <a-input v-model:value="taskForm.title" />
        </a-form-item>
        <a-form-item label="Description">
          <a-textarea v-model:value="taskForm.description" :rows="3" />
        </a-form-item>
        <a-form-item label="Due Date" required>
          <a-input v-model:value="taskForm.dueDate" type="datetime-local" />
        </a-form-item>
        <a-form-item label="Assign To">
          <a-select
            v-model:value="taskForm.assignedUserId"
            allow-clear
            placeholder="Select a contractor"
            :options="assignableUserOptions"
          />
        </a-form-item>
      </a-form>
    </a-modal>

    <a-modal
      v-model:open="variationModalOpen"
      :title="variationModalMode === 'create' ? 'Create Variation' : 'Edit Variation'"
      :confirm-loading="variationSaving"
      @ok="submitVariation"
    >
      <a-form layout="vertical">
        <a-form-item label="Title" required>
          <a-input v-model:value="variationForm.title" />
        </a-form-item>
        <a-form-item label="Description">
          <a-textarea v-model:value="variationForm.description" :rows="3" />
        </a-form-item>
        <a-form-item label="Amount" required>
          <a-input-number v-model:value="variationForm.amount" style="width: 100%" :min="0" />
        </a-form-item>
      </a-form>
    </a-modal>

    <a-modal
      v-model:open="attachmentModalOpen"
      :title="attachmentModalMode === 'create' ? 'Create Attachment Metadata' : 'Edit Attachment Metadata'"
      :confirm-loading="attachmentSaving"
      @ok="submitAttachment"
    >
      <a-form layout="vertical">
        <a-form-item label="File Name" required>
          <a-input v-model:value="attachmentForm.fileName" />
        </a-form-item>
        <a-form-item label="File Path" required>
          <a-input v-model:value="attachmentForm.filePath" />
        </a-form-item>
        <a-form-item label="Content Type">
          <a-input v-model:value="attachmentForm.contentType" />
        </a-form-item>
      </a-form>
    </a-modal>

    <a-modal v-model:open="attachmentUploadOpen" title="Upload Attachment" :confirm-loading="uploadingAttachment" @ok="submitAttachmentUpload">
      <a-space direction="vertical" style="width: 100%">
        <input type="file" @change="handleFileSelection" />
        <a-alert message="Allowed types: pdf, png, jpg, jpeg, doc, docx, xlsx. Max 10 MB." type="info" show-icon />
      </a-space>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'
import { message } from 'ant-design-vue'
import { useRoute, useRouter } from 'vue-router'

import api from '@/services/api'
import type { Attachment, CreateAttachmentPayload, UpdateAttachmentPayload } from '@/types/attachment'
import type { PagedResult } from '@/types/common'
import type { ProjectDetail, UpdateProjectPayload } from '@/types/project'
import type { CreateTaskPayload, TaskItem, UpdateTaskPayload, UpdateTaskStatusPayload } from '@/types/task'
import type { UserSummary } from '@/types/user'
import type { CreateVariationPayload, UpdateVariationPayload, UpdateVariationStatusPayload, Variation } from '@/types/variation'

const route = useRoute()
const router = useRouter()
const supportedTabs = ['tasks', 'variations', 'attachments'] as const

const project = ref<ProjectDetail | null>(null)
const projectLoading = ref(false)
const activeTab = ref('tasks')

const tasks = ref<TaskItem[]>([])
const taskTotal = ref(0)
const tasksLoading = ref(false)
const assignableUsers = ref<UserSummary[]>([])

const variations = ref<Variation[]>([])
const variationTotal = ref(0)
const variationsLoading = ref(false)

const attachments = ref<Attachment[]>([])
const attachmentTotal = ref(0)
const attachmentsLoading = ref(false)

const projectModalOpen = ref(false)
const projectSaving = ref(false)

const taskModalOpen = ref(false)
const taskModalMode = ref<'create' | 'edit'>('create')
const taskSaving = ref(false)
const selectedTaskId = ref<string | null>(null)

const variationModalOpen = ref(false)
const variationModalMode = ref<'create' | 'edit'>('create')
const variationSaving = ref(false)
const selectedVariationId = ref<string | null>(null)

const attachmentModalOpen = ref(false)
const attachmentModalMode = ref<'create' | 'edit'>('create')
const attachmentSaving = ref(false)
const selectedAttachmentId = ref<string | null>(null)

const attachmentUploadOpen = ref(false)
const uploadingAttachment = ref(false)
const selectedFile = ref<File | null>(null)

const projectForm = reactive<UpdateProjectPayload>({
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

const taskForm = reactive<CreateTaskPayload & UpdateTaskPayload>({
  title: '',
  description: '',
  dueDate: '',
  assignedUserId: undefined,
})

const variationForm = reactive<CreateVariationPayload & UpdateVariationPayload>({
  title: '',
  description: '',
  amount: 0,
})

const attachmentForm = reactive<CreateAttachmentPayload & UpdateAttachmentPayload>({
  fileName: '',
  filePath: '',
  contentType: '',
})

const taskQuery = reactive({
  status: '',
  pageNumber: 1,
  pageSize: 10,
})

const variationQuery = reactive({
  status: '',
  pageNumber: 1,
  pageSize: 10,
})

const attachmentQuery = reactive({
  pageNumber: 1,
  pageSize: 10,
})

const projectId = computed(() => String(route.params.projectId))

function getRequestedTab() {
  const requestedTab = String(route.query.tab ?? 'tasks')
  return supportedTabs.includes(requestedTab as (typeof supportedTabs)[number]) ? requestedTab : 'tasks'
}

const taskColumns = [
  { title: 'Title', dataIndex: 'title', key: 'title' },
  { title: 'Assigned To', dataIndex: 'assignedUserName', key: 'assignedUserName' },
  { title: 'Description', dataIndex: 'description', key: 'description' },
  { title: 'Due Date', dataIndex: 'dueDate', key: 'dueDate' },
  { title: 'Status', dataIndex: 'status', key: 'status', width: 140 },
  { title: 'Actions', key: 'actions', width: 100 },
]

const variationColumns = [
  { title: 'Title', dataIndex: 'title', key: 'title' },
  { title: 'Description', dataIndex: 'description', key: 'description' },
  { title: 'Amount', dataIndex: 'amount', key: 'amount' },
  { title: 'Status', dataIndex: 'status', key: 'status', width: 160 },
  { title: 'Actions', key: 'actions', width: 100 },
]

const attachmentColumns = [
  { title: 'File Name', dataIndex: 'fileName', key: 'fileName' },
  { title: 'Path', dataIndex: 'filePath', key: 'filePath' },
  { title: 'Content Type', dataIndex: 'contentType', key: 'contentType' },
  { title: 'Size', dataIndex: 'fileSize', key: 'fileSize' },
  { title: 'Uploaded', dataIndex: 'uploadedAt', key: 'uploadedAt' },
  { title: 'Actions', key: 'actions', width: 140 },
]

const assignableUserOptions = computed(() =>
  assignableUsers.value.map((user) => ({
    label: `${user.name} (${user.email})`,
    value: user.userId,
  })),
)

async function fetchProject() {
  projectLoading.value = true
  try {
    const { data } = await api.get<ProjectDetail>(`/projects/${projectId.value}`)
    project.value = data
  } catch {
    project.value = null
    message.error('Failed to load project detail.')
  } finally {
    projectLoading.value = false
  }
}

async function fetchTasks() {
  tasksLoading.value = true
  try {
    const { data } = await api.get<PagedResult<TaskItem>>(`/projects/${projectId.value}/tasks`, {
      params: {
        status: taskQuery.status || undefined,
        pageNumber: taskQuery.pageNumber,
        pageSize: taskQuery.pageSize,
      },
    })
    tasks.value = data.items
    taskTotal.value = data.totalCount
  } catch {
    message.error('Failed to load tasks.')
  } finally {
    tasksLoading.value = false
  }
}

async function fetchAssignableUsers() {
  try {
    const { data } = await api.get<PagedResult<UserSummary>>('/users', {
      params: {
        role: 'Contractor',
        isActive: true,
        pageNumber: 1,
        pageSize: 100,
      },
    })

    assignableUsers.value = data.items
  } catch {
    message.error('Failed to load contractors for task assignment.')
  }
}

async function fetchVariations() {
  variationsLoading.value = true
  try {
    const { data } = await api.get<PagedResult<Variation>>(`/projects/${projectId.value}/variations`, {
      params: {
        status: variationQuery.status || undefined,
        pageNumber: variationQuery.pageNumber,
        pageSize: variationQuery.pageSize,
      },
    })
    variations.value = data.items
    variationTotal.value = data.totalCount
  } catch {
    message.error('Failed to load variations.')
  } finally {
    variationsLoading.value = false
  }
}

async function fetchAttachments() {
  attachmentsLoading.value = true
  try {
    const { data } = await api.get<PagedResult<Attachment>>(`/projects/${projectId.value}/attachments`, {
      params: {
        pageNumber: attachmentQuery.pageNumber,
        pageSize: attachmentQuery.pageSize,
      },
    })
    attachments.value = data.items
    attachmentTotal.value = data.totalCount
  } catch {
    message.error('Failed to load attachments.')
  } finally {
    attachmentsLoading.value = false
  }
}

async function fetchAll() {
  await Promise.all([fetchProject(), fetchTasks(), fetchVariations(), fetchAttachments()])
}

function openEditProjectModal() {
  if (!project.value) return
  projectForm.name = project.value.name
  projectForm.address = project.value.address
  projectForm.code = project.value.code
  projectForm.description = project.value.description ?? ''
  projectForm.clientName = project.value.clientName ?? ''
  projectForm.status = project.value.status
  projectForm.budget = project.value.budget ?? null
  projectForm.startDate = toDateInputValue(project.value.startDate)
  projectForm.endDate = toDateInputValue(project.value.endDate)
  projectModalOpen.value = true
}

async function submitProject() {
  if (!projectForm.code.trim() || !projectForm.name.trim() || !projectForm.address.trim()) {
    message.warning('Project code, name, and address are required.')
    return
  }

  projectSaving.value = true
  try {
    await api.put(`/projects/${projectId.value}`, {
      ...projectForm,
      description: projectForm.description || null,
      clientName: projectForm.clientName || null,
      budget: projectForm.budget ?? null,
      startDate: projectForm.startDate ? new Date(projectForm.startDate).toISOString() : null,
      endDate: projectForm.endDate ? new Date(projectForm.endDate).toISOString() : null,
    })
    message.success('Project updated.')
    projectModalOpen.value = false
    await fetchProject()
  } catch {
    message.error('Failed to update project.')
  } finally {
    projectSaving.value = false
  }
}

async function deleteProject() {
  try {
    await api.delete(`/projects/${projectId.value}`)
    message.success('Project deleted.')
    router.push({ name: 'projects' })
  } catch {
    message.error('Failed to delete project.')
  }
}

function openCreateTaskModal() {
  taskModalMode.value = 'create'
  selectedTaskId.value = null
  taskForm.title = ''
  taskForm.description = ''
  taskForm.dueDate = ''
  taskForm.assignedUserId = undefined
  taskModalOpen.value = true
}

function openEditTaskModal(task: TaskItem) {
  taskModalMode.value = 'edit'
  selectedTaskId.value = task.taskItemId
  taskForm.title = task.title
  taskForm.description = task.description ?? ''
  taskForm.dueDate = toLocalInputValue(task.dueDate)
  taskForm.assignedUserId = task.assignedUserId ?? undefined
  taskModalOpen.value = true
}

async function submitTask() {
  if (!taskForm.title.trim() || !taskForm.dueDate) {
    message.warning('Task title and due date are required.')
    return
  }

  taskSaving.value = true
  try {
    const payload = {
      title: taskForm.title,
      description: taskForm.description || null,
      dueDate: new Date(taskForm.dueDate).toISOString(),
      assignedUserId: taskForm.assignedUserId || null,
    }

    if (taskModalMode.value === 'create') {
      await api.post(`/projects/${projectId.value}/tasks`, payload)
      message.success('Task created.')
    } else if (selectedTaskId.value) {
      await api.put(`/tasks/${selectedTaskId.value}`, payload)
      message.success('Task updated.')
    }

    taskModalOpen.value = false
    await fetchTasks()
  } catch {
    message.error('Failed to save task.')
  } finally {
    taskSaving.value = false
  }
}

async function updateTaskStatus(taskId: string, status: UpdateTaskStatusPayload['status']) {
  try {
    await api.patch(`/tasks/${taskId}/status`, { status })
    message.success('Task status updated.')
    await fetchTasks()
  } catch {
    message.error('Failed to update task status.')
  }
}

function openCreateVariationModal() {
  variationModalMode.value = 'create'
  selectedVariationId.value = null
  variationForm.title = ''
  variationForm.description = ''
  variationForm.amount = 0
  variationModalOpen.value = true
}

function openEditVariationModal(variation: Variation) {
  variationModalMode.value = 'edit'
  selectedVariationId.value = variation.variationId
  variationForm.title = variation.title
  variationForm.description = variation.description ?? ''
  variationForm.amount = variation.amount
  variationModalOpen.value = true
}

async function submitVariation() {
  if (!variationForm.title.trim()) {
    message.warning('Variation title is required.')
    return
  }

  variationSaving.value = true
  try {
    const payload = {
      title: variationForm.title,
      description: variationForm.description || null,
      amount: variationForm.amount,
    }

    if (variationModalMode.value === 'create') {
      await api.post(`/projects/${projectId.value}/variations`, payload)
      message.success('Variation created.')
    } else if (selectedVariationId.value) {
      await api.put(`/variations/${selectedVariationId.value}`, payload)
      message.success('Variation updated.')
    }

    variationModalOpen.value = false
    await fetchVariations()
  } catch {
    message.error('Failed to save variation.')
  } finally {
    variationSaving.value = false
  }
}

async function updateVariationStatus(variationId: string, status: UpdateVariationStatusPayload['status']) {
  try {
    await api.patch(`/variations/${variationId}/status`, { status })
    message.success('Variation status updated.')
    await fetchVariations()
  } catch {
    message.error('Failed to update variation status.')
  }
}

function openCreateAttachmentModal() {
  attachmentModalMode.value = 'create'
  selectedAttachmentId.value = null
  attachmentForm.fileName = ''
  attachmentForm.filePath = ''
  attachmentForm.contentType = ''
  attachmentModalOpen.value = true
}

function openEditAttachmentModal(attachment: Attachment) {
  attachmentModalMode.value = 'edit'
  selectedAttachmentId.value = attachment.attachmentId
  attachmentForm.fileName = attachment.fileName
  attachmentForm.filePath = attachment.filePath
  attachmentForm.contentType = attachment.contentType ?? ''
  attachmentModalOpen.value = true
}

async function submitAttachment() {
  if (!attachmentForm.fileName.trim() || !attachmentForm.filePath.trim()) {
    message.warning('File name and file path are required.')
    return
  }

  attachmentSaving.value = true
  try {
    const payload = {
      fileName: attachmentForm.fileName,
      filePath: attachmentForm.filePath,
      contentType: attachmentForm.contentType || null,
    }

    if (attachmentModalMode.value === 'create') {
      await api.post(`/projects/${projectId.value}/attachments`, payload)
      message.success('Attachment metadata created.')
    } else if (selectedAttachmentId.value) {
      await api.put(`/attachments/${selectedAttachmentId.value}`, payload)
      message.success('Attachment metadata updated.')
    }

    attachmentModalOpen.value = false
    await fetchAttachments()
  } catch {
    message.error('Failed to save attachment metadata.')
  } finally {
    attachmentSaving.value = false
  }
}

function handleFileSelection(event: Event) {
  const input = event.target as HTMLInputElement
  selectedFile.value = input.files?.[0] ?? null
}

async function submitAttachmentUpload() {
  if (!selectedFile.value) {
    message.warning('Please choose a file first.')
    return
  }

  uploadingAttachment.value = true
  try {
    const formData = new FormData()
    formData.append('file', selectedFile.value)
    await api.post(`/projects/${projectId.value}/attachments/upload`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
    message.success('Attachment uploaded.')
    attachmentUploadOpen.value = false
    selectedFile.value = null
    await fetchAttachments()
  } catch {
    message.error('Failed to upload attachment.')
  } finally {
    uploadingAttachment.value = false
  }
}

async function deleteAttachment(attachmentId: string) {
  try {
    await api.delete(`/attachments/${attachmentId}`)
    message.success('Attachment deleted.')
    await fetchAttachments()
  } catch {
    message.error('Failed to delete attachment.')
  }
}

function handleTaskPageChange(page: number, pageSize: number) {
  taskQuery.pageNumber = page
  taskQuery.pageSize = pageSize
  fetchTasks()
}

function handleVariationPageChange(page: number, pageSize: number) {
  variationQuery.pageNumber = page
  variationQuery.pageSize = pageSize
  fetchVariations()
}

function handleAttachmentPageChange(page: number, pageSize: number) {
  attachmentQuery.pageNumber = page
  attachmentQuery.pageSize = pageSize
  fetchAttachments()
}

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}

function formatOptionalDate(value?: string | null) {
  return value ? new Date(value).toLocaleDateString() : 'Not set'
}

function formatCurrency(value: number) {
  return new Intl.NumberFormat(undefined, { style: 'currency', currency: 'USD' }).format(value)
}

function projectStatusColor(status: string) {
  return ({
    Planning: 'default',
    Active: 'processing',
    OnHold: 'warning',
    Completed: 'success',
  } as Record<string, string>)[status] ?? 'default'
}

function formatFileSize(value: number) {
  if (!value) return '0 B'
  if (value < 1024) return `${value} B`
  if (value < 1024 * 1024) return `${(value / 1024).toFixed(1)} KB`
  return `${(value / (1024 * 1024)).toFixed(1)} MB`
}

function toLocalInputValue(value: string) {
  const date = new Date(value)
  const offset = date.getTimezoneOffset()
  const localDate = new Date(date.getTime() - offset * 60_000)
  return localDate.toISOString().slice(0, 16)
}

function toDateInputValue(value?: string | null) {
  return value ? new Date(value).toISOString().slice(0, 10) : ''
}

watch(
  () => route.query.tab,
  () => {
    activeTab.value = getRequestedTab()
  },
  { immediate: true },
)

watch(activeTab, (tab) => {
  if (route.query.tab === tab) {
    return
  }

  router.replace({
    query: {
      ...route.query,
      tab,
    },
  })
})

watch(
  () => route.params.projectId,
  () => {
    fetchAll()
  },
)

onMounted(() => {
  activeTab.value = getRequestedTab()
  fetchAll()
  fetchAssignableUsers()
})
</script>

<style scoped>
.detail-actions {
  flex-wrap: wrap;
  justify-content: flex-end;
}

.project-mobile {
  display: none;
}

.mobile-project-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 12px;
}

.muted {
  color: #64748b;
}

@media (max-width: 768px) {
  .project-desktop {
    display: none;
  }

  .project-mobile {
    display: block;
  }
}
</style>
