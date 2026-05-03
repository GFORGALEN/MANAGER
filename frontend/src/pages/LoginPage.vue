<template>
  <div class="login-shell">
    <transition name="welcome-fade">
      <div v-if="welcomeVisible" class="welcome-overlay">
        <div class="welcome-panel">
          <span>{{ copy.welcomeBack }}</span>
          <strong>{{ welcomeName }}</strong>
          <p>{{ copy.signingInAs }} {{ welcomeRole }}</p>
          <div class="welcome-bar"><i></i></div>
        </div>
      </div>
    </transition>

    <main class="login-stage">
      <section class="login-showcase">
        <div>
          <div class="brand-row">
            <div class="brand-mark">BM</div>
            <div>
              <strong>BUILDMATE</strong>
              <span>CONSTRUCTION MANAGEMENT</span>
            </div>
          </div>

          <div class="showcase-copy">
            <h1>{{ copy.stageTitle }}</h1>
            <p>{{ copy.stageSubtitle }}</p>
          </div>

          <div class="feature-list">
            <article>
              <span><SafetyCertificateOutlined /></span>
              <div>
                <strong>{{ copy.featureProjectTitle }}</strong>
                <small>{{ copy.featureProjectCopy }}</small>
              </div>
            </article>
            <article>
              <span><FolderOpenOutlined /></span>
              <div>
                <strong>{{ copy.featureVariationTitle }}</strong>
                <small>{{ copy.featureVariationCopy }}</small>
              </div>
            </article>
            <article>
              <span><FileTextOutlined /></span>
              <div>
                <strong>{{ copy.featureDocumentTitle }}</strong>
                <small>{{ copy.featureDocumentCopy }}</small>
              </div>
            </article>
            <article>
              <span><LineChartOutlined /></span>
              <div>
                <strong>{{ copy.featureReportTitle }}</strong>
                <small>{{ copy.featureReportCopy }}</small>
              </div>
            </article>
          </div>
        </div>

        <div class="showcase-footer">
          <span><LockOutlined /> {{ copy.securityLine }}</span>
          <small>© 2024 BuildMate Construction Management System</small>
        </div>
      </section>

      <section class="login-panel">
        <div class="login-toolbar">
          <LanguageSwitcher />
        </div>

        <div class="login-copy">
          <h2>{{ copy.loginTitle }}</h2>
          <p>{{ copy.loginSubtitle }}</p>
        </div>

        <div class="module-tabs">
          <button
            v-for="module in moduleCards"
            :key="module.key"
            type="button"
            :class="{ active: activeModule === module.key }"
            @click="selectModule(module.key)"
          >
            {{ module.name }}
          </button>
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
              size="large"
              :placeholder="copy.userPlaceholder"
              @press-enter="handleSubmit"
            >
              <template #prefix>
                <UserOutlined />
              </template>
            </a-input>
          </a-form-item>

          <a-form-item :label="copy.passwordLabel" name="password">
            <a-input-password
              v-model:value="formState.password"
              size="large"
              :placeholder="copy.passwordPlaceholder"
              @press-enter="handleSubmit"
            >
              <template #prefix>
                <LockOutlined />
              </template>
            </a-input-password>
          </a-form-item>

          <div class="form-options">
            <a-checkbox>{{ copy.rememberMe }}</a-checkbox>
            <a>{{ copy.forgotPassword }}</a>
          </div>

          <a-button
            type="primary"
            html-type="button"
            size="large"
            block
            :loading="submitting"
            @click="handleSubmit"
          >
            {{ copy.loginAction }}
          </a-button>
        </a-form>

        <div class="login-divider">
          <i></i><span>{{ copy.or }}</span><i></i>
        </div>

        <a-button class="sso-button" block>
          <BankOutlined />
          {{ copy.ssoLogin }}
        </a-button>

        <div class="login-secondary-actions">
          <span>{{ copy.noAccount }}</span>
          <a v-if="activeModule === 'worker'" @click="router.push({ name: 'register' })">{{ copy.createWorker }}</a>
          <a v-else>{{ copy.contactAdmin }}</a>
        </div>
      </section>
    </main>
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
import {
  BankOutlined,
  FileTextOutlined,
  FolderOpenOutlined,
  LineChartOutlined,
  LockOutlined,
  SafetyCertificateOutlined,
  UserOutlined,
} from '@ant-design/icons-vue'

