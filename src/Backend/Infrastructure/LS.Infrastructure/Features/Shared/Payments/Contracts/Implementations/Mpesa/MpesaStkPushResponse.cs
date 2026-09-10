using System.Text.Json.Serialization;

namespace LS.Infrastructure.Features.Shared.Payments.Contracts.Implementations.Mpesa;

internal sealed record MpesaStkPushResponse(
    [property: JsonPropertyName("CheckoutRequestID")] string? CheckoutRequestId,
    [property: JsonPropertyName("ResponseDescription")] string? ResponseDescription);
