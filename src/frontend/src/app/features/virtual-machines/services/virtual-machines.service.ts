import { Injectable, inject } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import {
  VirtualMachine,
  CreateVirtualMachineRequest,
} from '@core/models/virtual-machine.model';

@Injectable({ providedIn: 'root' })
export class VirtualMachinesService {
  private readonly api = inject(ApiService);
  private readonly path = '/virtualmachines';

  getAll() {
    return this.api.get<VirtualMachine[]>(this.path);
  }

  getById(id: string) {
    return this.api.get<VirtualMachine>(`${this.path}/${id}`);
  }

  create(request: CreateVirtualMachineRequest) {
    return this.api.post<VirtualMachine>(this.path, request);
  }

  start(id: string) {
    return this.api.post<VirtualMachine>(`${this.path}/${id}/start`, {});
  }

  stop(id: string, force = false) {
    return this.api.post<VirtualMachine>(`${this.path}/${id}/stop?force=${force}`, {});
  }

  pause(id: string) {
    return this.api.post<VirtualMachine>(`${this.path}/${id}/pause`, {});
  }

  save(id: string) {
    return this.api.post<VirtualMachine>(`${this.path}/${id}/save`, {});
  }

  restart(id: string) {
    return this.api.post<VirtualMachine>(`${this.path}/${id}/restart`, {});
  }

  delete(id: string) {
    return this.api.delete<void>(`${this.path}/${id}`);
  }
}
