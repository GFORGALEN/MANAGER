<template>
  <div class="login-shell">
    <a-card class="login-card" :bordered="false">
      <div class="login-toolbar">
        <LanguageSwitcher />
      </div>
      <div class="login-copy">
        <span class="eyebrow">{{ t('loginEyebrow') }}</span>
        <h1>{{ t('loginTitle') }}</h1>
        <p>{{ t('loginCopy') }}</p>
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

      <a-divider />

      <a-button block @click="router.push({ name: 'register' })">
        {{ t('loginCreate') }}
      </a-button>

      <a-divider />

      <a-space direction="vertical" size="small">
        <span><strong>{{ t('loginDemo') }}</strong></span>
        <span>`admin / Admin123!`</span>
        <span>`pm / Pm123!`</span>
        <span>`contractor / Contractor123!`</span>
      </a-space>
    </a-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
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
  } catch (error) {
    clearSession()
    errorMessage.value = t('loginError')
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
.login-shell {
  display: grid;
  place-items: center;
  min-height: 100vh;
  padding: 24px;
  background:
    radial-gradient(circle at top left, rgba(59, 130, 246, 0.18), transparent 24%),
    radial-gradient(circle at bottom right, rgba(16, 185, 129, 0.12), transparent 22%);
}

.login-card {
  width: 100%;
  max-width: 460px;
  border-radius: 28px;
  border: 1px solid rgba(226, 232, 240, 0.85);
  background: rgba(255, 255, 255, 0.92);
  box-shadow: 0 28px 80px rgba(15, 23, 42, 0.12);
  backdrop-filter: blur(14px);
}

.login-toolbar {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 12px;
}

.login-copy {
  margin-bottom: 24px;
}

.login-copy h1 {
  margin: 8px 0 10px;
  font-size: 30px;
  line-height: 1.1;
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
</style>
