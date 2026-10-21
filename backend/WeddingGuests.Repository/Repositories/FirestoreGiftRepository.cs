using System.Globalization;
using Google.Cloud.Firestore;
using WeddingGuests.Application.Abstractions;
using WeddingGuests.Domain.Entities;
using WeddingGuests.Repository.Persistence;

namespace WeddingGuests.Repository.Repositories;

public sealed class FirestoreGiftRepository(FirestoreDb firestore) : IGiftRepository
{
    private const string GiftsCollection = "gifts";
    private const string CountersCollection = "system";
    private const string GiftCounterId = "giftCounter";

    public async Task<IReadOnlyList<GiftItem>> ListAsync(CancellationToken cancellationToken)
    {
        var snapshot = await firestore.Collection(GiftsCollection).GetSnapshotAsync(cancellationToken);
        return snapshot.Documents
            .Select(document => FromDocument(document.ConvertTo<GiftDocument>()))
            .OrderBy(gift => gift.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public async Task<GiftItem?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var snapshot = await firestore.Collection(GiftsCollection).Document(id.ToString()).GetSnapshotAsync(cancellationToken);
        return snapshot.Exists ? FromDocument(snapshot.ConvertTo<GiftDocument>()) : null;
    }

    public async Task AddAsync(GiftItem gift, CancellationToken cancellationToken)
    {
        var counterReference = firestore.Collection(CountersCollection).Document(GiftCounterId);
        var giftId = await firestore.RunTransactionAsync(async transaction =>
        {
            var counterSnapshot = await transaction.GetSnapshotAsync(counterReference, cancellationToken);
            var lastId = counterSnapshot.Exists ? counterSnapshot.GetValue<int>(nameof(CounterDocument.LastId)) : 0;
            var nextId = checked(lastId + 1);
            gift.Id = nextId;
            transaction.Set(counterReference, new CounterDocument { LastId = nextId });
            transaction.Set(firestore.Collection(GiftsCollection).Document(nextId.ToString()), ToDocument(gift));
            return nextId;
        }, cancellationToken: cancellationToken);

        gift.Id = giftId;
    }

    public async Task<bool> ClaimAsync(int id, string guestName, DateTimeOffset claimedAt, CancellationToken cancellationToken)
    {
        var giftReference = firestore.Collection(GiftsCollection).Document(id.ToString());
        return await firestore.RunTransactionAsync(async transaction =>
        {
            var snapshot = await transaction.GetSnapshotAsync(giftReference, cancellationToken);
            if (!snapshot.Exists) return false;

            var gift = snapshot.ConvertTo<GiftDocument>();
            if (gift.Status != "available") return false;

            gift.Status = "gifted";
            gift.ClaimedBy = guestName;
            gift.ClaimedAtUtc = claimedAt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
            transaction.Set(giftReference, gift);
            return true;
        }, cancellationToken: cancellationToken);
    }

    public async Task<bool> ReleaseAsync(int id, CancellationToken cancellationToken)
    {
        var giftReference = firestore.Collection(GiftsCollection).Document(id.ToString());
        return await firestore.RunTransactionAsync(async transaction =>
        {
            var snapshot = await transaction.GetSnapshotAsync(giftReference, cancellationToken);
            if (!snapshot.Exists) return false;

            var gift = snapshot.ConvertTo<GiftDocument>();
            gift.Status = "available";
            gift.ClaimedBy = null;
            gift.ClaimedAtUtc = null;
            transaction.Set(giftReference, gift);
            return true;
        }, cancellationToken: cancellationToken);
    }

    private static GiftDocument ToDocument(GiftItem gift) => new()
    {
        Id = gift.Id,
        Name = gift.Name,
        Description = gift.Description,
        AmountInCents = gift.AmountInCents,
        ImageUrl = gift.ImageUrl,
        Status = gift.Status,
        ClaimedBy = gift.ClaimedBy,
        ClaimedAtUtc = gift.ClaimedAt?.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture)
    };

    private static GiftItem FromDocument(GiftDocument gift) => new()
    {
        Id = gift.Id,
        Name = gift.Name,
        Description = gift.Description,
        AmountInCents = gift.AmountInCents,
        ImageUrl = gift.ImageUrl,
        Status = gift.Status,
        ClaimedBy = gift.ClaimedBy,
        ClaimedAt = DateTimeOffset.TryParse(gift.ClaimedAtUtc, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var claimedAt)
            ? claimedAt
            : null
    };
}
