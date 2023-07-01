using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Pandora.Admin.WebAPI.DataAccess;
using Pandora.Admin.WebAPI.Extensions;
using Pandora.Admin.WebAPI.Models;
using Pandora.Admin.WebAPI.Models.Auth;

namespace Pandora.Admin.WebAPI.Controllers;

[ApiController]
public class OverrideController : BaseController
{
    private readonly PandoraAdminContext _dbContext;
    private readonly IMemoryCache _cahce;
    private readonly IConfiguration _configuration;


    public OverrideController(PandoraAdminContext dbContext, ILoggerFactory loggerFactory, IConfiguration configuration)
        : base(loggerFactory,
            dbContext)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    [HttpPost("/auth/login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromForm] LoginRequestModel loginModel)
    {
        // TODO fix this
        var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Email == loginModel.username);
        if (user == null)
        {
            throw new UnauthorizedAccessException();
        }

        // check password
        if (!BCrypt.Net.BCrypt.Verify(loginModel.password, user.Password))
        {
            throw new UnauthorizedAccessException();
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypesExtension.UserId, user.Id.ToString())
        };

        if (user.IsAdmin == true)
        {
            claims.Add(new Claim(ClaimTypesExtension.Administrator, string.Empty));
        }

        var key = new SymmetricSecurityKey(
            Encoding.ASCII.GetBytes(_configuration.GetSection("JWTSecurityKey").Get<string>()));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(string.Empty,
            string.Empty,
            claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds);

        Response.Cookies.Append("x-pandora-admin-token", new JwtSecurityTokenHandler().WriteToken(token),
            new CookieOptions()
            {
                Expires = DateTimeOffset.Now.AddDays(30),
                Path = "/"
            });

        Response.Cookies.Append("access-token", user.UserToken!, new CookieOptions()
        {
            Expires = DateTimeOffset.Now.AddDays(30),
            Path = "/"
        });

        return Redirect("/");
    }

    [HttpGet("/api/auth/session")]
    public async Task<IActionResult> Session()
    {
        var currentUser = CurrentUser;
        var accessToken = Request.Cookies["access-token"];
        GetSessionResponseModel result = new GetSessionResponseModel();
        result.Expires = DateTime.Now.AddYears(1).ToString("yyyy-MM-ddTHH:mm:ss");
        result.AuthProvider = "auth0";
        result.AccessToken = accessToken;
        result.User = new GetSessionResponseModel.User1()
        {
            Email = currentUser.Email,
            Groups = new string[] { },
            Id = currentUser.Id.ToString(),
            Image = "",
            Name = currentUser.Email,
            Picture = ""
        };
        return Ok(result);
    }


    [HttpPost("/log_conversation")]
    public async Task<IActionResult> LogConversation([FromBody] string[] conversationIds)
    {
        return NoContent();
    }
}