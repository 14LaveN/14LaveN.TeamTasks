using System.Net;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TeamTasks.Application.ApiHelpers.Contracts;
using TeamTasks.Application.ApiHelpers.Policy;
using TeamTasks.Application.ApiHelpers.Responses;
using TeamTasks.Application.Core.Abstractions;
using TeamTasks.Application.Core.Abstractions.Endpoint;
using TeamTasks.Application.Core.Abstractions.Helpers.JWT;
using TeamTasks.Application.Core.Abstractions.Idempotency;
using TeamTasks.Application.Core.Abstractions.Messaging;
using TeamTasks.Application.Core.Errors;
using TeamTasks.Application.Core.Extensions;
using TeamTasks.Domain.Common.Core.Errors;
using TeamTasks.Domain.Common.Core.Primitives.Maybe;
using TeamTasks.Domain.Common.Core.Primitives.Result;
using TeamTasks.Domain.Common.ValueObjects;
using TeamTasks.Domain.Core.Exceptions;
using TeamTasks.Domain.Core.Primitives.Result;
using TeamTasks.Domain.Enumerations;
using TeamTasks.Identity.Contracts.ChangeName;
using TeamTasks.Identity.Domain.Entities;
using TeamTasks.Infrastructure.Authentication;

namespace TeamTasks.Identity.Application.Commands;

/// <summary>
/// Represents the change name static class.
/// </summary>
public static class ChangeName
{
    /// <summary>
    /// Represents the change <see cref="Name"/> command record.
    /// </summary>
    /// <param name="FirstName">The first name.</param>
    /// <param name="LastName">The last name.</param>
    /// <param name="RequestId">The request identifier.</param>
    public sealed record Command(
        Guid RequestId,
        FirstName FirstName,
        LastName LastName)
        : IdempotentCommand<User>(RequestId);
    
    /// <summary>
    /// Represents the change name <see cref="IEndpoint"/> class.
    /// </summary>
    public sealed class ChangeNameEndpoint
        : IEndpoint
    {
        /// <inheritdoc />
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("api/v{version:apiVersion}/" +ApiRoutes.Users.ChangeName, async (
                    [FromBody] ChangeNameRequest request,
                    [FromHeader(Name = "X-Idempotency-Key")] string requestId,
                    ISender sender) =>
                {
                    if (!Guid.TryParse(requestId, out Guid parsedRequestId))
                        throw new GuidParseException(nameof(requestId), requestId);
                    
                    var result = await Result.Create(request, DomainErrors.General.UnProcessableRequest)
                        .Map(changeNameRequest => new Command(
                            parsedRequestId,
                            FirstName.Create(changeNameRequest.FirstName).Value,
                            LastName.Create(changeNameRequest.LastName).Value))
                        .Bind(command => Task.FromResult(BaseRetryPolicy.Policy.Execute(async () =>
                            await sender.Send(command)).Result.Data));

                    return result;
                })
                .HasPermission(Permission.UpdateMember.ToString())
                .Produces(StatusCodes.Status401Unauthorized, typeof(ApiErrorResponse))
                .Produces(StatusCodes.Status200OK)
                .RequireRateLimiting("fixed");
        }
    }
    
    /// <summary>
    /// Represents the <see cref="Command"/> handler.
    /// </summary>
    internal sealed class CommandHandler : ICommandHandler<Command, IBaseResponse<Result<User>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;
        private readonly IUserIdentifierProvider _userIdentifier;
    
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="unitOfWork">The unit of work.</param>
        /// <param name="userManager">The user manager.</param>
        /// <param name="userIdentifier">The user identifier provider.</param>
        public CommandHandler(
            IUnitOfWork unitOfWork,
            UserManager<User> userManager,
            IUserIdentifierProvider userIdentifier)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _userIdentifier = userIdentifier;
        }
    
        /// <inheritdoc />
        public async Task<IBaseResponse<Result<User>>> Handle(
            Command request, 
            CancellationToken cancellationToken)
        {
            Result<FirstName> nameResult = FirstName.Create(request.FirstName);
    
            if (nameResult.IsFailure)
            {
                return new BaseResponse<Result<User>>
                {
                    Data = Result.Failure<User>(nameResult.Error),
                    StatusCode = HttpStatusCode.InternalServerError,
                    Description = "First Name result is failure."
                };
            }
            
            Result<LastName> lastNameResult = LastName.Create(request.LastName);
    
            if (lastNameResult.IsFailure)
            {
                return new BaseResponse<Result<User>>
                {
                    Data = Result.Failure<User>(lastNameResult.Error),
                    StatusCode = HttpStatusCode.InternalServerError,
                    Description = "Last Name result is failure."
                };
            }
            
            Maybe<User> maybeUser = await _userManager.FindByIdAsync(_userIdentifier.UserId.ToString()) 
                                    ?? throw new ArgumentException();
    
            if (maybeUser.HasNoValue)
            {
                return new BaseResponse<Result<User>>
                {
                    Data = Result.Failure<User>(DomainErrors.User.NotFound),
                    StatusCode = HttpStatusCode.NotFound,
                    Description = "User not found."
                };
            }
    
            User user = maybeUser.Value;
    
            user.ChangeName(request.FirstName,request.LastName);
    
            await _unitOfWork.SaveChangesAsync(cancellationToken);
    
            return new BaseResponse<Result<User>>
            {
                Data = Result.Create(user, DomainErrors.General.ServerError),
                Description = "Change name.",
                StatusCode = HttpStatusCode.OK
            };
        }
    }
    
    /// <summary>
    /// Represents the <see cref="Command"/> validator class.
    /// </summary>
    internal sealed class CommandValidator
        : AbstractValidator<Command>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandValidator"/> class.
        /// </summary>
        public CommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithError(ValidationErrors.ChangeName.NameIsRequired);
        
            RuleFor(x => x.LastName)
                .NotEmpty()
                .WithError(ValidationErrors.ChangeName.NameIsRequired);
        }
    }
}