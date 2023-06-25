var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// builder.Services.AddOcelot();

builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddSwaggerGen();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// app.UseHttpsRedirection();
//
// app.UseAuthorization();

Dictionary<string, HashSet<string>> overridePaths = new Dictionary<string, HashSet<string>>()
{
    { "/auth/login", new HashSet<string>() { "POST" } },
    { "/log_conversation", new HashSet<string>() { "POST" } }
};


app.UseStaticFiles();

app.UseRouting();

app.UseWhen(context =>
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
}, action => { app.MapReverseProxy(); });

app.MapControllers();

app.Run();