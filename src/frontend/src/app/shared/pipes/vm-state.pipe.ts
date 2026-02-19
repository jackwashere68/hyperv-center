import { Pipe, PipeTransform } from '@angular/core';
import { VmState } from '@core/models/virtual-machine.model';

@Pipe({ name: 'vmState' })
export class VmStatePipe implements PipeTransform {
  private readonly labels: Record<VmState, string> = {
    [VmState.PoweredOff]: 'Powered Off',
    [VmState.Running]: 'Running',
    [VmState.Paused]: 'Paused',
    [VmState.Saved]: 'Saved',
  };

  transform(state: VmState): string {
    return this.labels[state] ?? 'Unknown';
  }
}
