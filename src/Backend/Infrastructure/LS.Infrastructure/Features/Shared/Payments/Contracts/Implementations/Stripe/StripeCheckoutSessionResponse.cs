using System.Text.Json.Serialization;

namespace LS.Infrastructure.Features.Shared.Payments.Contracts.Implementations.Stripe;

internal sealed record StripeCheckoutSessionResponse(
    [property: JsonPropertyName("id")] string? Id,
    [property: JsonPropertyName("url")] string? Url,
    [property: JsonPropertyName("status")] string? Status);
