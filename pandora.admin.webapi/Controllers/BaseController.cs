using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pandora.Admin.WebAPI.DataAccess;
using Pandora.Admin.WebAPI.Entities;
using Pandora.Admin.WebAPI.Extensions;

namespace Pandora.Admin.WebAPI.Controllers
{
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        protected ILogger Logger;
        private PandoraAdminContext _dbContext;

        public BaseController(ILoggerFactory loggerFactory, PandoraAdminContext dbContext)
        {
            _dbContext = dbContext;
            Logger = loggerFactory.CreateLogger(this.GetType());
        }

        protected User CurrentUser
        {
            get
            {
                if (HttpContext.User == null)
                {
                    throw new UnauthorizedAccessException();
                }

                var userId = int.Parse(HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypesExtension.UserId)
                    .Value);

                var user = _dbContext.Users.FirstOrDefault(c => c.Id == userId);
                return user;
            }
        }
    }
}