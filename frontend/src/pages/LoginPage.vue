<template>
  <div class="login-shell">
    <transition name="welcome-fade">
      <div v-if="welcomeVisible" class="welcome-overlay">
        <div class="welcome-panel">
          <span class="eyebrow subtle">{{ copy.welcomeBack }}</span>
          <strong>{{ welcomeName }}</strong>
          <p>{{ copy.signingInAs }} {{ welcomeRole }}</p>
          <div class="welcome-bar">
            <span></span>
          </div>
        </div>
      </div>
    </transition>

    <div class="login-stage" @mousemove="handleMouseMove" @mouseleave="resetMouse">
      <div class="stage-bridge" aria-hidden="true"></div>

      <section class="login-showcase" :style="showcaseStyle">
        <div class="login-toolbar">
          <LanguageSwitcher />
        </div>
        <div class="hero-grid"></div>
        <div class="hero-orb hero-orb-a"></div>
        <div class="hero-orb hero-orb-b"></div>

        <div class="showcase-copy">
          <span class="eyebrow">{{ copy.platformEyebrow }}</span>
          <h1>{{ copy.stageTitle }}</h1>
          <p class="showcase-subtitle">{{ copy.stageSubtitle }}</p>
        </div>

        <div class="ambient-stage">
          <div class="ambient-frame ambient-frame-large"></div>
          <div class="ambient-frame ambient-frame-medium"></div>
          <div class="ambient-frame ambient-frame-small"></div>
          <div class="ambient-pulse ambient-pulse-a"></div>
          <div class="ambient-pulse ambient-pulse-b"></div>
          <div class="ambient-line ambient-line-a"></div>
          <div class="ambient-line ambient-line-b"></div>
        </div>
      </section>

      <a-card class="login-card" :bordered="false">
        <div class="login-card-glow" aria-hidden="true"></div>

        <div class="module-selection-grid">
          <button
            v-for="module in moduleCards"
            :key="module.key"
            type="button"
            class="module-selection-card"
            :class="{ active: activeModule === module.key }"
            @click="selectModule(module.key)"
          >
            <span>{{ module.badge }}</span>
            <strong>{{ module.name }}</strong>
            <small>{{ module.hint }}</small>
          </button>
        </div>

        <div class="login-copy">
          <span class="eyebrow subtle">{{ selectedModule.badge }}</span>
          <h2>{{ selectedModule.formTitle }}</h2>
          <p>{{ selectedModule.formCopy }}</p>
        </div>

        <a-alert
          v-if="errorMessage"
          type="error"
          :message="errorMessage"
          show-icon
          style="margin-bottom: 16px"
        />

        <a-form :model="formState" layout="vertical">
          <a-form-item :label="copy.userLabel" name="username">
            <a-input
              v-model:value="formState.username"
              :placeholder="selectedModule.demoUsername"
              size="large"
              @press-enter="handleSubmit"
            />
          </a-form-item>

          <a-form-item :label="copy.passwordLabel" name="password">
            <a-input-password
              v-model:value="formState.password"
              :placeholder="selectedModule.demoPassword"
              size="large"
              @press-enter="handleSubmit"
            />
          </a-form-item>

          <a-button
            type="primary"
            html-type="button"
            size="large"
            block
            :loading="submitting"
            @click="handleSubmit"
          >
            {{ selectedModule.submitLabel }}
          </a-button>
        </a-form>

        <div class="login-secondary-actions">
          <a-button v-if="activeModule === 'worker'" block @click="router.push({ name: 'register' })">
            {{ copy.createWorker }}
          </a-button>
        </div>

        <div class="demo-panel">
          <span><strong>{{ copy.demoTitle }}</strong></span>
          <div
            v-for="demo in selectedModule.demoAccounts"
            :key="demo.label"
            class="demo-item"
          >
            <span>{{ demo.label }}</span>
            <code>{{ demo.username }} / {{ demo.password }}</code>
          </div>
        </div>
      </a-card>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import LanguageSwitcher from '@/components/LanguageSwitcher.vue'
import api from '@/services/api'
import { clearSession, getDefaultRouteForRole, setCurrentUser, setToken } from '@/services/auth'
import { useI18n } from '@/services/i18n'
import type { AuthResponse, CurrentUser, LoginRequest } from '@/types/auth'

