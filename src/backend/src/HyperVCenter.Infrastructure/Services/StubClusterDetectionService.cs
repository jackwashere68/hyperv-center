using HyperVCenter.Application.Common.Interfaces;

namespace HyperVCenter.Infrastructure.Services;

public class StubClusterDetectionService : IClusterDetectionService
{
    public Task<ClusterDetectionResult?> DetectClusterAsync(
        string hostname, Guid credentialId, CancellationToken ct)
    {
        return Task.FromResult<ClusterDetectionResult?>(null);
    }
}
