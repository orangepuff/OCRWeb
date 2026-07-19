using MediatR;
using OCRWeb.Identity.Contract;
using OCRWeb.Identity.Domain.Repositories;

namespace OCRWeb.Identity.Application.Queries.GetUserAvatar;

public class GetUserAvatarQueryHandler(IUserRepository repository) : IRequestHandler<GetUserAvatarQuery, UserAvatarDto?>
{
    public async Task<UserAvatarDto?> Handle(GetUserAvatarQuery request, CancellationToken cancellationToken)
    {
        var avatar = await repository.GetAvatarAsync(request.UserId, cancellationToken);
        return avatar is null ? null : new UserAvatarDto(avatar.Image, avatar.ContentType);
    }
}
