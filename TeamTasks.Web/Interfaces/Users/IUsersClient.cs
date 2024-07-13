using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Refit;
using TeamTasks.Application.ApiHelpers.Contracts;
using TeamTasks.Domain.Core.Primitives.Result;
using TeamTasks.Identity.Domain.Entities;

namespace TeamTasks.Web.Interfaces.Users;

/// <summary>
/// Represents the users client interface.
/// </summary>
public interface IUsersClient
{
    /// <summary>
    /// The register controller component.
    /// </summary>
    /// <param name="request">The <see cref="RegisterRequest"/>.</param>
    /// <param name="requestId">The request identifier.</param>
    /// <returns>Returns the result of <see cref="User"/> entity.</returns>
    [Post("api/v1/"+ApiRoutes.Users.Register)]
    Task<Result<User>?> Register(
        [FromBody] RegisterRequest request,
        [FromHeader(Name = "X-Idempotency-Key")] string requestId);
}