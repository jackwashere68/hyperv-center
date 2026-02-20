using HyperVCenter.Application.Common.Interfaces;
using HyperVCenter.Infrastructure.Persistence;
using HyperVCenter.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HyperVCenter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsql => npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        var keysPath = configuration["DataProtection:KeysPath"];
        var dpBuilder = services.AddDataProtection()
            .SetApplicationName("HyperVCenter");
        if (!string.IsNullOrEmpty(keysPath))
            dpBuilder.PersistKeysToFileSystem(new DirectoryInfo(keysPath));
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IClusterDetectionService, StubClusterDetectionService>();

        var useStub = configuration["HyperV:UseStubService"]?.Equals("false", StringComparison.OrdinalIgnoreCase) != true;
        if (useStub)
            services.AddScoped<IHyperVManagementService, StubHyperVManagementService>();
        else
            services.AddScoped<IHyperVManagementService, PowerShellHyperVManagementService>();

        services.AddHostedService<HostSyncBackgroundService>();

        return services;
    }
}
