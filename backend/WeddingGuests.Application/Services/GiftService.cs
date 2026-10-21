using WeddingGuests.Application.Abstractions;
using WeddingGuests.Application.Contracts;
using WeddingGuests.Application.Exceptions;
using WeddingGuests.Domain.Entities;

namespace WeddingGuests.Application.Services;

public sealed class GiftService(IGiftRepository repository)
{
    public async Task<IReadOnlyList<GiftResponse>> ListAsync(CancellationToken cancellationToken)
    {
        var gifts = await repository.ListAsync(cancellationToken);
        return gifts.Select(ToResponse).ToArray();
    }

    public async Task<IReadOnlyList<PublicGiftResponse>> ListPublicAsync(CancellationToken cancellationToken)
    {
        var gifts = await repository.ListAsync(cancellationToken);
        return gifts.Select(gift => new PublicGiftResponse(
            gift.Id,
            gift.Name,
            gift.Description,
            gift.AmountInCents,
            gift.ImageUrl,
            gift.Status)).ToArray();
    }

    public async Task<PublicGiftResponse> GetPublicAsync(int id, CancellationToken cancellationToken)
    {
        var gift = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new GuestApplicationException("Presente não encontrado.", GuestApplicationException.NotFound);
        return new PublicGiftResponse(gift.Id, gift.Name, gift.Description, gift.AmountInCents, gift.ImageUrl, gift.Status);
    }

    public async Task<GiftResponse> CreateAsync(CreateGiftRequest request, CancellationToken cancellationToken)
    {
        Validate(request);
        var gift = new GiftItem
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            AmountInCents = request.AmountInCents,
            ImageUrl = CleanImageUrl(request.ImageUrl)
        };

        await repository.AddAsync(gift, cancellationToken);
        return ToResponse(gift);
    }

    public async Task<PublicGiftResponse> ClaimAsync(int id, ClaimGiftRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.GuestName) || request.GuestName.Trim().Length > 120)
        {
            throw new GuestApplicationException("Informe seu nome para registrar a escolha do presente.", GuestApplicationException.BadRequest);
        }

        var claimed = await repository.ClaimAsync(id, request.GuestName.Trim(), DateTimeOffset.UtcNow, cancellationToken);
        if (!claimed)
        {
            var gift = await repository.GetByIdAsync(id, cancellationToken);
            if (gift is null) throw new GuestApplicationException("Presente não encontrado.", GuestApplicationException.NotFound);
            throw new GuestApplicationException("Este presente já foi escolhido. Atualize a lista para ver as opções disponíveis.", GuestApplicationException.Conflict);
        }

        var updatedGift = await repository.GetByIdAsync(id, cancellationToken)
            ?? throw new GuestApplicationException("Presente não encontrado após registrar a escolha.", GuestApplicationException.NotFound);
        return new PublicGiftResponse(
            updatedGift.Id,
            updatedGift.Name,
            updatedGift.Description,
            updatedGift.AmountInCents,
            updatedGift.ImageUrl,
            updatedGift.Status);
    }

    public async Task ReleaseAsync(int id, CancellationToken cancellationToken)
    {
        var released = await repository.ReleaseAsync(id, cancellationToken);
        if (!released)
        {
            throw new GuestApplicationException("Presente não encontrado.", GuestApplicationException.NotFound);
        }
    }

    private static void Validate(CreateGiftRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name) || request.Name.Trim().Length > 120)
        {
            throw new GuestApplicationException("Informe o nome do presente, com até 120 caracteres.", GuestApplicationException.BadRequest);
        }

        if (string.IsNullOrWhiteSpace(request.Description) || request.Description.Trim().Length > 500)
        {
            throw new GuestApplicationException("Informe uma descrição de até 500 caracteres.", GuestApplicationException.BadRequest);
        }

        if (request.AmountInCents <= 0 || request.AmountInCents > 100_000_000)
        {
            throw new GuestApplicationException("O valor simbólico deve estar entre R$ 0,01 e R$ 1.000.000,00.", GuestApplicationException.BadRequest);
        }

        if (!string.IsNullOrWhiteSpace(request.ImageUrl) &&
            (!Uri.TryCreate(request.ImageUrl, UriKind.Absolute, out var imageUri) || imageUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new GuestApplicationException("A imagem deve usar uma URL HTTPS válida.", GuestApplicationException.BadRequest);
        }
    }

    private static string? CleanImageUrl(string? imageUrl) => string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl.Trim();

    private static GiftResponse ToResponse(GiftItem gift) => new(
        gift.Id,
        gift.Name,
        gift.Description,
        gift.AmountInCents,
        gift.ImageUrl,
        gift.Status,
        gift.ClaimedBy,
        gift.ClaimedAt);
}
