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
    private readonly CreateVirtualMachineValidator _validator = new();

    [Fact]
    public void Validator_ShouldPass_WhenCommandIsValid()
    {
        var command = new CreateVirtualMachineCommand(
            Name: "TestVM",
            Host: "HV-HOST-01",
            CpuCount: 2,
            MemoryBytes: 2147483648,
            Notes: null);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validator_ShouldFail_WhenNameIsEmpty()
    {
        var command = new CreateVirtualMachineCommand(
            Name: "",
            Host: "HV-HOST-01",
            CpuCount: 2,
            MemoryBytes: 2147483648,
            Notes: null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validator_ShouldFail_WhenCpuCountIsZero()
    {
        var command = new CreateVirtualMachineCommand(
            Name: "TestVM",
            Host: "HV-HOST-01",
            CpuCount: 0,
            MemoryBytes: 2147483648,
            Notes: null);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CpuCount);
    }

    [Fact]
    public async Task Handler_ShouldCreateVm_AndReturnDto()
    {
        // Arrange
        var dbSetMock = Substitute.For<DbSet<VirtualMachine>>();
        var contextMock = Substitute.For<IApplicationDbContext>();
        contextMock.VirtualMachines.Returns(dbSetMock);
        contextMock.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var handler = new CreateVirtualMachineHandler(contextMock);
        var command = new CreateVirtualMachineCommand(
            Name: "TestVM",
            Host: "HV-HOST-01",
            CpuCount: 4,
            MemoryBytes: 4294967296,
            Notes: "Test notes");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("TestVM");
        result.Host.Should().Be("HV-HOST-01");
        result.CpuCount.Should().Be(4);
        result.MemoryBytes.Should().Be(4294967296);
        result.Notes.Should().Be("Test notes");
        result.State.Should().Be(VmState.PoweredOff);
        result.Id.Should().NotBeEmpty();

        dbSetMock.Received(1).Add(Arg.Is<VirtualMachine>(vm =>
            vm.Name == "TestVM" &&
            vm.Host == "HV-HOST-01" &&
            vm.CpuCount == 4));

        await contextMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
