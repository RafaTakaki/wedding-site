using System.Security.Cryptography;
using System.Text;

namespace WeddingGuests.Api;

public sealed class AdminKeyEndpointFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var configuredKey = context.HttpContext.RequestServices
            .GetRequiredService<IConfiguration>()["GUEST_ADMIN_KEY"];
        if (string.IsNullOrWhiteSpace(configuredKey))
        {
            return Results.Problem(
                title: "Administração indisponível",
                detail: "Configure a variável de ambiente GUEST_ADMIN_KEY.",
                statusCode: StatusCodes.Status503ServiceUnavailable);
        }

        var suppliedKey = context.HttpContext.Request.Headers["X-Admin-Key"].ToString();
        var expectedBytes = Encoding.UTF8.GetBytes(configuredKey);
        var suppliedBytes = Encoding.UTF8.GetBytes(suppliedKey);
        if (expectedBytes.Length != suppliedBytes.Length ||
            !CryptographicOperations.FixedTimeEquals(expectedBytes, suppliedBytes))
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}
