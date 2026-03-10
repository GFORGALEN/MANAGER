<template>
  <div class="page-shell">
    <a-card class="page-card" :bordered="false">
      <template #title>Users</template>
      <template #extra>
        <a-space>
          <a-input-search
            v-model:value="query.keyword"
            placeholder="Search by name or email"
            style="width: 240px"
            @search="fetchUsers"
          />
          <a-select v-model:value="query.role" style="width: 140px" @change="fetchUsers">
            <a-select-option value="">All Roles</a-select-option>
            <a-select-option value="Admin">Admin</a-select-option>
            <a-select-option value="PM">PM</a-select-option>
            <a-select-option value="Contractor">Contractor</a-select-option>
          </a-select>
          <a-select v-model:value="activeFilter" style="width: 140px" @change="handleActiveChange">
            <a-select-option value="all">All Status</a-select-option>
            <a-select-option value="active">Active</a-select-option>
            <a-select-option value="inactive">Inactive</a-select-option>
          </a-select>
          <a-button @click="fetchUsers">Refresh</a-button>
        </a-space>
      </template>

      <a-table
        :columns="columns"
        :data-source="users"
        :loading="loading"
        row-key="userId"
        :pagination="false"
      >
        <template #bodyCell="{ column, record }">
          <template v-if="column.key === 'isActive'">
            <a-tag :color="record.isActive ? 'success' : 'default'">
              {{ record.isActive ? 'Active' : 'Inactive' }}
            </a-tag>
          </template>
          <template v-else-if="column.key === 'actions'">
            <a-space>
              <a-button size="small" @click="openUserModal(record)">View</a-button>
              <a-button v-if="isAdmin" size="small" type="primary" @click="openUserModal(record)">Manage</a-button>
            </a-space>
          </template>
        </template>
      </a-table>

      <div class="table-footer">
        <a-pagination
          :current="query.pageNumber"
          :page-size="query.pageSize"
          :total="totalCount"
          show-size-changer
          @change="handlePageChange"
          @showSizeChange="handlePageChange"
        />
      </div>
    </a-card>

    <a-modal
      v-model:open="modalOpen"
      title="User Management"
      :footer="null"
      width="640px"
    >
      <a-form v-if="selectedUser" layout="vertical">
        <a-form-item label="Name">
          <a-input v-model:value="editForm.name" :disabled="!isAdmin" />
        </a-form-item>
        <a-form-item label="Email">
          <a-input v-model:value="editForm.email" :disabled="!isAdmin" />
        </a-form-item>
        <a-form-item label="Phone Number">
          <a-input v-model:value="editForm.phoneNumber" :disabled="!isAdmin" />
        </a-form-item>
        <a-form-item label="Role">
          <a-select v-model:value="roleForm.role" :disabled="!isAdmin">
            <a-select-option value="Admin">Admin</a-select-option>
            <a-select-option value="PM">PM</a-select-option>
            <a-select-option value="Contractor">Contractor</a-select-option>
          </a-select>
        </a-form-item>
        <a-form-item label="Active">
          <a-switch v-model:checked="activeForm.isActive" :disabled="!isAdmin" />
        </a-form-item>

        <a-space v-if="isAdmin">
          <a-button type="primary" :loading="savingProfile" @click="saveUserProfile">Save Profile</a-button>
          <a-button :loading="savingRole" @click="saveUserRole">Save Role</a-button>
          <a-button :loading="savingActive" @click="saveUserActive">Save Active</a-button>
        </a-space>
      </a-form>
    </a-modal>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue'
import { message } from 'ant-design-vue'

import api from '@/services/api'
import { getCurrentRole } from '@/services/auth'
import type { PagedResult } from '@/types/common'
import type {
  UpdateUserActivePayload,
  UpdateUserPayload,
  UpdateUserRolePayload,
  UserDetail,
  UserSummary,
} from '@/types/user'

const users = ref<UserSummary[]>([])
const totalCount = ref(0)
const loading = ref(false)
const modalOpen = ref(false)
const selectedUser = ref<UserDetail | null>(null)
const savingProfile = ref(false)
const savingRole = ref(false)
const savingActive = ref(false)

const isAdmin = computed(() => getCurrentRole() === 'Admin')
const activeFilter = ref<'all' | 'active' | 'inactive'>('all')

const query = reactive({
  keyword: '',
  role: '',
  isActive: undefined as boolean | undefined,
  pageNumber: 1,
  pageSize: 10,
})

const editForm = reactive<UpdateUserPayload>({
  name: '',
  email: '',
  phoneNumber: '',
})

const roleForm = reactive<UpdateUserRolePayload>({
  role: 'Contractor',
})

const activeForm = reactive<UpdateUserActivePayload>({
  isActive: true,
})

const columns = [
  { title: 'Name', dataIndex: 'name', key: 'name' },
  { title: 'Email', dataIndex: 'email', key: 'email' },
  { title: 'Role', dataIndex: 'role', key: 'role' },
  { title: 'Active', dataIndex: 'isActive', key: 'isActive' },
  { title: 'Actions', key: 'actions', width: 160 },
]

async function fetchUsers() {
  loading.value = true
  try {
    const { data } = await api.get<PagedResult<UserSummary>>('/users', {
      params: {
        keyword: query.keyword || undefined,
        role: query.role || undefined,
        isActive: query.isActive,
        pageNumber: query.pageNumber,
        pageSize: query.pageSize,
      },
    })

    users.value = data.items
    totalCount.value = data.totalCount
  } catch {
    message.error('Failed to load users.')
  } finally {
    loading.value = false
  }
}

async function openUserModal(user: UserSummary) {
  try {
    const { data } = await api.get<UserDetail>(`/users/${user.userId}`)
    selectedUser.value = data
    editForm.name = data.name
    editForm.email = data.email
    editForm.phoneNumber = data.phoneNumber ?? ''
    roleForm.role = data.role
    activeForm.isActive = data.isActive
    modalOpen.value = true
  } catch {
    message.error('Failed to load user detail.')
  }
}

async function saveUserProfile() {
  if (!selectedUser.value) {
    return
  }

  savingProfile.value = true
  try {
    await api.put(`/users/${selectedUser.value.userId}`, editForm)
    message.success('User profile updated.')
    await fetchUsers()
  } catch {
    message.error('Failed to update user profile.')
  } finally {
    savingProfile.value = false
  }
}

async function saveUserRole() {
  if (!selectedUser.value) {
    return
  }

  savingRole.value = true
  try {
    await api.patch(`/users/${selectedUser.value.userId}/role`, roleForm)
    message.success('User role updated.')
    await fetchUsers()
  } catch {
    message.error('Failed to update user role.')
  } finally {
    savingRole.value = false
  }
}

async function saveUserActive() {
  if (!selectedUser.value) {
    return
  }

  savingActive.value = true
  try {
    await api.patch(`/users/${selectedUser.value.userId}/active`, activeForm)
    message.success('User active status updated.')
    await fetchUsers()
  } catch {
    message.error('Failed to update user active status.')
  } finally {
    savingActive.value = false
  }
}

function handlePageChange(page: number, pageSize: number) {
  query.pageNumber = page
  query.pageSize = pageSize
  fetchUsers()
}

function handleActiveChange(value: 'all' | 'active' | 'inactive') {
  activeFilter.value = value
  query.isActive = value === 'all' ? undefined : value === 'active'
  fetchUsers()
}

onMounted(fetchUsers)
</script>

<style scoped>
.table-footer {
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}
</style>
