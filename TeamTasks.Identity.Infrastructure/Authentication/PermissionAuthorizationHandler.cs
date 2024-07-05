using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using TeamTasks.Application.Core.Abstractions.Helpers.JWT;

namespace TeamTasks.Identity.Infrastructure.Authentication;

/// <summary>
/// Represents the permission authorization handler class.
/// </summary>
/// <param name="scopeFactory">The service scope factory.</param>
public sealed class PermissionAuthorizationHandler(IServiceScopeFactory scopeFactory)
    : AuthorizationHandler<PermissionRequirement>
{
    /// <inheritdoc />
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        using IServiceScope scope = scopeFactory.CreateScope();

        IPermissionProvider permissionProvider = 
            scope.ServiceProvider.GetRequiredService<IPermissionProvider>();

        HashSet<string> permissions = permissionProvider.Permissions;

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}