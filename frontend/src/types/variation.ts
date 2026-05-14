export interface Variation {
  variationId: string
  projectId: string
  title: string
  description?: string | null
  amount: number
  status: 'Draft' | 'Submitted' | 'Approved' | 'Rejected' | 'NeedInfo'
  createdAt: string
}

export interface VariationStatusHistory {
  variationStatusHistoryId: string
  fromStatus: string
  toStatus: string
  comment?: string | null
  actorUserId?: string | null
  actorName: string
  createdAt: string
}

export interface VariationDetail extends Variation {
  statusHistory: VariationStatusHistory[]
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
  comment?: string | null
}
