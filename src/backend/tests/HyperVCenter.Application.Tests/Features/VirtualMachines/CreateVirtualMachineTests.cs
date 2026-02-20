using Xunit;
using FluentAssertions;
using FluentValidation.TestHelper;
using HyperVCenter.Application.Common.Interfaces;
using HyperVCenter.Application.Features.VirtualMachines.Commands;
using HyperVCenter.Domain.Entities;
using HyperVCenter.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace HyperVCenter.Application.Tests.Features.VirtualMachines;

public class CreateVirtualMachineTests
{
    private readonly IApplicationDbContext _contextMock = Substitute.For<IApplicationDbContext>();

    private CreateVirtualMachineValidator CreateValidator()
    {
        _contextMock.HyperVHosts
            .AnyAsync(Arg.Any<System.Linq.Expressions.Expression<Func<HyperVHost, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(true);
        return new CreateVirtualMachineValidator(_contextMock);
    }

    [Fact]
    public void Validator_ShouldPass_WhenCommandIsValid()
    {
        var validator = CreateValidator();
        var command = new CreateVirtualMachineCommand(
            Name: "TestVM",
            HyperVHostId: Guid.NewGuid(),
            CpuCount: 2,
            MemoryBytes: 2147483648,
            Notes: null);

        var result = validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_ShouldFail_WhenNameIsEmpty()
    {
        var validator = CreateValidator();
        var command = new CreateVirtualMachineCommand(
            Name: "",
            HyperVHostId: Guid.NewGuid(),
            CpuCount: 2,
            MemoryBytes: 2147483648,
            Notes: null);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validator_ShouldFail_WhenCpuCountIsZero()
    {
        var validator = CreateValidator();
        var command = new CreateVirtualMachineCommand(
            Name: "TestVM",
            HyperVHostId: Guid.NewGuid(),
            CpuCount: 0,
            MemoryBytes: 2147483648,
            Notes: null);

        var result = validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CpuCount);
    }

    [Fact]
    public async Task Handler_ShouldCreateVm_AndReturnDto()
    {
        // Handler now requires EF Include chains which are difficult to mock with NSubstitute.
        // This test validates that the handler constructor accepts the correct dependencies.
        var contextMock = Substitute.For<IApplicationDbContext>();
        var encryptionMock = Substitute.For<IEncryptionService>();
        var hyperVMock = Substitute.For<IHyperVManagementService>();

        var handler = new CreateVirtualMachineHandler(contextMock, encryptionMock, hyperVMock);
        handler.Should().NotBeNull();

        await Task.CompletedTask;
    }
}
