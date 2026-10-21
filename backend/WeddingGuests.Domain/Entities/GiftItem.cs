namespace WeddingGuests.Domain.Entities;

public sealed class GiftItem
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int AmountInCents { get; set; }
    public string? ImageUrl { get; set; }
    public string Status { get; set; } = "available";
    public string? ClaimedBy { get; set; }
    public DateTimeOffset? ClaimedAt { get; set; }
}
