namespace WeddingGuests.Domain.Entities;

public sealed class GuestInvitation
{
    public int Id { get; set; }
    public required string RsvpToken { get; set; }
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public string? GroupName { get; set; }
    public string? WhatsApp { get; set; }
    public bool AllowFreeCompanions { get; set; }
    public int FreeCompanionLimit { get; set; }
    public List<Invitee> People { get; set; } = [];
}
