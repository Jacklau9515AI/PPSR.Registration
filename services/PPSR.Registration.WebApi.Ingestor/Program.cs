using Microsoft.EntityFrameworkCore;
using PPSR.Registration.Application.Interfaces;
using PPSR.Registration.Infrastructure.Persistence;
using PPSR.Registration.Infrastructure.Repositories;
using PPSR.Registration.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add DbContext with InMemory database
DotNetEnv.Env.Load();

var host = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? "localhost";
var port = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? "5433";
var db = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? "ppsrdb";
var user = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? "ppsruser";
var pass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? "ppsrpass";

var connString = $"Host={host};Port={port};Database={db};Username={user};Password={pass}";

builder.Services.AddDbContext<PpsrDbContext>(options =>
    options.UseNpgsql(connString));

// Register repository and service
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();
builder.Services.AddScoped<IBatchRegistrationService, BatchRegistrationService>();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendDev", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // The frontend dev server
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Controllers
builder.Services.AddControllers();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontendDev");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();