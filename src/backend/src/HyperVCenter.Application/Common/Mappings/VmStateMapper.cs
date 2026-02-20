using HyperVCenter.Domain.Enums;

namespace HyperVCenter.Application.Common.Mappings;

public static class VmStateMapper
{
    public static VmState MapFromHyperV(string state) => state switch
    {
        "Off" => VmState.Off,
        "Running" => VmState.Running,
        "Paused" => VmState.Paused,
        "Saved" => VmState.Saved,
        "Starting" => VmState.Starting,
        "Stopping" => VmState.Stopping,
        "Saving" => VmState.Saving,
        "Pausing" => VmState.Pausing,
        "Resuming" => VmState.Resuming,
        _ => VmState.Other,
    };
}
