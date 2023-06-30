using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Pandora.Admin.WebAPI.DataAccess;
using Pandora.Admin.WebAPI.Models.Auth;

namespace Pandora.Admin.WebAPI.Controllers;

[ApiController]
public class OverrideController : BaseController
{
    private readonly PandoraAdminContext _dbContext;
    private readonly IMemoryCache _cahce;

    public OverrideController(PandoraAdminContext dbContext, ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        _dbContext = dbContext;
    }

    [HttpPost("/auth/login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromForm] LoginRequestModel loginModel)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(c => c.Email.Equals(loginModel.username));

        if (user == null)
        {
            throw new Exception("user not found or password error");
        }

        if (!BCrypt.Net.BCrypt.Verify(loginModel.password,user.Password))
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