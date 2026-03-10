export interface ProjectListItem {
  projectId: string
  code: string
  name: string
  address: string
  clientName?: string | null
  status: string
  createdAt: string
}

export interface ProjectDetail {
  projectId: string
  code: string
  name: string
  address: string
  description?: string | null
  clientName?: string | null
  status: string
  budget?: number | null
  startDate?: string | null
  endDate?: string | null
  createdAt: string
}

export interface CreateProjectPayload {
  code: string
  name: string
  address: string
  description?: string | null
  clientName?: string | null
  status: string
  budget?: number | null
  startDate?: string | null
  endDate?: string | null
}

export interface UpdateProjectPayload {
  code: string
  name: string
  address: string
  description?: string | null
  clientName?: string | null
  status: string
  budget?: number | null
  startDate?: string | null
  endDate?: string | null
}
