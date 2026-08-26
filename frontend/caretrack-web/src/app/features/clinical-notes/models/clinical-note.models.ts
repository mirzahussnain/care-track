export interface ClinicalNote {
  readonly id: string;
  readonly appointmentId: string;
  readonly content: string;
  readonly createdBy: string;
  readonly createdAt: string;
  readonly updatedAt: string | null;
}

export interface CreateClinicalNoteRequest {
  readonly content: string;
}

export interface UpdateClinicalNoteRequest {
  readonly content: string;
}
