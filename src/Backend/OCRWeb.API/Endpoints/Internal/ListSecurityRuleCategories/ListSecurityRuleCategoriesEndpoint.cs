using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Queries.ListSecurityRuleCategories;
using OCRWeb.Identity.Contract;

namespace OCRWeb.API.Endpoints.Internal.ListSecurityRuleCategories
{
    /// <summary>
    /// GET /internal/identity/security-rule-categories.
    /// </summary>
    public class ListSecurityRuleCategoriesEndpoint(IMediator mediator) : EndpointWithoutRequest<IReadOnlyList<SecurityRuleCategoryListItemDto>>
    {
        public override void Configure()
        {
            Get("/internal/identity/security-rule-categories");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var result = await mediator.Send(new ListSecurityRuleCategoriesQuery(), ct);
            await Send.OkAsync(result, ct);
        }
    }
}
