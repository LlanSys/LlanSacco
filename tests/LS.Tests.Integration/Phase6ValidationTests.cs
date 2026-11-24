using System.Text.Json;
using FluentValidation;
using LS.Application.Extensions;
using LS.Application.Features.Banking.Shares.Commands;
using LS.Application.Features.IAM.Users.Commands;
using LS.Application.Features.Loans.Collections.Commands;
using LS.Application.Features.Shared.Payments.CommandHandlers.Mpesa;
using LS.Domain.Features.Banking.Contracts;
using LS.SharedKernel.Features.Accounting.Dtos;
using LS.SharedKernel.Features.Dividends.Dtos;
using LS.SharedKernel.Features.Loans.Dtos;
using LS.SharedKernel.Features.Shared.Common.Enums;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace LS.Tests.Integration;

public sealed class Phase6ValidationTests
{
    [Fact]
    public async Task Production_registration_resolves_all_command_validators_with_exact_no_input_exceptions()
    {
        var noInputCommands = new HashSet<Type>
        {
            typeof(GetCurrentUserCommand),
            typeof(LogoutCommand),
            typeof(RequestPasskeyLoginOptionsCommand),
            typeof(RequestPasskeyRegistrationOptionsCommand),
            typeof(ProcessPendingPromisesCommand),
            typeof(ScanForArrearsCommand),
            typeof(SendPromiseRemindersCommand),
            typeof(RegisterMpesaC2BUrlsCommand),
            typeof(LS.Application.Features.Banking.GuarantorReservations.Commands.ExpireGuarantorReservationsCommand)
        };

        await using var services = ApplicationServices().BuildServiceProvider();
        await using var scope = services.CreateAsyncScope();
        var assembly = typeof(CreateShareProductCommand).Assembly;
        var types = assembly.GetTypes();
        var commands = types.Where(type => type.Name.EndsWith("Command", StringComparison.Ordinal) &&
            type.GetInterfaces().Any(contract => contract.IsGenericType && contract.GetGenericTypeDefinition() == typeof(IRequest<>)))
            .ToArray();

        Assert.All(noInputCommands, command => Assert.Contains(command, commands));

        foreach (var command in commands.Where(command => !noInputCommands.Contains(command)))
        {
            var contract = typeof(IValidator<>).MakeGenericType(command);
            Assert.True(scope.ServiceProvider.GetServices(contract).Any(), $"Missing validator for {command.FullName}.");
        }

        foreach (var validator in types.Where(type => !type.IsAbstract && typeof(IValidator).IsAssignableFrom(type)))
        {
            var contract = Assert.Single(
                validator.GetInterfaces(),
                type => type.IsGenericType &&
                    type.GetGenericTypeDefinition() == typeof(IValidator<>));

            Assert.Contains(scope.ServiceProvider.GetServices(contract), instance => instance?.GetType() == validator);
        }
    }

    [Fact]
    public async Task Invalid_share_product_never_reaches_the_application_handler()
    {
        var unitOfWork = Substitute.For<IBankingUnitOfWork>();
        var services = ApplicationServices();
        services.AddSingleton(unitOfWork);
        await using var provider = services.BuildServiceProvider();
        await using var scope = provider.CreateAsyncScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.Send(new CreateShareProductCommand("Ordinary shares", "ORD", "", -10m, 1));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error?.Type);
        Assert.Contains(nameof(CreateShareProductCommand.PricePerShare), result.Error!.ValidationErrors!.Keys);
        Assert.Empty(unitOfWork.ReceivedCalls());
    }

    [Fact]
    public void Renamed_transport_models_keep_the_existing_json_property_names()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        const string journal = """
            {"referenceNumber":"J-001","description":"Opening journal","entries":[{"accountId":"0194f700-0000-7000-8000-000000000001","debit":100,"credit":0}]}
            """;

        var request = JsonSerializer.Deserialize<CreateJournalRequest>(journal, options);
        Assert.NotNull(request);
        Assert.Equal(100m, Assert.IsType<CreateJournalEntryRequest>(Assert.Single(request.Entries)).Debit);
        using var dividend = JsonDocument.Parse(JsonSerializer.Serialize(new DividendDeclarationResponse { FinancialYear = "2026" }, options));
        Assert.Equal("2026", dividend.RootElement.GetProperty("financialYear").GetString());
        var guarantor = JsonSerializer.Deserialize<LoanGuarantorRequest>("""{"guaranteedAmount":500}""", options);
        Assert.Equal(500m, guarantor!.GuaranteedAmount);
    }

    private static IServiceCollection ApplicationServices()
        => new ServiceCollection().AddLogging().AddApplicationServices(new ConfigurationBuilder().Build());
}
