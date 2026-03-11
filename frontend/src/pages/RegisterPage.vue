<template>
  <div class="login-shell">
    <a-card class="login-card" :bordered="false">
      <div class="login-toolbar">
        <LanguageSwitcher />
      </div>
      <div class="login-copy">
        <span class="eyebrow">{{ t('loginEyebrow') }}</span>
        <h1>{{ t('registerTitle') }}</h1>
        <p>{{ t('registerCopy') }}</p>
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
        <a-form-item :label="t('registerName')">
          <a-input v-model:value="formState.name" size="large" />
        </a-form-item>

        <a-form-item :label="t('registerEmail')">
          <a-input v-model:value="formState.email" size="large" />
        </a-form-item>

        <a-form-item :label="t('registerPhone')">
          <a-input v-model:value="formState.phoneNumber" size="large" />
        </a-form-item>

        <a-form-item :label="t('registerPassword')">
          <a-input-password v-model:value="formState.password" size="large" />
        </a-form-item>

        <a-form-item :label="t('registerConfirm')">
          <a-input-password v-model:value="formState.confirmPassword" size="large" @press-enter="handleSubmit" />
        </a-form-item>

        <a-space direction="vertical" style="width: 100%">
          <a-button type="primary" block size="large" :loading="submitting" @click="handleSubmit">
            {{ t('registerAction') }}
          </a-button>
          <a-button block @click="router.push({ name: 'login' })">{{ t('registerBack') }}</a-button>
        </a-space>
      </a-form>
    </a-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref } from 'vue'
import { useRouter } from 'vue-router'

import LanguageSwitcher from '@/components/LanguageSwitcher.vue'
import api from '@/services/api'
import { useI18n } from '@/services/i18n'
import type { RegisterRequest, RegisterResponse } from '@/types/auth'

const router = useRouter()
const { t } = useI18n()

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
    errorMessage.value = t('registerRequired')
    return
  }

  if (formState.password !== formState.confirmPassword) {
    errorMessage.value = t('registerMatch')
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
  background:
    radial-gradient(circle at top left, rgba(59, 130, 246, 0.18), transparent 24%),
    radial-gradient(circle at bottom right, rgba(16, 185, 129, 0.12), transparent 22%);
}

.login-card {
  width: 100%;
  max-width: 500px;
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
