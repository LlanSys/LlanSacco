using LS.SharedKernel.Features.Shared.Common.Enums;
using System.Net;
using System.Net.Http.Headers;
using LS.SharedKernel.Dtos.Utilities;
using System.Net.Http.Json;
using System.Text.Json;
using LS.SharedKernel.Dtos.Common;
using LS.UI.Blazor.Features.Shared.BackendApi.Contracts.Interfaces;
using LS.UI.Blazor.Features.Shared.Messaging;
using LS.UI.Blazor.Logging;
using LS.UI.Rcl.Features.IAM.Users.Contracts.Interfaces;
using LS.UI.Blazor.Configuration;
using Microsoft.Extensions.Options;

namespace LS.UI.Blazor.Features.Shared.BackendApi.Contracts.Implementations;

internal sealed class BackendApiClient(
    HttpClient httpClient,
    ITokenStorage storage,
    IOptions<BackendApiSettings> apiSettings,
    ILogger<BackendApiClient> logger) : IBackendApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
    };

    private readonly BackendApiSettings _apiSettings = apiSettings.Value;

    public async Task<AppResponse<T>> SendAsync<T>(
        HttpMethod method,
        string endpoint,
        object? request = null,
        bool requiresAuthentication = true,
        string? unavailableMessage = null,
        string? timeoutMessage = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(method);
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);

        string? accessToken = null;
        string? sessionId = null;

        if (requiresAuthentication)
        {
            (accessToken, _, sessionId) = await storage.GetAsync().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return AppResponses.Failure<T>("Please sign in to continue.");
            }
        }

        var maxAttempts = Math.Max(1, _apiSettings.TransientRetryCount + 1);
        var delay = TimeSpan.FromMilliseconds(_apiSettings.TransientRetryDelayMilliseconds);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            using var message = CreateRequest(method, endpoint, request);

            if (requiresAuthentication)
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                if (!string.IsNullOrWhiteSpace(sessionId))
                {
                    message.Headers.TryAddWithoutValidation("X-Session-Id", sessionId);
                }
            }

            try
            {
                using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
                return await ReadAppResponseAsync<T>(response, endpoint, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException ex) when (ShouldRetryTransportFailure(attempt, maxAttempts, cancellationToken))
            {
                BackendApiLogDefinitions.LogRequestRetrying(logger, method.Method, endpoint, attempt, maxAttempts, (int)delay.TotalMilliseconds, ex);
                await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                BackendApiLogDefinitions.LogRequestFailed(logger, method.Method, endpoint, ex);
                return AppResponses.Failure<T>(unavailableMessage ?? "The backend service is unavailable. Please try again.");
            }
            catch (TaskCanceledException ex)
            {
                BackendApiLogDefinitions.LogRequestTimedOut(logger, method.Method, endpoint, ex);
                return AppResponses.Failure<T>(timeoutMessage ?? "The backend service timed out. Please try again.");
            }
        }

        return AppResponses.Failure<T>(unavailableMessage ?? "The backend service is unavailable. Please try again.");
    }

    public async Task<AppResponse<T>> SendMultipartAsync<T>(
        string endpoint,
        MultipartFormDataContent content,
        bool requiresAuthentication = true,
        string? unavailableMessage = null,
        string? timeoutMessage = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        ArgumentNullException.ThrowIfNull(content);

        using var message = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = content
        };

        if (requiresAuthentication)
        {
            var (accessToken, _, sessionId) = await storage.GetAsync().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return AppResponses.Failure<T>("Please sign in to continue.");
            }

            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                message.Headers.TryAddWithoutValidation("X-Session-Id", sessionId);
            }
        }

        try
        {
            using var response = await httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            return await ReadAppResponseAsync<T>(response, endpoint, cancellationToken).ConfigureAwait(false);
        }
        catch (HttpRequestException ex)
        {
            BackendApiLogDefinitions.LogRequestFailed(logger, HttpMethod.Post.Method, endpoint, ex);
            return AppResponses.Failure<T>(unavailableMessage ?? "The backend service is unavailable. Please try again.");
        }
        catch (TaskCanceledException ex)
        {
            BackendApiLogDefinitions.LogRequestTimedOut(logger, HttpMethod.Post.Method, endpoint, ex);
            return AppResponses.Failure<T>(timeoutMessage ?? "The backend service timed out. Please try again.");
        }
    }

    public async Task<AppResponse<FileContentResponse>> SendFileAsync(
        string endpoint,
        bool requiresAuthentication = true,
        string? unavailableMessage = null,
        string? timeoutMessage = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);

        using var message = new HttpRequestMessage(HttpMethod.Get, endpoint);

        if (requiresAuthentication)
        {
            var (accessToken, _, sessionId) = await storage.GetAsync().ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return AppResponses.Failure<FileContentResponse>("Please sign in to continue.");
            }

            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            if (!string.IsNullOrWhiteSpace(sessionId))
            {
                message.Headers.TryAddWithoutValidation("X-Session-Id", sessionId);
            }
        }

        try
        {
            using var response = await httpClient.SendAsync(
                message,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return await ReadAppResponseAsync<FileContentResponse>(response, endpoint, cancellationToken).ConfigureAwait(false);
            }

            var content = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            return AppResponses.Success("Profile picture loaded.", new FileContentResponse(content, contentType));
        }
        catch (HttpRequestException ex)
        {
            BackendApiLogDefinitions.LogRequestFailed(logger, HttpMethod.Get.Method, endpoint, ex);
            return AppResponses.Failure<FileContentResponse>(unavailableMessage ?? "The backend service is unavailable. Please try again.");
        }
        catch (TaskCanceledException ex)
        {
            BackendApiLogDefinitions.LogRequestTimedOut(logger, HttpMethod.Get.Method, endpoint, ex);
            return AppResponses.Failure<FileContentResponse>(timeoutMessage ?? "The backend service timed out. Please try again.");
        }
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, object? request)
        => new(method, endpoint)
        {
            Content = request is null ? null : JsonContent.Create(request, options: JsonOptions)
        };

    private static bool ShouldRetryTransportFailure(int attempt, int maxAttempts, CancellationToken cancellationToken)
        => attempt < maxAttempts && !cancellationToken.IsCancellationRequested;

    private const string RejectedMessage = "The request could not be completed. Please try again or contact support if the problem persists.";

    private async Task<AppResponse<T>> ReadAppResponseAsync<T>(
        HttpResponseMessage response, string endpoint, CancellationToken cancellationToken)
    {
        var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(content))
        {
            try
            {
                using var document = JsonDocument.Parse(content);
                var root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    var envelope = root.TryGetProperty("isSuccess", out var flag)
                        && flag.ValueKind is JsonValueKind.True or JsonValueKind.False
                        ? root.Deserialize<AppResponse<T>>(JsonOptions)
                        : null;

                    if (response.IsSuccessStatusCode && envelope is { IsSuccess: true })
                    {
                        return envelope with
                        {
                            Message = UserMessageSanitizer.NormalizeNullable(envelope.Message, "Operation completed.")
                        };
                    }

                    return ReadFailure(response.StatusCode, root, envelope);
                }
            }
            catch (JsonException ex)
            {
                BackendApiLogDefinitions.LogUnreadableResponse(logger, response.StatusCode.ToString(), endpoint, ex);
            }
        }

        return response.IsSuccessStatusCode
            ? AppResponses.Failure<T>(AppError.Unexpected()) with { Message = "The backend service returned an unreadable response." }
            : ReadFailure<T>(response.StatusCode, default, null);
    }

    private static AppResponse<T> ReadFailure<T>(HttpStatusCode status, JsonElement problem, AppResponse<T>? envelope)
    {
        var (type, defaultCode, fallback) = status switch
        {
            HttpStatusCode.BadRequest => (ErrorType.Validation, ErrorCodes.Validation, "Check the information you entered."),
            HttpStatusCode.Unauthorized => (ErrorType.Unauthorized, ErrorCodes.Unauthorized, "Please sign in to continue."),
            HttpStatusCode.Forbidden => (ErrorType.Forbidden, ErrorCodes.Forbidden, "You are not allowed to perform this action."),
            HttpStatusCode.NotFound => (ErrorType.NotFound, ErrorCodes.NotFound, "The requested item could not be found."),
            HttpStatusCode.Conflict => (ErrorType.Conflict, ErrorCodes.Conflict, "The request conflicts with the current state."),
            HttpStatusCode.UnprocessableEntity => (ErrorType.BusinessRule, ErrorCodes.BusinessRule, RejectedMessage),
            HttpStatusCode.BadGateway or HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout =>
                (ErrorType.DependencyUnavailable, ErrorCodes.DependencyUnavailable, "The backend service is temporarily unavailable."),
            _ => (ErrorType.Unexpected, ErrorCodes.Unexpected, RejectedMessage)
        };

        if ((int)status >= 500 || (int)status < 300 && envelope is not { IsSuccess: false })
        {
            return AppResponses.Failure<T>(new AppError(type, defaultCode, fallback)) with { Message = fallback };
        }

        var legacy = envelope is { IsSuccess: false } ? envelope.Error : null;
        var code = legacy?.Code ?? TryGetString(problem, "code") ?? defaultCode;
        if (status == HttpStatusCode.UnprocessableEntity && string.Equals(code, ErrorCodes.Validation, StringComparison.Ordinal))
        {
            type = ErrorType.Validation;
        }

        var errors = ReadValidationErrors(problem, legacy);
        var fields = errors is null ? null : string.Join("; ", errors.Values.SelectMany(values => values));
        var detail = !string.IsNullOrEmpty(fields) ? fields
            : envelope is { IsSuccess: false } ? legacy?.Message ?? envelope.Message
            : TryGetString(problem, "detail") ?? TryGetString(problem, "message") ?? TryGetString(problem, "title");

        if (string.Equals(code, "SESSION_INVALID", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(detail))
        {
            detail = "Your session has expired. Please sign in again.";
        }

        var message = UserMessageSanitizer.Normalize(detail, fallback);
        type = legacy?.Type ?? ((int)status < 300 ? ErrorType.BusinessRule : type);

        if ((int)status < 300 && legacy is null)
        {
            code = ErrorCodes.BusinessRule;
        }

        return AppResponses.Failure<T>(new AppError(type, code, message, errors)) with
        {
            Message = message,
            Data = envelope is { IsSuccess: false } ? envelope.Data : default
        };
    }

    private static IReadOnlyDictionary<string, string[]>? ReadValidationErrors(JsonElement problem, AppError? legacy)
    {
        var fields = new Dictionary<string, string[]>();

        if (problem.ValueKind == JsonValueKind.Object && problem.TryGetProperty("errors", out var errors))
        {
            if (errors.ValueKind == JsonValueKind.Object)
            {
                foreach (var field in errors.EnumerateObject())
                {
                    fields[field.Name] = ReadMessages(field.Value);
                }
            }
            else if (errors.ValueKind == JsonValueKind.Array)
            {
                fields[string.Empty] = ReadMessages(errors);
            }
        }

        if (legacy?.ValidationErrors is { } legacyFields)
        {
            foreach (var field in legacyFields)
            {
                fields[field.Key] = field.Value.Select(message => UserMessageSanitizer.Normalize(message, "Invalid value.")).ToArray();
            }
        }

        return fields.Count == 0 ? null : fields;
    }

    private static string[] ReadMessages(JsonElement element)
        => element.ValueKind != JsonValueKind.Array ? [] : element.EnumerateArray()
            .Where(value => value.ValueKind == JsonValueKind.String)
            .Select(value => UserMessageSanitizer.Normalize(value.GetString(), "Invalid value."))
            .ToArray();

    private static string? TryGetString(JsonElement element, string name)
        => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() : null;
}
