using System.Reflection.Metadata;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using pandora.admin.webapi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOcelot();
builder.Services.AddSwaggerGen();

var app = builder.Build();

//使用静态文件
app.UseStaticFiles();

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

app.MapWhen(context =>
{
    var redirectToOcelot = true;
    foreach (var pathInfo in overridePaths)
    {
        if (context.Request.Path.StartsWithSegments(pathInfo.Key))
        {
            if (pathInfo.Value.Contains(context.Request.Method))
            {
                redirectToOcelot = false;
                break;
            }
        }
    }

    if (redirectToOcelot == false)
    {
    }

    return redirectToOcelot;
}, action =>
{
    //.UseMiddleware<CustomOcelotMiddleware>()
    action
        .UseOcelot()
        .Wait();
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();