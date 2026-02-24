import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { BreakpointObserver } from '@angular/cdk/layout';
import { Subscription } from 'rxjs';
import { VirtualMachinesStore } from './features/virtual-machines/store/virtual-machines.store';
import { HostsStore } from './features/hosts/store/hosts.store';
import { ClustersStore } from './features/clusters/store/clusters.store';
import { CredentialsStore } from './features/credentials/store/credentials.store';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
  ],
})
export class AppComponent implements OnInit, OnDestroy {
  readonly vmStore = inject(VirtualMachinesStore);
  readonly hostsStore = inject(HostsStore);
  readonly clustersStore = inject(ClustersStore);
  readonly credentialsStore = inject(CredentialsStore);
  private readonly breakpointObserver = inject(BreakpointObserver);

  readonly sidebarCollapsed = signal(false);
  private breakpointSub?: Subscription;

  ngOnInit(): void {
    this.breakpointSub = this.breakpointObserver
      .observe(['(max-width: 1279px)'])
      .subscribe((result) => {
        this.sidebarCollapsed.set(result.matches);
      });
  }

  ngOnDestroy(): void {
    this.breakpointSub?.unsubscribe();
  }

  toggleSidebar(): void {
    this.sidebarCollapsed.update((v) => !v);
  }
}