type LoginModuleKey = 'manager' | 'worker'

const router = useRouter()
const route = useRoute()
const { locale } = useI18n()

const activeModule = ref<LoginModuleKey>('manager')
const formState = reactive<LoginRequest>({
  username: 'admin',
  password: 'Admin123!',
})

const submitting = ref(false)
const errorMessage = ref('')
const mouseX = ref(0)
const mouseY = ref(0)
const welcomeVisible = ref(false)
const welcomeName = ref('')
const welcomeRole = ref('')

const isZh = computed(() => locale.value === 'zh')

const copy = computed(() =>
  isZh.value
    ? {
        platformEyebrow: '施工管理系统',
        stageTitle: '选择登录身份',
        stageSubtitle: '管理端和施工人员都从同一个页面进入，只是流程和权限不同。',
        welcomeBack: '欢迎回来',
        signingInAs: '正在进入',
        userLabel: '用户名或邮箱',
        passwordLabel: '密码',
        createWorker: '创建施工人员账号',
        demoTitle: '演示账号',
        managerOnlyError: '这个入口只允许 Admin 或 PM 登录。',
        workerOnlyError: '这个入口只允许施工人员登录。',
        loginError: '登录失败，请检查账号、密码或后端服务。',
        managerRole: '管理端',
        workerRole: '施工人员',
      }
    : {
        platformEyebrow: 'Construction Management',
        stageTitle: 'Choose Your Entry',
        stageSubtitle: 'Managers and field workers enter from the same page, with different permissions and workflows.',
        welcomeBack: 'Welcome back',
        signingInAs: 'Signing in to',
        userLabel: 'Username or Email',
        passwordLabel: 'Password',
        createWorker: 'Create Contractor Account',
        demoTitle: 'Demo accounts',
        managerOnlyError: 'This entry only allows Admin or PM accounts.',
        workerOnlyError: 'This entry only allows worker accounts.',
        loginError: 'Login failed. Check username, password, or backend availability.',
        managerRole: 'Manager Workspace',
        workerRole: 'Worker Workspace',
      },
)

const moduleCards = computed(() =>
  isZh.value
    ? [
        {
          key: 'manager' as const,
          badge: '管理端入口',
          name: 'Admin / PM',
          hint: '项目、任务、变更、附件统一管理',
        },
        {
          key: 'worker' as const,
          badge: '施工入口',
          name: '施工人员',
          hint: '查看任务、上传回传、反馈问题',
        },
      ]
    : [
        {
          key: 'manager' as const,
          badge: 'Manager Entry',
          name: 'Admin / PM',
          hint: 'Manage projects, tasks, variations, and attachments',
        },
        {
          key: 'worker' as const,
          badge: 'Worker Entry',
          name: 'Field Worker',
          hint: 'Open tasks, upload proof, and report issues',
        },
      ],
)

