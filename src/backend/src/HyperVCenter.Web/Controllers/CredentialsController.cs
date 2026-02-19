using HyperVCenter.Application.Features.Credentials;
using HyperVCenter.Application.Features.Credentials.Commands;
using HyperVCenter.Application.Features.Credentials.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace HyperVCenter.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CredentialsController : ControllerBase
{
    private readonly ISender _sender;

    public CredentialsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CredentialDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCredentialsQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCredentialByIdQuery(id), cancellationToken);
        return result is not null ? Ok(result) : NotFound();
    }

    [HttpPost]
    [ProducesResponseType(typeof(CredentialDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCredentialCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteCredentialCommand(id), cancellationToken);
        return result ? NoContent() : NotFound();
    }
}
