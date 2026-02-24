import { Component, input, computed } from '@angular/core';
import { DatePipe } from '@angular/common';
import { VirtualMachine } from '@core/models/virtual-machine.model';
import { VmStatePipe } from '@shared/pipes/vm-state.pipe';
import { StatusBadgeComponent, StatusVariant } from '@shared/components/status-badge/status-badge.component';
import { VmState } from '@core/models/virtual-machine.model';

@Component({
  selector: 'app-recent-activity',
  templateUrl: './recent-activity.component.html',
  imports: [DatePipe, VmStatePipe, StatusBadgeComponent],
})
export class RecentActivityComponent {
  readonly vms = input.required<VirtualMachine[]>();

  readonly recentVms = computed(() => {
    return [...this.vms()]
      .sort((a, b) => {
        const dateA = a.updatedAt ?? a.createdAt;
        const dateB = b.updatedAt ?? b.createdAt;
        return new Date(dateB).getTime() - new Date(dateA).getTime();
      })
      .slice(0, 8);
  });

  stateVariant(state: VmState): StatusVariant {
    const map: Partial<Record<VmState, StatusVariant>> = {
      [VmState.Running]: 'success',
      [VmState.Off]: 'muted',
      [VmState.Paused]: 'warning',
      [VmState.Saved]: 'info',
      [VmState.Starting]: 'info',
      [VmState.Stopping]: 'warning',
    };
    return map[state] ?? 'muted';
  }

  isRunning(state: VmState): boolean {
    return state === VmState.Running;
  }
}
