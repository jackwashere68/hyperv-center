using System.Text.Json.Serialization;
using HyperVCenter.Application;
using HyperVCenter.Infrastructure;
using HyperVCenter.Web.Middleware;
using HyperVCenter.Web.WebSockets;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Layer DI
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// Error handling
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// WSL keep-alive (prevents WSL2 from shutting down when guacd is needed)
builder.Services.AddHostedService<WslKeepAliveService>();

// API
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "HyperV Center API",
        Version = "v1",
    });
});

var app = builder.Build();

// Middleware pipeline
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(30),
});
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapVmConsole();
app.MapFallbackToFile("index.html");

app.Run();