type LoginModuleKey = 'manager' | 'worker'

const router = useRouter()
const route = useRoute()
const { locale } = useI18n()

const activeModule = ref<LoginModuleKey>('manager')
const formState = reactive<LoginRequest>({
  username: '',
  password: '',
})

const submitting = ref(false)
const errorMessage = ref('')
const welcomeVisible = ref(false)
const welcomeName = ref('')
const welcomeRole = ref('')

const isZh = computed(() => locale.value === 'zh')

const copy = computed(() =>
  isZh.value
    ? {
        platformEyebrow: '施工项目管理系统',
        stageTitle: '施工项目管理系统',
        stageSubtitle: '为项目经理、管理层和施工队提供高效的项目管理解决方案',
        loginTitle: '欢迎登录',
        loginSubtitle: '请输入您的账号信息登录系统',
        welcomeBack: '欢迎回来',
        signingInAs: '正在进入',
        userLabel: '用户名或邮箱',
        userPlaceholder: '请输入用户名或邮箱地址',
        passwordLabel: '密码',
        passwordPlaceholder: '请输入密码',
        rememberMe: '记住我',
        forgotPassword: '忘记密码?',
        loginAction: '登录系统',
        or: '或',
        ssoLogin: '企业 SSO 登录',
        noAccount: '还没有账号？',
        contactAdmin: '联系系统管理员',
        createWorker: '创建施工人员账号',
        securityLine: '企业级安全保障，数据加密传输',
        featureProjectTitle: '项目与任务管理',
        featureProjectCopy: '规划、跟踪和管理项目任务进度',
        featureVariationTitle: '变更与审批流程',
        featureVariationCopy: '变更管理、审批流转和版本控制',
        featureDocumentTitle: '文档与图纸管理',
        featureDocumentCopy: '集中存储、版本控制和权限管理',
        featureReportTitle: '报表与数据分析',
        featureReportCopy: '实时数据分析和项目报表',
        managerOnlyError: '这个入口只允许 Admin 或 PM 登录。',
        workerOnlyError: '这个入口只允许施工人员登录。',
        loginError: '登录失败，请检查账号、密码或后端服务。',
        managerRole: '管理工作台',
        workerRole: '施工人员工作台',
      }
    : {
        platformEyebrow: 'Construction Management',
        stageTitle: 'Construction Project Management System',
        stageSubtitle: 'Efficient project management for owners, project managers, and construction teams.',
        loginTitle: 'Welcome Back',
        loginSubtitle: 'Enter your account details to access the system.',
        welcomeBack: 'Welcome back',
        signingInAs: 'Signing in to',
        userLabel: 'Username or Email',
        userPlaceholder: 'Enter username or email address',
        passwordLabel: 'Password',
        passwordPlaceholder: 'Enter password',
        rememberMe: 'Remember me',
        forgotPassword: 'Forgot password?',
        loginAction: 'Sign In',
        or: 'or',
        ssoLogin: 'Enterprise SSO Login',
        noAccount: 'No account?',
        contactAdmin: 'Contact system administrator',
        createWorker: 'Create Contractor Account',
        securityLine: 'Enterprise-grade security with encrypted data transfer',
        featureProjectTitle: 'Project & Task Management',
        featureProjectCopy: 'Plan, track, and manage project task progress',
        featureVariationTitle: 'Variation & Approval Flow',
        featureVariationCopy: 'Variation management, approvals, and version control',
        featureDocumentTitle: 'Documents & Drawings',
        featureDocumentCopy: 'Centralized storage, version control, and permissions',
        featureReportTitle: 'Reports & Analytics',
        featureReportCopy: 'Real-time analytics and project reporting',
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
          badge: '管理入口',
          name: 'Admin / PM',
          hint: '项目、任务、变更、附件与用户管理',
        },
        {
          key: 'worker' as const,
          badge: '现场入口',
          name: '施工人员',
          hint: '查看任务、上传回传、反馈现场问题',
        },
      ]
    : [
        {
          key: 'manager' as const,
          badge: 'Manager Entry',
          name: 'Admin / PM',
          hint: 'Projects, tasks, variations, documents, and users',
        },
        {
          key: 'worker' as const,
          badge: 'Field Entry',
          name: 'Contractor',
          hint: 'Open tasks, upload proof, and report issues',
        },
      ],
)

