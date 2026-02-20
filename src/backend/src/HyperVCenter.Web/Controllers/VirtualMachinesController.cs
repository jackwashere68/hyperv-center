using HyperVCenter.Application.Features.VirtualMachines;
using HyperVCenter.Application.Features.VirtualMachines.Commands;
using HyperVCenter.Application.Features.VirtualMachines.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HyperVCenter.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VirtualMachinesController : ControllerBase
{
    private readonly ISender _sender;

    public VirtualMachinesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VirtualMachineDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetVirtualMachinesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VirtualMachineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetVirtualMachineByIdQuery(id), cancellationToken);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(VirtualMachineDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateVirtualMachineCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}/hardware")]
    [ProducesResponseType(typeof(VmHardwareDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHardware(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetVmHardwareQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/start")]
    [ProducesResponseType(typeof(VirtualMachineDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new StartVmCommand(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/stop")]
    [ProducesResponseType(typeof(VirtualMachineDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Stop(Guid id, [FromQuery] bool force = false, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new StopVmCommand(id, force), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/pause")]
    [ProducesResponseType(typeof(VirtualMachineDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Pause(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new PauseVmCommand(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/save")]
    [ProducesResponseType(typeof(VirtualMachineDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Save(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SaveVmCommand(id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/restart")]
    [ProducesResponseType(typeof(VirtualMachineDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Restart(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RestartVmCommand(id), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteVmCommand(id), cancellationToken);
        return result ? NoContent() : NotFound();
    }
}
