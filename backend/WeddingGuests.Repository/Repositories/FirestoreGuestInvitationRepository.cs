using Google.Cloud.Firestore;
using WeddingGuests.Application.Abstractions;
using WeddingGuests.Application.Contracts;
using WeddingGuests.Domain.Entities;
using WeddingGuests.Repository.Persistence;

namespace WeddingGuests.Repository.Repositories;

public sealed class FirestoreGuestInvitationRepository(FirestoreDb firestore) : IGuestInvitationRepository
{
    private const string InvitationsCollection = "guestInvitations";
    private const string CountersCollection = "system";
    private const string RsvpTokensCollection = "rsvpTokens";

    public async Task<IReadOnlyList<GuestInvitation>> ListAsync(CancellationToken cancellationToken)
    {
        var snapshot = await firestore.Collection(InvitationsCollection).GetSnapshotAsync(cancellationToken);
        return snapshot.Documents
            .Select(document => FromDocument(document.ConvertTo<InvitationDocument>()))
            .OrderBy(invitation => invitation.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public async Task<GuestInvitation?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var snapshot = await firestore.Collection(InvitationsCollection).Document(id.ToString()).GetSnapshotAsync(cancellationToken);
        return snapshot.Exists ? FromDocument(snapshot.ConvertTo<InvitationDocument>()) : null;
    }

    public async Task<GuestInvitation?> GetByRsvpTokenAsync(string token, CancellationToken cancellationToken)
    {
        var tokenSnapshot = await firestore.Collection(RsvpTokensCollection).Document(token).GetSnapshotAsync(cancellationToken);
        if (!tokenSnapshot.Exists) return null;

        var tokenDocument = tokenSnapshot.ConvertTo<RsvpTokenDocument>();
        return await GetByIdAsync(tokenDocument.InvitationId, cancellationToken);
    }

    public async Task<IReadOnlyList<GuestInvitation>> FindByNormalizedNameAsync(string normalizedName, CancellationToken cancellationToken)
    {
        var snapshot = await firestore.Collection(InvitationsCollection)
            .WhereEqualTo(nameof(InvitationDocument.NormalizedName), normalizedName)
            .GetSnapshotAsync(cancellationToken);
        return snapshot.Documents.Select(document => FromDocument(document.ConvertTo<InvitationDocument>())).ToArray();
    }

    public async Task AddAsync(GuestInvitation invitation, CancellationToken cancellationToken)
    {
        invitation.Id = await ReserveIdsAsync("guestInvitationCounter", 1, cancellationToken);
        foreach (var person in invitation.People) person.GuestInvitationId = invitation.Id;
        await SaveAsync(invitation, cancellationToken);
    }

    public async Task SaveAsync(GuestInvitation invitation, CancellationToken cancellationToken)
    {
        var peopleWithoutId = invitation.People.Where(person => person.Id == 0).ToArray();
        if (peopleWithoutId.Length > 0)
        {
            var firstPersonId = await ReserveIdsAsync("inviteeCounter", peopleWithoutId.Length, cancellationToken);
            for (var index = 0; index < peopleWithoutId.Length; index++)
            {
                peopleWithoutId[index].Id = firstPersonId + index;
            }
        }

        foreach (var person in invitation.People)
        {
            person.GuestInvitationId = invitation.Id;
        }

        var document = ToDocument(invitation);
        var invitationReference = firestore.Collection(InvitationsCollection).Document(invitation.Id.ToString());
        var tokenReference = firestore.Collection(RsvpTokensCollection).Document(invitation.RsvpToken);

        await firestore.RunTransactionAsync(async transaction =>
        {
            transaction.Set(invitationReference, document);
            transaction.Set(tokenReference, new RsvpTokenDocument { InvitationId = invitation.Id });
            return true;
        }, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(GuestInvitation invitation, CancellationToken cancellationToken)
    {
        var invitationReference = firestore.Collection(InvitationsCollection).Document(invitation.Id.ToString());
        var tokenReference = firestore.Collection(RsvpTokensCollection).Document(invitation.RsvpToken);
        await firestore.RunTransactionAsync(async transaction =>
        {
            transaction.Delete(invitationReference);
            transaction.Delete(tokenReference);
            return true;
        }, cancellationToken: cancellationToken);
    }

    public async Task<bool> SaveRsvpAsync(string token, IReadOnlyList<RsvpAnswerRequest> answers, CancellationToken cancellationToken)
    {
        var tokenReference = firestore.Collection(RsvpTokensCollection).Document(token);
        return await firestore.RunTransactionAsync(async transaction =>
        {
            var tokenSnapshot = await transaction.GetSnapshotAsync(tokenReference, cancellationToken);
            if (!tokenSnapshot.Exists) return false;

            var tokenDocument = tokenSnapshot.ConvertTo<RsvpTokenDocument>();
            var invitationReference = firestore.Collection(InvitationsCollection).Document(tokenDocument.InvitationId.ToString());
            var invitationSnapshot = await transaction.GetSnapshotAsync(invitationReference, cancellationToken);
            if (!invitationSnapshot.Exists) return false;

            var invitation = FromDocument(invitationSnapshot.ConvertTo<InvitationDocument>());
            var peopleById = invitation.People.ToDictionary(person => person.Id);
            foreach (var answer in answers)
            {
                if (!peopleById.TryGetValue(answer.InviteeId, out var person)) return false;
                if (person.Status != "a-confirmar" && person.Status != answer.Status) return false;
            }

            foreach (var answer in answers)
            {
                var person = peopleById[answer.InviteeId];
                if (person.Status == "a-confirmar") person.Status = answer.Status;
            }

            transaction.Set(invitationReference, ToDocument(invitation));
            return true;
        }, cancellationToken: cancellationToken);
    }

    private async Task<int> ReserveIdsAsync(string counterId, int count, CancellationToken cancellationToken)
    {
        var counterReference = firestore.Collection(CountersCollection).Document(counterId);
        return await firestore.RunTransactionAsync(async transaction =>
        {
            var snapshot = await transaction.GetSnapshotAsync(counterReference, cancellationToken);
            var lastId = snapshot.Exists ? snapshot.GetValue<int>(nameof(CounterDocument.LastId)) : 0;
            var firstId = checked(lastId + 1);
            transaction.Set(counterReference, new CounterDocument { LastId = checked(lastId + count) });
            return firstId;
        }, cancellationToken: cancellationToken);
    }

    private static InvitationDocument ToDocument(GuestInvitation invitation) => new()
    {
        Id = invitation.Id,
        RsvpToken = invitation.RsvpToken,
        Name = invitation.Name,
        NormalizedName = invitation.NormalizedName,
        GroupName = invitation.GroupName,
        WhatsApp = invitation.WhatsApp,
        AllowFreeCompanions = invitation.AllowFreeCompanions,
        FreeCompanionLimit = invitation.FreeCompanionLimit,
        People = invitation.People.Select(person => new InviteeDocument
        {
            Id = person.Id,
            GuestInvitationId = invitation.Id,
            Name = person.Name,
            NormalizedName = person.NormalizedName,
            IsPrimary = person.IsPrimary,
            IsFreeCompanion = person.IsFreeCompanion,
            Status = person.Status
        }).ToList()
    };

    private static GuestInvitation FromDocument(InvitationDocument document) => new()
    {
        Id = document.Id,
        RsvpToken = document.RsvpToken,
        Name = document.Name,
        NormalizedName = document.NormalizedName,
        GroupName = document.GroupName,
        WhatsApp = document.WhatsApp,
        AllowFreeCompanions = document.AllowFreeCompanions,
        FreeCompanionLimit = document.FreeCompanionLimit,
        People = document.People.Select(person => new Invitee
        {
            Id = person.Id,
            GuestInvitationId = document.Id,
            Name = person.Name,
            NormalizedName = person.NormalizedName,
            IsPrimary = person.IsPrimary,
            IsFreeCompanion = person.IsFreeCompanion,
            Status = person.Status
        }).ToList()
    };
}
