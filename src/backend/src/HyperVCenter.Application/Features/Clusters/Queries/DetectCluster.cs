using HyperVCenter.Application.Common.Interfaces;
using MediatR;

namespace HyperVCenter.Application.Features.Clusters.Queries;

public record ClusterDetectionResultDto(string ClusterName, string[] NodeHostnames);

public record DetectClusterQuery(string Hostname, Guid CredentialId) : IRequest<ClusterDetectionResultDto?>;

public class DetectClusterHandler : IRequestHandler<DetectClusterQuery, ClusterDetectionResultDto?>
{
    private readonly IClusterDetectionService _detectionService;

    public DetectClusterHandler(IClusterDetectionService detectionService)
    {
        _detectionService = detectionService;
    }

    public async Task<ClusterDetectionResultDto?> Handle(
        DetectClusterQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _detectionService.DetectClusterAsync(
            request.Hostname, request.CredentialId, cancellationToken);

        if (result is null) return null;

        return new ClusterDetectionResultDto(
            result.ClusterName,
            result.NodeHostnames.ToArray());
    }
}
