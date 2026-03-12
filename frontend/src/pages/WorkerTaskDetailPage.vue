<template>
  <div class="page-shell">
    <a-space direction="vertical" size="large" style="width: 100%">
      <a-card class="page-card worker-task-hero" :bordered="false" :loading="loading">
        <template #extra>
          <a-button @click="router.push({ name: 'worker-tasks' })">返回任务列表</a-button>
        </template>

        <div v-if="task" class="hero-layout">
          <div class="hero-copy">
            <span class="hero-eyebrow">现场任务</span>
            <div class="hero-title-row">
              <h2>{{ task.title }}</h2>
              <a-tag :color="statusColorMap[task.status]">{{ statusLabelMap[task.status] }}</a-tag>
            </div>
            <p>{{ task.projectName }} · {{ task.projectAddress }}</p>
          </div>

          <div class="hero-actions">
            <a-button v-if="task.status === 'Todo'" type="primary" :loading="savingStatus === 'Doing'" @click="setStatus('Doing')">
              开始任务
            </a-button>
            <a-button v-if="task.status !== 'Done'" :loading="savingStatus === 'Done'" @click="setStatus('Done')">
              标记完成
            </a-button>
            <a-button @click="openNotifyModal('Issue')">反馈问题</a-button>
            <a-button @click="openNotifyModal('Completion')">完工汇报</a-button>
            <a-button @click="attachmentUploadOpen = true">上传现场回传</a-button>
          </div>
        </div>

        <a-empty v-else description="未找到任务" />
      </a-card>

      <a-row :gutter="[16, 16]">
        <a-col :xs="24" :xl="16">
          <a-card class="page-card" :bordered="false" title="任务信息">
            <div v-if="task" class="task-fact-grid">
              <div class="task-fact-card">
                <span>计划开始</span>
                <strong>{{ formatDate(task.startDate) }}</strong>
              </div>
              <div class="task-fact-card">
                <span>截止时间</span>
                <strong>{{ formatDate(task.dueDate) }}</strong>
              </div>
              <div class="task-fact-card">
                <span>团队成员</span>
                <strong>{{ task.assignedUsers.length ? task.assignedUsers.map((user) => user.name).join('、') : '未分配' }}</strong>
              </div>
              <div class="task-fact-card">
                <span>任务说明</span>
                <strong>{{ task.description || '暂无任务说明' }}</strong>
              </div>
            </div>
          </a-card>

          <a-card class="page-card" :bordered="false" title="参考附件">
            <a-list v-if="task?.attachments.length" :data-source="task.attachments" item-layout="horizontal">
              <template #renderItem="{ item }">
                <a-list-item>
                  <template #actions>
                    <a-button size="small" @click="previewAttachment(item.attachmentId, item.fileName)">在线预览</a-button>
                  </template>
                  <a-list-item-meta :title="item.fileName" :description="`${formatFileSize(item.fileSize)} · ${formatDate(item.uploadedAt)}`" />
                </a-list-item>
              </template>
            </a-list>

            <a-empty v-else description="当前没有可参考的附件" />
          </a-card>
        </a-col>

        <a-col :xs="24" :xl="8">
          <a-card class="page-card" :bordered="false" title="现场 SOP">
            <a-space direction="vertical" style="width: 100%" size="middle">
              <div class="sop-step">
                <strong>1. 查看资料</strong>
                <span>先打开任务附件，确认图纸、照片和要求。</span>
              </div>
              <div class="sop-step">
                <strong>2. 开始执行</strong>
                <span>点击“开始任务”，让管理端知道现场已经开工。</span>
              </div>
              <div class="sop-step">
                <strong>3. 上传回传</strong>
                <span>把现场照片、文档、记录上传，管理员会在附件区看到。</span>
              </div>
              <div class="sop-step">
                <strong>4. 异常反馈或完工汇报</strong>
                <span>遇阻就反馈问题，做完就发完工汇报，再标记完成。</span>
              </div>
            </a-space>
          </a-card>

          <a-card class="page-card" :bordered="false" title="快捷沟通">
            <a-space direction="vertical" style="width: 100%">
              <a-button block @click="openNotifyModal('Issue')">向 Admin / PM 反馈问题</a-button>
              <a-button block @click="openNotifyModal('Completion')">发送完工说明</a-button>
              <a-alert message="回传文件会进入项目附件区，管理端可以直接查看。" type="info" show-icon />
            </a-space>
          </a-card>
        </a-col>
      </a-row>
    </a-space>

    <a-modal
      v-model:open="notifyModalOpen"
      :title="notifyTopic === 'Issue' ? '反馈问题' : '完工汇报'"
      :confirm-loading="sendingNotification"
      ok-text="发送"
      @ok="submitAdminNotification"
    >
      <a-form layout="vertical">
        <a-form-item :label="notifyTopic === 'Issue' ? '问题说明' : '完工说明'" required>
          <a-textarea
            v-model:value="notifyMessage"
            :rows="5"
            :placeholder="notifyTopic === 'Issue' ? '例如：现场材料不足、尺寸不符、需要管理端确认。' : '例如：已完成施工，请管理端复核。'"
          />
        </a-form-item>
      </a-form>
    </a-modal>

    <a-modal v-model:open="attachmentUploadOpen" title="上传现场回传" :confirm-loading="uploadingAttachment" ok-text="上传" @ok="submitAttachmentUpload">
      <a-space direction="vertical" style="width: 100%">
        <input type="file" @change="handleFileSelection" />
        <a-alert message="支持 pdf、png、jpg、jpeg、doc、docx、xlsx，最大 10 MB。" type="info" show-icon />
      </a-space>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { message } from 'ant-design-vue'
