import { Component, input } from '@angular/core';
import { HyperVHost, HostStatus } from '@core/models/hyperv-host.model';
import { HostStatusPipe } from '@shared/pipes/host-status.pipe';

@Component({
  selector: 'app-host-health-card',
  imports: [HostStatusPipe],
  template: `
    <div class="rounded-xl border border-border-subtle bg-bg-surface p-5">
      <h3 class="text-sm font-medium text-text-primary mb-4">Host Health</h3>
      @if (hosts().length === 0) {
        <p class="text-sm text-text-muted">No hosts configured</p>
      } @else {
        <div class="space-y-3">
          @for (host of hosts(); track host.id) {
            <div class="flex items-center justify-between">
              <div class="flex items-center gap-2 min-w-0">
                <span
                  class="inline-block h-2 w-2 rounded-full shrink-0"
                  [class]="dotClass(host.status)"
                  [class.animate-pulse-dot]="host.status === HostStatus.Online"
                ></span>
                <span class="text-sm text-text-primary truncate">{{ host.name }}</span>
              </div>
              <span class="text-xs text-text-muted shrink-0 ml-2">{{ host.status | hostStatus }}</span>
            </div>
          }
        </div>
      }
    </div>
  `,
})
export class HostHealthCardComponent {
  readonly hosts = input.required<HyperVHost[]>();
  readonly HostStatus = HostStatus;

  dotClass(status: HostStatus): string {
    const map: Record<HostStatus, string> = {
      [HostStatus.Online]: 'bg-success',
      [HostStatus.Offline]: 'bg-error',
      [HostStatus.Error]: 'bg-warning',
      [HostStatus.Unknown]: 'bg-text-muted',
    };
    return map[status] ?? 'bg-text-muted';
  }
}
