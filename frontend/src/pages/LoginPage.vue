<template>
  <div class="login-shell">
    <div class="login-stage" @mousemove="handleMouseMove" @mouseleave="resetMouse">
      <section class="login-showcase" :style="showcaseStyle">
        <div class="login-toolbar">
          <LanguageSwitcher />
        </div>
        <div class="hero-grid"></div>
        <div class="hero-orb hero-orb-a"></div>
        <div class="hero-orb hero-orb-b"></div>

        <div class="showcase-copy">
          <span class="eyebrow">{{ t('loginEyebrow') }}</span>
          <h1>{{ t('loginTitle') }}</h1>
          <p>{{ t('loginCopy') }}</p>
        </div>

        <div class="signal-board">
          <div class="signal-chip">
            <span class="signal-kicker">Today</span>
            <strong>12</strong>
            <span>new assignments</span>
          </div>
          <div class="signal-chip">
            <span class="signal-kicker">On site</span>
            <strong>28</strong>
            <span>active workers</span>
          </div>
          <div class="signal-chip">
            <span class="signal-kicker">Alerts</span>
            <strong>3</strong>
            <span>pending changes</span>
          </div>
        </div>

        <div class="timeline-card">
          <div class="timeline-head">
            <strong>Daily coordination</strong>
            <span>Live</span>
          </div>
          <div class="timeline-row">
            <span>07:30</span>
            <div>
              <strong>Morning briefing</strong>
              <p>Assign crews, confirm access windows, set priority work fronts.</p>
            </div>
          </div>
          <div class="timeline-row">
            <span>11:00</span>
            <div>
              <strong>Progress checkpoint</strong>
              <p>Review task movement, notify teams, and capture site changes.</p>
            </div>
          </div>
          <div class="timeline-row">
            <span>16:30</span>
            <div>
              <strong>Close-out sync</strong>
              <p>Mark completed work, update tomorrow's plan, and issue notices.</p>
            </div>
          </div>
        </div>
      </section>

      <a-card class="login-card" :bordered="false">
        <div class="login-copy">
          <span class="eyebrow subtle">Workspace Access</span>
          <h2>{{ t('loginAction') }}</h2>
          <p>Use your account to enter the live project workspace.</p>
        </div>

        <a-alert
          v-if="errorMessage"
          type="error"
          :message="errorMessage"
          show-icon
          style="margin-bottom: 16px"
        />

        <a-form :model="formState" layout="vertical">
          <a-form-item :label="t('loginUser')" name="username">
            <a-input
              v-model:value="formState.username"
              placeholder="admin"
              size="large"
              @press-enter="handleSubmit"
            />
          </a-form-item>

          <a-form-item :label="t('loginPassword')" name="password">
            <a-input-password
              v-model:value="formState.password"
              placeholder="Admin123!"
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
            {{ t('loginAction') }}
          </a-button>
        </a-form>

        <div class="login-secondary-actions">
          <a-button block @click="router.push({ name: 'register' })">
            {{ t('loginCreate') }}
          </a-button>
        </div>

        <div class="demo-panel">
          <span><strong>{{ t('loginDemo') }}</strong></span>
          <div class="demo-item"><span>Admin</span><code>admin / Admin123!</code></div>
          <div class="demo-item"><span>PM</span><code>pm / Pm123!</code></div>
          <div class="demo-item"><span>Worker</span><code>contractor / Contractor123!</code></div>
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

const router = useRouter()
const route = useRoute()
const { t } = useI18n()

const formState = reactive<LoginRequest>({
  username: 'admin',
  password: 'Admin123!',
})

const submitting = ref(false)
const errorMessage = ref('')
const mouseX = ref(0)
const mouseY = ref(0)

const showcaseStyle = computed(() => ({
  transform: `perspective(1200px) rotateX(${mouseY.value * -5}deg) rotateY(${mouseX.value * 7}deg)`,
}))

