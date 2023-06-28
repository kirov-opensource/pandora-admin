using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pandora.Admin.WebAPI.DataAccess;
using Pandora.Admin.WebAPI.Extensions;
using Pandora.Admin.WebAPI.Models.Session;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pandora.Admin.WebAPI.Controllers
{
    [Route("api/sessions")]
    [ApiController]
    public class SessionController : BaseController
    {
        private readonly PandoraAdminContext context;
        private readonly IConfiguration configuration;

        public SessionController(ILoggerFactory loggerFactory,
            PandoraAdminContext context,
            IConfiguration configuration) : base(loggerFactory)
        {
            this.context = context;
            this.configuration = configuration;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<string> Login([FromBody] UserLoginModel model)
        {
            // TODO fix this
            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == model.Email);
            if (user == null)
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

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration.GetSection("JWTSecurityKey").Get<string>()));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(string.Empty,
              string.Empty,
              claims,
              expires: DateTime.Now.AddDays(7),
              signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
