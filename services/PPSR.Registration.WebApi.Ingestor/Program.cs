using Microsoft.EntityFrameworkCore;
using PPSR.Registration.Application.Interfaces;
using PPSR.Registration.Infrastructure.Persistence;
using PPSR.Registration.Infrastructure.Repositories;
using PPSR.Registration.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add DbContext with InMemory database
builder.Services.AddDbContext<PpsrDbContext>(options =>
    options.UseInMemoryDatabase("PpsrDb"));

// Register repository and service
builder.Services.AddScoped<IRegistrationRepository, RegistrationRepository>();
builder.Services.AddScoped<IBatchRegistrationService, BatchRegistrationService>();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();