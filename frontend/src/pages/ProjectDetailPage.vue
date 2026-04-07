<template>
  <div class="page-shell">
    <a-space direction="vertical" size="large" style="width: 100%">
      <a-card class="page-card project-overview-card" :bordered="false" :loading="projectLoading">
        <div v-if="project" class="project-overview">
          <div class="project-overview-head">
            <div class="project-overview-copy">
              <span class="project-overview-kicker">Project workspace</span>
              <div class="project-overview-title-row">
                <h1>{{ project.name }}</h1>
                <a-tag :color="projectStatusColor(project.status)">{{ projectStatusLabel(project.status) }}</a-tag>
              </div>
              <div class="project-overview-subtitle">#{{ project.code }} · {{ project.clientName || 'Unassigned client' }}</div>
            </div>

            <a-space class="detail-actions">
              <a-button type="primary" ghost @click="openWeeklySummaryModal" :disabled="!project">AI Weekly Summary</a-button>
              <a-button @click="openEditProjectModal" :disabled="!project">Edit Project</a-button>
              <a-popconfirm title="Delete this project?" ok-text="Delete" cancel-text="Cancel" @confirm="deleteProject">
                <a-button danger :disabled="!project">Delete</a-button>
              </a-popconfirm>
            </a-space>
          </div>

          <div class="project-stat-strip">
            <div class="project-stat-chip">
              <span>Tasks</span>
              <strong>{{ taskTotal }}</strong>
            </div>
            <div class="project-stat-chip">
              <span>Variations</span>
              <strong>{{ variationTotal }}</strong>
            </div>
            <div class="project-stat-chip">
              <span>Attachments</span>
              <strong>{{ attachmentTotal }}</strong>
            </div>
            <div class="project-stat-chip">
              <span>Budget</span>
              <strong>{{ formatCurrency(project.budget ?? 0) }}</strong>
            </div>
          </div>

          <div class="project-info-grid">
            <article class="project-info-card project-info-card-wide">
              <span class="project-info-label">Address</span>
              <strong>{{ project.address }}</strong>
              <p>{{ project.description || 'No description yet.' }}</p>
            </article>

            <article class="project-info-card">
              <span class="project-info-label">Project ID</span>
              <strong>{{ project.projectId }}</strong>
            </article>

            <article class="project-info-card">
              <span class="project-info-label">Timeline</span>
              <strong>{{ formatOptionalDate(project.startDate) }} - {{ formatOptionalDate(project.endDate) }}</strong>
            </article>

            <article class="project-info-card">
              <span class="project-info-label">Created</span>
              <strong>{{ formatDate(project.createdAt) }}</strong>
            </article>
          </div>
        </div>

        <a-empty v-else description="Project not found" />
      </a-card>

      <a-card class="page-card project-workspace-card" :bordered="false">
        <a-tabs v-model:activeKey="activeTab">
          <a-tab-pane key="tasks" tab="Tasks">
            <a-space direction="vertical" size="middle" style="width: 100%">
              <a-space wrap class="workspace-toolbar">
                <a-select v-model:value="taskQuery.status" style="width: 160px" @change="fetchTasks">
                  <a-select-option value="">All Statuses</a-select-option>
                  <a-select-option value="Draft">Draft</a-select-option>
                  <a-select-option value="InProgress">In Progress</a-select-option>
                  <a-select-option value="Blocked">Blocked</a-select-option>
                  <a-select-option value="Done">Done</a-select-option>
                </a-select>
                <a-button type="primary" @click="openCreateTaskModal">New Task</a-button>
                <a-button @click="fetchTasks">Refresh</a-button>
              </a-space>

              <a-alert :message="t('userPoolNote')" type="info" show-icon />
              <a-alert message="工人可在工人端上传现场回传、发送完工说明或反馈问题，回传文件会进入附件区。" type="info" show-icon />

              <div class="task-board">
                <div v-for="column in taskBoardColumns" :key="column.status" class="task-board-column">
                  <div class="task-board-title">{{ column.label }} ({{ column.items.length }})</div>
                  <a-space direction="vertical" style="width: 100%">
                    <a-card v-for="task in column.items" :key="task.taskItemId" size="small">
                      <a-space direction="vertical" style="width: 100%">
                        <strong>{{ task.title }}</strong>
                        <a-space wrap size="small">
                          <a-tag :color="taskPriorityColor(task.priority)">{{ taskPriorityLabel(task.priority) }}</a-tag>
                          <a-tag v-if="task.category">{{ task.category }}</a-tag>
                          <a-tag v-if="task.estimatedHours" color="gold">约 {{ task.estimatedHours }} 小时</a-tag>
                        </a-space>
                        <span class="muted">{{ taskDescriptionPreview(task.description) }}</span>
                        <span class="muted">{{ task.assignedUserName || '-' }}</span>
                        <span class="muted">{{ formatDate(task.dueDate) }}</span>
                      </a-space>
                    </a-card>
                  </a-space>
                </div>
              </div>

              <a-table :columns="taskColumns" :data-source="tasks" row-key="taskItemId" :loading="tasksLoading" :pagination="false" :scroll="{ x: 980 }">
                <template #bodyCell="{ column, record }">
                  <template v-if="column.key === 'startDate'">
                    {{ formatDate(record.startDate ?? record.createdAt) }}
                  </template>
                  <template v-else-if="column.key === 'dueDate'">
                    {{ formatDate(record.dueDate) }}
                  </template>
                  <template v-else-if="column.key === 'status'">
                    <a-select :value="record.status" style="width: 120px" @change="(value) => updateTaskStatus(record.taskItemId, value)">
                      <a-select-option value="Draft">Draft</a-select-option>
                      <a-select-option value="InProgress">In Progress</a-select-option>
                      <a-select-option value="Blocked">Blocked</a-select-option>
                      <a-select-option value="Done">Done</a-select-option>
                    </a-select>
                  </template>
                  <template v-else-if="column.key === 'priority'">
                    <a-tag :color="taskPriorityColor(record.priority)">{{ taskPriorityLabel(record.priority) }}</a-tag>
                  </template>
                  <template v-else-if="column.key === 'estimatedHours'">
                    {{ record.estimatedHours ? `${record.estimatedHours} h` : '-' }}
                  </template>
                  <template v-else-if="column.key === 'description'">
                    {{ taskDescriptionPreview(record.description) }}
                  </template>
                  <template v-else-if="column.key === 'assignedUsers'">
                    <a-space wrap>
                      <a-tag v-for="user in record.assignedUsers" :key="user.userId">{{ user.name }}</a-tag>
                    </a-space>
                  </template>
                  <template v-else-if="column.key === 'actions'">
                    <a-space wrap>
                      <a-button size="small" @click="openEditTaskModal(record)">Edit</a-button>
                      <a-button size="small" @click="openTaskEmailModal(record)">Email Team</a-button>
                    </a-space>
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
              <a-space wrap class="workspace-toolbar">
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
              <a-space wrap class="workspace-toolbar">
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
      v-model:open="weeklySummaryModalOpen"
      title="AI Weekly Summary"
      :footer="null"
      width="760px"
    >
      <a-space direction="vertical" size="middle" style="width: 100%">
        <a-alert
          message="Generate a PM-ready weekly summary from current project, task, and variation data."
          type="info"
          show-icon
        />
        <a-form layout="vertical">
          <a-form-item label="Optional PM Notes">
            <a-textarea
              v-model:value="weeklySummaryPrompt.contextNotes"
              :rows="4"
              placeholder="Example: Client requested faster wet-area completion. Ceiling leak at level 3 remains sensitive and requires electrical coordination."
            />
          </a-form-item>
          <a-form-item style="margin-bottom: 0">
            <a-space>
              <a-button type="primary" :loading="weeklySummaryGenerating" @click="generateWeeklySummary">Generate Summary</a-button>
              <a-button v-if="weeklySummary" @click="generateWeeklySummary">Regenerate</a-button>
            </a-space>
          </a-form-item>
        </a-form>

        <a-card v-if="weeklySummary" class="weekly-summary-card">
          <template #title>{{ weeklySummary.headline }}</template>
          <p class="weekly-summary-copy">{{ weeklySummary.summary }}</p>

          <section class="weekly-summary-section">
            <h4>Progress Highlights</h4>
            <ul>
              <li v-for="item in weeklySummary.progressHighlights" :key="`progress-${item}`">{{ item }}</li>
            </ul>
          </section>

          <section class="weekly-summary-section">
            <h4>Risk Alerts</h4>
            <ul v-if="weeklySummary.riskAlerts.length > 0">
              <li v-for="item in weeklySummary.riskAlerts" :key="`risk-${item}`">{{ item }}</li>
            </ul>
            <p v-else class="muted">No major risks flagged for this reporting cycle.</p>
          </section>

          <section class="weekly-summary-section">
            <h4>Next Week Plan</h4>
            <ul>
              <li v-for="item in weeklySummary.nextWeekPlan" :key="`next-${item}`">{{ item }}</li>
            </ul>
          </section>

          <section class="weekly-summary-section">
            <h4>Open Decisions</h4>
            <ul v-if="weeklySummary.openDecisions.length > 0">
              <li v-for="item in weeklySummary.openDecisions" :key="`decision-${item}`">{{ item }}</li>
            </ul>
            <p v-else class="muted">No open decisions were highlighted.</p>
          </section>
        </a-card>
      </a-space>
    </a-modal>

    <a-modal
      v-model:open="taskModalOpen"
      :title="taskModalMode === 'create' ? 'Create Task' : 'Edit Task'"
      :confirm-loading="taskSaving"
      @ok="submitTask"
    >
      <a-form layout="vertical">
        <template v-if="taskModalMode === 'create'">
          <a-alert
            message="AI Task Draft"
            description="Paste a site description and let AI suggest a task title, summary, category, priority, and execution steps."
            type="info"
            show-icon
            style="margin-bottom: 16px"
          />
          <a-form-item label="Site Description for AI">
            <a-textarea
              v-model:value="aiDraftPrompt.siteDescription"
              :rows="4"
              placeholder="Example: Ceiling leak found near the north stairwell on level 3. Water staining is spreading and electrical conduit is nearby."
            />
          </a-form-item>
          <a-form-item>
            <a-button :loading="aiGenerating" @click="generateTaskDraft">Generate with AI</a-button>
          </a-form-item>
          <a-card v-if="aiDraftSuggestion" size="small" style="margin-bottom: 16px">
            <template #title>AI Suggestion</template>
            <a-space wrap style="margin-bottom: 12px">
              <a-tag :color="taskPriorityColor(aiDraftSuggestion.priority)">{{ taskPriorityLabel(aiDraftSuggestion.priority) }}</a-tag>
              <a-tag>{{ aiDraftSuggestion.category }}</a-tag>
              <a-tag color="gold">约 {{ aiDraftSuggestion.estimatedHours }} 小时</a-tag>
            </a-space>
            <p style="margin-bottom: 12px">{{ aiDraftSuggestion.summary }}</p>
            <ol style="padding-left: 18px; margin: 0">
              <li v-for="step in aiDraftSuggestion.executionSteps" :key="step">{{ step }}</li>
            </ol>
          </a-card>
        </template>
        <a-form-item label="Title" required>
          <a-input v-model:value="taskForm.title" />
        </a-form-item>
        <a-form-item label="Description">
          <a-textarea v-model:value="taskForm.description" :rows="3" />
        </a-form-item>
        <a-form-item label="Priority">
          <a-select v-model:value="taskForm.priority">
            <a-select-option value="Low">低 / Low</a-select-option>
            <a-select-option value="Medium">中 / Medium</a-select-option>
            <a-select-option value="High">高 / High</a-select-option>
            <a-select-option value="Critical">紧急 / Critical</a-select-option>
          </a-select>
        </a-form-item>
        <a-form-item label="Category">
          <a-input v-model:value="taskForm.category" placeholder="例如：机电 / MEP，防水 / Waterproofing" />
        </a-form-item>
        <a-form-item label="Estimated Hours">
          <a-input-number v-model:value="taskForm.estimatedHours" style="width: 100%" :min="0.5" :step="0.5" />
        </a-form-item>
        <a-form-item label="Start Date" required>
          <a-input v-model:value="taskForm.startDate" type="datetime-local" />
        </a-form-item>
        <a-form-item label="Due Date" required>
          <a-input v-model:value="taskForm.dueDate" type="datetime-local" />
        </a-form-item>
        <a-form-item :label="t('assignTo')">
          <a-select
            v-model:value="taskForm.assignedUserIds"
            mode="multiple"
            allow-clear
            show-search
            :placeholder="t('assignAllUsers')"
            :options="assignableUserOptions"
          />
        </a-form-item>
      </a-form>
    </a-modal>

    <a-modal
      v-model:open="taskEmailModalOpen"
      title="Send Email to Task Team"
      :confirm-loading="taskEmailSending"
      ok-text="Send Email"
      @ok="submitTaskEmail"
    >
      <a-space direction="vertical" style="width: 100%">
        <a-alert
          v-if="selectedEmailTask"
          :message="`This will email ${selectedEmailTask.assignedUsers.length || 0} assigned team members for ${selectedEmailTask.title}.`"
          type="info"
          show-icon
        />
        <a-card v-if="selectedEmailTask" size="small">
          <template #title>Recipients</template>
          <a-space wrap>
            <a-tag v-for="user in selectedEmailTask.assignedUsers" :key="user.userId">
              {{ user.name }} - {{ user.email }}
            </a-tag>
          </a-space>
        </a-card>
        <a-form layout="vertical">
          <a-form-item label="Custom Message">
            <a-textarea
              v-model:value="taskEmailForm.message"
              :rows="5"
              placeholder="Optional. Leave blank to use the default task summary email."
            />
          </a-form-item>
        </a-form>
      </a-space>
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
import { useI18n } from '@/services/i18n'
import type { Attachment, CreateAttachmentPayload, UpdateAttachmentPayload } from '@/types/attachment'
import type { PagedResult } from '@/types/common'
import type { AiWeeklySummary, AiWeeklySummaryRequest, ProjectDetail, UpdateProjectPayload } from '@/types/project'
import type { AiTaskDraftRequest, AiTaskDraftSuggestion, CreateTaskPayload, TaskEmailResult, TaskItem, UpdateTaskPayload, UpdateTaskStatusPayload } from '@/types/task'
import type { UserSummary } from '@/types/user'
import type { CreateVariationPayload, UpdateVariationPayload, UpdateVariationStatusPayload, Variation } from '@/types/variation'

