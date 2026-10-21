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
public sealed record UpdateCompanionRequest(string Name);
public sealed record UpdateInviteeStatusRequest(string Status);
public sealed record RsvpAnswerRequest(int InviteeId, string Status);
public sealed record SaveRsvpRequest(IReadOnlyList<RsvpAnswerRequest> Answers);
public sealed record AdminInvitationResponse(int Id, string Name, string? GroupName, string? WhatsApp, bool AllowFreeCompanions, int FreeCompanionLimit, AdminInviteeResponse[] People);
public sealed record AdminInviteeResponse(int Id, string Name, bool IsPrimary, bool IsFreeCompanion, string Status);
public sealed record PublicInvitationResponse(string RsvpToken, string Name, bool IsRsvpOpen, bool AllowFreeCompanions, int FreeCompanionSlots, PublicInviteeResponse[] People);
public sealed record PublicInviteeResponse(int Id, string Name, string Status);
