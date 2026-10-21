using Google.Cloud.Firestore;

namespace WeddingGuests.Repository.Persistence;

[FirestoreData]
public sealed class GiftDocument
{
    [FirestoreProperty] public int Id { get; set; }
    [FirestoreProperty] public string Name { get; set; } = string.Empty;
    [FirestoreProperty] public string Description { get; set; } = string.Empty;
    [FirestoreProperty] public int AmountInCents { get; set; }
    [FirestoreProperty] public string? ImageUrl { get; set; }
    [FirestoreProperty] public string Status { get; set; } = "available";
    [FirestoreProperty] public string? ClaimedBy { get; set; }
    [FirestoreProperty] public string? ClaimedAtUtc { get; set; }
}
