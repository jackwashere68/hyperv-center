import { Component, inject, computed } from '@angular/core';
import { VirtualMachinesStore } from '@features/virtual-machines/store/virtual-machines.store';
import { HostsStore } from '@features/hosts/store/hosts.store';
import { ClustersStore } from '@features/clusters/store/clusters.store';
import { CredentialsStore } from '@features/credentials/store/credentials.store';
import { VmState } from '@core/models/virtual-machine.model';
import { HostStatus } from '@core/models/hyperv-host.model';
import { PageHeaderComponent } from '@shared/components/page-header/page-header.component';
import { StatCardComponent } from '../stat-card/stat-card.component';
import { VmStateChartComponent } from '../vm-state-chart/vm-state-chart.component';
import { HostHealthCardComponent } from '../host-health-card/host-health-card.component';
import { RecentActivityComponent } from '../recent-activity/recent-activity.component';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  imports: [
    PageHeaderComponent,
    StatCardComponent,
    VmStateChartComponent,
    HostHealthCardComponent,
    RecentActivityComponent,
  ],
})
export class DashboardComponent {
  readonly vmStore = inject(VirtualMachinesStore);
  readonly hostsStore = inject(HostsStore);
  readonly clustersStore = inject(ClustersStore);
  readonly credentialsStore = inject(CredentialsStore);

  readonly runningVms = computed(
    () => this.vmStore.vmEntities().filter((vm) => vm.state === VmState.Running).length,
  );

  readonly stoppedVms = computed(
    () => this.vmStore.vmEntities().filter((vm) => vm.state === VmState.Off).length,
  );

  readonly onlineHosts = computed(
    () => this.hostsStore.hostEntities().filter((h) => h.status === HostStatus.Online).length,
  );

  readonly totalMemory = computed(() => {
    return this.hostsStore
      .hostEntities()
      .reduce((sum, h) => sum + (h.totalMemoryBytes ?? 0), 0);
  });

  readonly vmSubtitle = computed(() => {
    const running = this.runningVms();
    const stopped = this.stoppedVms();
    return `${running} running, ${stopped} stopped`;
  });

  readonly hostSubtitle = computed(() => {
    const online = this.onlineHosts();
    const total = this.hostsStore.hostCount();
    return `${online} of ${total} online`;
  });
}
