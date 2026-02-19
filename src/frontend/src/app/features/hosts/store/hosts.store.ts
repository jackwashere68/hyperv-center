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
  updateEntity,
  withEntities,
} from '@ngrx/signals/entities';
import {
  HyperVHost,
  CreateHyperVHostRequest,
  UpdateHyperVHostRequest,
} from '@core/models/hyperv-host.model';
import { HostsService } from '../services/hosts.service';
import { firstValueFrom } from 'rxjs';

export const HostsStore = signalStore(
  { providedIn: 'root' },
  withState({ loading: false, error: null as string | null }),
  withEntities({ entity: type<HyperVHost>(), collection: 'host' }),
  withComputed(({ hostEntities, loading }) => ({
    hostCount: computed(() => hostEntities().length),
    isLoading: computed(() => loading()),
  })),
  withMethods(
    (store, hostsService = inject(HostsService)) => ({
      async loadAll() {
        patchState(store, { loading: true, error: null });
        try {
          const hosts = await firstValueFrom(hostsService.getAll());
          patchState(
            store,
            setAllEntities(hosts, { collection: 'host' }),
          );
        } catch {
          patchState(store, {
            error: 'Failed to load hosts.',
          });
        } finally {
          patchState(store, { loading: false });
        }
      },
      async create(request: CreateHyperVHostRequest) {
        try {
          const host = await firstValueFrom(hostsService.create(request));
          patchState(
            store,
            addEntity(host, { collection: 'host' }),
          );
          return host;
        } catch {
          patchState(store, {
            error: 'Failed to create host.',
          });
          throw new Error('Failed to create host.');
        }
      },
      async update(id: string, request: UpdateHyperVHostRequest) {
        try {
          const host = await firstValueFrom(
            hostsService.update(id, request),
          );
          patchState(
            store,
            updateEntity(
              { id, changes: host },
              { collection: 'host' },
            ),
          );
          return host;
        } catch {
          patchState(store, {
            error: 'Failed to update host.',
          });
          throw new Error('Failed to update host.');
        }
      },
      async remove(id: string) {
        try {
          await firstValueFrom(hostsService.delete(id));
          patchState(
            store,
            removeEntity(id, { collection: 'host' }),
          );
        } catch {
          patchState(store, {
            error: 'Failed to delete host.',
          });
          throw new Error('Failed to delete host.');
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
