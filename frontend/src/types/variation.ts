export interface Variation {
  variationId: string
  projectId: string
  title: string
  description?: string | null
  amount: number
  status: 'Draft' | 'Submitted' | 'Approved' | 'Rejected' | 'NeedInfo'
  createdAt: string
}

export interface CreateVariationPayload {
  title: string
  description?: string | null
  amount: number
}

export interface UpdateVariationPayload {
  title: string
  description?: string | null
  amount: number
}

export interface UpdateVariationStatusPayload {
  status: 'Draft' | 'Submitted' | 'Approved' | 'Rejected' | 'NeedInfo'
}
