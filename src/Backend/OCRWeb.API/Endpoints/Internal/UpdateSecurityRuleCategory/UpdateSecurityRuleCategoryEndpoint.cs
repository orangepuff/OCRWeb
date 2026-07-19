using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Commands.UpdateSecurityRuleCategory;

namespace OCRWeb.API.Endpoints.Internal.UpdateSecurityRuleCategory
{
    /// <summary>
    /// PUT /internal/identity/security-rule-categories/{id}.
    /// </summary>
    public class UpdateSecurityRuleCategoryEndpoint(IMediator mediator) : Endpoint<UpdateSecurityRuleCategoryRequest, UpdateSecurityRuleCategoryResponse>
    {
        public override void Configure()
        {
            Put("/internal/identity/security-rule-categories/{id}");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(UpdateSecurityRuleCategoryRequest req, CancellationToken ct)
        {
            var result = await mediator.Send(new UpdateSecurityRuleCategoryCommand(req.Id, req.CategoryDesc, req.TextCode, req.Hidden), ct);

            await Send.OkAsync(new UpdateSecurityRuleCategoryResponse(result.Success, result.RejectionReason), ct);
        }
    }
}
