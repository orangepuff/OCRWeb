using MediatR;
using OCRWeb.Identity.Contract;

namespace OCRWeb.Identity.Application.Queries.GetUserAvatar;

public record GetUserAvatarQuery(int UserId) : IRequest<UserAvatarDto?>;
