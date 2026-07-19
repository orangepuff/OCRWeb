using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Commands.AddSecurityRuleCategory;

namespace OCRWeb.API.Endpoints.Internal.AddSecurityRuleCategory
{
    /// <summary>
    /// POST /internal/identity/security-rule-categories.
    /// </summary>
    public class AddSecurityRuleCategoryEndpoint(IMediator mediator) : Endpoint<AddSecurityRuleCategoryRequest, AddSecurityRuleCategoryResponse>
    {
        public override void Configure()
        {
            Post("/internal/identity/security-rule-categories");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(AddSecurityRuleCategoryRequest req, CancellationToken ct)
        {
            var result = await mediator.Send(new AddSecurityRuleCategoryCommand(req.CategoryDesc, req.TextCode), ct);

            await Send.OkAsync(new AddSecurityRuleCategoryResponse(result.Success, result.CategoryId, result.RejectionReason), ct);
        }
    }
}
