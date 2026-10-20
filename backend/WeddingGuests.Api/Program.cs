using Microsoft.AspNetCore.Diagnostics;
using Google.Cloud.Firestore;
using WeddingGuests.Api;
using WeddingGuests.Application.Abstractions;
using WeddingGuests.Application.Exceptions;
using WeddingGuests.Application.Services;
using WeddingGuests.Repository.Repositories;

LoadEnvironmentFile();
var builder = WebApplication.CreateBuilder(args);
var projectId = builder.Configuration["FIREBASE_PROJECT_ID"]
    ?? builder.Configuration["GOOGLE_CLOUD_PROJECT"];
if (string.IsNullOrWhiteSpace(projectId))
{
    throw new InvalidOperationException("Configure FIREBASE_PROJECT_ID com o ID do projeto Firebase/GCP.");
}

builder.Services.AddSingleton(FirestoreDb.Create(projectId));
builder.Services.AddScoped<IGuestInvitationRepository, FirestoreGuestInvitationRepository>();
builder.Services.AddScoped<GuestInvitationService>();
builder.Services.AddSingleton(TimeProvider.System);
var allowedOrigins = (builder.Configuration["FRONTEND_ORIGINS"] ?? "http://localhost:5500,http://127.0.0.1:5500")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(allowedOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

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
app.UseCors();

app.MapGuestEndpoints();
app.Run();

static void LoadEnvironmentFile()
{
    var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (directory is not null)
    {
        var envPath = Path.Combine(directory.FullName, ".env");
        if (File.Exists(envPath))
        {
            foreach (var rawLine in File.ReadLines(envPath))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || line.StartsWith('#')) continue;

                var separator = line.IndexOf('=');
                if (separator <= 0) continue;

                var key = line[..separator].Trim();
                var value = line[(separator + 1)..].Trim();
                if (value.Length >= 2 && value[0] == value[^1] && value[0] is '\'' or '"')
                {
                    value = value[1..^1];
                }

                if (!string.IsNullOrWhiteSpace(key) && Environment.GetEnvironmentVariable(key) is null)
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }

            return;
        }

        directory = directory.Parent;
    }
}
