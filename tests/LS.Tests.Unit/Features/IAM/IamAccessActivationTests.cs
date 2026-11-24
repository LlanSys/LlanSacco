using LS.Application.Contracts.Interfaces.Common;
using LS.Application.Features.IAM.Users.Commands;
using LS.Application.Features.Shared.Notifications.Contracts.Interfaces;
using LS.Domain.Features.IAM.Contracts;
using LS.Domain.Features.IAM.Users.Entities;
using LS.Domain.Features.IAM.Users.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.Infrastructure.Configuration;
using LS.Infrastructure.Extensions;
using LS.Infrastructure.Features.IAM.AspNetCoreIdentity.CommandHandlers;
using LS.Infrastructure.Features.IAM.Users.Contracts.Implementations.Services;
using LS.Infrastructure.Features.IAM.Users.Security;
using LS.Persistence.Features.IAM.DataContext;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Extensions;
using LS.SharedKernel.Features.IAM.Users.Dtos;
using LS.SharedKernel.Features.Shared.Common.Enums;
using LS.SharedKernel.Features.Shared.Notifications.Dtos;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using MediatR;
using NSubstitute;
using System.Net;
using System.Text.RegularExpressions;

namespace LS.Tests.Unit.Features.IAM;

public sealed partial class IamAccessActivationTests : IAsyncDisposable
{
    private const string Staff = "staff-user-id";
    private const string KnownPassword = "Original-Password1!";
    private static readonly Guid TenantId = Guid.CreateVersion7();

    private readonly IEmailService _email = Substitute.For<IEmailService>();
    private readonly IIamUnitOfWork _uow = Substitute.For<IIamUnitOfWork>();
    private readonly List<SendEmailRequest> _sent = [];
    private readonly ServiceProvider _provider;
    private readonly AsyncServiceScope _scope;
    private bool _deliverEmails = true;