const route = useRoute()
const router = useRouter()
const { t, projectStatusLabel } = useI18n()
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
const weeklySummaryModalOpen = ref(false)
const weeklySummaryGenerating = ref(false)
const weeklySummary = ref<AiWeeklySummary | null>(null)

const taskModalOpen = ref(false)
const taskModalMode = ref<'create' | 'edit'>('create')
const taskSaving = ref(false)
const selectedTaskId = ref<string | null>(null)
const aiGenerating = ref(false)
const aiDraftSuggestion = ref<AiTaskDraftSuggestion | null>(null)
const taskEmailModalOpen = ref(false)
const taskEmailSending = ref(false)
const selectedEmailTask = ref<TaskItem | null>(null)

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

const weeklySummaryPrompt = reactive<AiWeeklySummaryRequest>({
  contextNotes: '',
})

const taskForm = reactive<CreateTaskPayload & UpdateTaskPayload>({
  title: '',
  description: '',
  priority: 'Medium',
  category: '',
  estimatedHours: null,
  startDate: '',
  dueDate: '',
  assignedUserId: undefined,
  assignedUserIds: [],
})

const aiDraftPrompt = reactive<AiTaskDraftRequest>({
  siteDescription: '',
})

const taskEmailForm = reactive({
  message: '',
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
  { title: 'Priority', dataIndex: 'priority', key: 'priority', width: 120 },
  { title: 'Category', dataIndex: 'category', key: 'category', width: 180 },
  { title: 'Est. Hours', dataIndex: 'estimatedHours', key: 'estimatedHours', width: 120 },
  { title: 'Team', dataIndex: 'assignedUsers', key: 'assignedUsers' },
  { title: 'Description', dataIndex: 'description', key: 'description' },
  { title: 'Start Date', dataIndex: 'startDate', key: 'startDate' },
  { title: 'Due Date', dataIndex: 'dueDate', key: 'dueDate' },
  { title: 'Status', dataIndex: 'status', key: 'status', width: 140 },
  { title: 'Actions', key: 'actions', width: 180 },
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
    label: `${user.name} - ${user.role} - ${user.email}`,
    value: user.userId,
  })),
)

