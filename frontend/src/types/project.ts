export interface ProjectListItem {
  projectId: string
  name: string
  address: string
  createdAt: string
}

export interface ProjectDetail {
  projectId: string
  name: string
  address: string
  createdAt: string
}

export interface CreateProjectPayload {
  name: string
  address: string
}

export interface UpdateProjectPayload {
  name: string
  address: string
}
