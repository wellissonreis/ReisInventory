using Microsoft.EntityFrameworkCore;
using SmartInventoryAI.Infrastructure;
using SmartInventoryAI.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

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

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

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

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<SmartInventoryDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
