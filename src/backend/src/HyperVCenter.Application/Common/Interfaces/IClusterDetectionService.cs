namespace HyperVCenter.Application.Common.Interfaces;

public record ClusterDetectionResult(string ClusterName, IReadOnlyList<string> NodeHostnames);

public interface IClusterDetectionService
{
    Task<ClusterDetectionResult?> DetectClusterAsync(string hostname, Guid credentialId, CancellationToken ct);
}
