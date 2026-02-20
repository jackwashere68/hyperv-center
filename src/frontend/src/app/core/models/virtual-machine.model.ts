export enum VmState {
  Off = 'Off',
  Running = 'Running',
  Paused = 'Paused',
  Saved = 'Saved',
  Starting = 'Starting',
  Stopping = 'Stopping',
  Saving = 'Saving',
  Pausing = 'Pausing',
  Resuming = 'Resuming',
  Other = 'Other',
}

export interface VirtualMachine {
  id: string;
  name: string;
  hyperVHostId: string;
  hostName: string;
  externalId: string | null;
  state: VmState;
  cpuCount: number;
  memoryBytes: number;
  notes: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateVirtualMachineRequest {
  name: string;
  hyperVHostId: string;
  cpuCount: number;
  memoryBytes: number;
  notes?: string;
}

export type VmAction = 'start' | 'stop' | 'pause' | 'save' | 'restart';