const selectedModule = computed(() => {
  if (activeModule.value === 'worker') {
    return isZh.value
      ? {
          title: '施工人员登录',
          subtitle: '进入移动优先的现场作业区，处理任务、上传回传并反馈现场问题。',
          badge: '施工入口',
          formTitle: '施工人员登录',
          formCopy: '只给现场施工人员使用，登录后直接进入我的任务。',
          submitLabel: '进入施工页面',
          demoUsername: 'contractor',
          demoPassword: 'Contractor123!',
          allowedRoles: ['Contractor'],
          demoAccounts: [
            {
              label: 'Worker',
              username: 'contractor',
              password: 'Contractor123!',
            },
          ],
        }
      : {
          title: 'Worker Sign In',
          subtitle: 'Use the mobile-first field workspace to handle tasks, uploads, and issue reporting.',
          badge: 'Worker Entry',
          formTitle: 'Worker Sign In',
          formCopy: 'For field staff only. Successful login opens My Tasks directly.',
          submitLabel: 'Enter Worker Workspace',
          demoUsername: 'contractor',
          demoPassword: 'Contractor123!',
          allowedRoles: ['Contractor'],
          demoAccounts: [
            {
              label: 'Worker',
              username: 'contractor',
              password: 'Contractor123!',
            },
          ],
        }
  }

  return isZh.value
    ? {
        title: '管理端登录',
        subtitle: '用于 Admin 和 PM，进入项目、任务、变更和附件的统一工作台。',
        badge: '管理端入口',
        formTitle: '管理端登录',
        formCopy: '适合管理员和项目经理使用，登录后进入管理工作台。',
        submitLabel: '进入管理页面',
        demoUsername: 'admin',
        demoPassword: 'Admin123!',
        allowedRoles: ['Admin', 'PM'],
        demoAccounts: [
          {
            label: 'Admin',
            username: 'admin',
            password: 'Admin123!',
          },
          {
            label: 'PM',
            username: 'pm',
            password: 'Pm123!',
          },
        ],
      }
    : {
        title: 'Manager Sign In',
        subtitle: 'For Admin and PM users to enter the unified workspace for projects, tasks, variations, and attachments.',
        badge: 'Manager Entry',
        formTitle: 'Manager Sign In',
        formCopy: 'Use this entry for Admin and PM accounts.',
        submitLabel: 'Enter Manager Workspace',
        demoUsername: 'admin',
        demoPassword: 'Admin123!',
        allowedRoles: ['Admin', 'PM'],
        demoAccounts: [
          {
            label: 'Admin',
            username: 'admin',
            password: 'Admin123!',
          },
          {
            label: 'PM',
            username: 'pm',
            password: 'Pm123!',
          },
        ],
      }
})

const showcaseStyle = computed(() => ({
  transform: `perspective(1200px) rotateX(${mouseY.value * -5}deg) rotateY(${mouseX.value * 7}deg)`,
}))

function selectModule(module: LoginModuleKey) {
  activeModule.value = module
  errorMessage.value = ''

  if (module === 'worker') {
    formState.username = 'contractor'
    formState.password = 'Contractor123!'
    return
  }

  formState.username = 'admin'
  formState.password = 'Admin123!'
}

async function handleSubmit() {
  submitting.value = true
  errorMessage.value = ''

  try {
    const { data } = await api.post<AuthResponse>('/auth/login', formState)
    setToken(data.token)

    const profileResponse = await api.get<CurrentUser>('/auth/me')
    const currentUser = profileResponse.data

    if (!selectedModule.value.allowedRoles.includes(currentUser.role)) {
      clearSession()
      errorMessage.value = activeModule.value === 'manager' ? copy.value.managerOnlyError : copy.value.workerOnlyError
      return
    }

    setCurrentUser(currentUser)
    welcomeName.value = currentUser.name || currentUser.email
    welcomeRole.value = currentUser.role === 'Contractor' ? copy.value.workerRole : copy.value.managerRole
    welcomeVisible.value = true

    const redirect = typeof route.query.redirect === 'string'
      ? route.query.redirect
      : getDefaultRouteForRole(currentUser.role)

    await new Promise((resolve) => window.setTimeout(resolve, 1100))
    router.push(redirect)
  } catch {
    clearSession()
    errorMessage.value = copy.value.loginError
    welcomeVisible.value = false
  } finally {
    submitting.value = false
  }
}

function handleMouseMove(event: MouseEvent) {
  if (window.innerWidth <= 768) {
    return
  }

  const target = event.currentTarget as HTMLDivElement
  const rect = target.getBoundingClientRect()
  mouseX.value = ((event.clientX - rect.left) / rect.width - 0.5) * 1.2
  mouseY.value = ((event.clientY - rect.top) / rect.height - 0.5) * 1.2
}

function resetMouse() {
  mouseX.value = 0
  mouseY.value = 0
}
</script>

