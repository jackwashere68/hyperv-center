import { computed, inject } from '@angular/core';
import {
  patchState,
  signalStore,
  type,
  withComputed,
  withHooks,
  withMethods,
  withState,
} from '@ngrx/signals';
import {
  addEntity,
  removeEntity,
  setAllEntities,
  withEntities,
} from '@ngrx/signals/entities';
import {
  VirtualMachine,
  CreateVirtualMachineRequest,
  VmAction,
} from '@core/models/virtual-machine.model';
import { VirtualMachinesService } from '../services/virtual-machines.service';
import { firstValueFrom } from 'rxjs';

export const VirtualMachinesStore = signalStore(
  { providedIn: 'root' },
  withState({ loading: false, error: null as string | null }),
  withEntities({ entity: type<VirtualMachine>(), collection: 'vm' }),
  withComputed(({ vmEntities, loading }) => ({
    vmCount: computed(() => vmEntities().length),
    isLoading: computed(() => loading()),
  })),
  withMethods(
    (store, vmService = inject(VirtualMachinesService)) => ({
      async loadAll() {
        patchState(store, { loading: true, error: null });
        try {
          const vms = await firstValueFrom(vmService.getAll());
          patchState(store, setAllEntities(vms, { collection: 'vm' }));
        } catch {
          patchState(store, {
            error: 'Failed to load virtual machines.',
          });
        } finally {
          patchState(store, { loading: false });
        }
      },
      async create(request: CreateVirtualMachineRequest) {
        try {
          const vm = await firstValueFrom(vmService.create(request));
          patchState(store, addEntity(vm, { collection: 'vm' }));
          return vm;
        } catch {
          patchState(store, {
            error: 'Failed to create virtual machine.',
          });
          throw new Error('Failed to create virtual machine.');
        }
      },
      async performAction(id: string, action: VmAction, force = false) {
        try {
          let vm: VirtualMachine;
          switch (action) {
            case 'start':
              vm = await firstValueFrom(vmService.start(id));
              break;
            case 'stop':
              vm = await firstValueFrom(vmService.stop(id, force));
              break;
            case 'pause':
              vm = await firstValueFrom(vmService.pause(id));
              break;
            case 'save':
              vm = await firstValueFrom(vmService.save(id));
              break;
            case 'restart':
              vm = await firstValueFrom(vmService.restart(id));
              break;
          }
          // Reload all to get fresh state
          const vms = await firstValueFrom(vmService.getAll());
          patchState(store, setAllEntities(vms, { collection: 'vm' }));
          return vm!;
        } catch {
          patchState(store, {
            error: `Failed to ${action} virtual machine.`,
          });
          throw new Error(`Failed to ${action} virtual machine.`);
        }
      },
      async remove(id: string) {
        try {
          await firstValueFrom(vmService.delete(id));
          patchState(store, removeEntity(id, { collection: 'vm' }));
        } catch {
          patchState(store, {
            error: 'Failed to delete virtual machine.',
          });
          throw new Error('Failed to delete virtual machine.');
        }
      },
    }),
  ),
  withHooks({
    onInit(store) {
      store.loadAll();
    },
  }),
);
