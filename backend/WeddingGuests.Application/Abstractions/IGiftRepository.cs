using WeddingGuests.Domain.Entities;

namespace WeddingGuests.Application.Abstractions;

public interface IGiftRepository
{
    Task<IReadOnlyList<GiftItem>> ListAsync(CancellationToken cancellationToken);
    Task<GiftItem?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task AddAsync(GiftItem gift, CancellationToken cancellationToken);
    Task<bool> ClaimAsync(int id, string guestName, DateTimeOffset claimedAt, CancellationToken cancellationToken);
    Task<bool> ReleaseAsync(int id, CancellationToken cancellationToken);
}
