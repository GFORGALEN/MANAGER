import { createRouter, createWebHistory } from 'vue-router'

import AppLayout from '@/layouts/AppLayout.vue'
import AttachmentHubPage from '@/pages/AttachmentHubPage.vue'
import ProjectDetailPage from '@/pages/ProjectDetailPage.vue'
import ProjectListPage from '@/pages/ProjectListPage.vue'
import RegisterPage from '@/pages/RegisterPage.vue'
import TaskHubPage from '@/pages/TaskHubPage.vue'
import VariationHubPage from '@/pages/VariationHubPage.vue'
import LoginPage from '@/pages/LoginPage.vue'
import UserManagementPage from '@/pages/UserManagementPage.vue'
import WorkerDashboardPage from '@/pages/WorkerDashboardPage.vue'
import WorkerProfilePage from '@/pages/WorkerProfilePage.vue'
import WorkerTaskDetailPage from '@/pages/WorkerTaskDetailPage.vue'
import WorkerTasksPage from '@/pages/WorkerTasksPage.vue'
import { clearSession, getDefaultRouteForRole, getCurrentRole, hasRole, isAuthenticated } from '@/services/auth'

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
      path: '/register',
      name: 'register',
      component: RegisterPage,
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
            roles: ['Admin', 'PM'],
          },
        },
        {
          path: 'projects/:projectId',
          name: 'project-detail',
          component: ProjectDetailPage,
          props: true,
          meta: {
            title: 'Project Detail',
            roles: ['Admin', 'PM'],
          },
        },
        {
          path: 'tasks',
          name: 'tasks',
          component: TaskHubPage,
          meta: {
            title: 'Tasks',
            roles: ['Admin', 'PM'],
          },
        },
        {
          path: 'variations',
          name: 'variations',
          component: VariationHubPage,
          meta: {
            title: 'Variations',
            roles: ['Admin', 'PM'],
          },
        },
        {
          path: 'attachments',
          name: 'attachments',
          component: AttachmentHubPage,
          meta: {
            title: 'Attachments',
            roles: ['Admin', 'PM'],
          },
        },
        {
          path: 'users',
          name: 'users',
          component: UserManagementPage,
          meta: {
            title: 'Users',
            roles: ['Admin', 'PM'],
          },
        },
        {
          path: 'worker/dashboard',
          name: 'worker-dashboard',
          component: WorkerDashboardPage,
          meta: {
            title: 'Worker Dashboard',
            roles: ['Contractor'],
          },
        },
        {
          path: 'worker/tasks',
          name: 'worker-tasks',
          component: WorkerTasksPage,
          meta: {
            title: 'My Tasks',
            roles: ['Contractor'],
          },
        },
        {
          path: 'worker/tasks/:id',
          name: 'worker-task-detail',
          component: WorkerTaskDetailPage,
          props: true,
          meta: {
            title: 'Task Detail',
            roles: ['Contractor'],
          },
        },
        {
          path: 'worker/profile',
          name: 'worker-profile',
          component: WorkerProfilePage,
          meta: {
            title: 'My Profile',
            roles: ['Contractor'],
          },
        },
      ],
    },
  ],
})

router.beforeEach((to) => {
  const authed = isAuthenticated()
  const currentRole = getCurrentRole()

  if (to.meta.requiresAuth && !authed) {
    return {
      name: 'login',
      query: { redirect: to.fullPath },
    }
  }

  if (authed && !currentRole) {
    clearSession()

    if (to.name === 'login' || to.name === 'register') {
      return true
    }

    return {
      name: 'login',
      query: { redirect: to.fullPath },
    }
  }

  if (to.meta.guestOnly && authed) {
    const redirectPath = getDefaultRouteForRole(currentRole)
    return redirectPath === to.path ? true : redirectPath
  }

  const roles = Array.isArray(to.meta.roles) ? (to.meta.roles as string[]) : undefined
  if (authed && roles && !hasRole(roles)) {
    const redirectPath = getDefaultRouteForRole(currentRole)
    return redirectPath === to.path ? true : redirectPath
  }

  return true
})

export default router
