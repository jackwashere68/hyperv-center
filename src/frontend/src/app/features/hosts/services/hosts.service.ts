import { Injectable, inject } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import {
  HyperVHost,
  CreateHyperVHostRequest,
  UpdateHyperVHostRequest,
} from '@core/models/hyperv-host.model';

@Injectable({ providedIn: 'root' })
export class HostsService {
  private readonly api = inject(ApiService);
  private readonly path = '/hypervhosts';

  getAll() {
    return this.api.get<HyperVHost[]>(this.path);
  }

  getById(id: string) {
    return this.api.get<HyperVHost>(`${this.path}/${id}`);
  }

  create(request: CreateHyperVHostRequest) {
    return this.api.post<HyperVHost>(this.path, request);
  }

  update(id: string, request: UpdateHyperVHostRequest) {
    return this.api.put<HyperVHost>(`${this.path}/${id}`, request);
  }

  delete(id: string) {
    return this.api.delete<void>(`${this.path}/${id}`);
  }
}
