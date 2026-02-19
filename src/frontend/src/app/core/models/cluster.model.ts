import { HyperVHost } from './hyperv-host.model';

export enum ClusterStatus {
  Unknown = 'Unknown',
  Online = 'Online',
  Degraded = 'Degraded',
  Offline = 'Offline',
  Error = 'Error',
}

export interface Cluster {
  id: string;
  name: string;
  credentialId: string;
  credentialName: string;
  status: ClusterStatus;
  notes: string | null;
  nodeCount: number;
  createdAt: string;
  updatedAt: string | null;
}

export interface ClusterDetail {
  id: string;
  name: string;
  credentialId: string;
  credentialName: string;
  status: ClusterStatus;
  notes: string | null;
  nodes: HyperVHost[];
  createdAt: string;
  updatedAt: string | null;
}

export interface CreateClusterRequest {
  name: string;
  credentialId: string;
  nodeHostnames: string[];
  notes?: string;
}

export interface UpdateClusterRequest {
  id: string;
  name: string;
  credentialId: string;
  notes?: string;
}

export interface ClusterDetectionRequest {
  hostname: string;
  credentialId: string;
}

export interface ClusterDetectionResult {
  clusterName: string;
  nodeHostnames: string[];
}
