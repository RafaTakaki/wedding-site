namespace WeddingGuests.Application.Contracts;

public sealed record CreateInvitationRequest(
    string Name,
    string? GroupName,
    string? WhatsApp,
    List<string>? Companions,
    bool AllowFreeCompanions = false,
    int FreeCompanionLimit = 0);

public sealed record UpdateInvitationRequest(
    string? Name,
    string? GroupName,
    string? WhatsApp,
    bool? AllowFreeCompanions,
    int? FreeCompanionLimit);

public sealed record AddCompanionRequest(string Name);
public sealed record AdminInvitationResponse(int Id, string Name, string? GroupName, string? WhatsApp, bool AllowFreeCompanions, int FreeCompanionLimit, AdminInviteeResponse[] People);
public sealed record AdminInviteeResponse(int Id, string Name, bool IsPrimary, bool IsFreeCompanion, string Status);
public sealed record PublicInvitationResponse(string Name, PublicInviteeResponse[] People);
public sealed record PublicInviteeResponse(string Name, string Status);
