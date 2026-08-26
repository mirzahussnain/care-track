import { formatDate } from '@angular/common';

const OFFSET_SUFFIX = /(z|[+-]\d{2}:\d{2})$/i;
const UTC_INPUT_PATTERN =
  /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})(?::(\d{2})(?:\.(\d{1,3}))?)?$/;

/**
 * Phase 6G convention: offset-less backend appointment values are interpreted as UTC.
 */
export function normalizeAppointmentDateTime(value: string): string {
  return OFFSET_SUFFIX.test(value) ? value : `${value}Z`;
}

export function appointmentInputToUtcIso(value: string): string {
  const match = UTC_INPUT_PATTERN.exec(value);
  if (!match) {
    throw new RangeError('Appointment date/time must use a valid UTC date and time.');
  }

  const [, year, month, day, hour, minute, second = '0', milliseconds = '0'] = match;
  const timestamp = Date.UTC(
    Number(year),
    Number(month) - 1,
    Number(day),
    Number(hour),
    Number(minute),
    Number(second),
    Number(milliseconds.padEnd(3, '0')),
  );
  const date = new Date(timestamp);

  if (
    date.getUTCFullYear() !== Number(year) ||
    date.getUTCMonth() !== Number(month) - 1 ||
    date.getUTCDate() !== Number(day) ||
    date.getUTCHours() !== Number(hour) ||
    date.getUTCMinutes() !== Number(minute)
  ) {
    throw new RangeError('Appointment date/time must use a valid UTC date and time.');
  }

  return date.toISOString();
}

export function appointmentInputTimestamp(value: string): number | null {
  try {
    return Date.parse(appointmentInputToUtcIso(value));
  } catch {
    return null;
  }
}

export function formatAppointmentUtc(
  value: string | null | undefined,
  format = 'dd MMM yyyy, HH:mm',
): string {
  if (!value) {
    return 'Not recorded';
  }

  try {
    return `${formatDate(normalizeAppointmentDateTime(value), format, 'en-GB', 'UTC')} UTC`;
  } catch {
    return 'Unavailable';
  }
}
