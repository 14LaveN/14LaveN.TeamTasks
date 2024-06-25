using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeamTasks.Application.ApiHelpers.Contracts;
using TeamTasks.Domain.Common.Core.Primitives;
using TeamTasks.Domain.Common.Core.Primitives.Maybe;
using TeamTasks.Domain.Common.Core.Primitives.Result;
using TeamTasks.Identity.Domain.Entities;
using TeamTasks.Identity.Domain.Repositories;

namespace TeamTasks.Identity.Infrastructure.ApiHelpers.Infrastructure;

/// <summary>
/// Represents the identity api controller class.
/// </summary>
[ApiController]
[Produces("application/json")]
[ApiExplorerSettings(GroupName = "v1")]
public class IdentityApiController 
    : ControllerBase
{
    protected IdentityApiController(
        ISender sender,
        IUserRepository userRepository)
    {
        Sender = sender;
        UserRepository = userRepository;
    }

    protected ISender Sender { get; }

    protected IUserRepository UserRepository { get; }

    [HttpGet("get-profile-by-id/{authorId}")]
    public async Task<Maybe<User>> GetProfileById([FromRoute] Guid authorId)
    {
        var profile = 
            await UserRepository.GetByIdAsync(authorId);

        return profile;
    }
        
    /// <summary>
    /// Creates an <see cref="BadRequestObjectResult"/> that produces a <see cref="StatusCodes.Status400BadRequest"/>.
    /// response based on the specified <see cref="Result"/>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>The created <see cref="BadRequestObjectResult"/> for the response.</returns>
    protected IActionResult BadRequest(Error error) => BadRequest(new ApiErrorResponse(new[] { error }));
    
    /// <summary>
    /// Creates an <see cref="BadRequestObjectResult"/> that produces a <see cref="StatusCodes.Status400BadRequest"/>.
    /// response based on the specified <see cref="Result"/>.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns>The created <see cref="BadRequestObjectResult"/> for the response.</returns>
    protected IActionResult Unauthorized(Error error) => Unauthorized(new ApiErrorResponse(new[] { error }));

    /// <summary>
    /// Creates an <see cref="OkObjectResult"/> that produces a <see cref="StatusCodes.Status200OK"/>.
    /// </summary>
    /// <returns>The created <see cref="OkObjectResult"/> for the response.</returns>
    /// <returns></returns>
    protected new IActionResult Ok(object value) => base.Ok(value);

    /// <summary>
    /// Creates an <see cref="NotFoundResult"/> that produces a <see cref="StatusCodes.Status404NotFound"/>.
    /// </summary>
    /// <returns>The created <see cref="NotFoundResult"/> for the response.</returns>
    protected new IActionResult NotFound() => base.NotFound();
}