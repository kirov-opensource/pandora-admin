using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using pandora.admin.webapi.DataAccess;
using pandora.admin.webapi.Models.Auth;

namespace pandora.admin.webapi.Controllers;

[ApiController]
public class OverrideController : ControllerBase
{
    private readonly ILogger<OverrideController> _logger;
    private readonly PandoraAdminContext _dbContext;
    private readonly IMemoryCache _cahce;

    public OverrideController(ILogger<OverrideController> logger, PandoraAdminContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
        // var c = dbContext.Users.ToList();
    }

    [HttpPost("/auth/login")]
    public async Task<IActionResult> Login([FromForm] LoginRequestModel loginModel)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(c => c.Email.Equals(loginModel.username));
        if (user == null)
        {
            throw new Exception("user not found or password error");
        }

        if (!user.Password.Equals(loginModel.password))
        {
            throw new Exception("user not found or password error");
        }

        Response.Cookies.Append("access-token", user.UserToken, new CookieOptions()
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