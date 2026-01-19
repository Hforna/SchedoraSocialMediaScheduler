using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
using Schedora.Application;
using Schedora.Domain;
using Schedora.Infrastructure;
using Schedora.Infrastructure.Externals.Services;
using Schedora.Infrastructure.RabbitMq;
using Schedora.Infrastructure.Services;
using Schedora.WebApi.Helpers;
using Schedora.Workers;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetExecutingAssembly());
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme.
                        Enter 'Bearer' [space] and then your token in the text input below.
                        Example: 'Bearer 12345abcdef'",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    options.SwaggerDoc("v1", new OpenApiInfo()
    {
        Version = "v1",
        Title = "Schedora API",
        Description = "A social media management api",
        Contact = new OpenApiContact()
        {
            Email = "hfornabest@gmail.com",
            Url = new Uri("https://github.com/Hforna"),
            Name = "Henrique",
        },
    });
    
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
                
            },
            new List<string>()
        }
    });
    
    options.ExampleFilters();
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = long.MaxValue;
});

builder.Services.AddRouting(d => d.LowercaseUrls = true);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddDomain(builder.Configuration);
//builder.Services.AddWorkers(builder.Configuration);

var tokenValidationParameters = new TokenValidationParameters()
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration.GetValue<string>("services:auth:jwt:signKey")!)),
    ValidateIssuer = false,
    ValidateAudience = false,
    ValidateLifetime = true,
    RequireExpirationTime = true,
    ClockSkew = TimeSpan.FromMinutes(5),
    NameClaimType = JwtRegisteredClaimNames.Sub,
    RoleClaimType = "role"
};

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(cfg =>
{
    cfg.TokenValidationParameters = tokenValidationParameters;
    cfg.SaveToken = true;
});

builder.Services.AddScoped<ILinkHelper, LinkHelper>();

builder.Services.Configure<RabbitMqConnection>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.AddHttpContextAccessor();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<TokenValidationParameters>(tokenValidationParameters);

builder.Services.Configure<SmtpConfigurations>(builder.Configuration.GetSection("services:smtp"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//var scheduler = app.Services.GetRequiredService<IWorkerScheduler>();
//scheduler.ScheduleWorks();

//app.UseHttpsRedirection();

app.UseAuthentication();

app.UseSession();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
    
}