    public IamAccessActivationTests()
    {
        var databaseName = Guid.CreateVersion7().ToString();
        var services = new ServiceCollection().AddLogging();
        services.AddScoped(_ => new IamDBContext(
            new DbContextOptionsBuilder<IamDBContext>().UseInMemoryDatabase(databaseName).Options,
            new FixedTenantProvider(TenantId),
            new FixedActorProvider(Staff)));

        services.AddDataProtection().UseEphemeralDataProtectionProvider();
        services.AddIdentityCore<AppUser>(DependencyInjection.ConfigureIdentityOptions)
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<IamDBContext>()
            .AddDefaultTokenProviders()
            .AddTokenProvider<AccountActivationTokenProvider>(AccountActivationTokenProvider.ProviderName);

        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateAsyncScope();

        // The fake Unit of Work runs each transactional callback once (the InMemory provider has no transactions).
        _uow.ExecuteInTransactionWithRetryAsync(
            Arg.Any<Func<Task<AppResponse<bool>>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>(), Arg.Any<Func<AppResponse<bool>, bool>?>())
            .Returns(call => call.Arg<Func<Task<AppResponse<bool>>>>()());

        _uow.ExecuteInTransactionWithRetryAsync(
            Arg.Any<Func<Task<bool>>>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>(), Arg.Any<Func<bool, bool>?>())
            .Returns(call => call.Arg<Func<Task<bool>>>()());

        _email.SendEmailAsync(Arg.Any<SendEmailRequest>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                if (!_deliverEmails)
                {
                    return AppResponses.Failure<SendEmailResponse>(AppError.DependencyUnavailable("Email provider rejected the message."));
                }

                var request = call.Arg<SendEmailRequest>();
                _sent.Add(request);
                return AppResponses.Success(new SendEmailResponse("message-id", DateTimeOffset.UtcNow, request.To ?? string.Empty, request.Subject ?? string.Empty));
            });
    }

    private UserManager<AppUser> Users => _scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    [Fact]
    public async Task Activation_email_contains_a_single_use_set_password_link_and_no_password()
    {
        var user = await SeedMemberUserAsync(isActive: false);

        var result = await GrantAsync(user);

        Assert.True(result.IsSuccess, result.Message);
        var email = Assert.Single(_sent);
        Assert.Equal(user.Email, email.To);
        Assert.DoesNotContain("password:", email.Body, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(KnownPassword, email.Body, StringComparison.Ordinal);
        Assert.Contains("Set your password", email.Body, StringComparison.Ordinal);
        Assert.Contains("expires in 72 hours", email.Body, StringComparison.Ordinal);

        var link = new Uri(WebUtility.HtmlDecode(LinkPattern().Match(email.Body!).Groups["href"].Value));
        Assert.Equal("https://ui.example.test/iam/reset-password", link.GetLeftPart(UriPartial.Path));
        var query = QueryHelpers.ParseQuery(link.Query);
        Assert.Equal(user.Email, query["email"].ToString());
        var token = query["token"].ToString();

        var activated = (await Users.FindByIdAsync(user.Id))!;
        Assert.True(activated.IsActive);
        Assert.True(activated.RequirePasswordChange);
        Assert.True(await Users.VerifyUserTokenAsync(
            activated, AccountActivationTokenProvider.ProviderName, UserManager<AppUser>.ResetPasswordTokenPurpose, token));

        // Setting the password rotates the security stamp, so the same link cannot be used again.
        var resetToken = await Users.GeneratePasswordResetTokenAsync(activated);
        Assert.True((await Users.ResetPasswordAsync(activated, resetToken, "Chosen-By-User1!")).Succeeded);
        Assert.False(await Users.VerifyUserTokenAsync(
            activated, AccountActivationTokenProvider.ProviderName, UserManager<AppUser>.ResetPasswordTokenPurpose, token));
    }

    [Fact]
    public async Task Reset_password_flow_accepts_the_activation_link_once_and_clears_the_password_change_flag()
    {
        var user = await SeedMemberUserAsync(isActive: false);
        Assert.True((await GrantAsync(user)).IsSuccess);
        var link = new Uri(WebUtility.HtmlDecode(LinkPattern().Match(Assert.Single(_sent).Body!).Groups["href"].Value));
        var token = QueryHelpers.ParseQuery(link.Query)["token"].ToString();
        _uow.TokenRepository.GetActiveTokensByUserIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns([]);

        var handler = new ResetPassword(
            Users,
            _uow,
            Substitute.For<ICacheService>(),
            Substitute.For<IHttpContextAccessor>(),
            Substitute.For<IPublisher>(),
            NullLogger<ResetPassword>.Instance);

        var command = new ResetPasswordCommand(new ResetPasswordRequest(user.Email!, "Chosen-By-User1!", null, "Chosen-By-User1!", token));

        var first = await handler.Handle(command, CancellationToken.None);

        Assert.True(first.IsSuccess, first.Message);
        var updated = (await Users.FindByIdAsync(user.Id))!;
        Assert.False(updated.RequirePasswordChange);
        Assert.NotNull(updated.PasswordLastChanged);
        Assert.True(await Users.CheckPasswordAsync(updated, "Chosen-By-User1!"));

        var replay = await handler.Handle(
            new ResetPasswordCommand(new ResetPasswordRequest(user.Email!, "Another-Choice2!", null, "Another-Choice2!", token)),
            CancellationToken.None);

        Assert.False(replay.IsSuccess);
        Assert.True(await Users.CheckPasswordAsync((await Users.FindByIdAsync(user.Id))!, "Chosen-By-User1!"));
    }

    [Fact]
    public async Task Change_password_clears_the_password_change_flag_only_when_it_succeeds()
    {
        var user = await SeedMemberUserAsync(isActive: true);
        var flagged = (await Users.FindByIdAsync(user.Id))!;
        flagged.RequirePasswordChange = true;
        Assert.True((await Users.UpdateAsync(flagged)).Succeeded);
        var handler = new ChangePasswordCommandHandler(Users, new FixedActorProvider(user.Id), NullLogger<ChangePasswordCommandHandler>.Instance);

        var wrong = await handler.Handle(new ChangePasswordCommand(new ChangePasswordRequest("Wrong-Password1!", "Chosen-By-User1!")), CancellationToken.None);
        Assert.False(wrong.IsSuccess);

        var changed = await handler.Handle(new ChangePasswordCommand(new ChangePasswordRequest(KnownPassword, "Chosen-By-User1!")), CancellationToken.None);

        Assert.True(changed.IsSuccess, changed.Message);
        var updated = (await Users.FindByIdAsync(user.Id))!;
        Assert.False(updated.RequirePasswordChange);
        Assert.NotNull(updated.PasswordLastChanged);
    }

    [Fact]
    public async Task Activated_accounts_cannot_sign_in_with_a_previous_or_shared_password()
    {
        var first = await SeedMemberUserAsync(isActive: false);
        var second = await SeedMemberUserAsync(isActive: false);

        Assert.True((await GrantAsync(first)).IsSuccess);
        Assert.True((await GrantAsync(second)).IsSuccess);

        var firstUser = (await Users.FindByIdAsync(first.Id))!;
        var secondUser = (await Users.FindByIdAsync(second.Id))!;

        Assert.False(await Users.CheckPasswordAsync(firstUser, KnownPassword));
        Assert.False(await Users.CheckPasswordAsync(secondUser, KnownPassword));
        Assert.NotNull(firstUser.PasswordHash);
        Assert.NotEqual(firstUser.PasswordHash, secondUser.PasswordHash);
    }

    [Fact]
    public async Task Unusable_passwords_are_random_and_satisfy_the_password_policy()
    {
        var passwords = Enumerable.Range(0, 20).Select(_ => IamAccessActivator.CreateUnusablePassword()).ToList();

        Assert.Equal(passwords.Count, passwords.Distinct(StringComparer.Ordinal).Count());
        var user = AppUser.CreateSystemUser(TenantId, "policy-check", "policy@example.test", Staff);

        foreach (var validator in Users.PasswordValidators)
        {
            Assert.True((await validator.ValidateAsync(Users, user, passwords[0])).Succeeded);
        }
    }

    [Fact]
    public async Task Failed_email_rolls_back_member_activation_and_restores_email_confirmation()
    {
        var user = await SeedMemberUserAsync(isActive: false);
        _deliverEmails = false;

        var result = await GrantAsync(user);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.DependencyUnavailable, result.Error?.Type);
        var reverted = (await Users.FindByIdAsync(user.Id))!;
        Assert.False(reverted.IsActive);
        Assert.False(reverted.EmailConfirmed);
        Assert.False(await Users.IsInRoleAsync(reverted, Roles.Customer.ToDisplayString()));
    }

    [Fact]
    public async Task Already_active_member_is_idempotent_and_is_not_emailed()
    {
        var user = await SeedMemberUserAsync(isActive: true);
        var hashBefore = user.PasswordHash;

        var result = await GrantAsync(user);

        Assert.True(result.IsSuccess);
        Assert.Empty(_sent);
        Assert.Equal(hashBefore, (await Users.FindByIdAsync(user.Id))!.PasswordHash);
    }

    [Fact]
    public async Task Activation_is_refused_when_no_link_base_url_is_configured()
    {
        var user = await SeedMemberUserAsync(isActive: false);

        var result = await GrantAsync(user, clientBaseUrl: string.Empty);

        Assert.Equal(ErrorType.DependencyUnavailable, result.Error?.Type);
        Assert.Empty(_sent);
        Assert.False((await Users.FindByIdAsync(user.Id))!.IsActive);
    }

    public async ValueTask DisposeAsync()
    {
        await _scope.DisposeAsync();
        await _provider.DisposeAsync();
    }

    private Task<AppResponse<bool>> GrantAsync(AppUser user, string clientBaseUrl = "https://ui.example.test")
    {
        var userManager = Users;
        var activator = new IamAccessActivator(
            userManager,
            _uow,
            _email,
            Options.Create(new IamProvisioningSettings { SignInUrl = "https://ui.example.test/iam/sign-in" }),
            Options.Create(new EmailSettings { ClientBaseUrl = clientBaseUrl }),
            Options.Create(new PasswordRecoverySettings()));

        var service = new MemberPortalAccessService(userManager, _uow, activator, NullLogger<MemberPortalAccessService>.Instance);
        return service.GrantPortalAccessAsync(Guid.Parse(user.Id), user.MemberId!.Value, TenantId, Staff, CancellationToken.None);
    }

    private async Task<AppUser> SeedMemberUserAsync(bool isActive)
    {
        var roles = _scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var roleName = Roles.Customer.ToDisplayString();

        if (!await roles.RoleExistsAsync(roleName))
        {
            Assert.True((await roles.CreateAsync(new AppRole { Name = roleName })).Succeeded);
        }

        var suffix = Guid.CreateVersion7().ToString("N");
        var user = AppUser.CreateForMember(
            TenantId, Guid.CreateVersion7(), $"member-{suffix}", "Test", "Member", $"{suffix}@example.test", "+254700000000", "12345678", Staff);

        if (isActive)
        {
            user.GrantAccess(Staff, [roleName]);
            user.RequirePasswordChange = false;
        }

        Assert.True((await Users.CreateAsync(user, KnownPassword)).Succeeded);

        if (isActive)
        {
            Assert.True((await Users.AddToRoleAsync(user, roleName)).Succeeded);
        }

        return user;
    }

    [GeneratedRegex("href=\"(?<href>[^\"]*reset-password[^\"]*)\"")]
    private static partial Regex LinkPattern();

    private sealed class FixedTenantProvider(Guid tenantId) : ICurrentTenantProvider
    {
        public Guid TenantId { get; } = tenantId;
    }

    private sealed class FixedActorProvider(string actorId) : ICurrentActorProvider
    {
        public string ActorId { get; } = actorId;
    }
}
