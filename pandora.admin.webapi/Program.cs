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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// builder.Services.AddOcelot();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

//builder.Configuration.GetConnectionString("Default")
builder.Services.AddDbContext<PandoraAdminContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("Default")));

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("JWTSecurityKey").Get<string>())),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyConstant.AdministratorOnly, policy => policy.RequireClaim(ClaimTypesExtension.Administrator));
});

// ���Swagger UI����
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Pandora.Admin.WebAPI", Version = "v1" });

    options.DocInclusionPredicate((docName, description) => true);
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Token",
        Name = HeaderNames.Authorization,
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });
    //��֤��ʽ���˷�ʽΪȫ�����
    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
                    { new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference()
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    }, Array.Empty<string>() }
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
    { "/log_conversation", new HashSet<string>() { "POST" } }
};

Dictionary<string, HashSet<string>> redirectWithMiddlewarePaths = new Dictionary<string, HashSet<string>>()
{
    { "/gpt/api/conversations", new HashSet<string>() { "GET" } }
};

app.UseStaticFiles();

app.UseRouting();


app.UseWhen(context =>
{
    var needRedirect = false;
    foreach (var pathInfo in redirectWithMiddlewarePaths)
    {
        if (context.Request.Path.StartsWithSegments(pathInfo.Key))
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
}, action => { app.MapReverseProxy(); });

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();