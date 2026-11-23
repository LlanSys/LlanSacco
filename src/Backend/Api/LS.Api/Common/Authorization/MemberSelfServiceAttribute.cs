using Microsoft.AspNetCore.Authorization;

namespace LS.Api.Common.Authorization;

[AttributeUsage(AttributeTargets.Method)]
public sealed class MemberSelfServiceAttribute : AuthorizeAttribute
{
    internal const string PolicyName = "MemberSelfService";

    public MemberSelfServiceAttribute() : base(PolicyName)
    {
    }
}
