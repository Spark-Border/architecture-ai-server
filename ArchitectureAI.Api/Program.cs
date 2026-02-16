using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration; // Aadded
using ArchitectureAI.Persistence.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Ensure Environment Variables are loaded (defaults to true in CreateBuilder, but good to be explicit for hierarchy)
builder.Configuration.AddEnvironmentVariables();


// Add services to the container.
builder.Services.AddPersistenceServices(builder.Configuration);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
