using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using OCRWeb.Identity.Application.Commands.DeleteSecurityRuleItem;

namespace OCRWeb.API.Endpoints.Internal.DeleteSecurityRuleItem
{
    /// <summary>
    /// DELETE /internal/identity/security-rule-items/{id}.
    /// </summary>
    public class DeleteSecurityRuleItemEndpoint(IMediator mediator) : Endpoint<DeleteSecurityRuleItemRequest, DeleteSecurityRuleItemResponse>
    {
        public override void Configure()
        {
            Delete("/internal/identity/security-rule-items/{id}");
            AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        }

        public override async Task HandleAsync(DeleteSecurityRuleItemRequest req, CancellationToken ct)
        {
            var result = await mediator.Send(new DeleteSecurityRuleItemCommand(req.Id), ct);

            await Send.OkAsync(new DeleteSecurityRuleItemResponse(result.Success, result.RejectionReason), ct);
        }
    }
}
