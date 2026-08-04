using Tracking.Storage.DependencyInjection;
using Tracking.Storage.Repositories;
using Tracking.Storage.Data;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi();

// Tracking Storage (Database + DbContext)
builder.Services.AddTrackingStorage(
    builder.Configuration);

// Repositories
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<TrackingDbContext>();

    db.Database.EnsureCreated();
}
// OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Controllers
app.MapControllers();

app.Run();