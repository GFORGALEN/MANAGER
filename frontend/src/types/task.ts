export interface TaskAttachment {
  attachmentId: string
  fileName: string
  filePath: string
  contentType?: string | null
  fileSize: number
  uploadedAt: string
}

export interface TaskAssignee {
  userId: string
  name: string
  email: string
  role: 'Admin' | 'PM' | 'Contractor' | string
}

export interface TaskItem {
  taskItemId: string
  projectId: string
  title: string
  projectName: string
  description?: string | null
  status: 'Todo' | 'Doing' | 'Done'
  assignedUserId?: string | null
  assignedUserName?: string | null
  assignedUserIds: string[]
  assignedUsers: TaskAssignee[]
  startDate?: string | null
  dueDate: string
  createdAt: string
}

export interface CreateTaskPayload {
  title: string
  description?: string | null
  startDate?: string | null
  dueDate: string
  assignedUserId?: string | null
  assignedUserIds?: string[]
}

export interface UpdateTaskPayload {
  title: string
  description?: string | null
  startDate?: string | null
  dueDate: string
  assignedUserId?: string | null
  assignedUserIds?: string[]
}

export interface UpdateTaskStatusPayload {
  status: 'Todo' | 'Doing' | 'Done'
}

export interface TaskSmsResult {
  taskItemId: string
  taskTitle: string
  attemptedCount: number
  sentCount: number
  skippedRecipients: string[]
  failedRecipients: string[]
}

export interface TaskEmailResult {
  taskItemId: string
  taskTitle: string
  attemptedCount: number
  sentCount: number
  sentRecipients: string[]
  skippedRecipients: string[]
  failedRecipients: string[]
}

export interface TaskDetail extends TaskItem {
  projectAddress: string
  attachments: TaskAttachment[]
}
