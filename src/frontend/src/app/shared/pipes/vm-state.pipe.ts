import { Pipe, PipeTransform } from '@angular/core';
import { VmState } from '@core/models/virtual-machine.model';

@Pipe({ name: 'vmState' })
export class VmStatePipe implements PipeTransform {
  private readonly labels: Record<VmState, string> = {
    [VmState.Off]: 'Off',
    [VmState.Running]: 'Running',
    [VmState.Paused]: 'Paused',
    [VmState.Saved]: 'Saved',
    [VmState.Starting]: 'Starting',
    [VmState.Stopping]: 'Stopping',
    [VmState.Saving]: 'Saving',
    [VmState.Pausing]: 'Pausing',
    [VmState.Resuming]: 'Resuming',
    [VmState.Other]: 'Other',
  };

  transform(state: VmState): string {
    return this.labels[state] ?? 'Unknown';
  }
}
