import {
  appointmentInputTimestamp,
  appointmentInputToUtcIso,
  formatAppointmentUtc,
  normalizeAppointmentDateTime,
} from './appointment-datetime';

describe('appointment UTC convention', () => {
  it('converts datetime-local input to an explicit UTC ISO value', () => {
    expect(appointmentInputToUtcIso('2026-09-01T09:30')).toBe('2026-09-01T09:30:00.000Z');
  });

  it('interprets offset-less backend appointment values as UTC', () => {
    expect(normalizeAppointmentDateTime('2026-09-01T09:30:00')).toBe('2026-09-01T09:30:00Z');
    expect(normalizeAppointmentDateTime('2026-09-01T09:30:00+01:00')).toBe(
      '2026-09-01T09:30:00+01:00',
    );
  });

  it('formats every appointment timestamp with an explicit UTC label', () => {
    expect(formatAppointmentUtc('2026-09-01T09:30:00')).toBe('01 Sep 2026, 09:30 UTC');
    expect(formatAppointmentUtc(null)).toBe('Not recorded');
  });

  it('rejects impossible values without browser-local reinterpretation', () => {
    expect(appointmentInputTimestamp('2026-02-30T09:30')).toBeNull();
  });
});
