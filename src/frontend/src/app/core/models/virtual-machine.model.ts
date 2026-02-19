export enum VmState {
  PoweredOff = 0,
  Running = 1,
  Paused = 2,
  Saved = 3,
}

export interface VirtualMachine {
  id: string;
  name: string;
  host: string;
  state: VmState;
  cpuCount: number;
  memoryBytes: number;
  notes: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateVirtualMachineRequest {
  name: string;
  host: string;
  cpuCount: number;
  memoryBytes: number;
  notes?: string;
}
