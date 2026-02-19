import { Injectable, inject } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import {
  Credential,
  CreateCredentialRequest,
  UpdateCredentialRequest,
} from '@core/models/credential.model';

@Injectable({ providedIn: 'root' })
export class CredentialsService {
  private readonly api = inject(ApiService);
  private readonly path = '/credentials';

  getAll() {
    return this.api.get<Credential[]>(this.path);
  }

  getById(id: string) {
    return this.api.get<Credential>(`${this.path}/${id}`);
  }

  create(request: CreateCredentialRequest) {
    return this.api.post<Credential>(this.path, request);
  }

  update(id: string, request: UpdateCredentialRequest) {
    return this.api.put<Credential>(`${this.path}/${id}`, request);
  }

  delete(id: string) {
    return this.api.delete<void>(`${this.path}/${id}`);
  }
}
