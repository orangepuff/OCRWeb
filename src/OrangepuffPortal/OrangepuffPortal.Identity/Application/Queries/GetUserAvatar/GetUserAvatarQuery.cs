using MediatR;
using OrangepuffPortal.Identity.Contract;

namespace OrangepuffPortal.Identity.Application.Queries.GetUserAvatar;

public record GetUserAvatarQuery(int UserId) : IRequest<UserAvatarDto?>;