const taskBoardColumns = computed(() => ([
  { status: 'Draft', label: t('todo'), items: tasks.value.filter((task) => task.status === 'Draft') },
  { status: 'InProgress', label: t('doing'), items: tasks.value.filter((task) => task.status === 'InProgress') },
  { status: 'Blocked', label: t('blocked'), items: tasks.value.filter((task) => task.status === 'Blocked') },
  { status: 'Done', label: t('done'), items: tasks.value.filter((task) => task.status === 'Done') },
]))

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
        isActive: true,
        pageNumber: 1,
        pageSize: 100,
      },
    })

    assignableUsers.value = data.items
  } catch {
    message.error('Failed to load users for task assignment.')
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

function openWeeklySummaryModal() {
  weeklySummaryModalOpen.value = true
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
  aiDraftPrompt.siteDescription = ''
  aiDraftSuggestion.value = null
  taskForm.title = ''
  taskForm.description = ''
  taskForm.priority = 'Medium'
  taskForm.category = ''
  taskForm.estimatedHours = null
  taskForm.startDate = ''
  taskForm.dueDate = ''
  taskForm.assignedUserId = undefined
  taskForm.assignedUserIds = []
  taskModalOpen.value = true
}

function openEditTaskModal(task: TaskItem) {
  taskModalMode.value = 'edit'
  selectedTaskId.value = task.taskItemId
  aiDraftPrompt.siteDescription = ''
  aiDraftSuggestion.value = null
  taskForm.title = task.title
  taskForm.description = task.description ?? ''
  taskForm.priority = task.priority
  taskForm.category = task.category ?? ''
  taskForm.estimatedHours = task.estimatedHours ?? null
  taskForm.startDate = toLocalInputValue(task.startDate ?? task.createdAt)
  taskForm.dueDate = toLocalInputValue(task.dueDate)
  taskForm.assignedUserId = task.assignedUserId ?? undefined
  taskForm.assignedUserIds = [...task.assignedUserIds]
  taskModalOpen.value = true
}

async function generateTaskDraft() {
  if (!aiDraftPrompt.siteDescription.trim()) {
    message.warning('Site description is required for AI generation.')
    return
  }

  aiGenerating.value = true
  try {
    const { data } = await api.post<AiTaskDraftSuggestion>(`/projects/${projectId.value}/tasks/ai-draft`, {
      siteDescription: aiDraftPrompt.siteDescription,
    }, {
      timeout: 60000,
    })

    aiDraftSuggestion.value = data
    taskForm.title = data.title
    taskForm.priority = data.priority
    taskForm.category = data.category
    taskForm.estimatedHours = data.estimatedHours
    taskForm.description = [
      data.summary,
      '',
      `优先级 / Priority: ${taskPriorityLabel(data.priority)}`,
      `分类 / Category: ${data.category}`,
      `预计工时 / Estimated effort: ${data.estimatedHours} 小时 / hours`,
      '',
      '建议步骤 / Suggested steps:',
      ...data.executionSteps.map((step, index) => `${index + 1}. ${step}`),
    ].join('\n')

    if (!taskForm.startDate) {
      taskForm.startDate = toLocalInputValue(new Date().toISOString())
    }

    message.success('AI task draft generated.')
  } catch (error) {
    message.error(extractApiError(error, 'Failed to generate AI task draft.'))
  } finally {
    aiGenerating.value = false
  }
}

async function generateWeeklySummary() {
  weeklySummaryGenerating.value = true
  try {
    const { data } = await api.post<AiWeeklySummary>(`/projects/${projectId.value}/ai/weekly-summary`, {
      contextNotes: weeklySummaryPrompt.contextNotes || null,
    }, {
      timeout: 60000,
    })

    weeklySummary.value = data
    message.success('AI weekly summary generated.')
  } catch (error) {
    message.error(extractApiError(error, 'Failed to generate AI weekly summary.'))
  } finally {
    weeklySummaryGenerating.value = false
  }
}

function openTaskEmailModal(task: TaskItem) {
  selectedEmailTask.value = task
  taskEmailForm.message = ''
  taskEmailModalOpen.value = true
}

async function submitTask() {
  if (!taskForm.title.trim() || !taskForm.startDate || !taskForm.dueDate) {
    message.warning('Task title, start date, and due date are required.')
    return
  }

  taskSaving.value = true
  try {
    const payload = {
      title: taskForm.title,
      description: taskForm.description || null,
      priority: taskForm.priority,
      category: taskForm.category || null,
      estimatedHours: taskForm.estimatedHours ?? null,
      startDate: new Date(taskForm.startDate).toISOString(),
      dueDate: new Date(taskForm.dueDate).toISOString(),
      assignedUserId: taskForm.assignedUserIds?.[0] || null,
      assignedUserIds: taskForm.assignedUserIds?.length ? taskForm.assignedUserIds : [],
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

async function submitTaskEmail() {
  if (!selectedEmailTask.value) {
    return
  }

  taskEmailSending.value = true
  try {
    const { data } = await api.post<TaskEmailResult>(`/tasks/${selectedEmailTask.value.taskItemId}/notify-email`, {
      message: taskEmailForm.message || null,
    })

    const summaryParts = [`Sent ${data.sentCount} of ${data.attemptedCount} attempted emails.`]
    if (data.sentRecipients.length > 0) {
      summaryParts.push(`Delivered to: ${data.sentRecipients.join(', ')}`)
    }
    if (data.skippedRecipients.length > 0) {
      summaryParts.push(`Skipped: ${data.skippedRecipients.join(', ')}`)
    }
    if (data.failedRecipients.length > 0) {
      summaryParts.push(`Failed: ${data.failedRecipients.join(', ')}`)
    }

    message.success(summaryParts.join(' '))
    taskEmailModalOpen.value = false
  } catch (error) {
    message.error(extractApiError(error, 'Failed to send email notifications.'))
  } finally {
    taskEmailSending.value = false
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

function taskPriorityColor(priority: string) {
  return ({
    Low: 'default',
    Medium: 'blue',
    High: 'orange',
    Critical: 'red',
  } as Record<string, string>)[priority] ?? 'default'
}

function taskPriorityLabel(priority: string) {
  return ({
    Low: '低 / Low',
    Medium: '中 / Medium',
    High: '高 / High',
    Critical: '紧急 / Critical',
  } as Record<string, string>)[priority] ?? priority
}

function taskDescriptionPreview(value?: string | null) {
  if (!value) return 'No detail yet.'

  const firstMeaningfulLine = value
    .split('\n')
    .map((line) => line.trim())
    .find((line) => line.length > 0)

  if (!firstMeaningfulLine) return 'No detail yet.'

  return firstMeaningfulLine.length > 88 ? `${firstMeaningfulLine.slice(0, 88)}...` : firstMeaningfulLine
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

function extractApiError(error: unknown, fallback: string) {
  const apiMessage = (error as { response?: { data?: { message?: string } } })?.response?.data?.message
  return apiMessage || fallback
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
.project-overview-card {
  overflow: hidden;
}

.project-overview {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.project-overview-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 18px;
}

.project-overview-copy {
  min-width: 0;
}

.project-overview-kicker {
  display: inline-flex;
  padding: 6px 10px;
  border-radius: 999px;
  background: rgba(15, 23, 42, 0.06);
  color: #64748b;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
}

.project-overview-title-row {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 12px;
}

.project-overview-title-row h1 {
  margin: 0;
  font-size: clamp(28px, 4vw, 38px);
  line-height: 0.98;
  letter-spacing: -0.06em;
}

.project-overview-subtitle {
  margin-top: 10px;
  color: #64748b;
  font-size: 14px;
  font-weight: 600;
}

.detail-actions {
  flex-wrap: wrap;
  justify-content: flex-end;
}

.project-stat-strip {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 12px;
}

.project-stat-chip {
  padding: 16px 18px;
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.58);
  border: 1px solid rgba(255, 255, 255, 0.72);
}

.project-stat-chip span {
  display: block;
  color: #64748b;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
}

.project-stat-chip strong {
  display: block;
  margin-top: 10px;
  font-size: clamp(22px, 2vw, 28px);
  line-height: 1.05;
  letter-spacing: -0.05em;
}

.project-info-grid {
  display: grid;
  grid-template-columns: minmax(0, 1.5fr) repeat(3, minmax(0, 1fr));
  gap: 12px;
}

.project-info-card {
  padding: 18px;
  border-radius: 22px;
  background: rgba(255, 255, 255, 0.5);
  border: 1px solid rgba(255, 255, 255, 0.72);
}

.project-info-card-wide {
  grid-column: span 1;
}

.project-info-label {
  display: block;
  color: #64748b;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
}

.project-info-card strong {
  display: block;
  margin-top: 10px;
  line-height: 1.45;
  font-size: 16px;
}

.project-info-card p {
  margin: 10px 0 0;
  color: #64748b;
  line-height: 1.6;
}

.muted {
  color: #64748b;
}

.project-workspace-card :deep(.ant-tabs-nav) {
  margin-bottom: 18px;
}

.weekly-summary-card {
  border-radius: 22px;
}

.weekly-summary-copy {
  margin: 0;
  color: #475569;
  line-height: 1.7;
}

.weekly-summary-section + .weekly-summary-section {
  margin-top: 18px;
}

.weekly-summary-section h4 {
  margin: 0 0 10px;
  font-size: 13px;
  font-weight: 800;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: #0f172a;
}

.weekly-summary-section ul {
  margin: 0;
  padding-left: 18px;
  color: #334155;
}

.weekly-summary-section li + li {
  margin-top: 6px;
}

.workspace-toolbar {
  align-items: center;
  gap: 10px;
}

.task-board {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
}

.task-board-column {
  padding: 12px;
  border: 1px solid #e5e7eb;
  border-radius: 16px;
  background: #f8fafc;
}

.task-board-title {
  margin-bottom: 12px;
  font-weight: 700;
}

@media (max-width: 768px) {
  .project-overview-head {
    flex-direction: column;
  }

  .project-stat-strip,
  .project-info-grid {
    grid-template-columns: 1fr;
  }

  .task-board {
    grid-template-columns: 1fr;
  }
}
</style>
