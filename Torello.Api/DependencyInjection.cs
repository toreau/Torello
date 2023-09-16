using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.OpenApi.Models;
using Torello.Application.Common.Errors;
using Torello.Application.Common.Settings;

namespace Torello.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddSingleton<ProblemDetailsFactory, TorelloProblemDetailsFactory>();
        services.AddSwagger(configuration);

        return services;
    }

    private static IServiceCollection AddSwagger(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var swaggerSettings = new SwaggerSettings();
        configuration.Bind(SwaggerSettings.SectionName, swaggerSettings);

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(swaggerSettings.Version, new OpenApiInfo
            {
                Version = swaggerSettings.Version,
                Title = swaggerSettings.Title,
                Description = swaggerSettings.Description,
                Contact = new OpenApiContact
                {
                    Name = swaggerSettings.ContactName,
                    Email = swaggerSettings.ContactEmail
                }
            });

            // Enable Swagger UI authentication functionality
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Please enter a valid token",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            // Useful for grouping actions in the Swagger UI
            c.TagActionsBy(api =>
            {
                if (api.GroupName != null)
                    return new[] { api.GroupName };

                if (api.ActionDescriptor is ControllerActionDescriptor controllerActionDescriptor)
                    return new[] { controllerActionDescriptor.ControllerName };

                throw new InvalidOperationException("Unable to determine tag for endpoint.");
            });

            c.DocInclusionPredicate((name, api) => true);

            // var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            // c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });

        return services;
    }
}
