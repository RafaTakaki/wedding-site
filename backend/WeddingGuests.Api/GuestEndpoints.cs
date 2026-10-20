using WeddingGuests.Application.Contracts;
using WeddingGuests.Application.Services;

namespace WeddingGuests.Api;

public static class GuestEndpoints
{
    public static void MapGuestEndpoints(this WebApplication app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
        app.MapGet("/api/guests/lookup", LookupAsync);
        app.MapPost("/api/guests/rsvp/{rsvpToken}", SaveRsvpAsync);

        var admin = app.MapGroup("/api/admin").AddEndpointFilter<AdminKeyEndpointFilter>();
        admin.MapGet("/guests", ListAsync);
        admin.MapGet("/guests/{invitationId:int}", GetAsync);
        admin.MapPost("/guests", CreateAsync);
        admin.MapPatch("/guests/{invitationId:int}", UpdateAsync);
        admin.MapPost("/guests/{invitationId:int}/companions", AddCompanionAsync);
        admin.MapDelete("/guests/{invitationId:int}/companions/{personId:int}", DeleteCompanionAsync);
        admin.MapDelete("/guests/{invitationId:int}", DeleteAsync);
    }

    private static async Task<IResult> LookupAsync(
        string? name,
        GuestInvitationService service,
        IConfiguration configuration,
        TimeProvider timeProvider,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.LookupAsync(name, GetDeadline(configuration), timeProvider.GetUtcNow(), cancellationToken));

    private static async Task<IResult> SaveRsvpAsync(
        string rsvpToken,
        WeddingGuests.Application.Contracts.SaveRsvpRequest request,
        GuestInvitationService service,
        IConfiguration configuration,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        await service.SaveRsvpAsync(rsvpToken, request, GetDeadline(configuration), timeProvider.GetUtcNow(), cancellationToken);
        return Results.NoContent();
    }

    private static DateTimeOffset GetDeadline(IConfiguration configuration) =>
        DateTimeOffset.TryParse(configuration["RSVP_DEADLINE"], out var deadline)
            ? deadline
            : new DateTimeOffset(2027, 6, 6, 23, 59, 0, TimeSpan.FromHours(-3));

    private static async Task<IResult> ListAsync(GuestInvitationService service, CancellationToken cancellationToken) =>
        Results.Ok(await service.ListAsync(cancellationToken));

    private static async Task<IResult> GetAsync(int invitationId, GuestInvitationService service, CancellationToken cancellationToken) =>
        Results.Ok(await service.GetAsync(invitationId, cancellationToken));

    private static async Task<IResult> CreateAsync(
        CreateInvitationRequest request,
        GuestInvitationService service,
        CancellationToken cancellationToken)
    {
        var response = await service.CreateAsync(request, cancellationToken);
        return Results.Created($"/api/admin/guests/{response.Id}", response);
    }

    private static async Task<IResult> UpdateAsync(
        int invitationId,
        UpdateInvitationRequest request,
        GuestInvitationService service,
        CancellationToken cancellationToken) =>
        Results.Ok(await service.UpdateAsync(invitationId, request, cancellationToken));

    private static async Task<IResult> AddCompanionAsync(
        int invitationId,
        AddCompanionRequest request,
        GuestInvitationService service,
        CancellationToken cancellationToken)
    {
        var response = await service.AddCompanionAsync(invitationId, request, cancellationToken);
        return Results.Created($"/api/admin/guests/{response.Id}", response);
    }

    private static async Task<IResult> DeleteCompanionAsync(
        int invitationId,
        int personId,
        GuestInvitationService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteCompanionAsync(invitationId, personId, cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteAsync(
        int invitationId,
        GuestInvitationService service,
        CancellationToken cancellationToken)
    {
        await service.DeleteAsync(invitationId, cancellationToken);
        return Results.NoContent();
    }
}
