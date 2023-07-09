using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Pandora.Admin.WebAPI.DataAccess;
using Pandora.Admin.WebAPI.Extensions;
using Pandora.Admin.WebAPI.Middlewares;
using Pandora.Admin.WebAPI.Policies;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Pandora.Admin.WebAPI;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

const string pandoraAdminHeaderNames = "access-token";

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// builder.Services.AddOcelot();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(builderContext =>
    {
        builderContext.AddRequestTransform(async transformContext =>
        {
            if (transformContext.HttpContext.Request.Cookies.ContainsKey(pandoraAdminHeaderNames))
            {
                var userId =
                    int.Parse(transformContext.HttpContext.User.Claims
                        .FirstOrDefault(c => c.Type == ClaimTypesExtension.UserId)?.Value ?? "0");
                if (userId > 0)
                {
                    var dbContext = transformContext.HttpContext.RequestServices.GetService<PandoraAdminContext>();
                    var cache = transformContext.HttpContext.RequestServices.GetService<IMemoryCache>();

                    var originToken = await dbContext.GetUserOriginToken(cache, userId);

                    transformContext.ProxyRequest.Headers.Remove("Cookie");
                    transformContext.ProxyRequest.Headers.Add("Cookie", $"access-token={originToken}");
                    transformContext.ProxyRequest.Headers.Remove(Consts.TOKEN_HEADER_NAME);
                    transformContext.ProxyRequest.Headers.Add(Consts.TOKEN_HEADER_NAME, $"Bearer {originToken}");
                }
            }
        });
    });

//builder.Configuration.GetConnectionString("Default")
builder.Services.AddDbContext<PandoraAdminContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("Default"),
        new MySqlServerVersion(new Version(8, 0, 29))));

builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(cfg =>
    {
        cfg.RequireHttpsMetadata = false;
        cfg.SaveToken = false;
        cfg.TokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWTSecurityKey").Get<string>())),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };

        //use custom auth header name
        cfg.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.ContainsKey(pandoraAdminHeaderNames))
                {
                    context.Token = context.Request.Cookies[pandoraAdminHeaderNames].ToString().Replace("Bearer ", "");
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyConstant.AdministratorOnly,
        policy => policy.RequireClaim(ClaimTypesExtension.Administrator));
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Pandora.Admin.WebAPI", Version = "v1" });

    options.DocInclusionPredicate((docName, description) => true);

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Token",
        Name = pandoraAdminHeaderNames,
        In = ParameterLocation.Cookie,
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference()
                {
                    Id = "Bearer",
                    Type = ReferenceType.SecurityScheme
                }
            },
            Array.Empty<string>()
        }
    });
});

//
// builder.WebHost.UseDefaultServiceProvider(o =>
// {
//     o.ValidateOnBuild = true;
//     o.ValidateScopes = true;
// });

var app = builder.Build();
app.UseExceptionHandlerMiddleware();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

Dictionary<string, HashSet<string>> overridePaths = new Dictionary<string, HashSet<string>>()
{
    { "/auth/login", new HashSet<string>() { "POST" } },
    { "/api/auth/session", new HashSet<string>() { "GET" } },
    { "/log_conversation", new HashSet<string>() { "POST" } }
};

Dictionary<string, HashSet<string>> redirectWithMiddlewarePaths = new Dictionary<string, HashSet<string>>()
{
    { "/_next/static/chunks/pages/app-", new HashSet<string>() { "GET" } },
    { "/gpt/api/conversations", new HashSet<string>() { "GET" } }
};

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseWhen(context =>
{
    var needRedirect = false;
    foreach (var pathInfo in redirectWithMiddlewarePaths)
    {
        if (context.Request.Path.StartsWithSegments(pathInfo.Key) || context.Request.Path.ToString().StartsWith(pathInfo.Key))
        {
            if (pathInfo.Value.Contains(context.Request.Method))
            {
                needRedirect = true;
                break;
            }
        }
    }

    if (needRedirect == false)
    {
    }
    else
    {
        // context.Request.EnableBuffering();
    }

    return needRedirect;
}, action =>
{
    action.UseMiddleware<CustomMiddleware>();
    app.MapReverseProxy();
});

app.UseWhen(context =>
{
    foreach (var pathInfo in redirectWithMiddlewarePaths)
    {
        if (context.Request.Path.StartsWithSegments(pathInfo.Key))
        {
            if (pathInfo.Value.Contains(context.Request.Method))
            {
                return false;
            }
        }
    }

    var needRedirect = true;
    foreach (var pathInfo in overridePaths)
    {
        if (context.Request.Path.StartsWithSegments(pathInfo.Key))
        {
            if (pathInfo.Value.Contains(context.Request.Method))
            {
                needRedirect = false;
                break;
            }
        }
    }

    if (needRedirect == false)
    {
    }

    return needRedirect;
}, action =>
{
    app.MapReverseProxy();
});


app.MapControllers();

app.Run();