using Microsoft.EntityFrameworkCore;
using WeddingGuests.Application.Abstractions;
using WeddingGuests.Domain.Entities;
using WeddingGuests.Repository.Persistence;

namespace WeddingGuests.Repository.Repositories;

public sealed class GuestInvitationRepository(WeddingDbContext database) : IGuestInvitationRepository
{
    public async Task<IReadOnlyList<GuestInvitation>> ListAsync(CancellationToken cancellationToken) =>
        await database.Invitations
            .Include(invitation => invitation.People)
            .OrderBy(invitation => invitation.Name)
            .ToListAsync(cancellationToken);

    public Task<GuestInvitation?> GetByIdAsync(int id, CancellationToken cancellationToken) =>
        database.Invitations
            .Include(invitation => invitation.People)
            .FirstOrDefaultAsync(invitation => invitation.Id == id, cancellationToken);

    public async Task<IReadOnlyList<GuestInvitation>> FindByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken) =>
        await database.Invitations
            .Where(invitation => invitation.NormalizedName == normalizedName)
            .Include(invitation => invitation.People)
            .ToListAsync(cancellationToken);

    public Task AddAsync(GuestInvitation invitation, CancellationToken cancellationToken) =>
        database.Invitations.AddAsync(invitation, cancellationToken).AsTask();

    public Task AddInviteeAsync(Invitee invitee, CancellationToken cancellationToken) =>
        database.Invitees.AddAsync(invitee, cancellationToken).AsTask();

    public void RemoveInvitee(Invitee invitee) => database.Invitees.Remove(invitee);

    public void RemoveInvitation(GuestInvitation invitation) => database.Invitations.Remove(invitation);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => database.SaveChangesAsync(cancellationToken);
}
