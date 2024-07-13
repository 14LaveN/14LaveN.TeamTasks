using System.Net;
using System.Security.Authentication;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TeamTasks.Application.ApiHelpers.Contracts;
using TeamTasks.Application.ApiHelpers.Policy;
using TeamTasks.Application.ApiHelpers.Responses;
using TeamTasks.Application.Core.Abstractions;
using TeamTasks.Application.Core.Abstractions.Endpoint;
using TeamTasks.Application.Core.Abstractions.Helpers.JWT;
using TeamTasks.Application.Core.Abstractions.Idempotency;
using TeamTasks.Application.Core.Abstractions.Messaging;
using TeamTasks.Domain.Common.Core.Errors;
using TeamTasks.Domain.Common.Core.Primitives.Result;
using TeamTasks.Domain.Common.ValueObjects;
using TeamTasks.Domain.Core.Exceptions;
using TeamTasks.Domain.Core.Primitives.Result;
using TeamTasks.Domain.Entities;
using TeamTasks.Identity.Domain.Entities;
using TeamTasks.Identity.Infrastructure.Settings.User;
using TeamTasks.Identity.Persistence.Extensions;

namespace TeamTasks.Identity.Application.Commands;

/// <summary>
/// Represents the register static class.
/// </summary>
public static class Register
{
    /// <summary>
    /// Represents the register command record class.
    /// </summary>
    /// <param name="RequestId">The request identifier. </param>>
    /// <param name="FirstName">The first name.</param>
    /// <param name="LastName">The last name.</param>
    /// <param name="Email">The email.</param>
    /// <param name="Password">The password.</param>
    /// <param name="UserName">The username.</param>
    public sealed record Command(
        Guid RequestId,
        FirstName FirstName,
        LastName LastName,
        EmailAddress Email,
        Password Password,
        string UserName)
        : IdentityIdempotentCommand(RequestId);
    
    /// <summary>
    /// Represents the register endpoint class.
    /// </summary>
    public sealed class RegisterEndpoint
        : IEndpoint
    {
        /// <inheritdoc />
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("api/v{version:apiVersion}/" + ApiRoutes.Users.Register, async (
                    [FromBody] Contracts.Register.RegisterRequest request,
                    [FromHeader(Name = "X-Idempotency-Key")] string requestId,
                    ISender sender) =>
                {
                    if (!Guid.TryParse(requestId, out Guid parsedRequestId))
                        throw new GuidParseException(nameof(requestId), requestId);
                    
                    var result = await Result.Create(request, DomainErrors.General.UnProcessableRequest)
                        .Map(registerRequest => new Command(
                            parsedRequestId,
                            FirstName.Create(registerRequest.FirstName).Value,
                            LastName.Create(registerRequest.LastName).Value,
                            new EmailAddress(registerRequest.Email),
                            Password.Create(registerRequest.Password).Value,
                            registerRequest.UserName))
                        .Bind(command => BaseRetryPolicy.Policy.Execute(async () =>
                            await sender.Send(command)).Result.Data);

                    return result;
                })
                .AllowAnonymous()
                .Produces(StatusCodes.Status401Unauthorized, typeof(ApiErrorResponse))
                .Produces(StatusCodes.Status200OK)
                .RequireRateLimiting("fixed");
        }
    }
    
    /// <summary>
    /// Represents the register command handler class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="userManager">The user manager.</param>
    /// <param name="signInManager">The sign in manager.</param>
    /// <param name="jwtOptions">The json web token options.</param>
    /// <param name="dbContext">The database context.</param>
    internal sealed class CommandHandler(
        ILogger<CommandHandler> logger,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IOptions<JwtOptions> jwtOptions,
        IDbContext dbContext)
        : ICommandHandler<Command, LoginResponse<Result<User>>>
    {
        private readonly JwtOptions _jwtOptions = jwtOptions.Value;
        private readonly SignInManager<User> _signInManager = signInManager ?? throw new ArgumentNullException();
     
        /// <inheritdoc />
        public async Task<LoginResponse<Result<User>>> Handle(
            Command request,
            CancellationToken cancellationToken)
        {
            try
            {
                logger.LogInformation($"Request for login an account - {request.UserName} {request.LastName}");
                
                Result<FirstName> firstNameResult = FirstName.Create(request.FirstName);
                Result<LastName> lastNameResult = LastName.Create(request.LastName);
                Result<EmailAddress> emailResult = EmailAddress.Create(request.Email);
                Result<Password> passwordResult = Password.Create(request.Password);
                
                User? user = await userManager.FindByNameAsync(request.UserName);
    
                if (user is not null)
                {
                    logger.LogWarning("User with the same name already taken");
                    throw new NotFoundException(nameof(user), "User with the same name");
                }

                string str = "koxs";
                var c = str[~1];
                
                user = User.Create(firstNameResult.Value, lastNameResult.Value,request.UserName, emailResult.Value, passwordResult.Value);
                
               var result = await userManager.CreateAsync(user, request.Password);
                
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    
                    logger.LogInformation($"User authorized - {user.UserName} {DateTime.UtcNow}");
                }
                
                var (refreshToken, refreshTokenExpireAt) = user.GenerateRefreshToken(_jwtOptions);
                
                if (result.Succeeded)
                {
                    user.RefreshToken = refreshToken;
                }
                
                return new LoginResponse<Result<User>>
                {
                    Description = "Register account",
                    StatusCode = HttpStatusCode.OK,
                    Data = Task.FromResult(Result.Create(user, DomainErrors.General.ServerError)),
                    AccessToken = await user.GenerateAccessToken(dbContext,_jwtOptions, cancellationToken),
                    RefreshToken = refreshToken,
                    RefreshTokenExpireAt = refreshTokenExpireAt
                };
            } 
            catch (Exception exception)
            {
                logger.LogError(exception, $"[RegisterCommandHandler]: {exception.Message}");
                throw new AuthenticationException(exception.Message);
            }
        }
    }
    
    /// <summary>
    /// Represents the register command validator class.
    /// </summary>
    internal class CommandValidator
        : AbstractValidator<Command>
    {
        /// <summary>
        /// Validate the login command.
        /// </summary>
        public CommandValidator()
        {
            RuleFor(registerCommand =>
                    registerCommand.UserName).NotEqual(string.Empty)
                .WithMessage("You don't enter a user name")
                .MaximumLength(28)
                .WithMessage("Your user name is too big");
        
            RuleFor(registerCommand =>
                    registerCommand.Password.Value).NotEqual(string.Empty)
                .WithMessage("You don't enter a password")
                .MaximumLength(36)
                .WithMessage("Your password is too big");

            RuleFor(registerCommand =>
                    registerCommand.Email.Value).NotEqual(string.Empty)
                .WithMessage("You don't enter a password")
                .EmailAddress();
        
            RuleFor(registerCommand =>
                    registerCommand.FirstName.Value).NotEqual(string.Empty)
                .WithMessage("You don't enter a first name")
                .MaximumLength(36)
                .WithMessage("Your first name is too big");
        
            RuleFor(registerCommand =>
                    registerCommand.LastName.Value).NotEqual(string.Empty)
                .WithMessage("You don't enter a last name")
                .MaximumLength(36)
                .WithMessage("Your last name is too big");
        }
    }
}