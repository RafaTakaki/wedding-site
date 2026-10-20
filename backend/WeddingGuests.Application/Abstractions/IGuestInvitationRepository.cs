using WeddingGuests.Domain.Entities;

namespace WeddingGuests.Application.Abstractions;

public interface IGuestInvitationRepository
{
    Task<IReadOnlyList<GuestInvitation>> ListAsync(CancellationToken cancellationToken);
    Task<GuestInvitation?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<GuestInvitation>> FindByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken);
    Task AddAsync(GuestInvitation invitation, CancellationToken cancellationToken);
    Task AddInviteeAsync(Invitee invitee, CancellationToken cancellationToken);
    void RemoveInvitee(Invitee invitee);
    void RemoveInvitation(GuestInvitation invitation);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
