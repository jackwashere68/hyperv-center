export enum HostStatus {
  Unknown = 'Unknown',
  Online = 'Online',
  Offline = 'Offline',
  Error = 'Error',
}

export interface HyperVHost {
  id: string;
  name: string;
  hostname: string;
  credentialId: string;
  credentialName: string;
  status: HostStatus;
  notes: string | null;
  clusterId: string | null;
  clusterName: string | null;
  osVersion: string | null;
  processorCount: number | null;
  totalMemoryBytes: number | null;
  lastSyncedAt: string | null;
  lastSyncError: string | null;
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateHyperVHostRequest {
  name: string;
  hostname: string;
  credentialId: string;
  notes?: string;
}

export interface UpdateHyperVHostRequest {
  id: string;
  name: string;
  hostname: string;
  credentialId: string;
  notes?: string;
}