import type { AxiosError } from 'axios'

import api from '@/services/api'
import type { TaskDetail, UpdateTaskStatusPayload } from '@/types/task'

const route = useRoute()
const router = useRouter()

const task = ref<TaskDetail | null>(null)
const loading = ref(false)
const savingStatus = ref<UpdateTaskStatusPayload['status'] | null>(null)
const notifyModalOpen = ref(false)
const notifyTopic = ref<'Issue' | 'Completion'>('Issue')
const notifyMessage = ref('')
const sendingNotification = ref(false)
const attachmentUploadOpen = ref(false)
const uploadingAttachment = ref(false)
const selectedFile = ref<File | null>(null)

const statusColorMap: Record<UpdateTaskStatusPayload['status'], string> = {
  Todo: 'default',
  Doing: 'processing',
  Done: 'success',
}

const statusLabelMap: Record<UpdateTaskStatusPayload['status'], string> = {
  Todo: '待开始',
  Doing: '进行中',
  Done: '已完成',
}

async function fetchTask() {
  loading.value = true
  try {
    const { data } = await api.get<TaskDetail>(`/my/tasks/${route.params.id}`)
    task.value = data
  } catch {
    message.error('加载任务详情失败。')
  } finally {
    loading.value = false
  }
}

async function setStatus(status: UpdateTaskStatusPayload['status']) {
  savingStatus.value = status

  try {
    const { data } = await api.patch<TaskDetail>(`/my/tasks/${route.params.id}/status`, {
      status,
    })
    task.value = data
    message.success(status === 'Done' ? '任务已标记完成。' : '任务状态已更新。')
  } catch {
    message.error('更新任务状态失败。')
  } finally {
    savingStatus.value = null
  }
}

function openNotifyModal(topic: 'Issue' | 'Completion') {
  notifyTopic.value = topic
  notifyMessage.value = ''
  notifyModalOpen.value = true
}

