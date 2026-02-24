import { Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { VirtualMachine, VmState, VmAction } from '@core/models/virtual-machine.model';
import { BytesPipe } from '@shared/pipes/bytes.pipe';
import { VmStatePipe } from '@shared/pipes/vm-state.pipe';
import { StatusBadgeComponent, StatusVariant } from '@shared/components/status-badge/status-badge.component';

@Component({
  selector: 'app-vm-card',
  imports: [
    RouterLink,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatProgressSpinnerModule,
    BytesPipe,
    VmStatePipe,
    StatusBadgeComponent,
  ],
  template: `
    <div class="group rounded-xl border border-border-subtle bg-bg-surface p-4 hover:border-border hover:shadow-[0_0_20px_rgba(34,211,238,0.05)] transition-all duration-200">
      <!-- Header -->
      <div class="flex items-start justify-between mb-3">
        <div class="min-w-0 flex-1">
          <h3 class="text-sm font-semibold text-text-primary truncate">{{ vm().name }}</h3>
          <p class="text-xs text-text-muted mt-0.5 truncate">{{ vm().hostName }}</p>
        </div>
        <app-status-badge
          [label]="vm().state | vmState"
          [variant]="stateVariant()"
          [pulse]="vm().state === VmState.Running"
        />
      </div>

      <!-- Resources -->
      <div class="flex items-center gap-4 mb-4 text-xs text-text-secondary">
        <div class="flex items-center gap-1">
          <mat-icon class="!text-[14px] !w-3.5 !h-3.5 text-text-muted">memory</mat-icon>
          <span>{{ vm().cpuCount }} vCPU</span>
        </div>
        <div class="flex items-center gap-1">
          <mat-icon class="!text-[14px] !w-3.5 !h-3.5 text-text-muted">sd_card</mat-icon>
          <span>{{ vm().memoryBytes | bytes }}</span>
        </div>
      </div>

      <!-- Actions -->
      <div class="flex items-center gap-1 -mx-1">
        @if (actioning()) {
          <mat-spinner diameter="20" class="mx-auto"></mat-spinner>
        } @else {
          <button
            mat-icon-button
            matTooltip="Start"
            [disabled]="isTransitional() || vm().state === VmState.Running"
            (click)="action.emit({ id: vm().id, action: 'start' })"
            class="!w-8 !h-8"
          >
            <mat-icon class="!text-[18px]">play_arrow</mat-icon>
          </button>
          <button
            mat-icon-button
            matTooltip="Stop"
            [disabled]="isTransitional() || vm().state === VmState.Off"
            (click)="action.emit({ id: vm().id, action: 'stop' })"
            class="!w-8 !h-8"
          >
            <mat-icon class="!text-[18px]">stop</mat-icon>
          </button>
          <button
            mat-icon-button
            matTooltip="Restart"
            [disabled]="vm().state !== VmState.Running"
            (click)="action.emit({ id: vm().id, action: 'restart' })"
            class="!w-8 !h-8"
          >
            <mat-icon class="!text-[18px]">restart_alt</mat-icon>
          </button>
          @if (canConsole() && vm().externalId) {
            <a
              mat-icon-button
              matTooltip="Console"
              [routerLink]="['/virtual-machines', vm().id, 'console']"
              [queryParams]="{ name: vm().name }"
              class="!w-8 !h-8 !text-accent"
            >
              <mat-icon class="!text-[18px]">desktop_windows</mat-icon>
            </a>
          }
          <div class="flex-1"></div>
          <button
            mat-icon-button
            matTooltip="Properties"
            (click)="properties.emit(vm())"
            class="!w-8 !h-8"
          >
            <mat-icon class="!text-[18px]">info_outline</mat-icon>
          </button>
          <button
            mat-icon-button
            matTooltip="Delete"
            (click)="delete.emit({ id: vm().id, name: vm().name })"
            class="!w-8 !h-8 text-error"
          >
            <mat-icon class="!text-[18px]">delete_outline</mat-icon>
          </button>
        }
      </div>
    </div>
  `,
})
export class VmCardComponent {
  readonly vm = input.required<VirtualMachine>();
  readonly actioning = input(false);

  readonly action = output<{ id: string; action: VmAction }>();
  readonly properties = output<VirtualMachine>();
  readonly delete = output<{ id: string; name: string }>();

  readonly VmState = VmState;

  stateVariant(): StatusVariant {
    const map: Partial<Record<VmState, StatusVariant>> = {
      [VmState.Running]: 'success',
      [VmState.Off]: 'muted',
      [VmState.Paused]: 'warning',
      [VmState.Saved]: 'info',
      [VmState.Starting]: 'info',
      [VmState.Stopping]: 'warning',
      [VmState.Saving]: 'info',
      [VmState.Pausing]: 'warning',
      [VmState.Resuming]: 'info',
    };
    return map[this.vm().state] ?? 'muted';
  }

  isTransitional(): boolean {
    return [VmState.Starting, VmState.Stopping, VmState.Saving, VmState.Pausing, VmState.Resuming].includes(
      this.vm().state,
    );
  }

  canConsole(): boolean {
    return [VmState.Running, VmState.Paused, VmState.Saved].includes(this.vm().state);
  }
}
