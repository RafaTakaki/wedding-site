namespace WeddingGuests.Application.Exceptions;

public sealed class GuestApplicationException(string message, int statusCode) : Exception(message)
{
    public const int BadRequest = 400;
    public const int NotFound = 404;
    public const int Conflict = 409;

    public int StatusCode { get; } = statusCode;
}
