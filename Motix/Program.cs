using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Motix.Infrastructure;
using Motix.Security;

namespace Motix;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();

        // 🔢 Versionamento de API
        builder.Services
            .AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.ReportApiVersions = true;
                o.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader() // /api/v{version}/...
                );
            })
            .AddApiExplorer(o =>
            {
                o.GroupNameFormat = "'v'VVV"; // v1, v2, v2.1
                o.SubstituteApiVersionInUrl = true;
            });

        // 📘 Swagger (docs por versão via ConfigureSwaggerOptions)
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            // XML comments (se existir)
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                c.IncludeXmlComments(xmlPath);

            // mostra campo X-API-KEY nos métodos de escrita
            c.OperationFilter<ApiKeyHeaderOperationFilter>();
        });
        builder.Services.ConfigureOptions<ConfigureSwaggerOptions>(); // gera v1, v2... dinamicamente

        // ❤️ HealthChecks
        builder.Services.AddHealthChecks();

        // 🧱 Infra (DB + Repositórios)
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

        // /health público
        app.MapHealthChecks("/health");

        // 🔐 Segurança (API Key)
        app.UseMiddleware<ApiKeyAuthMiddleware>();

        // Controllers
        app.MapControllers();

        app.Run();
    }
}