<style scoped>
.login-shell {
  position: relative;
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 100vh;
  padding: 24px;
  background:
    radial-gradient(circle at 10% 15%, rgba(146, 64, 14, 0.16), transparent 18%),
    radial-gradient(circle at 86% 18%, rgba(21, 128, 61, 0.12), transparent 22%),
    linear-gradient(135deg, #d9cfbf 0%, #e7dece 48%, #ddd2c1 100%);
}

.welcome-overlay {
  position: fixed;
  inset: 0;
  z-index: 50;
  display: grid;
  place-items: center;
  padding: 20px;
  background: rgba(15, 23, 42, 0.42);
  backdrop-filter: blur(16px);
}

.welcome-panel {
  width: min(100%, 420px);
  padding: 28px 24px;
  border-radius: 28px;
  text-align: center;
  color: #fff;
  background:
    radial-gradient(circle at top, rgba(59, 130, 246, 0.24), transparent 42%),
    linear-gradient(160deg, rgba(15, 23, 42, 0.96), rgba(30, 41, 59, 0.94));
  border: 1px solid rgba(255, 255, 255, 0.12);
  box-shadow: 0 30px 80px rgba(15, 23, 42, 0.35);
}

.welcome-panel strong {
  display: block;
  margin-top: 14px;
  font-size: clamp(28px, 8vw, 42px);
  line-height: 1;
  letter-spacing: -0.05em;
}

.welcome-panel p {
  margin: 10px 0 0;
  color: rgba(226, 232, 240, 0.82);
}

.welcome-bar {
  margin-top: 20px;
  height: 4px;
  border-radius: 999px;
  overflow: hidden;
  background: rgba(255, 255, 255, 0.12);
}

.welcome-bar span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, #60a5fa, #f59e0b);
  animation: welcome-progress 1s linear forwards;
}

.login-stage {
  position: relative;
  width: 100%;
  max-width: 1220px;
  display: grid;
  grid-template-columns: minmax(0, 1.15fr) minmax(380px, 460px);
  gap: 24px;
  align-items: stretch;
}

.stage-bridge {
  position: absolute;
  top: 50%;
  left: calc(100% - 470px);
  width: 180px;
  height: 480px;
  transform: translate(-58%, -50%);
  border-radius: 999px;
  background:
    radial-gradient(circle at center, rgba(56, 189, 248, 0.16), transparent 58%),
    linear-gradient(180deg, rgba(255, 255, 255, 0.18), rgba(255, 255, 255, 0.02));
  filter: blur(18px);
  opacity: 0.9;
  pointer-events: none;
  z-index: 1;
}

.login-showcase {
  position: relative;
  overflow: hidden;
  border-radius: 34px;
  padding: 28px 160px 28px 28px;
  min-height: 690px;
  background:
    linear-gradient(160deg, rgba(15, 23, 42, 0.95), rgba(30, 41, 59, 0.92)),
    linear-gradient(135deg, #1e293b, #0f172a);
  color: #fff;
  box-shadow: 0 34px 90px rgba(15, 23, 42, 0.22);
  transition: transform 0.22s ease;
  transform-style: preserve-3d;
  z-index: 0;
}

.login-card {
  position: relative;
  width: 100%;
  align-self: center;
  border-radius: 30px;
  border: 1px solid rgba(226, 232, 240, 0.85);
  background:
    linear-gradient(180deg, rgba(255, 252, 248, 0.92), rgba(247, 241, 233, 0.88)),
    rgba(249, 244, 237, 0.94);
  box-shadow:
    0 28px 80px rgba(51, 38, 24, 0.16),
    inset 0 1px 0 rgba(255, 255, 255, 0.72);
  backdrop-filter: blur(18px);
  padding-top: 10px;
  margin-left: -120px;
  z-index: 2;
}

.login-card-glow {
  position: absolute;
  inset: 20px auto 20px -54px;
  width: 120px;
  border-radius: 999px;
  background: linear-gradient(180deg, rgba(56, 189, 248, 0.18), rgba(59, 130, 246, 0.04));
  filter: blur(12px);
  pointer-events: none;
  opacity: 0.9;
}

.login-toolbar,
.showcase-copy,
.ambient-stage {
  position: relative;
  z-index: 2;
}

.login-toolbar {
  display: flex;
  justify-content: flex-end;
}

.module-selection-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;
  margin-bottom: 22px;
}

.module-selection-card {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 16px;
  border-radius: 20px;
  text-align: left;
  background: linear-gradient(180deg, rgba(255, 255, 255, 0.58), rgba(244, 238, 230, 0.82));
  border: 1px solid rgba(191, 168, 138, 0.42);
  color: #1f2937;
  transition: transform 0.22s ease, border-color 0.22s ease, box-shadow 0.22s ease;
}

