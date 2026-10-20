namespace WeddingGuests.Domain.Entities;

public sealed class Invitee
{
    public int Id { get; set; }
    public int GuestInvitationId { get; set; }
    public required string Name { get; set; }
    public required string NormalizedName { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsFreeCompanion { get; set; }
    public string Status { get; set; } = "a-confirmar";
}
