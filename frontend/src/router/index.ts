import { createRouter, createWebHistory } from 'vue-router'

import AppLayout from '@/layouts/AppLayout.vue'
import AttachmentHubPage from '@/pages/AttachmentHubPage.vue'
import ProjectDetailPage from '@/pages/ProjectDetailPage.vue'
import ProjectListPage from '@/pages/ProjectListPage.vue'
import TaskHubPage from '@/pages/TaskHubPage.vue'
import VariationHubPage from '@/pages/VariationHubPage.vue'
import LoginPage from '@/pages/LoginPage.vue'
import { isAuthenticated } from '@/services/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: LoginPage,
      meta: {
        guestOnly: true,
      },
    },
    {
      path: '/',
      component: AppLayout,
      meta: {
        requiresAuth: true,
      },
      children: [
        {
          path: '',
          redirect: '/projects',
        },
        {
          path: 'projects',
          name: 'projects',
          component: ProjectListPage,
          meta: {
            title: 'Projects',
          },
        },
        {
          path: 'projects/:projectId',
          name: 'project-detail',
          component: ProjectDetailPage,
          props: true,
          meta: {
            title: 'Project Detail',
          },
        },
        {
          path: 'tasks',
          name: 'tasks',
          component: TaskHubPage,
          meta: {
            title: 'Tasks',
          },
        },
        {
          path: 'variations',
          name: 'variations',
          component: VariationHubPage,
          meta: {
            title: 'Variations',
          },
        },
        {
          path: 'attachments',
          name: 'attachments',
          component: AttachmentHubPage,
          meta: {
            title: 'Attachments',
          },
        },
      ],
    },
  ],
})

router.beforeEach((to) => {
  const authed = isAuthenticated()

  if (to.meta.requiresAuth && !authed) {
    return {
      name: 'login',
      query: { redirect: to.fullPath },
    }
  }

  if (to.meta.guestOnly && authed) {
    return { name: 'projects' }
  }

  return true
})

export default router