async function handleSubmit() {
  submitting.value = true
  errorMessage.value = ''

  try {
    const { data } = await api.post<AuthResponse>('/auth/login', formState)
    setToken(data.token)

    const profileResponse = await api.get<CurrentUser>('/auth/me')
    setCurrentUser(profileResponse.data)

    const redirect = typeof route.query.redirect === 'string'
      ? route.query.redirect
      : getDefaultRouteForRole(profileResponse.data.role)

    router.push(redirect)
  } catch {
    clearSession()
    errorMessage.value = t('loginError')
  } finally {
    submitting.value = false
  }
}

function handleMouseMove(event: MouseEvent) {
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

.login-stage {
  width: 100%;
  max-width: 1220px;
  display: grid;
  grid-template-columns: minmax(0, 1.15fr) minmax(380px, 460px);
  gap: 24px;
  align-items: stretch;
}

.login-showcase {
  position: relative;
  overflow: hidden;
  border-radius: 34px;
  padding: 28px;
  min-height: 690px;
  background:
    linear-gradient(160deg, rgba(15, 23, 42, 0.95), rgba(30, 41, 59, 0.92)),
    linear-gradient(135deg, #1e293b, #0f172a);
  color: #fff;
  box-shadow: 0 34px 90px rgba(15, 23, 42, 0.22);
  transition: transform 0.22s ease;
  transform-style: preserve-3d;
}

.login-card {
  width: 100%;
  align-self: center;
  border-radius: 30px;
  border: 1px solid rgba(226, 232, 240, 0.85);
  background: rgba(249, 244, 237, 0.94);
  box-shadow: 0 28px 80px rgba(51, 38, 24, 0.16);
  backdrop-filter: blur(14px);
  padding-top: 10px;
}

.login-toolbar,
.showcase-copy,
.signal-board,
.timeline-card {
  position: relative;
  z-index: 2;
}

.login-toolbar {
  display: flex;
  justify-content: flex-end;
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
  margin: 44px 0 28px;
}

.showcase-copy h1 {
  margin: 12px 0;
  font-size: clamp(42px, 5vw, 64px);
  line-height: 0.98;
  letter-spacing: -0.06em;
}

.showcase-copy p {
  max-width: 520px;
  color: rgba(226, 232, 240, 0.86);
  font-size: 16px;
}

.signal-board {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 14px;
  margin-bottom: 18px;
}

.signal-chip {
  padding: 18px;
  border-radius: 22px;
  background: rgba(255, 255, 255, 0.07);
  border: 1px solid rgba(255, 255, 255, 0.1);
  backdrop-filter: blur(10px);
}

.signal-chip strong {
  display: block;
  margin: 4px 0;
  font-size: 28px;
}

.signal-chip span {
  color: rgba(226, 232, 240, 0.74);
}

.signal-kicker {
  font-size: 11px;
  letter-spacing: 0.08em;
  text-transform: uppercase;
}

.timeline-card {
  margin-top: 22px;
  padding: 22px;
  border-radius: 24px;
  background: rgba(255, 255, 255, 0.92);
  color: #0f172a;
  box-shadow: 0 18px 38px rgba(15, 23, 42, 0.18);
}

.timeline-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.timeline-head span {
  padding: 5px 10px;
  border-radius: 999px;
  background: #dcfce7;
  color: #166534;
  font-size: 12px;
  font-weight: 700;
}

.timeline-row {
  display: grid;
  grid-template-columns: 70px minmax(0, 1fr);
  gap: 14px;
  padding: 12px 0;
  border-top: 1px dashed rgba(148, 163, 184, 0.36);
}

.timeline-row:first-of-type {
  border-top: 0;
  padding-top: 0;
}

.timeline-row span {
  color: #475569;
  font-weight: 700;
}

.timeline-row p {
  margin: 4px 0 0;
  color: #64748b;
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

@media (max-width: 1024px) {
  .login-stage {
    grid-template-columns: 1fr;
  }

  .login-showcase {
    min-height: 560px;
  }
}

@media (max-width: 768px) {
  .login-shell {
    padding: 14px;
  }

  .login-showcase {
    min-height: auto;
    padding: 18px;
    border-radius: 24px;
  }

  .showcase-copy {
    margin-top: 20px;
  }

  .signal-board {
    grid-template-columns: 1fr;
  }

  .timeline-row {
    grid-template-columns: 1fr;
    gap: 6px;
  }
}
</style>
