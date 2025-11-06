using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Motix.Infrastructure;
using Motix.Security;                         // ADD

namespace Motix;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        // Versionamento + Explorer (Swagger agrupa por versão)
        builder.Services
            .AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true; // header api-supported-versions
                o.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader() // /api/v{version}/...
                );
            })
            .AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV"; // v1, v2, v2.1
                o.SubstituteApiVersionInUrl = true;
            });

        // Swagger (um doc por versão)
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

            c.OperationFilter<ApiKeyHeaderOperationFilter>(); // ADD: mostra X-API-KEY nos métodos de escrita
        });
        builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

        // HealthChecks
        builder.Services.AddHealthChecks();

        // Infra
        builder.Services.AddDBContext(builder.Configuration);
        builder.Services.AddRepositories();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            app.UseSwaggerUI(c =>
            {
                foreach (var desc in provider.ApiVersionDescriptions)
                    c.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", $"Motix {desc.GroupName}");
            });
        }

        app.UseHttpsRedirection();

        // /health (fora do Swagger — normal)
        app.MapHealthChecks("/health");

        app.UseMiddleware<ApiKeyAuthMiddleware>();          // ADD: protege POST/PUT/DELETE na API versionada

        app.MapControllers();
        app.Run();
    }
}