async function submitAdminNotification() {
  if (!notifyMessage.value.trim()) {
    message.warning('请先填写说明。')
    return
  }

  sendingNotification.value = true
  try {
    await api.post(`/my/tasks/${route.params.id}/notify-admin`, {
      topic: notifyTopic.value,
      message: notifyMessage.value.trim(),
    })
    notifyModalOpen.value = false
    message.success(notifyTopic.value === 'Issue' ? '问题已反馈给管理端。' : '完工说明已发送给管理端。')
  } catch (error) {
    message.error(extractApiMessage(error, '发送失败，请检查后端邮件配置。'))
  } finally {
    sendingNotification.value = false
  }
}

function handleFileSelection(event: Event) {
  const input = event.target as HTMLInputElement
  selectedFile.value = input.files?.[0] ?? null
}

async function submitAttachmentUpload() {
  if (!selectedFile.value) {
    message.warning('请先选择文件。')
    return
  }

  uploadingAttachment.value = true
  try {
    const formData = new FormData()
    formData.append('file', selectedFile.value)
    await api.post(`/my/tasks/${route.params.id}/attachments/upload`, formData, {
      headers: {
        'Content-Type': 'multipart/form-data',
      },
    })
    attachmentUploadOpen.value = false
    selectedFile.value = null
    message.success('现场回传已上传。')
    await fetchTask()
  } catch {
    message.error('上传回传失败。')
  } finally {
    uploadingAttachment.value = false
  }
}

async function previewAttachment(attachmentId: string, fileName: string) {
  try {
    const response = await api.get(`/my/tasks/${route.params.id}/attachments/${attachmentId}/content`, {
      responseType: 'blob',
    })

    const blob = new Blob([response.data], {
      type: response.headers['content-type'] || 'application/octet-stream',
    })

    const previewUrl = URL.createObjectURL(blob)
    window.open(previewUrl, '_blank', 'noopener,noreferrer')
    window.setTimeout(() => URL.revokeObjectURL(previewUrl), 60_000)
  } catch {
    message.error(`打开附件失败：${fileName}`)
  }
}

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}

function formatFileSize(value: number) {
  if (!value) return '0 B'
  if (value < 1024) return `${value} B`
  if (value < 1024 * 1024) return `${(value / 1024).toFixed(1)} KB`
  return `${(value / (1024 * 1024)).toFixed(1)} MB`
}

onMounted(fetchTask)

function extractApiMessage(error: unknown, fallback: string) {
  return (error as AxiosError<{ message?: string }>)?.response?.data?.message ?? fallback
}
</script>

<style scoped>
.worker-task-hero {
  overflow: hidden;
}

.hero-layout {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 18px;
}

.hero-copy {
  min-width: 0;
}

.hero-eyebrow {
  display: inline-flex;
  padding: 6px 11px;
  border-radius: 999px;
  background: rgba(15, 23, 42, 0.06);
  color: #64748b;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.14em;
  text-transform: uppercase;
}

.hero-title-row {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-top: 14px;
}

.hero-title-row h2 {
  margin: 0;
  font-size: clamp(28px, 4vw, 40px);
  line-height: 0.98;
  letter-spacing: -0.06em;
}

.hero-copy p {
  margin: 10px 0 0;
  color: #64748b;
  font-size: 15px;
  font-weight: 600;
}

.hero-actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 10px;
  max-width: 420px;
}

.task-fact-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px;
}

.task-fact-card {
  padding: 16px 18px;
  border-radius: 20px;
  background: rgba(255, 255, 255, 0.55);
  border: 1px solid rgba(255, 255, 255, 0.72);
}

.task-fact-card span {
  display: block;
  color: #64748b;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.task-fact-card strong {
  display: block;
  margin-top: 10px;
  line-height: 1.5;
}

.sop-step {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 14px 16px;
  border-radius: 18px;
  background: rgba(255, 255, 255, 0.46);
  border: 1px solid rgba(255, 255, 255, 0.7);
}

.sop-step span {
  color: #64748b;
  line-height: 1.6;
}

@media (max-width: 960px) {
  .hero-layout {
    flex-direction: column;
  }

  .hero-actions {
    max-width: none;
    justify-content: flex-start;
  }

  .task-fact-grid {
    grid-template-columns: 1fr;
  }
}
</style>
