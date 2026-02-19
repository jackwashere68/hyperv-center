import { Pipe, PipeTransform } from '@angular/core';
import { ClusterStatus } from '@core/models/cluster.model';

const labels: Record<ClusterStatus, string> = {
  [ClusterStatus.Unknown]: 'Unknown',
  [ClusterStatus.Online]: 'Online',
  [ClusterStatus.Degraded]: 'Degraded',
  [ClusterStatus.Offline]: 'Offline',
  [ClusterStatus.Error]: 'Error',
};

@Pipe({ name: 'clusterStatus', standalone: true })
export class ClusterStatusPipe implements PipeTransform {
  transform(value: ClusterStatus): string {
    return labels[value] ?? value;
  }
}
