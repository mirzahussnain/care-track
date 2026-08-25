export interface Patient {
  readonly id: string;
  readonly patientReference: string;
  readonly firstName: string;
  readonly lastName: string;
  readonly fullName: string;
  readonly dateOfBirth: string;
  readonly createdAt: string;
  readonly rowVersion: string;
}

export type PatientSortField = 'lastName' | 'firstName' | 'patientReference' | 'createdAt';

export type SortDirection = 'asc' | 'desc';

export interface PatientSearchQuery {
  readonly search?: string;
  readonly page: number;
  readonly pageSize: number;
  readonly sortBy: PatientSortField;
  readonly sortDirection: SortDirection;
}

export interface CreatePatientRequest {
  readonly patientReference: string;
  readonly firstName: string;
  readonly lastName: string;
  readonly dateOfBirth: string;
}

export interface UpdatePatientRequest {
  readonly firstName: string;
  readonly lastName: string;
  readonly dateOfBirth: string;
  readonly rowVersion: string;
}
