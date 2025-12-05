using Microsoft.EntityFrameworkCore;
using SmartInventoryAI.Infrastructure;
using SmartInventoryAI.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "SmartInventoryAI API",
        Version = "v1",
        Description = "API para gestão inteligente de estoque com previsões e sugestões de IA",
        Contact = new()
        {
            Name = "SmartInventoryAI Team",
            Email = "suporte@smartinventoryai.com"
        }
    });
});

// Add Infrastructure services (DbContext, Repositories, Ollama, Redis, OpenTelemetry)
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartInventoryAI API v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Apply migrations on startup (for development only)
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SmartInventoryDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
