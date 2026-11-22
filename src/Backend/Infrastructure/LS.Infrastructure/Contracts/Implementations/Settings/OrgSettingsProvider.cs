using System.Text.Json;
using LS.Infrastructure.Logging;
using LS.Domain.Features.Shared.Contracts;
using LS.Application.Contracts.Interfaces.Common;
using LS.Domain.Shared.Contracts.Common;
using LS.Domain.Shared.Contracts.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LS.Infrastructure.Contracts.Implementations.Settings;

public class OrgSettingsProvider(
    ISharedUnitOfWork unitOfWork,
    ICurrentTenantProvider tenantProvider,
    IConfiguration configuration,
    IEncryptionService encryptionService,
    ILogger<OrgSettingsProvider> logger) : IOrgSettingsProvider
{
    private readonly ICurrentTenantProvider _tenantProvider = tenantProvider;
    private readonly IConfiguration _configuration = configuration;
    private readonly IEncryptionService _encryptionService = encryptionService;
    private readonly ILogger<OrgSettingsProvider> _logger = logger;

    public async Task<string?> GetSettingAsync(string key, CancellationToken ct = default)
    {
        var tenantId = _tenantProvider.TenantId;
        if (tenantId == Guid.Empty)
        {
            // If no tenant context, fallback to global appsettings
            return _configuration[key];
        }

        string? value;
        try
        {
            var setting = await unitOfWork.OrgSettingRepository.FirstOrDefaultAsync(
                candidate => candidate.TenantId == tenantId && candidate.Key == key, ct).ConfigureAwait(false);
            value = setting?.Value;
        }
        catch (Exception ex) when (!ct.IsCancellationRequested)
        {
            ServiceLogDefinitions.LogReadOrgSettingDatabaseError(_logger, ex);
            return _configuration[key];
        }
        if (string.IsNullOrEmpty(value))
        {
            return _configuration[key];
        }

        try
        {
            return _encryptionService.Decrypt(value);
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            // Older non-sensitive settings can be stored as plaintext.
            return value;
        }
        catch (FormatException)
        {
            return value;
        }
    }

    public async Task<T?> GetSettingAsync<T>(string key, CancellationToken ct = default)
    {
        var stringValue = await GetSettingAsync(key, ct).ConfigureAwait(false);
        if (string.IsNullOrEmpty(stringValue))
        {
            return default;
        }

        try
        {
            // If T is a primitive type or string, try to convert directly
            if (typeof(T) == typeof(string))
            {
                return (T)(object)stringValue;
            }

            if (typeof(T) == typeof(Guid))
            {
                return (T)(object)Guid.Parse(stringValue);
            }

            if (typeof(T).IsPrimitive || typeof(T) == typeof(decimal))
            {
                return (T)Convert.ChangeType(stringValue, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
            }

            // Otherwise assume JSON
            return JsonSerializer.Deserialize<T>(stringValue);
        }
        catch (Exception ex)
        {
            ServiceLogDefinitions.LogDeserializeOrgSettingError(_logger, key, typeof(T).Name, ex);
            return default;
        }
    }
}
