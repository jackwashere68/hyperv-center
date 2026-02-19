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
}
