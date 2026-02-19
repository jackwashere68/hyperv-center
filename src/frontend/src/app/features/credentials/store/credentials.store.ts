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
  Credential,
  CreateCredentialRequest,
} from '@core/models/credential.model';
import { CredentialsService } from '../services/credentials.service';
import { firstValueFrom } from 'rxjs';

export const CredentialsStore = signalStore(
  { providedIn: 'root' },
  withState({ loading: false, error: null as string | null }),
  withEntities({ entity: type<Credential>(), collection: 'credential' }),
  withComputed(({ credentialEntities, loading }) => ({
    credentialCount: computed(() => credentialEntities().length),
    isLoading: computed(() => loading()),
  })),
  withMethods(
    (store, credentialsService = inject(CredentialsService)) => ({
      async loadAll() {
        patchState(store, { loading: true, error: null });
        try {
          const credentials = await firstValueFrom(
            credentialsService.getAll(),
          );
          patchState(
            store,
            setAllEntities(credentials, { collection: 'credential' }),
          );
        } catch {
          patchState(store, {
            error: 'Failed to load credentials.',
          });
        } finally {
          patchState(store, { loading: false });
        }
      },
      async create(request: CreateCredentialRequest) {
        try {
          const credential = await firstValueFrom(
            credentialsService.create(request),
          );
          patchState(
            store,
            addEntity(credential, { collection: 'credential' }),
          );
          return credential;
        } catch {
          patchState(store, {
            error: 'Failed to create credential.',
          });
          throw new Error('Failed to create credential.');
        }
      },
      async remove(id: string) {
        try {
          await firstValueFrom(credentialsService.delete(id));
          patchState(
            store,
            removeEntity(id, { collection: 'credential' }),
          );
        } catch {
          patchState(store, {
            error: 'Failed to delete credential.',
          });
          throw new Error('Failed to delete credential.');
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
