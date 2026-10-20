using Google.Cloud.Firestore;

namespace WeddingGuests.Repository.Persistence;

[FirestoreData]
public sealed class InvitationDocument
{
    [FirestoreProperty] public int Id { get; set; }
    [FirestoreProperty] public string RsvpToken { get; set; } = string.Empty;
    [FirestoreProperty] public string Name { get; set; } = string.Empty;
    [FirestoreProperty] public string NormalizedName { get; set; } = string.Empty;
    [FirestoreProperty] public string? GroupName { get; set; }
    [FirestoreProperty] public string? WhatsApp { get; set; }
    [FirestoreProperty] public bool AllowFreeCompanions { get; set; }
    [FirestoreProperty] public int FreeCompanionLimit { get; set; }
    [FirestoreProperty] public List<InviteeDocument> People { get; set; } = [];
}

[FirestoreData]
public sealed class InviteeDocument
{
    [FirestoreProperty] public int Id { get; set; }
    [FirestoreProperty] public int GuestInvitationId { get; set; }
    [FirestoreProperty] public string Name { get; set; } = string.Empty;
    [FirestoreProperty] public string NormalizedName { get; set; } = string.Empty;
    [FirestoreProperty] public bool IsPrimary { get; set; }
    [FirestoreProperty] public bool IsFreeCompanion { get; set; }
    [FirestoreProperty] public string Status { get; set; } = "a-confirmar";
}

[FirestoreData]
public sealed class CounterDocument
{
    [FirestoreProperty] public int LastId { get; set; }
}

[FirestoreData]
public sealed class RsvpTokenDocument
{
    [FirestoreProperty] public int InvitationId { get; set; }
}
