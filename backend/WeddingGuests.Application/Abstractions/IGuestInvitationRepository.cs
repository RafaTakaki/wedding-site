using WeddingGuests.Domain.Entities;
using WeddingGuests.Application.Contracts;

namespace WeddingGuests.Application.Abstractions;

public interface IGuestInvitationRepository
{
    Task<IReadOnlyList<GuestInvitation>> ListAsync(CancellationToken cancellationToken);
    Task<GuestInvitation?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<GuestInvitation?> GetByRsvpTokenAsync(string token, CancellationToken cancellationToken);
    Task<IReadOnlyList<GuestInvitation>> FindByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken);
    Task AddAsync(GuestInvitation invitation, CancellationToken cancellationToken);
    Task SaveAsync(GuestInvitation invitation, CancellationToken cancellationToken);
    Task DeleteAsync(GuestInvitation invitation, CancellationToken cancellationToken);
    Task<bool> SaveRsvpAsync(string token, IReadOnlyList<RsvpAnswerRequest> answers, CancellationToken cancellationToken);
}
