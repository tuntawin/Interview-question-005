using Microsoft.EntityFrameworkCore;
using QueueApp.Api.Data;
using QueueApp.Api.Endpoints;
using QueueApp.Api.Models;
using QueueApp.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=queue.db";

// Database Context
builder.Services.AddDbContext<QueueDbContext>(options =>
    options.UseSqlite(connectionString));

// Register Queue Service
builder.Services.AddScoped<IQueueService, QueueService>();

// Enable CORS for Frontend connection
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable CORS
app.UseCors("AllowAll");

// Auto-create database & Seed initial queue setting if empty on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<QueueDbContext>();
    dbContext.Database.EnsureCreated();

    var existingSetting = dbContext.QueueSettings.FirstOrDefault(s => s.Id == 1);
    if (existingSetting == null)
    {
        dbContext.QueueSettings.Add(new QueueSetting
        {
            Id = 1,
            CurrentIndex = -1,
            LastActive = DateTime.UtcNow
        });
        dbContext.SaveChanges();
    }
}

// Map Minimal API Endpoints
app.MapQueueEndpoints();

app.Run();

// Make Program class accessible for integration tests if needed
public partial class Program { }