.module-selection-card span {
  color: #8b5e1a;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.12em;
  text-transform: uppercase;
}

.module-selection-card strong {
  font-size: 22px;
  line-height: 1.05;
  letter-spacing: -0.04em;
}

.module-selection-card small {
  color: #64748b;
  line-height: 1.6;
}

.module-selection-card.active,
.module-selection-card:hover {
  transform: translateY(-2px);
  border-color: rgba(15, 23, 42, 0.18);
  box-shadow: 0 18px 28px rgba(51, 38, 24, 0.1);
}

.eyebrow {
  display: inline-flex;
  padding: 7px 12px;
  border-radius: 999px;
  background: linear-gradient(135deg, #dbeafe, #e0f2fe);
  color: #1d4ed8;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

.eyebrow.subtle {
  background: linear-gradient(135deg, #fde68a, #fcd34d);
  color: #92400e;
}

.hero-grid {
  position: absolute;
  inset: 0;
  background-image:
    linear-gradient(rgba(148, 163, 184, 0.08) 1px, transparent 1px),
    linear-gradient(90deg, rgba(148, 163, 184, 0.08) 1px, transparent 1px);
  background-size: 44px 44px;
  mask-image: linear-gradient(180deg, rgba(0, 0, 0, 0.7), transparent 85%);
}

.hero-orb {
  position: absolute;
  border-radius: 999px;
}

.hero-orb-a {
  width: 280px;
  height: 280px;
  top: -80px;
  right: -40px;
  background: radial-gradient(circle, rgba(14, 165, 233, 0.32), transparent 70%);
}

.hero-orb-b {
  width: 260px;
  height: 260px;
  bottom: -70px;
  left: -30px;
  background: radial-gradient(circle, rgba(249, 115, 22, 0.24), transparent 70%);
}

.showcase-copy {
  max-width: 560px;
  margin: 44px 0 0;
}

.showcase-copy h1 {
  margin: 12px 0;
  font-size: clamp(42px, 5vw, 64px);
  line-height: 0.98;
  letter-spacing: -0.06em;
}

.showcase-subtitle {
  max-width: 520px;
  margin: 0;
  color: rgba(226, 232, 240, 0.78);
  font-size: 16px;
  line-height: 1.7;
}

.login-copy {
  margin-bottom: 24px;
}

.login-copy h2 {
  margin: 8px 0 10px;
  font-size: 34px;
  line-height: 1.1;
  letter-spacing: -0.04em;
}

.login-copy p {
  margin: 0;
  color: #64748b;
}

.ambient-stage {
  position: relative;
  min-height: 320px;
  margin-top: 34px;
  margin-right: -40px;
}

.ambient-frame,
.ambient-pulse,
.ambient-line {
  position: absolute;
}

.ambient-frame {
  border-radius: 28px;
  border: 1px solid rgba(255, 255, 255, 0.12);
  background: linear-gradient(180deg, rgba(255, 255, 255, 0.12), rgba(255, 255, 255, 0.04));
  backdrop-filter: blur(16px);
  box-shadow:
    inset 0 1px 0 rgba(255, 255, 255, 0.12),
    0 30px 60px rgba(2, 6, 23, 0.18);
}

.ambient-frame-large {
  inset: auto 28px 0 0;
  height: 188px;
  transform: rotate(-4deg);
  animation: float-large 9s ease-in-out infinite;
}

.ambient-frame-medium {
  left: 24px;
  right: 180px;
  bottom: 76px;
  height: 112px;
  transform: rotate(6deg);
  animation: float-medium 8s ease-in-out infinite;
}

.ambient-frame-small {
  right: 6px;
  top: 32px;
  width: 180px;
  height: 102px;
  transform: rotate(10deg);
  animation: float-small 7s ease-in-out infinite;
}

.ambient-pulse {
  border-radius: 50%;
  filter: blur(10px);
}

.ambient-pulse-a {
  left: 16px;
  bottom: 18px;
  width: 180px;
  height: 180px;
  background: radial-gradient(circle, rgba(249, 115, 22, 0.24), transparent 70%);
  animation: pulse-drift 8s ease-in-out infinite;
}

.ambient-pulse-b {
  right: 8px;
  top: 8px;
  width: 220px;
  height: 220px;
  background: radial-gradient(circle, rgba(56, 189, 248, 0.26), transparent 70%);
  animation: pulse-drift 10s ease-in-out infinite reverse;
}

.ambient-line {
  height: 1px;
  background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.28), transparent);
  opacity: 0.65;
}

.ambient-line-a {
  left: 44px;
  right: 120px;
  top: 132px;
  transform: rotate(-15deg);
  animation: glow-line 6s ease-in-out infinite;
}

.ambient-line-b {
  left: 120px;
  right: 40px;
  bottom: 98px;
  transform: rotate(11deg);
  animation: glow-line 7s ease-in-out infinite reverse;
}

.login-secondary-actions {
  margin-top: 18px;
}

.demo-panel {
  margin-top: 22px;
  padding: 18px;
  border-radius: 20px;
  background: linear-gradient(180deg, #f3eadf, #eadfce);
  border: 1px solid rgba(191, 168, 138, 0.5);
}

.demo-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
  padding-top: 10px;
  color: #334155;
}

.demo-item code {
  padding: 6px 10px;
  border-radius: 12px;
  background: #ded0bc;
  color: #3f3123;
}

.welcome-fade-enter-active,
.welcome-fade-leave-active {
  transition: opacity 0.24s ease;
}

.welcome-fade-enter-from,
.welcome-fade-leave-to {
  opacity: 0;
}

@keyframes welcome-progress {
  from {
    width: 0;
  }

  to {
    width: 100%;
  }
}

@keyframes float-large {
  0%,
  100% {
    transform: rotate(-4deg) translateY(0);
  }

  50% {
    transform: rotate(-6deg) translateY(-10px);
  }
}

@keyframes float-medium {
  0%,
  100% {
    transform: rotate(6deg) translateY(0);
  }

  50% {
    transform: rotate(3deg) translateY(12px);
  }
}

@keyframes float-small {
  0%,
  100% {
    transform: rotate(10deg) translateY(0);
  }

  50% {
    transform: rotate(7deg) translateY(-8px);
  }
}

@keyframes pulse-drift {
  0%,
  100% {
    transform: scale(1) translate3d(0, 0, 0);
    opacity: 0.8;
  }

  50% {
    transform: scale(1.08) translate3d(8px, -10px, 0);
    opacity: 1;
  }
}

@keyframes glow-line {
  0%,
  100% {
    opacity: 0.25;
  }

  50% {
    opacity: 0.75;
  }
}

@media (max-width: 1024px) {
  .login-stage {
    grid-template-columns: 1fr;
  }

  .stage-bridge {
    display: none;
  }

  .login-showcase {
    min-height: 620px;
    padding: 24px;
  }

  .login-card {
    max-width: 680px;
    justify-self: center;
    margin-left: 0;
  }

  .login-card-glow {
    inset: -36px 24px auto 24px;
    width: auto;
    height: 80px;
  }

  .ambient-stage {
    margin-right: 0;
  }
}

@media (max-width: 768px) {
  .login-shell {
    padding: 12px;
  }

  .login-stage {
    gap: 14px;
  }

  .login-showcase {
    min-height: auto;
    padding: 18px;
    border-radius: 24px;
    transform: none !important;
  }

  .showcase-copy {
    margin: 18px 0 0;
  }

  .ambient-stage {
    min-height: 220px;
    margin-top: 24px;
  }

  .login-card {
    border-radius: 24px;
  }

  .login-card-glow {
    inset: -28px 18px auto 18px;
    height: 64px;
  }

  .login-copy h2 {
    font-size: 28px;
  }

  .demo-item {
    align-items: flex-start;
    flex-direction: column;
  }
}

@media (max-width: 480px) {
  .login-showcase {
    padding: 16px;
  }

  .showcase-copy h1 {
    font-size: 36px;
  }

  .ambient-stage {
    min-height: 190px;
  }

  .module-selection-grid {
    grid-template-columns: 1fr;
  }

  .login-card :deep(.ant-card-body) {
    padding: 18px;
  }
}
</style>
