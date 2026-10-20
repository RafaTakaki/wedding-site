using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using WeddingGuests.Application.Abstractions;
using WeddingGuests.Application.Contracts;
using WeddingGuests.Application.Exceptions;
using WeddingGuests.Domain.Entities;

namespace WeddingGuests.Application.Services;

public sealed class GuestInvitationService(IGuestInvitationRepository repository)
{
    public async Task<IReadOnlyList<AdminInvitationResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var invitations = await repository.ListAsync(cancellationToken);
        return invitations.Select(ToAdminResponse).ToArray();
    }

    public async Task<AdminInvitationResponse> GetAsync(int invitationId, CancellationToken cancellationToken)
    {
        var invitation = await GetRequiredAsync(invitationId, cancellationToken);
        return ToAdminResponse(invitation);
    }

    public async Task<AdminInvitationResponse> CreateAsync(CreateInvitationRequest request, CancellationToken cancellationToken)
    {
        ValidateInvitation(request);
        var invitation = new GuestInvitation
        {
            RsvpToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant(),
            Name = request.Name.Trim(),
            NormalizedName = NormalizeName(request.Name),
            GroupName = CleanOptional(request.GroupName),
            WhatsApp = CleanOptional(request.WhatsApp),
            AllowFreeCompanions = request.AllowFreeCompanions,
            FreeCompanionLimit = request.FreeCompanionLimit,
            People =
            [
                new Invitee { Name = request.Name.Trim(), NormalizedName = NormalizeName(request.Name), IsPrimary = true },
                .. (request.Companions ?? []).Select(name => new Invitee
                {
                    Name = name.Trim(),
                    NormalizedName = NormalizeName(name)
                })
            ]
        };

        await repository.AddAsync(invitation, cancellationToken);
        return ToAdminResponse(invitation);
    }

    public async Task<AdminInvitationResponse> UpdateAsync(int invitationId, UpdateInvitationRequest request, CancellationToken cancellationToken)
    {
        var invitation = await GetRequiredAsync(invitationId, cancellationToken);
        if (request.Name is not null)
        {
            ValidateName(request.Name, "Informe um nome de até 120 caracteres.");
            invitation.Name = request.Name.Trim();
            invitation.NormalizedName = NormalizeName(request.Name);
            var primary = invitation.People.Single(person => person.IsPrimary);
            primary.Name = invitation.Name;
            primary.NormalizedName = invitation.NormalizedName;
        }

        if (request.GroupName is not null) invitation.GroupName = CleanOptional(request.GroupName);
        if (request.WhatsApp is not null) invitation.WhatsApp = CleanOptional(request.WhatsApp);
        if (request.AllowFreeCompanions.HasValue) invitation.AllowFreeCompanions = request.AllowFreeCompanions.Value;
        if (request.FreeCompanionLimit.HasValue)
        {
            if (request.FreeCompanionLimit is < 0 or > 5)
            {
                throw new GuestApplicationException("O limite deve estar entre 0 e 5.", GuestApplicationException.BadRequest);
            }

            invitation.FreeCompanionLimit = request.FreeCompanionLimit.Value;
        }

        if (!invitation.AllowFreeCompanions) invitation.FreeCompanionLimit = 0;
        await repository.SaveAsync(invitation, cancellationToken);
        return ToAdminResponse(invitation);
    }

    public async Task<AdminInvitationResponse> AddCompanionAsync(int invitationId, AddCompanionRequest request, CancellationToken cancellationToken)
    {
        ValidateName(request.Name, "Informe o nome do acompanhante, com até 120 caracteres.");
        var invitation = await GetRequiredAsync(invitationId, cancellationToken);
        var freeCompanionCount = invitation.People.Count(person => person.IsFreeCompanion);
        if (!invitation.AllowFreeCompanions || freeCompanionCount >= invitation.FreeCompanionLimit)
        {
            throw new GuestApplicationException("Este convite não permite adicionar mais acompanhantes.", GuestApplicationException.Conflict);
        }

        var companion = new Invitee
        {
            Name = request.Name.Trim(),
            NormalizedName = NormalizeName(request.Name),
            IsFreeCompanion = true,
            GuestInvitationId = invitation.Id
        };
        invitation.People.Add(companion);
        await repository.SaveAsync(invitation, cancellationToken);
        return ToAdminResponse(invitation);
    }

    public async Task DeleteCompanionAsync(int invitationId, int personId, CancellationToken cancellationToken)
    {
        var invitation = await repository.GetByIdAsync(invitationId, cancellationToken);
        var companion = invitation?.People.FirstOrDefault(person => person.Id == personId && !person.IsPrimary);
        if (companion is null) throw NotFound("Acompanhante não encontrado.");

        invitation!.People.Remove(companion);
        await repository.SaveAsync(invitation, cancellationToken);
    }

    public async Task DeleteAsync(int invitationId, CancellationToken cancellationToken)
    {
        var invitation = await repository.GetByIdAsync(invitationId, cancellationToken);
        if (invitation is null) throw NotFound("Convite não encontrado.");

        await repository.DeleteAsync(invitation, cancellationToken);
    }

    public async Task<PublicInvitationResponse> LookupAsync(
        string? name,
        DateTimeOffset deadline,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 120)
        {
            throw new GuestApplicationException("Informe o nome completo do titular do convite.", GuestApplicationException.BadRequest);
        }

        var matches = await repository.FindByNormalizedNameAsync(NormalizeName(name), cancellationToken);
        if (matches.Count == 0)
        {
            throw NotFound("Não encontramos seu nome. Entre em contato com os noivos ou com a acessora.");
        }

        if (matches.Count > 1)
        {
            throw new GuestApplicationException("Não foi possível identificar o convite. Entre em contato com os noivos ou com a acessora.", GuestApplicationException.Conflict);
        }

        var invitation = matches[0];
        return new PublicInvitationResponse(
            invitation.RsvpToken,
            invitation.Name,
            now <= deadline,
            invitation.People.OrderByDescending(person => person.IsPrimary)
                .ThenBy(person => person.Name)
                .Select(person => new PublicInviteeResponse(person.Id, person.Name, person.Status))
                .ToArray());
    }

    public async Task SaveRsvpAsync(
        string rsvpToken,
        SaveRsvpRequest request,
        DateTimeOffset deadline,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        if (now > deadline)
        {
            throw new GuestApplicationException("O prazo para confirmação terminou. Entre em contato com os noivos.", GuestApplicationException.Forbidden);
        }

        if (request.Answers.Count == 0 || request.Answers.Select(answer => answer.InviteeId).Distinct().Count() != request.Answers.Count)
        {
            throw new GuestApplicationException("Selecione respostas válidas para o convite.", GuestApplicationException.BadRequest);
        }

        var invitation = await repository.GetByRsvpTokenAsync(rsvpToken, cancellationToken)
            ?? throw NotFound("Convite não encontrado.");
        var peopleById = invitation.People.ToDictionary(person => person.Id);
        foreach (var answer in request.Answers)
        {
            if (!peopleById.TryGetValue(answer.InviteeId, out var person))
            {
                throw new GuestApplicationException("Uma pessoa informada não pertence a este convite.", GuestApplicationException.BadRequest);
            }

            if (answer.Status is not ("vou" or "nao-vou" or "a-confirmar"))
            {
                throw new GuestApplicationException("Resposta de presença inválida.", GuestApplicationException.BadRequest);
            }

            if (person.Status != "a-confirmar" && answer.Status != person.Status)
            {
                throw new GuestApplicationException("Respostas já confirmadas só podem ser alteradas pela administração.", GuestApplicationException.Conflict);
            }
        }

        if (!await repository.SaveRsvpAsync(rsvpToken, request.Answers, cancellationToken))
        {
            throw new GuestApplicationException("Uma ou mais respostas já foram registradas. Atualize o convite e tente novamente.", GuestApplicationException.Conflict);
        }
    }

    private async Task<GuestInvitation> GetRequiredAsync(int invitationId, CancellationToken cancellationToken)
    {
        return await repository.GetByIdAsync(invitationId, cancellationToken)
            ?? throw NotFound("Convite não encontrado.");
    }

    private static void ValidateInvitation(CreateInvitationRequest request)
    {
        ValidateName(request.Name, "Informe o nome do titular, com até 120 caracteres.");
        var companions = request.Companions ?? [];
        if (companions.Count > 20 || companions.Any(name => string.IsNullOrWhiteSpace(name) || name.Trim().Length > 120))
        {
            throw new GuestApplicationException("Informe até 20 acompanhantes, cada um com nome de até 120 caracteres.", GuestApplicationException.BadRequest);
        }

        if (request.FreeCompanionLimit is < 0 or > 5)
        {
            throw new GuestApplicationException("O limite de acompanhantes livres deve estar entre 0 e 5.", GuestApplicationException.BadRequest);
        }

        if (!request.AllowFreeCompanions && request.FreeCompanionLimit != 0)
        {
            throw new GuestApplicationException("O limite precisa ser zero quando acompanhantes livres não são permitidos.", GuestApplicationException.BadRequest);
        }

        if (!string.IsNullOrWhiteSpace(request.WhatsApp) && request.WhatsApp.Trim().Length > 30)
        {
            throw new GuestApplicationException("O telefone deve ter até 30 caracteres.", GuestApplicationException.BadRequest);
        }
    }

    private static void ValidateName(string name, string message)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Trim().Length > 120)
        {
            throw new GuestApplicationException(message, GuestApplicationException.BadRequest);
        }
    }

    private static string NormalizeName(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var withoutDiacritics = new string(decomposed
            .Where(character => CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            .ToArray());

        return string.Join(' ', withoutDiacritics
            .Normalize(NormalizationForm.FormC)
            .Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
            .ToLowerInvariant();
    }

    private static string? CleanOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static GuestApplicationException NotFound(string message) => new(message, GuestApplicationException.NotFound);

    private static AdminInvitationResponse ToAdminResponse(GuestInvitation invitation) => new(
        invitation.Id,
        invitation.Name,
        invitation.GroupName,
        invitation.WhatsApp,
        invitation.AllowFreeCompanions,
        invitation.FreeCompanionLimit,
        invitation.People.OrderByDescending(person => person.IsPrimary)
            .ThenBy(person => person.Name)
            .Select(person => new AdminInviteeResponse(person.Id, person.Name, person.IsPrimary, person.IsFreeCompanion, person.Status))
            .ToArray());
}
