extern alias Blazor;

using LS.Api.Common.Controllers;
using LS.Api.Features.CheckOff.Controllers;
using LS.Api.Middleware;
using LS.Application.Features.CheckOff.Commands.Batches;
using LS.SharedKernel.Dtos.Common;
using LS.SharedKernel.Dtos.Utilities;
using LS.SharedKernel.Features.Shared.Common.Enums;
using Blazor::LS.UI.Blazor.Configuration;
using Blazor::LS.UI.Blazor.Features.Shared.BackendApi.Contracts.Implementations;
using LS.UI.Rcl.Features.IAM.Users.Contracts.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace LS.Tests.Integration;

public sealed class Phase4ResponseContractTests
{
    [Theory]
    [InlineData(ErrorType.Validation, HttpStatusCode.BadRequest, ErrorCodes.Validation)]
    [InlineData(ErrorType.NotFound, HttpStatusCode.NotFound, ErrorCodes.NotFound)]
    [InlineData(ErrorType.Forbidden, HttpStatusCode.Forbidden, ErrorCodes.Forbidden)]
    [InlineData(ErrorType.Conflict, HttpStatusCode.Conflict, ErrorCodes.Conflict)]
    [InlineData(ErrorType.BusinessRule, HttpStatusCode.UnprocessableEntity, ErrorCodes.BusinessRule)]
    public async Task Problem_details_keep_the_HTTP_status_code_and_typed_client_error(
        ErrorType type, HttpStatusCode status, string code)
    {
        var error = type switch
        {
            ErrorType.Validation => AppError.Validation(new Dictionary<string, string[]> { ["Entries"] = ["Provide a debit or a credit."] }),
            ErrorType.NotFound => AppError.NotFound("Batch not found."),
            ErrorType.Forbidden => AppError.Forbidden("A tenant is required."),
            ErrorType.Conflict => AppError.Conflict("The reference is already in use."),
            _ => AppError.BusinessRule("Debits and credits must balance.")
        };

        await using var server = await CreateHostAsync(context => Task.FromResult<IActionResult>(
            new ContractController { ControllerContext = new() { HttpContext = context } }
                .Map(AppResponses.Failure<bool>(error))));

        using var client = server.GetTestClient();
        using var http = await client.GetAsync("/", CancellationToken.None);
        Assert.Equal(status, http.StatusCode);
        Assert.Equal("application/problem+json", http.Content.Headers.ContentType?.MediaType);
        var problem = await http.Content.ReadFromJsonAsync<JsonElement>(CancellationToken.None);
        Assert.Equal(code, problem.GetProperty("code").GetString());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("traceId").GetString()));

        var result = await Backend(client).SendAsync<bool>(HttpMethod.Get, "/", requiresAuthentication: false);
        Assert.False(result.IsSuccess);
        Assert.Equal(type, result.Error?.Type);
        Assert.Equal(code, result.Error?.Code);

        if (type == ErrorType.Validation)
        {
            Assert.Equal("Provide a debit or a credit.", result.Message);
            Assert.Equal(["Provide a debit or a credit."], result.Error!.ValidationErrors!["Entries"]);
        }
        else
        {
            Assert.Equal(error.Message, result.Message);
        }
    }

    [Fact]
    public async Task Rejected_CheckOff_rows_are_a_successful_report_with_false_data()
    {
        var mediator = Substitute.For<IMediator>();
        mediator.Send(Arg.Any<ValidateCheckoffBatchCommand>(), Arg.Any<CancellationToken>())
            .Returns(AppResponses.Success("Batch validation completed. 2 invalid rows.", false));

        await using var server = await CreateHostAsync(context =>
            new CheckOffController(mediator) { ControllerContext = new() { HttpContext = context } }
                .ValidateBatch(Guid.CreateVersion7()));

        using var client = server.GetTestClient();
        using var http = await client.PostAsync("/", null, CancellationToken.None);
        Assert.Equal(HttpStatusCode.OK, http.StatusCode);
        var body = await http.Content.ReadFromJsonAsync<JsonElement>(CancellationToken.None);
        Assert.True(body.GetProperty("isSuccess").GetBoolean());
        Assert.False(body.GetProperty("data").GetBoolean());
        Assert.False(body.TryGetProperty("error", out _));

        var result = await Backend(client).SendAsync<bool>(HttpMethod.Post, "/", requiresAuthentication: false);
        Assert.True(result.IsSuccess);
        Assert.False(result.Data);
        Assert.Null(result.Error);
        Assert.Contains("2 invalid rows", result.Message);
    }

    [Fact]
    public async Task Unexpected_failures_keep_provider_details_out_of_the_API_and_client()
    {
        const string privateDetail = "Internal provider exception with private diagnostic information.";
        await using var server = await CreateHostAsync(_ => throw new InvalidOperationException(privateDetail));
        using var client = server.GetTestClient();
        using var http = await client.GetAsync("/", CancellationToken.None);
        Assert.Equal(HttpStatusCode.InternalServerError, http.StatusCode);
        Assert.Equal("application/problem+json", http.Content.Headers.ContentType?.MediaType);
        var body = await http.Content.ReadAsStringAsync(CancellationToken.None);
        Assert.DoesNotContain(privateDetail, body);
        using var problem = JsonDocument.Parse(body);
        Assert.Equal(ErrorCodes.Unexpected, problem.RootElement.GetProperty("code").GetString());

        var result = await Backend(client).SendAsync<bool>(HttpMethod.Get, "/", requiresAuthentication: false);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unexpected, result.Error?.Type);
        Assert.DoesNotContain(privateDetail, result.Message);
    }

    [Fact]
    public async Task Data_annotations_failures_keep_their_validation_type_with_HTTP_422()
    {
        await using var server = await CreateHostAsync(_ => throw new ValidationException(
            new ValidationResult("Provide an employee name.", ["Name"]), null, null));
        using var client = server.GetTestClient();
        using var http = await client.GetAsync("/", CancellationToken.None);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, http.StatusCode);
        var problem = await http.Content.ReadFromJsonAsync<JsonElement>(CancellationToken.None);
        Assert.Equal(ErrorCodes.Validation, problem.GetProperty("code").GetString());

        var result = await Backend(client).SendAsync<bool>(HttpMethod.Get, "/", requiresAuthentication: false);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error?.Type);
        Assert.Equal(ErrorCodes.Validation, result.Error?.Code);
        Assert.Equal(["Provide an employee name."], result.Error!.ValidationErrors!["Name"]);
    }

    private static BackendApiClient Backend(HttpClient client)
        => new(
            client,
            Substitute.For<ITokenStorage>(),
            Options.Create(new BackendApiSettings()),
            NullLogger<BackendApiClient>.Instance);

    private static async Task<WebApplication> CreateHostAsync(Func<HttpContext, Task<IActionResult>> handle)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddControllers();
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();

        var app = builder.Build();
        app.UseExceptionHandler();
        app.Run(async context =>
        {
            var result = await handle(context);
            await result.ExecuteResultAsync(new ActionContext(context, new RouteData(), new ActionDescriptor()));
        });
        await app.StartAsync();
        return app;
    }

    private sealed class ContractController : BaseController
    {
        public ActionResult Map<T>(AppResponse<T> response) => HandleResponse(response);
    }
}
