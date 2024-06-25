using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TeamTasks.Application.ApiHelpers.Contracts;
using TeamTasks.Domain.Common.Core.Primitives;
using TeamTasks.Domain.Common.Core.Primitives.Maybe;
using TeamTasks.Identity.Domain.Entities;
using TeamTasks.Identity.Domain.Repositories;

namespace TeamTasks.Identity.Infrastructure.ApiHelpers.Infrastructure;

public class IdentityApiModule
{
    protected IdentityApiModule(
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
}