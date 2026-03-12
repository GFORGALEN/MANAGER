<template>
  <div class="page-shell">
    <a-space direction="vertical" size="large" style="width: 100%">
      <a-card class="page-card worker-dashboard-hero" :bordered="false" :loading="loading">
        <div class="worker-hero">
          <div class="worker-hero-copy">
            <span class="worker-eyebrow">Worker Workspace</span>
            <h2>现场作业台</h2>
            <p>先看今天要做什么，再决定是开工、回传，还是反馈问题。</p>
          </div>

          <div class="worker-stat-grid">
            <div class="worker-stat-card">
              <span>今日到期</span>
              <strong>{{ todayCount }}</strong>
            </div>
            <div class="worker-stat-card">
              <span>进行中</span>
              <strong>{{ doingCount }}</strong>
            </div>
            <div class="worker-stat-card">
              <span>逾期</span>
              <strong>{{ overdueCount }}</strong>
            </div>
          </div>
        </div>
      </a-card>

      <a-row :gutter="[16, 16]">
        <a-col :xs="24" :xl="16">
          <a-card class="page-card" :bordered="false" title="优先处理">
            <a-space v-if="priorityTasks.length" direction="vertical" style="width: 100%" size="middle">
              <a-card v-for="task in priorityTasks" :key="task.taskItemId" size="small" class="priority-task-card">
                <div class="priority-task-head">
                  <div>
                    <strong>{{ task.title }}</strong>
                    <div class="muted">{{ task.projectName }}</div>
                  </div>
                  <a-tag :color="statusColorMap[task.status]">{{ statusLabelMap[task.status] }}</a-tag>
                </div>
                <div class="priority-task-foot">
                  <span class="muted">截止：{{ formatDate(task.dueDate) }}</span>
                  <a-button type="primary" @click="router.push({ name: 'worker-task-detail', params: { id: task.taskItemId } })">进入处理</a-button>
                </div>
              </a-card>
            </a-space>

            <a-empty v-else description="当前没有需要优先处理的任务。" />
          </a-card>
        </a-col>

        <a-col :xs="24" :xl="8">
          <a-card class="page-card" :bordered="false" title="现场 SOP">
            <a-space direction="vertical" style="width: 100%" size="middle">
              <div class="sop-card">
                <strong>开始前</strong>
                <span>进入任务详情，先看附件和说明。</span>
              </div>
              <div class="sop-card">
                <strong>施工中</strong>
                <span>点击开始任务，现场有阻碍就反馈问题。</span>
              </div>
              <div class="sop-card">
                <strong>施工后</strong>
                <span>上传照片或文档回传，再标记完成。</span>
              </div>
              <a-button type="primary" block size="large" @click="router.push({ name: 'worker-tasks' })">
                打开我的任务
              </a-button>
            </a-space>
          </a-card>
        </a-col>
      </a-row>
    </a-space>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { message } from 'ant-design-vue'

import api from '@/services/api'
import type { PagedResult } from '@/types/common'
import type { TaskItem } from '@/types/task'

const router = useRouter()
const loading = ref(false)
const tasks = ref<TaskItem[]>([])

const todayCount = computed(() => {
  const today = new Date().toDateString()
  return tasks.value.filter((task) => new Date(task.dueDate).toDateString() === today && task.status !== 'Done').length
})

const doingCount = computed(() => tasks.value.filter((task) => task.status === 'Doing').length)
const overdueCount = computed(() => tasks.value.filter((task) => task.status !== 'Done' && new Date(task.dueDate).getTime() < Date.now()).length)

const priorityTasks = computed(() =>
  [...tasks.value]
    .filter((task) => task.status !== 'Done')
    .sort((left, right) => new Date(left.dueDate).getTime() - new Date(right.dueDate).getTime())
    .slice(0, 4),
)

const statusColorMap: Record<TaskItem['status'], string> = {
  Todo: 'default',
  Doing: 'processing',
  Done: 'success',
}

const statusLabelMap: Record<TaskItem['status'], string> = {
  Todo: '待开始',
  Doing: '进行中',
  Done: '已完成',
}

async function fetchTasks() {
  loading.value = true
  try {
    const { data } = await api.get<PagedResult<TaskItem>>('/my/tasks', {
      params: {
        pageNumber: 1,
        pageSize: 100,
      },
    })
    tasks.value = data.items
  } catch {
    message.error('加载工人工作台失败。')
  } finally {
    loading.value = false
  }
}

function formatDate(value: string) {
  return new Date(value).toLocaleString()
}

onMounted(fetchTasks)
</script>

<style scoped>
.worker-dashboard-hero {
  overflow: hidden;
}

.worker-hero {
  display: grid;
  grid-template-columns: minmax(0, 1.3fr) minmax(280px, 0.9fr);
  gap: 16px;
}

.worker-eyebrow {
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

.worker-hero-copy h2 {
  margin: 14px 0 10px;
  font-size: clamp(30px, 4vw, 42px);
  line-height: 0.98;
  letter-spacing: -0.06em;
}

.worker-hero-copy p {
  margin: 0;
  color: #64748b;
}

.worker-stat-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 12px;
}

.worker-stat-card,
.priority-task-card,
.sop-card {
  border: 1px solid rgba(255, 255, 255, 0.72);
  background: rgba(255, 255, 255, 0.52);
}

.worker-stat-card {
  padding: 18px;
  border-radius: 20px;
}

.worker-stat-card span {
  display: block;
  color: #64748b;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.worker-stat-card strong {
  display: block;
  margin-top: 10px;
  font-size: 30px;
  line-height: 1;
}

.priority-task-card {
  border-radius: 18px;
}

.priority-task-head,
.priority-task-foot {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.priority-task-foot {
  margin-top: 14px;
}

.sop-card {
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: 14px 16px;
  border-radius: 18px;
}

.sop-card span,
.muted {
  color: #64748b;
}

@media (max-width: 960px) {
  .worker-hero {
    grid-template-columns: 1fr;
  }

  .worker-stat-grid {
    grid-template-columns: 1fr;
  }
}
</style>
