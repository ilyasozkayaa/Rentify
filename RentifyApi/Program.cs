using OpenAI.Responses;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using RentifyApi.Middleware;
using RentifyApplication.Dependency;
using RentifyInfrastructure.Dependency;
using Scalar.AspNetCore;
using System.ClientModel;
using System.ClientModel.Primitives;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource =>
    {
        resource.AddService("RentifyApi");
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddMeter("Rentify.LLM")
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddConsoleExporter();
    });

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<ResponsesClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    var apiKey = configuration["OpenAI:ApiKey"]
        ?? throw new InvalidOperationException(
            "OpenAI API key is not configured.");

    var options = new ResponsesClientOptions
    {
        RetryPolicy = new ClientRetryPolicy(1)
    };

    return new ResponsesClient(
        new ApiKeyCredential(apiKey),
        options);
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
