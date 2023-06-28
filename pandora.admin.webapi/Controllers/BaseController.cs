using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Pandora.Admin.WebAPI.Controllers
{
    [Authorize]
    public abstract class BaseController : ControllerBase
    {
        protected ILogger Logger;

        public BaseController(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger(this.GetType());
        }
    }
}
