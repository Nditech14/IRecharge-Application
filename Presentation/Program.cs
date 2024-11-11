using Application.PayStcak;
using Domain.Entities.Azure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Presentation.Configuration;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddServices();
builder.Services.RegisterApplicationServices(builder.Configuration);
//builder.Services.Configure<ServiceBusSettings>(builder.Configuration.GetSection("ServiceBusSettings"));
builder.Services.Configure<CosmosDbSettings>(builder.Configuration.GetSection("CosmosDb"));

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
});

// Determine if using B2C or B2B and configure accordingly
var authenticationType = builder.Configuration["AuthenticationType"];
if (authenticationType == "B2C")
{
    builder.Services.ADB2CSwaggerConfiguration(builder.Configuration);

    // Configure B2C authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAdB2C"));
}
else if (authenticationType == "B2B")
{
    builder.Services.ADB2BSwaggerConfiguration(builder.Configuration);

    // Configure B2B authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
}

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Conditional Swagger OAuth client based on authentication type
        if (authenticationType == "B2C")
        {
            c.OAuthClientId(builder.Configuration["AzureAdB2C:SwaggerClientId"]);
            c.OAuthUsePkce();
        }
        else if (authenticationType == "B2B")
        {
            c.OAuthClientId(builder.Configuration["SwaggerAzureAD:ClientId"]);
            c.OAuthUsePkce();
        }
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<UserMiddleWare>();
app.UseAuthorization();
app.MapControllers();

app.Run();
