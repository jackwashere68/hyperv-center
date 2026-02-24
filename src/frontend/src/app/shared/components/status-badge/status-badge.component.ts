import { Component, input, computed } from '@angular/core';

export type StatusVariant = 'success' | 'error' | 'warning' | 'info' | 'muted';

@Component({
  selector: 'app-status-badge',
  template: `
    <span
      class="inline-flex items-center gap-1.5 rounded-full px-2.5 py-0.5 text-xs font-medium"
      [class]="containerClasses()"
    >
      <span
        class="inline-block h-1.5 w-1.5 rounded-full"
        [class]="dotClasses()"
      ></span>
      {{ label() }}
    </span>
  `,
})
export class StatusBadgeComponent {
  readonly variant = input<StatusVariant>('muted');
  readonly label = input.required<string>();
  readonly pulse = input(false);

  readonly containerClasses = computed(() => {
    const map: Record<StatusVariant, string> = {
      success: 'bg-success/10 text-success',
      error: 'bg-error/10 text-error',
      warning: 'bg-warning/10 text-warning',
      info: 'bg-info/10 text-info',
      muted: 'bg-bg-elevated text-text-muted',
    };
    return map[this.variant()];
  });

  readonly dotClasses = computed(() => {
    const map: Record<StatusVariant, string> = {
      success: 'bg-success',
      error: 'bg-error',
      warning: 'bg-warning',
      info: 'bg-info',
      muted: 'bg-text-muted',
    };
    return `${map[this.variant()]}${this.pulse() ? ' animate-pulse-dot' : ''}`;
  });
}
