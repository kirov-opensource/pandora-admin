using Microsoft.EntityFrameworkCore;
using pandora.admin.webapi.DataAccess;
using pandora.admin.webapi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// builder.Services.AddOcelot();

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

//builder.Configuration.GetConnectionString("Default")
builder.Services.AddDbContext<PandoraAdminContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSwaggerGen();

builder.Services.AddMemoryCache();

//
// builder.WebHost.UseDefaultServiceProvider(o =>
// {
//     o.ValidateOnBuild = true;
//     o.ValidateScopes = true;
// });

var app = builder.Build();


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

app.MapControllers();

app.Run();