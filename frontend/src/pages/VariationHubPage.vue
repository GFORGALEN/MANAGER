<template>
  <ProjectModuleNavigator
    :title="t('hubVariationTitle')"
    :description="t('hubVariationDescription')"
    tab-key="variations"
    :cta-label="t('hubVariationsLabel')"
  />
</template>

<script setup lang="ts">
import ProjectModuleNavigator from '@/components/ProjectModuleNavigator.vue'
import { useI18n } from '@/services/i18n'

const { t } = useI18n()
</script>
