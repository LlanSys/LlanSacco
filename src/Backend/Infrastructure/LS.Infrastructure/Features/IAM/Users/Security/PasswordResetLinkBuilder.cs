using LS.Infrastructure.Configuration;

namespace LS.Infrastructure.Features.IAM.Users.Security;

/// <summary>
/// Builds links to the Blazor reset-password page (<c>{EmailSettings:ClientBaseUrl}{PasswordRecovery:ResetPath}?email=..&amp;token=..</c>).
/// Shared by forgot-password links and activation set-password links so both land on the same page and API endpoint.
/// </summary>
internal static class PasswordResetLinkBuilder
{
    public static bool TryGetBaseUri(EmailSettings emailSettings, out Uri? baseUri)
    {
        ArgumentNullException.ThrowIfNull(emailSettings);

        baseUri = null;
        var clientBaseUrl = emailSettings.ClientBaseUrl?.Trim();

        if (string.IsNullOrWhiteSpace(clientBaseUrl)
            || !Uri.TryCreate(clientBaseUrl.TrimEnd('/') + "/", UriKind.Absolute, out var parsed)
            || (parsed.Scheme != Uri.UriSchemeHttps && parsed.Scheme != Uri.UriSchemeHttp))
        {
            return false;
        }

        baseUri = parsed;
        return true;
    }

    public static Uri Build(EmailSettings emailSettings, PasswordRecoverySettings recoverySettings, string email, string token)
    {
        ArgumentNullException.ThrowIfNull(recoverySettings);

        if (!TryGetBaseUri(emailSettings, out var baseUri))
        {
            throw new InvalidOperationException("EmailSettings:ClientBaseUrl must be an absolute http(s) URL.");
        }

        var resetUri = new Uri(baseUri!, recoverySettings.ResetPath.TrimStart('/'));
        var query = $"email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(token)}";
        return new UriBuilder(resetUri) { Query = query }.Uri;
    }
}
