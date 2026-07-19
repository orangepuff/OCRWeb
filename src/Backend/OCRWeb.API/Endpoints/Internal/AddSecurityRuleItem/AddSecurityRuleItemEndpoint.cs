using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Commands.AddSecurityRuleItem;

namespace OCRWeb.API.Endpoints.Internal.AddSecurityRuleItem
{
    /// <summary>
    /// POST /internal/identity/security-rule-items.
    /// </summary>
    public class AddSecurityRuleItemEndpoint(IMediator mediator) : Endpoint<AddSecurityRuleItemRequest, AddSecurityRuleItemResponse>
    {
        public override void Configure()
        {
            Post("/internal/identity/security-rule-items");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(AddSecurityRuleItemRequest req, CancellationToken ct)
        {
            var result = await mediator.Send(
                new AddSecurityRuleItemCommand(req.CategoryId, req.Code, req.Description, req.RuleType, req.TextCode, req.SortOrder), ct);

            await Send.OkAsync(new AddSecurityRuleItemResponse(result.Success, result.RuleItemId, result.RejectionReason), ct);
        }
    }
}
