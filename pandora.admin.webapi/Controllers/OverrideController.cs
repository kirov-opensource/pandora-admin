using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pandora.admin.webapi.DataAccess;
using pandora.admin.webapi.Models.Auth;

namespace pandora.admin.webapi.Controllers;

[ApiController]
public class OverrideController : ControllerBase
{
    private readonly ILogger<OverrideController> _logger;
    private readonly PandoraAdminContext _dbContext;

    public OverrideController(ILogger<OverrideController> logger, PandoraAdminContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        var c = dbContext.Users.ToList();
        
    }

    [HttpPost("/auth/login")]
    public async Task<IActionResult> Login([FromForm] LoginRequestModel loginModel)
    {
        Response.Cookies.Append("access-token", Environment.GetEnvironmentVariable("access-token"), new CookieOptions()
        {
            Expires = DateTimeOffset.Now.AddDays(30),
            Path = "/"
        });

        return Redirect("/");
    }

    [HttpPost("/log_conversation")]
    public async Task<IActionResult> LogConversation([FromBody] string[] conversationIds)
    {
        return NoContent();
    }
}