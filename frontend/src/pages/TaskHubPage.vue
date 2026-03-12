<template>
  <ProjectModuleNavigator
    :title="t('hubTaskTitle')"
    :description="t('hubTaskDescription')"
    tab-key="tasks"
    :cta-label="t('hubTasksLabel')"
  />
</template>

<script setup lang="ts">
import ProjectModuleNavigator from '@/components/ProjectModuleNavigator.vue'
import { useI18n } from '@/services/i18n'

const { t } = useI18n()
</script>
