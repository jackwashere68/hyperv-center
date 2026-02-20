export interface VmHardwareInfo {
  generation: number;
  version: string;
  path: string;
  uptime: string; // TimeSpan serialized as "HH:MM:SS"
  dynamicMemoryEnabled: boolean;
  memoryStartup: number;
  memoryMinimum: number;
  memoryMaximum: number;
  memoryAssigned: number;
  memoryDemand: number;
  processorCount: number;
  notes: string | null;
  automaticStartAction: string;
  automaticStopAction: string;
  checkpointType: string;
  disks: VmDiskInfo[];
  networkAdapters: VmNetworkAdapterInfo[];
  snapshots: VmSnapshotInfo[];
}

export interface VmDiskInfo {
  controllerType: string;
  controllerNumber: number;
  controllerLocation: number;
  path: string;
  vhdFormat: string;
  vhdType: string;
  currentSize: number;
  maxSize: number;
}

export interface VmNetworkAdapterInfo {
  name: string;
  switchName: string;
  macAddress: string;
  ipAddresses: string[];
}

export interface VmSnapshotInfo {
  id: string;
  name: string;
  creationTime: string;
  parentSnapshotName: string | null;
}
