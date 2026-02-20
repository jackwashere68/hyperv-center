using HyperVCenter.Application.Features.HyperVHosts;
using HyperVCenter.Application.Features.HyperVHosts.Commands;
using HyperVCenter.Application.Features.HyperVHosts.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HyperVCenter.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HyperVHostsController : ControllerBase
{
    private readonly ISender _sender;

    public HyperVHostsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<HyperVHostDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetHyperVHostsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(HyperVHostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetHyperVHostByIdQuery(id), cancellationToken);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(HyperVHostDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateHyperVHostCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(HyperVHostDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateHyperVHostCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest();

        var result = await _sender.Send(command, cancellationToken);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteHyperVHostCommand(id), cancellationToken);
        return result ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/sync")]
    [ProducesResponseType(typeof(HyperVHostDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> Sync(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SyncHyperVHostCommand(id), cancellationToken);
        return Ok(result);
    }
}
