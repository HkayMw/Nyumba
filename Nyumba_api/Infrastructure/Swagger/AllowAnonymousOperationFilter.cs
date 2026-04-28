using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Nyumba_api.Infrastructure.Swagger;

public class AllowAnonymousOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var hasAllowAnonymous = context.ApiDescription.ActionDescriptor.EndpointMetadata
            .OfType<IAllowAnonymous>()
            .Any();

        if (hasAllowAnonymous)
        {
            // Override the document-level security requirement for anonymous endpoints.
            operation.Security = new List<OpenApiSecurityRequirement>();
        }
    }
}
