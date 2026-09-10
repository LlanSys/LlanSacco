namespace LS.Api.Features.ControlPlane.Tenants.Dtos;

public record CompleteTenantProvisioningRequest(
    string DatabaseConnectionString,
    string ApplicationInsightsKey);
