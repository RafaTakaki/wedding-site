namespace WeddingGuests.Application.Contracts;

public sealed record CreateGiftRequest(
    string Name,
    string Description,
    int AmountInCents,
    string? ImageUrl);

public sealed record ClaimGiftRequest(string GuestName);

public sealed record GiftResponse(
    int Id,
    string Name,
    string Description,
    int AmountInCents,
    string? ImageUrl,
    string Status,
    string? ClaimedBy,
    DateTimeOffset? ClaimedAt);

public sealed record PublicGiftResponse(
    int Id,
    string Name,
    string Description,
    int AmountInCents,
    string? ImageUrl,
    string Status);
