export interface TaskItem {
  taskItemId: string
  projectId: string
  title: string
  description?: string | null
  status: 'Todo' | 'Doing' | 'Done'
  dueDate: string
  createdAt: string
}

export interface CreateTaskPayload {
  title: string
  description?: string | null
  dueDate: string
}

export interface UpdateTaskPayload {
  title: string
  description?: string | null
  dueDate: string
}

export interface UpdateTaskStatusPayload {
  status: 'Todo' | 'Doing' | 'Done'
}
