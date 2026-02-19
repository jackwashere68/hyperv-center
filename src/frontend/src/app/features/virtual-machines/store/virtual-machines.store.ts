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
  setAllEntities,
  withEntities,
} from '@ngrx/signals/entities';
import {
  VirtualMachine,
  CreateVirtualMachineRequest,
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
    }),
  ),
  withHooks({
    onInit(store) {
      store.loadAll();
    },
  }),
);
