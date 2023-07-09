using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Pandora.Admin.WebAPI.DataAccess;
using Pandora.Admin.WebAPI.Models;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;
using Pandora.Admin.WebAPI.Extensions;

namespace Pandora.Admin.WebAPI.Middlewares;

using Microsoft.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

public class CustomMiddleware
{
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
        #region token convert

        var _dbContext = context.RequestServices.GetRequiredService<PandoraAdminContext>();

        var userId = int.Parse(context.User.Claims.FirstOrDefault(x => x.Type == ClaimTypesExtension.UserId)?.Value);

        var dbContext = context.RequestServices.GetService<PandoraAdminContext>();
        var cache = context.RequestServices.GetService<IMemoryCache>();

        var originToken = await dbContext.GetUserOriginToken(cache, userId);

        context.Request.Headers.Remove("Cookie");
        context.Request.Headers.Add("Cookie", $"access-token={originToken}");
        context.Request.Headers.Remove(Consts.TOKEN_HEADER_NAME);
        context.Request.Headers.Add(Consts.TOKEN_HEADER_NAME, $"Bearer {originToken}");

        #endregion

        //获取原始的Response Body
        var originalResponseBody = context.Response.Body;

        try
        {
            //声明一个MemoryStream替换Response Body
            using var swapStream = new MemoryStream();
            context.Response.Body = swapStream;

            context.Request.Headers[HeaderNames.AcceptEncoding] = "";

            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Remove(HeaderNames.ContentEncoding);
                return Task.CompletedTask;
            });

            // 调用下一个中间件（请求转发）
            await _next.Invoke(context);

            //重置标识位
            swapStream.Seek(0, SeekOrigin.Begin);
            //把替换后的Response Body复制到原始的Response Body
            // string bodyContent = await new StreamReader(swapStream).ReadToEndAsync();

            //只有子账户需要替换响应信息，避免错乱。
            if (context.Response.StatusCode == 200)
            {
                // TODO: 仅仅替换会话列表
                if (context.Request.Path.StartsWithSegments("/gpt/api/conversations")
                    && context.Request.Method == "GET"
                    && context.Response.ContentType == "application/json")
                {
                    var bodyText =
                        //new BrotliStream(, CompressionMode.Decompress)
                        await new StreamReader(context.Response.Body)
                            .ReadToEndAsync();
                    var parsedBody = JsonSerializer.Deserialize<GetConversationResponseModel>(bodyText);
                    var userConversationIds = await GetConverstaionListBySubToken(_dbContext, userId);
                    parsedBody.Items = parsedBody.Items.Where(c => userConversationIds.Contains(c.Id)).ToList();
                    var newBodyText = JsonSerializer.Serialize(parsedBody);

                    var bytes = Encoding.UTF8.GetBytes(newBodyText);
                    await new MemoryStream(bytes).CopyToAsync(originalResponseBody);
                    context.Response.Body = originalResponseBody;
                    _logger.LogInformation(bodyText);
                }
                else if (context.Request.Path.ToString().StartsWith("/_next/static/chunks/pages/app-")
                         && context.Request.Method == "GET"
                         && context.Response.ContentType == "application/javascript; charset=utf-8")
                {
                    var bodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
                    var newBodyText = bodyText.Replace(
                        @"onmessage:function(U){if(""[DONE]""===U.data)X.abort(),B({type:""done""});else if(""ping""===U.event);else try{",
                        @"onmessage:function(U){if(""[DONE]""===U.data)X.abort(),B({type:""done""});else if(""ping""===U.event);else try{ try{var data=JSON.parse(U.data);var conversationId=data.conversation_id;if(conversationId){if(!localStorage.getItem(conversationId)){localStorage.setItem(conversationId,'1');fetch('/log_conversation',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify([conversationId])})}}}catch(err){console.error(err)}");

                    var bytes = Encoding.UTF8.GetBytes(newBodyText);
                    context.Response.ContentLength += (newBodyText.Length - bodyText.Length);
                    await new MemoryStream(bytes).CopyToAsync(originalResponseBody);
                    // context.Response.StatusCode = 200;
                    // context.Response.ContentType = "text/plain";
                    context.Response.Body = originalResponseBody;
                }
                else
                {
                    //重置标识位
                    swapStream.Seek(0, SeekOrigin.Begin);
                    await swapStream.CopyToAsync(originalResponseBody);
                }
            }
            else
            {
                //重置标识位
                swapStream.Seek(0, SeekOrigin.Begin);
                await swapStream.CopyToAsync(originalResponseBody);
            }
        }
        finally
        {
            //无论异常与否都要把原始的Body给切换回来
            context.Response.Body = originalResponseBody;
        }
    }

    private async Task<HashSet<string>> GetConverstaionListBySubToken(PandoraAdminContext _dbContext, int userId)
    {
        var conversationIds = await _dbContext.Conversations.Where(c => c.CreateUserId == userId)
            .Select(c => c.ConversationId).ToListAsync();
        return new HashSet<string>(conversationIds);
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