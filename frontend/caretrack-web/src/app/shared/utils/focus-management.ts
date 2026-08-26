export function focusFirstInvalidControl(controlIds: readonly string[]): void {
  queueMicrotask(() => {
    const control = controlIds
      .map((id) => document.getElementById(id))
      .find(
        (candidate): candidate is HTMLElement =>
          candidate instanceof HTMLElement && candidate.getAttribute('aria-invalid') === 'true',
      );

    control?.focus();
  });
}

export function buttonFromEvent(event: MouseEvent): HTMLButtonElement | null {
  return event.target instanceof Element ? event.target.closest('button') : null;
}

export function restoreFocusIfAvailable(control: HTMLElement | null): void {
  queueMicrotask(() => {
    if (!control?.isConnected || (control instanceof HTMLButtonElement && control.disabled)) {
      return;
    }

    control.focus();
  });
}
