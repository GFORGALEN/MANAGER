<template>
  <a-select :value="locale" size="small" style="width: 110px" @change="handleChange">
    <a-select-option value="en">{{ t('english') }}</a-select-option>
    <a-select-option value="zh">{{ t('chinese') }}</a-select-option>
  </a-select>
</template>

<script setup lang="ts">
import type { Locale } from '@/services/i18n'
import { useI18n } from '@/services/i18n'

const { locale, setLocale, t } = useI18n()

function handleChange(value: Locale) {
  setLocale(value)
}
</script>
