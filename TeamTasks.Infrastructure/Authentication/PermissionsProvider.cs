using Microsoft.AspNetCore.Http;
using TeamTasks.Application.Core.Abstractions.Helpers.JWT;
using TeamTasks.Domain.Core.Exceptions;
using TeamTasks.Domain.Entities;
using TeamTasks.Identity.Domain.Repositories;
using TeamTasks.Identity.Infrastructure.Authentication;

namespace TeamTasks.Infrastructure.Authentication;

public sealed class PermissionProvider : IPermissionProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PermissionProvider"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public PermissionProvider(
        IHttpContextAccessor httpContextAccessor)
    {
        HashSet<string> permissions =  GetClaimByJwtToken
            .GetPermissionsByToken(httpContextAccessor.HttpContext?.Request.Headers["Authorization"]
            .FirstOrDefault()?.Split(" ").Last());
        
        Permissions = permissions;
    }

    /// <inheritdoc />
    public HashSet<string> Permissions { get; }
}