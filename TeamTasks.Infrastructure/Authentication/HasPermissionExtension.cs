using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace TeamTasks.Infrastructure.Authentication;

/// <summary>
/// Represents the has permission extension class.
/// </summary>
public static class HasPermissionExtension
{
    /// <summary>
    /// Has permission method.
    /// </summary>
    /// <param name="builder">The route handler builder.</param>
    /// <param name="permission">The permission.</param>
    /// <returns>Returns <see cref="RouteHandlerBuilder"/> class.</returns>
    public static RouteHandlerBuilder HasPermission(
        this RouteHandlerBuilder builder,
        string permission)
    {
        builder.Add(endpointBuilder =>
        {
            var originalRequestDelegate = endpointBuilder.RequestDelegate;
            endpointBuilder.RequestDelegate = async context =>
            {
                string userId = GetClaimByJwtToken.GetIdByToken(context.Request.Headers["Authorization"]
                    .FirstOrDefault()?.Split(" ").Last());

                HashSet<string> permissions = GetClaimByJwtToken.GetPermissionsByToken(context.Request
                    .Headers["Authorization"]
                    .FirstOrDefault()?.Split(" ").Last());

                if (userId.IsNullOrEmpty())
                {
                    await context.Response.WriteAsync(HttpStatusCode.Unauthorized.ToString());
                    return;
                }

                if (permissions.Any(p => p == permission))
                {
                    await context.Response.WriteAsync(HttpStatusCode.Forbidden.ToString());
                    return;
                }


                await originalRequestDelegate(context);
            };
        });
        return builder;
    }
}