const selectedModule = computed(() => {
  if (activeModule.value === 'worker') {
    return isZh.value
      ? {
          badge: '现场入口',
          formTitle: '施工人员登录',
          formCopy: '登录后直接进入现场任务视图，处理任务进度、附件和问题反馈。',
          submitLabel: '进入施工人员页面',
          allowedRoles: ['Contractor'],
        }
      : {
          badge: 'Field Entry',
          formTitle: 'Contractor Sign In',
          formCopy: 'Go directly to assigned tasks, uploads, and field issue reporting.',
          submitLabel: 'Enter Field Workspace',
          allowedRoles: ['Contractor'],
        }
  }

  return isZh.value
    ? {
        badge: '管理入口',
        formTitle: '管理端登录',
        formCopy: '适合管理员和项目经理使用，登录后进入项目管理工作台。',
        submitLabel: '进入管理工作台',
        allowedRoles: ['Admin', 'PM'],
      }
    : {
        badge: 'Manager Entry',
        formTitle: 'Manager Sign In',
        formCopy: 'For Admin and PM accounts managing projects, tasks, approvals, and teams.',
        submitLabel: 'Enter Manager Workspace',
        allowedRoles: ['Admin', 'PM'],
      }
})

function selectModule(module: LoginModuleKey) {
  activeModule.value = module
  errorMessage.value = ''
  formState.username = ''
  formState.password = ''
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

    await new Promise((resolve) => window.setTimeout(resolve, 800))
    router.push(redirect)
  } catch {
    clearSession()
    errorMessage.value = copy.value.loginError
    welcomeVisible.value = false
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.login-shell {
  position: relative;
  min-height: 100vh;
  overflow: hidden;
  background: #0b1f33;
}

.login-stage {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(420px, 520px);
  min-height: 100vh;
  background:
    linear-gradient(90deg, rgba(8, 28, 46, 0.92) 0%, rgba(8, 28, 46, 0.74) 42%, rgba(8, 28, 46, 0.1) 72%),
    url("https://images.unsplash.com/photo-1503387762-592deb58ef4e?auto=format&fit=crop&w=2200&q=80") center / cover;
}

.login-showcase {
  position: relative;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  min-height: 100vh;
  padding: 54px 58px;
  color: #fff;
}

.brand-row {
  display: flex;
  align-items: center;
  gap: 16px;
}

.brand-mark {
  display: grid;
  place-items: center;
  width: 58px;
  height: 58px;
  border-radius: 10px;
  background: linear-gradient(145deg, #ff8a00, #f05a00);
  color: #fff;
  font-size: 18px;
  font-weight: 900;
  box-shadow: 0 16px 36px rgba(249, 115, 22, 0.28);
}

.brand-row strong,
.brand-row span {
  display: block;
}

.brand-row strong {
  font-size: 30px;
  font-weight: 900;
  letter-spacing: 0.02em;
}

.brand-row span {
  color: rgba(226, 232, 240, 0.78);
  font-size: 14px;
  font-weight: 800;
  letter-spacing: 0.12em;
}

.showcase-copy {
  max-width: 760px;
  margin-top: 130px;
}

.showcase-copy h1 {
  margin: 0 0 18px;
  font-size: clamp(34px, 4vw, 44px);
  line-height: 1.02;
  font-weight: 900;
}

.showcase-copy p {
  max-width: 760px;
  margin: 0;
  color: rgba(226, 232, 240, 0.86);
  font-size: 18px;
  font-weight: 700;
}

.feature-list {
  display: grid;
  gap: 22px;
  margin-top: 56px;
}

.feature-list article {
  display: flex;
  align-items: center;
  gap: 18px;
}

.feature-list article > span {
  display: grid;
  place-items: center;
  width: 58px;
  height: 58px;
  flex: 0 0 auto;
  border-radius: 12px;
  background: rgba(255, 255, 255, 0.1);
  color: #ff8a00;
  font-size: 28px;
}

.feature-list strong,
.feature-list small {
  display: block;
}

.feature-list strong {
  color: #fff;
  font-size: 18px;
  font-weight: 900;
}

.feature-list small {
  margin-top: 5px;
  color: rgba(226, 232, 240, 0.78);
  font-size: 15px;
  font-weight: 700;
}

.login-panel {
  align-self: center;
  width: min(100% - 64px, 580px);
  margin: 0 54px 0 auto;
  padding: 34px 42px 42px;
  border: 1px solid rgba(226, 232, 240, 0.85);
  border-radius: 10px;
  background: rgba(255, 255, 255, 0.96);
  box-shadow: 0 28px 90px rgba(2, 6, 23, 0.2);
  backdrop-filter: blur(10px);
}

.login-toolbar {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 34px;
}

.login-copy {
  margin-bottom: 28px;
}

.login-copy h2 {
  margin: 0 0 10px;
  color: #0f172a;
  font-weight: 900;
  font-size: 34px;
}

.login-copy p {
  margin: 0;
  color: #64748b;
  font-size: 16px;
}

.module-tabs {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 8px;
  padding: 5px;
  margin-bottom: 22px;
  border-radius: 8px;
  background: #f1f5f9;
}

.module-tabs button {
  height: 36px;
  border: 0;
  border-radius: 6px;
  background: transparent;
  color: #64748b;
  font-weight: 900;
  cursor: pointer;
}

.module-tabs button.active {
  background: #fff;
  color: #0f172a;
  box-shadow: 0 1px 3px rgba(15, 23, 42, 0.08);
}

:deep(.ant-form-item-label > label) {
  color: #1e293b;
  font-weight: 900;
}

:deep(.ant-input-affix-wrapper-lg) {
  min-height: 54px;
  padding-inline: 16px;
}

.form-options {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin: -2px 0 26px;
  color: #64748b;
  font-weight: 700;
}

.form-options a {
  font-weight: 900;
}

.login-divider {
  display: grid;
  grid-template-columns: 1fr auto 1fr;
  gap: 16px;
  align-items: center;
  margin: 28px 0;
  color: #94a3b8;
  font-weight: 800;
}

.login-divider i {
  height: 1px;
  background: #e2e8f0;
}

.sso-button {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  height: 54px;
  font-size: 15px;
}

.login-secondary-actions {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  margin-top: 32px;
  color: #64748b;
  font-weight: 800;
}

.showcase-footer {
  display: grid;
  gap: 12px;
  color: rgba(226, 232, 240, 0.86);
  font-weight: 800;
}

.showcase-footer span {
  display: inline-flex;
  align-items: center;
  gap: 10px;
}

.showcase-footer small {
  color: rgba(226, 232, 240, 0.78);
  font-size: 14px;
}

.welcome-overlay {
  position: fixed;
  inset: 0;
  z-index: 50;
  display: grid;
  place-items: center;
  background: rgba(7, 25, 43, 0.48);
  backdrop-filter: blur(10px);
}

.welcome-panel {
  width: min(420px, calc(100vw - 32px));
  padding: 28px;
  border-radius: 10px;
  color: #fff;
  text-align: center;
  background: linear-gradient(160deg, #07192b, #0f304a);
  box-shadow: 0 30px 80px rgba(2, 6, 23, 0.32);
}

.welcome-panel span {
  color: #ffb86b;
  font-size: 12px;
  font-weight: 900;
  text-transform: uppercase;
}

.welcome-panel strong {
  display: block;
  margin-top: 12px;
  font-size: 34px;
  line-height: 1;
}

.welcome-panel p {
  margin: 10px 0 0;
  color: rgba(226, 232, 240, 0.78);
}

.welcome-bar {
  height: 4px;
  margin-top: 22px;
  overflow: hidden;
  border-radius: 999px;
  background: rgba(255, 255, 255, 0.12);
}

.welcome-bar i {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: #ff8a00;
  animation: welcome-progress 0.76s linear forwards;
}

.welcome-fade-enter-active,
.welcome-fade-leave-active {
  transition: opacity 0.2s ease;
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

@media (max-width: 980px) {
  .login-stage {
    grid-template-columns: 1fr;
  }

  .login-showcase {
    min-height: auto;
  }

  .showcase-copy {
    margin-top: 70px;
  }

  .login-panel {
    width: calc(100% - 32px);
    margin: 16px;
  }
}

@media (max-width: 560px) {
  .login-panel,
  .login-showcase {
    padding: 20px;
  }

  .brand-row strong {
    font-size: 22px;
  }

  .brand-row span {
    font-size: 10px;
  }

  .feature-list article > span {
    width: 48px;
    height: 48px;
  }

  .login-copy h2 {
    font-size: 28px;
  }
}
</style>
