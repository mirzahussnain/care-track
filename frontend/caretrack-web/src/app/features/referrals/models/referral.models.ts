import { SemanticTone } from '../../../design-system/tokens/semantic-tone';

export const REFERRAL_STATUSES = {
  draft: 0,
  submitted: 1,
  awaitingTriage: 2,
  moreInformationRequired: 3,
  accepted: 4,
  assigned: 5,
  scheduled: 6,
  inProgress: 7,
  completed: 8,
  rejected: 9,
  cancelled: 10,
} as const;

export type ReferralStatus =
  (typeof REFERRAL_STATUSES)[keyof typeof REFERRAL_STATUSES];

export const REFERRAL_PRIORITIES = {
  routine: 0,
  urgent: 1,
} as const;

export type ReferralPriority =
  (typeof REFERRAL_PRIORITIES)[keyof typeof REFERRAL_PRIORITIES];

export const REFERRAL_HISTORY_EVENT_TYPES = {
  created: 0,
  submitted: 1,
  triageStarted: 2,
  moreInformationRequested: 3,
  resubmitted: 4,
  accepted: 5,
  rejected: 6,
  triageAssessmentRecorded: 7,
  assigned: 8,
  reassigned: 9,
  scheduled: 10,
  started: 11,
  completed: 12,
} as const;

export type ReferralHistoryEventType =
  (typeof REFERRAL_HISTORY_EVENT_TYPES)[keyof typeof REFERRAL_HISTORY_EVENT_TYPES];

export type ReferralSortField =
  | 'createdAt'
  | 'updatedAt'
  | 'priority'
  | 'status'
  | 'referralReference';

export type SortDirection = 'asc' | 'desc';

export interface Referral {
  readonly id: string;
  readonly referralReference: string;
  readonly patientId: string;
  readonly status: ReferralStatus;
  readonly priority: ReferralPriority;
  readonly reason: string;
  readonly triageNote: string | null;
  readonly createdAt: string;
  readonly submittedAt: string | null;
  readonly updatedAt: string | null;
  readonly triagedAt: string | null;
  readonly assignedTo: string | null;
  readonly assignedAt: string | null;
}

export interface ReferralHistoryEntry {
  readonly id: string;
  readonly eventType: ReferralHistoryEventType;
  readonly fromStatus: ReferralStatus | null;
  readonly toStatus: ReferralStatus | null;
  readonly priority: ReferralPriority | null;
  readonly triageNote: string | null;
  readonly assignedTo: string | null;
  readonly occurredAt: string;
}

export interface ReferralSearchQuery {
  readonly status?: ReferralStatus;
  readonly priority?: ReferralPriority;
  readonly assignedTo?: string;
  readonly createdFrom?: string;
  readonly createdTo?: string;
  readonly page: number;
  readonly pageSize: number;
  readonly sortBy: ReferralSortField;
  readonly sortDirection: SortDirection;
}

export interface CreateReferralRequest {
  readonly referralReference: string;
  readonly patientId: string;
  readonly priority: ReferralPriority;
  readonly reason: string;
}

export interface RecordTriageAssessmentRequest {
  readonly priority: ReferralPriority;
  readonly note: string;
}

export interface AssignReferralRequest {
  readonly assignedTo: string;
}

export interface ReferralAssignmentTargetsResponse {
  readonly items: readonly string[];
}

export interface ProblemDetails {
  readonly status?: number;
  readonly title?: string;
  readonly detail?: string;
}

const STATUS_PRESENTATION: Readonly<
  Record<ReferralStatus, { label: string; tone: SemanticTone }>
> = {
  [REFERRAL_STATUSES.draft]: { label: 'Draft', tone: 'neutral' },
  [REFERRAL_STATUSES.submitted]: { label: 'Submitted', tone: 'info' },
  [REFERRAL_STATUSES.awaitingTriage]: { label: 'Awaiting triage', tone: 'warning' },
  [REFERRAL_STATUSES.moreInformationRequired]: {
    label: 'More information required',
    tone: 'warning',
  },
  [REFERRAL_STATUSES.accepted]: { label: 'Accepted', tone: 'success' },
  [REFERRAL_STATUSES.assigned]: { label: 'Assigned', tone: 'info' },
  [REFERRAL_STATUSES.scheduled]: { label: 'Scheduled', tone: 'info' },
  [REFERRAL_STATUSES.inProgress]: { label: 'In progress', tone: 'info' },
  [REFERRAL_STATUSES.completed]: { label: 'Completed', tone: 'success' },
  [REFERRAL_STATUSES.rejected]: { label: 'Rejected', tone: 'danger' },
  [REFERRAL_STATUSES.cancelled]: { label: 'Cancelled', tone: 'danger' },
};

const HISTORY_LABELS: Readonly<Record<ReferralHistoryEventType, string>> = {
  [REFERRAL_HISTORY_EVENT_TYPES.created]: 'Referral created',
  [REFERRAL_HISTORY_EVENT_TYPES.submitted]: 'Referral submitted',
  [REFERRAL_HISTORY_EVENT_TYPES.triageStarted]: 'Triage started',
  [REFERRAL_HISTORY_EVENT_TYPES.moreInformationRequested]: 'More information requested',
  [REFERRAL_HISTORY_EVENT_TYPES.resubmitted]: 'Referral resubmitted',
  [REFERRAL_HISTORY_EVENT_TYPES.accepted]: 'Referral accepted',
  [REFERRAL_HISTORY_EVENT_TYPES.rejected]: 'Referral rejected',
  [REFERRAL_HISTORY_EVENT_TYPES.triageAssessmentRecorded]: 'Triage assessment recorded',
  [REFERRAL_HISTORY_EVENT_TYPES.assigned]: 'Referral assigned',
  [REFERRAL_HISTORY_EVENT_TYPES.reassigned]: 'Referral reassigned',
  [REFERRAL_HISTORY_EVENT_TYPES.scheduled]: 'Referral scheduled',
  [REFERRAL_HISTORY_EVENT_TYPES.started]: 'Referral started',
  [REFERRAL_HISTORY_EVENT_TYPES.completed]: 'Referral completed',
};

export function referralStatusLabel(status: ReferralStatus): string {
  return STATUS_PRESENTATION[status].label;
}

export function referralStatusTone(status: ReferralStatus): SemanticTone {
  return STATUS_PRESENTATION[status].tone;
}

export function referralPriorityLabel(priority: ReferralPriority): string {
  return priority === REFERRAL_PRIORITIES.urgent ? 'Urgent' : 'Routine';
}

export function referralPriorityTone(priority: ReferralPriority): SemanticTone {
  return priority === REFERRAL_PRIORITIES.urgent ? 'warning' : 'neutral';
}

export function referralHistoryLabel(eventType: ReferralHistoryEventType): string {
  return HISTORY_LABELS[eventType];
}
