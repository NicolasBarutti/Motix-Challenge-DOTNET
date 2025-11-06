using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Motix;

public class ApiKeyHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.ApiDescription.HttpMethod?.ToUpperInvariant();
        if (method is "POST" or "PUT" or "DELETE" or "PATCH")
        {
            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-API-KEY",
                In = ParameterLocation.Header,
                Required = true,
                Description = "Chave de API para operações de escrita"
            });
        }
    }
}