using System.Text;
using System.Text.RegularExpressions;
using Ocelot.Logging;

namespace pandora.admin.webapi.Middlewares;

using System.Threading.Tasks;
using Ocelot.Middleware;
using Ocelot.Multiplexer;
using Ocelot.Responses;

public class CustomOcelotMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomOcelotMiddleware> _logger;

    public CustomOcelotMiddleware(RequestDelegate next, ILogger<CustomOcelotMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        //await _next.Invoke(context);
        //获取原始的Response Body
        var originalResponseBody = context.Response.Body;
        try
        {
            //声明一个MemoryStream替换Response Body
            using var swapStream = new MemoryStream();
            context.Response.Body = swapStream;
            // 调用下一个中间件（请求转发）
            await _next.Invoke(context);
        
            //重置标识位
            context.Response.Body.Seek(0, SeekOrigin.Begin);
            //把替换后的Response Body复制到原始的Response Body
            await swapStream.CopyToAsync(originalResponseBody);
        
            if (context.Response.ContentType == "text/event-stream; charset=utf-8")
            {
                var bytes = swapStream.ToArray();
                var responseText = Encoding.UTF8.GetString(bytes);
                var conversationId = GetConversationId(responseText);
                _logger.LogInformation(conversationId);
            }
        }
        finally
        {
            //无论异常与否都要把原始的Body给切换回来
            context.Response.Body = originalResponseBody;
        }
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