export interface Attachment {
  attachmentId: string
  projectId: string
  fileName: string
  filePath: string
  contentType?: string | null
  fileSize: number
  uploadedAt: string
}

export interface CreateAttachmentPayload {
  fileName: string
  filePath: string
  contentType?: string | null
}

export interface UpdateAttachmentPayload {
  fileName: string
  filePath: string
  contentType?: string | null
}
