using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Queries.ListSecurityRuleItems;
using OCRWeb.Identity.Contract;

namespace OCRWeb.API.Endpoints.Internal.ListSecurityRuleItems
{
    /// <summary>
    /// GET /internal/identity/security-rule-items?categoryId=.
    /// </summary>
    public class ListSecurityRuleItemsEndpoint(IMediator mediator) : Endpoint<ListSecurityRuleItemsRequest, IReadOnlyList<SecurityRuleItemListItemDto>>
    {
        public override void Configure()
        {
            Get("/internal/identity/security-rule-items");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(ListSecurityRuleItemsRequest req, CancellationToken ct)
        {
            var result = await mediator.Send(new ListSecurityRuleItemsQuery(req.CategoryId), ct);
            await Send.OkAsync(result, ct);
        }
    }
}
