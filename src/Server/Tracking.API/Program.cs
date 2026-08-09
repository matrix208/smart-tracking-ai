using Tracking.Storage.Data;
using Tracking.Storage.DependencyInjection;
using Tracking.Application.DependencyInjection;
using Tracking.Network;
using Tracking.Pipeline;
using Tracking.Runtime.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddTrackingStorage(
    builder.Configuration);

builder.Services.AddTrackingApplication();
builder.Services.AddTrackingRuntime();


// هنا سنضيف:
// Plugin Loader
// Pipeline
// TCP Server
// Background Workers


var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<TrackingDbContext>();

    db.Database.EnsureCreated();
}


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();

app.MapControllers();

app.Run();