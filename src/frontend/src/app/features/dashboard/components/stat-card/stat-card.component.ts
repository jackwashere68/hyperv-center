import { Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-stat-card',
  imports: [MatIconModule],
  template: `
    <div class="rounded-xl border border-border-subtle bg-bg-surface p-5 hover:border-border transition-colors">
      <div class="flex items-center justify-between mb-3">
        <div
          class="flex items-center justify-center h-9 w-9 rounded-lg"
          [class]="iconBgClass()"
        >
          <mat-icon class="!text-[18px] !w-[18px] !h-[18px]" [class]="iconColorClass()">{{ icon() }}</mat-icon>
        </div>
        @if (subtitle()) {
          <span class="text-xs text-text-muted">{{ subtitle() }}</span>
        }
      </div>
      <div class="text-2xl font-semibold text-text-primary">{{ value() }}</div>
      <div class="text-sm text-text-muted mt-1">{{ title() }}</div>
    </div>
  `,
})
export class StatCardComponent {
  readonly icon = input.required<string>();
  readonly value = input.required<string | number>();
  readonly title = input.required<string>();
  readonly subtitle = input<string>();
  readonly color = input<'accent' | 'success' | 'warning' | 'error' | 'info'>('accent');

  readonly iconBgClass = () => {
    const map = {
      accent: 'bg-accent/10',
      success: 'bg-success/10',
      warning: 'bg-warning/10',
      error: 'bg-error/10',
      info: 'bg-info/10',
    };
    return map[this.color()];
  };

  readonly iconColorClass = () => {
    const map = {
      accent: 'text-accent',
      success: 'text-success',
      warning: 'text-warning',
      error: 'text-error',
      info: 'text-info',
    };
    return map[this.color()];
  };
}
