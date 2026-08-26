import { SemanticTone } from '../../../design-system/tokens/semantic-tone';

export const APPOINTMENT_STATUSES = {
  scheduled: 0,
  checkedIn: 1,
  inProgress: 2,
  completed: 3,
  cancelled: 4,
  didNotAttend: 5,
} as const;

export type AppointmentStatus =
  (typeof APPOINTMENT_STATUSES)[keyof typeof APPOINTMENT_STATUSES];

export const APPOINTMENT_TYPES = {
  consultation: 0,
  followUp: 1,
  diagnostic: 2,
  procedure: 3,
} as const;

export type AppointmentType =
  (typeof APPOINTMENT_TYPES)[keyof typeof APPOINTMENT_TYPES];

export type AppointmentSortField =
  | 'scheduledStart'
  | 'scheduledEnd'
  | 'createdAt'
  | 'appointmentReference'
  | 'status';

export type SortDirection = 'asc' | 'desc';

export interface AppointmentSearchItem {
  readonly id: string;
  readonly appointmentReference: string;
  readonly patientId: string;
  readonly referralId: string;
  readonly appointmentType: AppointmentType;
  readonly scheduledStart: string;
  readonly scheduledEnd: string;
  readonly location: string;
  readonly status: AppointmentStatus;
  readonly createdAt: string;
}

export interface Appointment extends AppointmentSearchItem {
  readonly updatedAt: string | null;
  readonly checkedInAt: string | null;
  readonly startedAt: string | null;
  readonly completedAt: string | null;
  readonly cancelledAt: string | null;
  readonly didNotAttendAt: string | null;
}

export interface AppointmentSearchQuery {
  readonly patientId?: string;
  readonly referralId?: string;
  readonly status?: AppointmentStatus;
  readonly appointmentType?: AppointmentType;
  readonly location?: string;
  readonly scheduledFrom?: string;
  readonly scheduledTo?: string;
  readonly page: number;
  readonly pageSize: number;
  readonly sortBy: AppointmentSortField;
  readonly sortDirection: SortDirection;
}

export interface CreateAppointmentRequest {
  readonly appointmentReference: string;
  readonly patientId: string;
  readonly referralId: string;
  readonly appointmentType: AppointmentType;
  readonly scheduledStart: string;
  readonly scheduledEnd: string;
  readonly location: string;
}

const STATUS_PRESENTATION: Readonly<
  Record<AppointmentStatus, { label: string; tone: SemanticTone }>
> = {
  [APPOINTMENT_STATUSES.scheduled]: { label: 'Scheduled', tone: 'info' },
  [APPOINTMENT_STATUSES.checkedIn]: { label: 'Checked in', tone: 'warning' },
  [APPOINTMENT_STATUSES.inProgress]: { label: 'In progress', tone: 'warning' },
  [APPOINTMENT_STATUSES.completed]: { label: 'Completed', tone: 'success' },
  [APPOINTMENT_STATUSES.cancelled]: { label: 'Cancelled', tone: 'danger' },
  [APPOINTMENT_STATUSES.didNotAttend]: { label: 'Did not attend', tone: 'neutral' },
};

const TYPE_LABELS: Readonly<Record<AppointmentType, string>> = {
  [APPOINTMENT_TYPES.consultation]: 'Consultation',
  [APPOINTMENT_TYPES.followUp]: 'Follow-up',
  [APPOINTMENT_TYPES.diagnostic]: 'Diagnostic',
  [APPOINTMENT_TYPES.procedure]: 'Procedure',
};

export function appointmentStatusLabel(status: AppointmentStatus): string {
  return STATUS_PRESENTATION[status].label;
}

export function appointmentStatusTone(status: AppointmentStatus): SemanticTone {
  return STATUS_PRESENTATION[status].tone;
}

export function appointmentTypeLabel(type: AppointmentType): string {
  return TYPE_LABELS[type];
}
