using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Commands.DeleteSecurityRuleCategory;

namespace OCRWeb.API.Endpoints.Internal.DeleteSecurityRuleCategory
{
    /// <summary>
    /// DELETE /internal/identity/security-rule-categories/{id}.
    /// </summary>
    public class DeleteSecurityRuleCategoryEndpoint(IMediator mediator) : Endpoint<DeleteSecurityRuleCategoryRequest, DeleteSecurityRuleCategoryResponse>
    {
        public override void Configure()
        {
            Delete("/internal/identity/security-rule-categories/{id}");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(DeleteSecurityRuleCategoryRequest req, CancellationToken ct)
        {
            var result = await mediator.Send(new DeleteSecurityRuleCategoryCommand(req.Id), ct);

            await Send.OkAsync(new DeleteSecurityRuleCategoryResponse(result.Success, result.RejectionReason), ct);
        }
    }
}
