using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Motix.Infrastructure;
using Motix.Security;
using Motix.Services; // ADD para ML.NET

namespace Motix;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // 🚀 Controllers
        builder.Services.AddControllers();

        // 🔢 Versionamento da API
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

        // 📘 Swagger (gera doc por versão)
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            // XML comments (se existir)
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                c.IncludeXmlComments(xmlPath);

            // Mostra campo X-API-KEY no Swagger para endpoints protegidos
            c.OperationFilter<ApiKeyHeaderOperationFilter>();
        });

        builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

        // ❤️ HealthChecks
        builder.Services.AddHealthChecks();

        // 🧱 Infra (DB + Repositórios)
        builder.Services.AddDBContext(builder.Configuration);
        builder.Services.AddRepositories();

        // 🧠 ML.NET (serviço de previsão)
        builder.Services.AddScoped<IMlPredictionService, MlPredictionService>();

        var app = builder.Build();

        // 🧭 Swagger e versão dinâmica
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

        // 🔒 HTTPS
        app.UseHttpsRedirection();

        // ❤️ Health (endpoint público)
        app.MapHealthChecks("/health");

        // 🔐 Middleware de segurança (API KEY)
        app.UseMiddleware<ApiKeyAuthMiddleware>();

        // 🚀 Controllers
        app.MapControllers();

        // 🏁 Executa
        app.Run();
    }
}
