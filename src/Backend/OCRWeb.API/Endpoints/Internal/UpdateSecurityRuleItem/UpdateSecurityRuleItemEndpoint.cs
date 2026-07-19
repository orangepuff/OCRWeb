using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Commands.UpdateSecurityRuleItem;

namespace OCRWeb.API.Endpoints.Internal.UpdateSecurityRuleItem
{
    /// <summary>
    /// PUT /internal/identity/security-rule-items/{id}.
    /// </summary>
    public class UpdateSecurityRuleItemEndpoint(IMediator mediator) : Endpoint<UpdateSecurityRuleItemRequest, UpdateSecurityRuleItemResponse>
    {
        public override void Configure()
        {
            Put("/internal/identity/security-rule-items/{id}");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(UpdateSecurityRuleItemRequest req, CancellationToken ct)
        {
            var result = await mediator.Send(
                new UpdateSecurityRuleItemCommand(req.Id, req.CategoryId, req.Description, req.RuleType, req.TextCode, req.SortOrder, req.Hidden), ct);

            await Send.OkAsync(new UpdateSecurityRuleItemResponse(result.Success, result.RejectionReason), ct);
        }
    }
}
