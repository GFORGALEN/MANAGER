<template>
  <div class="login-shell">
    <a-card class="login-card" :bordered="false">
      <div class="login-copy">
        <span class="eyebrow">Construction Management</span>
        <h1>Sign in to the platform</h1>
        <p>Use your backend-seeded account to access projects and operational pages.</p>
      </div>

      <a-alert
        v-if="errorMessage"
        type="error"
        :message="errorMessage"
        show-icon
        style="margin-bottom: 16px"
      />

      <a-form :model="formState" layout="vertical">
        <a-form-item label="Username" name="username">
          <a-input
            v-model:value="formState.username"
            placeholder="admin"
            size="large"
            @press-enter="handleSubmit"
          />
        </a-form-item>

        <a-form-item label="Password" name="password">
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
          Sign in
        </a-button>
      </a-form>

      <a-divider />

      <a-space direction="vertical" size="small">
        <span><strong>Demo accounts</strong></span>
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

import api from '@/services/api'
import { setSession } from '@/services/auth'
import type { AuthResponse, LoginRequest } from '@/types/auth'

const router = useRouter()
const route = useRoute()

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
    setSession(data.token, data.username, data.role)

    const redirect = typeof route.query.redirect === 'string' ? route.query.redirect : '/projects'
    router.push(redirect)
  } catch (error) {
    errorMessage.value = 'Login failed. Check username, password, or backend availability.'
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
}

.login-card {
  width: 100%;
  max-width: 460px;
  border-radius: 28px;
  box-shadow: 0 28px 80px rgba(15, 23, 42, 0.12);
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
  padding: 6px 10px;
  border-radius: 999px;
  background: #dbeafe;
  color: #1d4ed8;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}
</style>
