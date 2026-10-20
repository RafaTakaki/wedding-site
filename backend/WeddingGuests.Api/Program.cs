using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using WeddingGuests.Api;
using WeddingGuests.Application.Abstractions;
using WeddingGuests.Application.Exceptions;
using WeddingGuests.Application.Services;
using WeddingGuests.Repository.Persistence;
using WeddingGuests.Repository.Repositories;

var builder = WebApplication.CreateBuilder(args);
var databasePath = Path.GetFullPath(
    builder.Configuration["GUESTS_DB_PATH"] ?? Path.Combine(builder.Environment.ContentRootPath, "data", "guests.db"));
Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);

builder.Services.AddDbContext<WeddingDbContext>(options => options.UseSqlite($"Data Source={databasePath}"));
builder.Services.AddScoped<IGuestInvitationRepository, GuestInvitationRepository>();
builder.Services.AddScoped<GuestInvitationService>();

var app = builder.Build();

app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    context.Response.ContentType = "application/json";

    if (exception is GuestApplicationException applicationException)
    {
        context.Response.StatusCode = applicationException.StatusCode;
        await context.Response.WriteAsJsonAsync(new { message = applicationException.Message });
        return;
    }

    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new { message = "Ocorreu um erro interno." });
}));

using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<WeddingDbContext>();
    await database.InitializeAsync();
}

app.MapGuestEndpoints();
app.Run();
