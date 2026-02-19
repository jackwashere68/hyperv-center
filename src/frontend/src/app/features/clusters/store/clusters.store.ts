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
  Cluster,
  CreateClusterRequest,
  UpdateClusterRequest,
} from '@core/models/cluster.model';
import { ClustersService } from '../services/clusters.service';
import { firstValueFrom } from 'rxjs';

export const ClustersStore = signalStore(
  { providedIn: 'root' },
  withState({ loading: false, error: null as string | null }),
  withEntities({ entity: type<Cluster>(), collection: 'cluster' }),
  withComputed(({ clusterEntities, loading }) => ({
    clusterCount: computed(() => clusterEntities().length),
    isLoading: computed(() => loading()),
  })),
  withMethods(
    (store, clustersService = inject(ClustersService)) => ({
      async loadAll() {
        patchState(store, { loading: true, error: null });
        try {
          const clusters = await firstValueFrom(clustersService.getAll());
          patchState(
            store,
            setAllEntities(clusters, { collection: 'cluster' }),
          );
        } catch {
          patchState(store, {
            error: 'Failed to load clusters.',
          });
        } finally {
          patchState(store, { loading: false });
        }
      },
      async create(request: CreateClusterRequest) {
        try {
          const cluster = await firstValueFrom(
            clustersService.create(request),
          );
          patchState(
            store,
            addEntity(cluster, { collection: 'cluster' }),
          );
          return cluster;
        } catch {
          patchState(store, {
            error: 'Failed to create cluster.',
          });
          throw new Error('Failed to create cluster.');
        }
      },
      async update(id: string, request: UpdateClusterRequest) {
        try {
          const cluster = await firstValueFrom(
            clustersService.update(id, request),
          );
          patchState(
            store,
            updateEntity(
              { id, changes: cluster },
              { collection: 'cluster' },
            ),
          );
          return cluster;
        } catch {
          patchState(store, {
            error: 'Failed to update cluster.',
          });
          throw new Error('Failed to update cluster.');
        }
      },
      async remove(id: string) {
        try {
          await firstValueFrom(clustersService.delete(id));
          patchState(
            store,
            removeEntity(id, { collection: 'cluster' }),
          );
        } catch {
          patchState(store, {
            error: 'Failed to delete cluster.',
          });
          throw new Error('Failed to delete cluster.');
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
