<template>
  <div class="login-shell">
    <a-card class="login-card" :bordered="false">
      <div class="login-copy">
        <span class="eyebrow">Construction Management</span>
        <h1>Create worker account</h1>
        <p>Public registration only creates Contractor accounts.</p>
      </div>

      <a-alert
        v-if="successMessage"
        type="success"
        :message="successMessage"
        show-icon
        style="margin-bottom: 16px"
      />

      <a-alert
        v-if="errorMessage"
        type="error"
        :message="errorMessage"
        show-icon
        style="margin-bottom: 16px"
      />

      <a-form :model="formState" layout="vertical">
        <a-form-item label="Name">
          <a-input v-model:value="formState.name" size="large" />
        </a-form-item>

        <a-form-item label="Email">
          <a-input v-model:value="formState.email" size="large" />
        </a-form-item>

        <a-form-item label="Phone Number">
          <a-input v-model:value="formState.phoneNumber" size="large" />
        </a-form-item>

        <a-form-item label="Password">
          <a-input-password v-model:value="formState.password" size="large" />
        </a-form-item>

        <a-form-item label="Confirm Password">
          <a-input-password v-model:value="formState.confirmPassword" size="large" @press-enter="handleSubmit" />
        </a-form-item>

        <a-space direction="vertical" style="width: 100%">
          <a-button type="primary" block size="large" :loading="submitting" @click="handleSubmit">
            Register
          </a-button>
          <a-button block @click="router.push({ name: 'login' })">Back to Sign In</a-button>
        </a-space>
      </a-form>
    </a-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'

import api from '@/services/api'
import type { RegisterRequest, RegisterResponse } from '@/types/auth'

const router = useRouter()

const formState = reactive<RegisterRequest>({
  name: '',
  email: '',
  phoneNumber: '',
  password: '',
  confirmPassword: '',
})

const submitting = ref(false)
const errorMessage = ref('')
const successMessage = ref('')

async function handleSubmit() {
  if (!formState.name.trim() || !formState.email.trim() || !formState.password || !formState.confirmPassword) {
    errorMessage.value = 'Name, email, password, and confirm password are required.'
    return
  }

  if (formState.password !== formState.confirmPassword) {
    errorMessage.value = 'Password and confirm password must match.'
    return
  }

  submitting.value = true
  errorMessage.value = ''
  successMessage.value = ''

  try {
    const { data } = await api.post<RegisterResponse>('/auth/register', formState)
    successMessage.value = data.message
    setTimeout(() => {
      router.push({ name: 'login' })
    }, 900)
  } catch (error: any) {
    errorMessage.value = error?.response?.data?.message ?? 'Registration failed.'
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
  max-width: 500px;
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
