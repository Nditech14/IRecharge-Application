using Microsoft.OpenApi.Models;

namespace Presentation.Configuration
{
    public static class SwaggerConfiguration
    {
        public static void ADB2CSwaggerConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Azure B2C Swagger Smart", Version = "v1" });

                var baseUrl = configuration["AzureAdB2C:BaseUrl"];
                var scopes = configuration["AzureAdB2C:Scopes"]?
                    .Split(' ')
                    .ToDictionary(scope => $"{baseUrl}{scope}", scope => $"Access API as {scope.Split('.').Last()}");

                if (!string.IsNullOrEmpty(configuration["AzureAdB2C:Instance"]) &&
                    !string.IsNullOrEmpty(configuration["AzureAdB2C:Domain"]) &&
                    !string.IsNullOrEmpty(configuration["AzureAdB2C:SignUpSignInPolicyId"]) &&
                    scopes != null)
                {
                    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                    {
                        Description = "Oauth2.0 which uses AuthorizationCode flow",
                        Name = "oauth2.0",
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri($"{configuration["AzureAdB2C:Instance"]}{configuration["AzureAdB2C:Domain"]}/oauth2/v2.0/authorize?p={configuration["AzureAdB2C:SignUpSignInPolicyId"]}"),
                                TokenUrl = new Uri($"{configuration["AzureAdB2C:Instance"]}{configuration["AzureAdB2C:Domain"]}/oauth2/v2.0/token?p={configuration["AzureAdB2C:SignUpSignInPolicyId"]}"),
                                Scopes = scopes
                            }
                        }
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                            },
                            scopes.Keys.ToArray()
                        }
                    });
                }
            });
        }

        public static void ADB2BSwaggerConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Azure B2B Swagger API", Version = "v1" });

                var authorizationUrl = configuration["SwaggerAzureAD:AuthorizationUrl"];
                var tokenUrl = configuration["SwaggerAzureAD:TokenUrl"];
                var scope = configuration["SwaggerAzureAD:Scope"];

                if (!string.IsNullOrEmpty(authorizationUrl) && !string.IsNullOrEmpty(tokenUrl) && !string.IsNullOrEmpty(scope))
                {
                    c.AddSecurityDefinition("OAuth 2.0", new OpenApiSecurityScheme
                    {
                        Description = "OAuth2.0 which uses AuthorizationCode flow for Azure B2B",
                        Name = "OAuth 2.0",
                        Type = SecuritySchemeType.OAuth2,
                        Flows = new OpenApiOAuthFlows
                        {
                            AuthorizationCode = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri(authorizationUrl),
                                TokenUrl = new Uri(tokenUrl),
                                Scopes = new Dictionary<string, string>
                            {
                                { scope, "Access API as User" }
                            }
                            }
                        }
                    });

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "OAuth 2.0"
                            },
                        },
                        new[] { scope }
                    }
                });
                }
            });
        }
    }


}
