import { Injectable, inject } from '@angular/core';
import { ApiService } from '@core/services/api.service';
import {
  Cluster,
  ClusterDetail,
  CreateClusterRequest,
  UpdateClusterRequest,
  ClusterDetectionRequest,
  ClusterDetectionResult,
} from '@core/models/cluster.model';

@Injectable({ providedIn: 'root' })
export class ClustersService {
  private readonly api = inject(ApiService);
  private readonly path = '/clusters';

  getAll() {
    return this.api.get<Cluster[]>(this.path);
  }

  getById(id: string) {
    return this.api.get<ClusterDetail>(`${this.path}/${id}`);
  }

  create(request: CreateClusterRequest) {
    return this.api.post<Cluster>(this.path, request);
  }

  update(id: string, request: UpdateClusterRequest) {
    return this.api.put<Cluster>(`${this.path}/${id}`, request);
  }

  delete(id: string) {
    return this.api.delete<void>(`${this.path}/${id}`);
  }

  detect(request: ClusterDetectionRequest) {
    return this.api.post<ClusterDetectionResult | null>(
      `${this.path}/detect`,
      request,
    );
  }
}
