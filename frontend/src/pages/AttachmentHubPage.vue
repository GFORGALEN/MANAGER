<template>
  <ProjectModuleNavigator
    :title="t('hubAttachmentTitle')"
    :description="t('hubAttachmentDescription')"
    tab-key="attachments"
    :cta-label="t('hubAttachmentsLabel')"
  />
</template>

<script setup lang="ts">
import ProjectModuleNavigator from '@/components/ProjectModuleNavigator.vue'
import { useI18n } from '@/services/i18n'

const { t } = useI18n()
</script>
