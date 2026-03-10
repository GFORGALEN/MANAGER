export interface TaskAttachment {
  attachmentId: string
  fileName: string
  filePath: string
  contentType?: string | null
  fileSize: number
  uploadedAt: string
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
  dueDate: string
  createdAt: string
}

export interface CreateTaskPayload {
  title: string
  description?: string | null
  dueDate: string
  assignedUserId?: string | null
}

export interface UpdateTaskPayload {
  title: string
  description?: string | null
  dueDate: string
  assignedUserId?: string | null
}

export interface UpdateTaskStatusPayload {
  status: 'Todo' | 'Doing' | 'Done'
}

export interface TaskDetail extends TaskItem {
  projectAddress: string
  attachments: TaskAttachment[]
}
