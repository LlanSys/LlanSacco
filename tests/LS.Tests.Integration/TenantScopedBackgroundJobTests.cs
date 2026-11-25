using System.Linq.Expressions;
using LS.Domain.Features.ControlPlane.Contracts;
using LS.Domain.Features.ControlPlane.Tenants.Contracts.Repositories;
using LS.Domain.Features.ControlPlane.Tenants.Entities;
using LS.Domain.Features.ControlPlane.Tenants.Enums;
using LS.Domain.Shared.Contracts.Common;
using LS.Infrastructure.Contracts.Implementations.Common;
using LS.Infrastructure.Jobs;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Quartz;

namespace LS.Tests.Integration;

/// <summary>
/// A queued job carries the tenant and actor that queued it, and runs only in that tenant while it is Active.
/// </summary>
public sealed class TenantScopedBackgroundJobTests
{
    [Fact]
    public async Task Queued_request_runs_in_the_queuing_tenant_and_actor()
    {
        var tenant = Tenant(TenantStatus.Active);
        var job = Queue(new SampleCommand("payload"), tenant.Id, "officer-1");
        var sender = new CapturingSender();

        await Run(job, sender, tenant);

        Assert.Equal(tenant.Id, sender.TenantId);
        Assert.Equal("officer-1", sender.ActorId);
        Assert.Equal("payload", Assert.IsType<SampleCommand>(sender.Request).Value);
    }

    [Fact]
    public async Task Job_for_an_inactive_tenant_is_refused()
    {
        var tenant = Tenant(TenantStatus.Suspended);
        var sender = new CapturingSender();

        await Assert.ThrowsAsync<JobExecutionException>(() => Run(Queue(new SampleCommand("x"), tenant.Id, "officer-1"), sender, tenant));
        Assert.Null(sender.Request);
    }

    [Fact]
    public async Task Job_without_a_tenant_is_refused()
    {
        var tenant = Tenant(TenantStatus.Active);
        var job = Queue(new SampleCommand("x"), tenant.Id, "officer-1");
        job.JobDataMap.Remove(MediatorSerializedJob.TenantIdKey);
        var sender = new CapturingSender();

        await Assert.ThrowsAsync<JobExecutionException>(() => Run(job, sender, tenant));
        Assert.Null(sender.Request);
    }

    private static IJobDetail Queue(IRequest request, Guid tenantId, string actorId)
    {
        var tenantProvider = Substitute.For<ICurrentTenantProvider>();
        tenantProvider.TenantId.Returns(tenantId);
        var actorProvider = Substitute.For<ICurrentActorProvider>();
        actorProvider.ActorId.Returns(actorId);
        var service = new BackgroundJobService(Substitute.For<ISchedulerFactory>(), NullLogger<BackgroundJobService>.Instance, tenantProvider, actorProvider);
        return service.BuildJob(request);
    }

    private static async Task Run(IJobDetail job, CapturingSender sender, params Tenant[] tenants)
    {
        var repository = Substitute.For<ITenantRepository>();
        repository.AnyAsync(Arg.Any<Expression<Func<Tenant, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(call => Task.FromResult(tenants.AsQueryable().Any(call.Arg<Expression<Func<Tenant, bool>>>())));
        var unitOfWork = Substitute.For<IControlPlaneUnitOfWork>();
        unitOfWork.TenantRepository.Returns(repository);

        var services = new ServiceCollection();
        services.AddScoped<BackgroundExecutionContext>();
        services.AddScoped(_ => unitOfWork);
        services.AddScoped<ISender>(provider => { sender.Context = provider.GetRequiredService<BackgroundExecutionContext>(); return sender; });
        await using var provider = services.BuildServiceProvider();

        var context = Substitute.For<IJobExecutionContext>();
        context.MergedJobDataMap.Returns(job.JobDataMap);
        await new MediatorSerializedJob(provider).Execute(context, CancellationToken.None);
    }

    private static Tenant Tenant(TenantStatus status) => new()
    {
        Identifier = "jobs",
        DisplayName = "Jobs",
        HostName = "jobs.example.test",
        CreatedBy = "job-test",
        Status = status,
    };

    public sealed record SampleCommand(string Value) : IRequest;

    private sealed class CapturingSender : ISender
    {
        public BackgroundExecutionContext? Context { get; set; }
        public object? Request { get; private set; }
        public Guid? TenantId { get; private set; }
        public string? ActorId { get; private set; }

        public Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
        {
            Request = request;
            TenantId = Context?.TenantId;
            ActorId = Context?.ActorId;
            return Task.CompletedTask;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<object?> Send(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default) => throw new NotSupportedException();
    }
}
