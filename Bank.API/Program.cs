using Bank.API.Middleware;
using Bank.Application;
using Bank.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Dependency Injection
// ==========================================

// Application layer
builder.Services.AddApplication();

// Infrastructure layer
builder.Services.AddInfrastructure(builder.Configuration);

// API services
builder.Services.AddControllers();

builder.Services.AddProblemDetails();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// ==========================================
// HTTP Request Pipeline
// ==========================================

var app = builder.Build();

// Global exception handling
app.UseExceptionHandler();

// OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// HTTPS
app.UseHttpsRedirection();

// Controllers
app.MapControllers();

app.Run();
