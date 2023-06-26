using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using pandora.admin.webapi.DataAccess;

namespace pandora.admin.webapi.Middlewares;

using System.Threading.Tasks;

public class CustomMiddleware
{
    const string TOKEN_HEADER_NAME = "X-Authorization";
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomMiddleware> _logger;

    private readonly IMemoryCache _cache;
    // private readonly PandoraAdminContext _dbContext;

    public CustomMiddleware(RequestDelegate next, ILogger<CustomMiddleware> logger, IMemoryCache cache)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
        // _dbContext = dbContext;
    }

    public async Task Invoke(HttpContext context)
    {
        var _dbContext = context.RequestServices.GetRequiredService<PandoraAdminContext>();

        bool isSubUser = false;
        string userToken = string.Empty;
        context.Request.Headers.TryGetValue(TOKEN_HEADER_NAME, out StringValues authToken);
        if (authToken.Count > 0)
        {
            isSubUser = true;
            userToken = authToken[0].Replace("Bearer ", "");
            if (userToken.StartsWith("fk-"))
            {
                var originToken = await ConvertSubTokenToOriginToken(_dbContext, userToken);
                context.Request.Headers[TOKEN_HEADER_NAME] = new StringValues($"Bearer {originToken}");
            }
        }

        // convert auth token
        await _next.Invoke(context);

        if (isSubUser)
        {
            //filter conversation
        }

        //获取原始的Response Body
        // var originalResponseBody = context.Response.Body;
        // try
        // {
        //     //声明一个MemoryStream替换Response Body
        //     using var swapStream = new MemoryStream();
        //     context.Response.Body = swapStream;
        //     // 调用下一个中间件（请求转发）
        //     await _next.Invoke(context);
        //
        //     //重置标识位
        //     context.Response.Body.Seek(0, SeekOrigin.Begin);
        //     //把替换后的Response Body复制到原始的Response Body
        //     await swapStream.CopyToAsync(originalResponseBody);
        //
        //     if (context.Response.ContentType == "text/event-stream; charset=utf-8")
        //     {
        //         var bytes = swapStream.ToArray();
        //         var responseText = Encoding.UTF8.GetString(bytes);
        //         var conversationId = GetConversationId(responseText);
        //         _logger.LogInformation(conversationId);
        //     }
        // }
        // finally
        // {
        //     //无论异常与否都要把原始的Body给切换回来
        //     context.Response.Body = originalResponseBody;
        // }
    }

    private async Task<string> ConvertSubTokenToOriginToken(PandoraAdminContext _dbContext, string userToken)
    {
        string tokenKey = $"USER:{userToken}:ORIGINTOKEN";
        var hasValue = _cache.TryGetValue<string>(tokenKey, out string? originToken);
        if (hasValue)
        {
            return originToken;
        }

        var userDefaultAccessTokenId = await _dbContext.Users.Where(c => c.UserToken == userToken)
            .Select(c => c.DefaultAccessTokenId).FirstOrDefaultAsync();
        originToken = await _dbContext.AccessTokens.Where(c => c.Id == userDefaultAccessTokenId)
            .Select(c => c.AccessToken1).FirstOrDefaultAsync();

        _cache.Set($"USER:{userToken}:ORIGINTOKEN", originToken);
        return originToken;
    }

    private string GetConversationId(string text)
    {
        try
        {
            Regex regex = new Regex("\"conversation_id\": \"(.*?)\"");
            Match match = regex.Match(text);
            if (match.Success)
            {
                _logger.LogInformation("Conversation ID: " + match.Groups[1].Value);
                return match.Groups[1].Value;
            }
            else
            {
                _logger.LogInformation("No match found.");
            }

            return null;
        }
        catch (Exception e)
        {
            return null;
        }
    }
}