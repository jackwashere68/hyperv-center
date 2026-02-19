import { Pipe, PipeTransform } from '@angular/core';
import { HostStatus } from '@core/models/hyperv-host.model';

const labels: Record<HostStatus, string> = {
  [HostStatus.Unknown]: 'Unknown',
  [HostStatus.Online]: 'Online',
  [HostStatus.Offline]: 'Offline',
  [HostStatus.Error]: 'Error',
};

@Pipe({ name: 'hostStatus', standalone: true })
export class HostStatusPipe implements PipeTransform {
  transform(value: HostStatus): string {
    return labels[value] ?? value;
  }
}
