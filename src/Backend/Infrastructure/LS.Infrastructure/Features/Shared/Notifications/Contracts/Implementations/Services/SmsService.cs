using LS.Application.Features.IAM.Users.Contracts.Interfaces;
using LS.Application.Features.Shared.Notifications.Contracts.Interfaces;
using LS.Infrastructure.Configuration;
using LS.Infrastructure.Contracts.Interfaces;
using LS.Infrastructure.Logging;
using LS.SharedKernel.Dtos.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using Twilio.Rest.Api.V2010.Account;

namespace LS.Infrastructure.Features.Shared.Notifications.Contracts.Implementations.Services;

internal sealed partial class SmsService(
    IOptions<SmsSettings> smsSettings,
    IApiService apiService,
    IHttpClientFactory httpClientFactory,
    ILogger<SmsService> logger

) : ISmsService
{
    private readonly SmsSettings _smsSettings = smsSettings.Value;
    private readonly IApiService _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    private readonly ILogger<SmsService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public async Task<AppResponse<bool>> TwilioSendAsync(string phoneNumber, string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return AppResponses.Failure<bool>("Phone number is required.");
            }

            var formattedNumber = StandardizePhoneNumber(phoneNumber);
            var twilioConfig = _smsSettings.Twilio;

            // Twilio's MessageResource.CreateAsync returns a MessageResource object
            var result = await MessageResource.CreateAsync(
                body: message,
                from: new Twilio.Types.PhoneNumber(twilioConfig.FromNumber),
                to: new Twilio.Types.PhoneNumber(formattedNumber)

            ).ConfigureAwait(false);

            if (result.ErrorCode is int errorCode)
            {
                LogSmsRejected(_logger, errorCode);
                return AppResponses.Failure<bool>(AppError.DependencyUnavailable("The SMS provider could not deliver the message."));
            }
            return AppResponses.Success("SMS queued successfully", true);
        }
        catch (Exception ex)
        {
            HttpClientLogDefinitions.LogExternalApiError(_logger, "Twilio API Send", "N/A", ex);
            throw;
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "SMS provider rejected delivery with code {ErrorCode}")]
    private static partial void LogSmsRejected(ILogger logger, int errorCode);

    private static string StandardizePhoneNumber(string number)
    {
        // Kenyan numbers:
        return !number.StartsWith('0')
            ? number.StartsWith("+254", StringComparison.Ordinal)
                ? number
                : throw new ArgumentException("Invalid phone number format. Expected 07... or +254...", nameof(number))
            : string.Concat("+254", number.AsSpan(1).ToString());
    }


}
