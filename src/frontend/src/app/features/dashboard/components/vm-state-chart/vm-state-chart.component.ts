import { Component, input, computed } from '@angular/core';
import { VirtualMachine, VmState } from '@core/models/virtual-machine.model';
import { VmStatePipe } from '@shared/pipes/vm-state.pipe';

@Component({
  selector: 'app-vm-state-chart',
  templateUrl: './vm-state-chart.component.html',
  imports: [VmStatePipe],
})
export class VmStateChartComponent {
  readonly vms = input.required<VirtualMachine[]>();

  readonly stateCounts = computed(() => {
    const vms = this.vms();
    return [
      { state: VmState.Running, count: vms.filter((v) => v.state === VmState.Running).length, color: 'bg-success', dotColor: 'bg-success', textColor: 'text-success' },
      { state: VmState.Off, count: vms.filter((v) => v.state === VmState.Off).length, color: 'bg-text-muted', dotColor: 'bg-text-muted', textColor: 'text-text-muted' },
      { state: VmState.Paused, count: vms.filter((v) => v.state === VmState.Paused).length, color: 'bg-warning', dotColor: 'bg-warning', textColor: 'text-warning' },
      { state: VmState.Saved, count: vms.filter((v) => v.state === VmState.Saved).length, color: 'bg-info', dotColor: 'bg-info', textColor: 'text-info' },
    ].filter((s) => s.count > 0);
  });

  readonly total = computed(() => this.vms().length);

  barWidth(count: number): string {
    const t = this.total();
    return t > 0 ? `${(count / t) * 100}%` : '0%';
  }
}
