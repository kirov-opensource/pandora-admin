using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Pandora.Admin.WebAPI.DataAccess;
using Pandora.Admin.WebAPI.Models;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Pandora.Admin.WebAPI.Middlewares;

using Microsoft.Net.Http.Headers;
using System.Text;
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
        #region token convert

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

        #endregion

        //获取原始的Response Body
        var originalResponseBody = context.Response.Body;
        {
            // Console.WriteLine("LoggingMiddleware invoked.");

            // var originalBody = context.Response.Body;
            // using var newBody = new MemoryStream();
            // context.Response.Body = newBody;

            // try
            // {
            //     await this._next(context);
            // }
            // finally
            // {
            //     newBody.Seek(0, SeekOrigin.Begin);
            //     var bodyText = await new StreamReader(new BrotliStream(context.Response.Body, CompressionMode.Decompress)).ReadToEndAsync();
            //     Console.WriteLine($"LoggingMiddleware: {bodyText}");
            //     newBody.Seek(0, SeekOrigin.Begin);
            //     await newBody.CopyToAsync(originalBody);
            // }
        }

        try
        {
            //声明一个MemoryStream替换Response Body
            using var swapStream = new MemoryStream();
            context.Response.Body = swapStream;

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
            if (context.Response.ContentType == "application/json" && context.Response.StatusCode == 200 && isSubUser)
            {
                var bodyText =
                    await new StreamReader(new BrotliStream(context.Response.Body, CompressionMode.Decompress))
                        .ReadToEndAsync();
                // var bodyText = await new StreamReader(new BrotliStream(swapStream, CompressionMode.Decompress)).ReadToEndAsync();
                // TODO: 仅仅替换会话列表
                if (context.Request.Path.StartsWithSegments("/gpt/api/conversations") &&
                    context.Request.Method == "GET")
                {
                    var parsedBody = JsonSerializer.Deserialize<GetConversationResponseModel>(bodyText);
                    var userConversationIds = await GetConverstaionListBySubToken(_dbContext, userToken);
                    parsedBody.Items = parsedBody.Items.Where(c => userConversationIds.Contains(c.Id)).ToList();
                    var newBodyText = JsonSerializer.Serialize(parsedBody);
                    //write json to response body
                    // context.Response.Headers["Content-Encoding"] = "none";
                    // await context.Response.WriteAsync(newBodyText);


                    // 重新压缩 02
                    // var bytes = Encoding.UTF8.GetBytes(newBodyText);
                    // var outputStream = new MemoryStream();
                    // var compressor = new BrotliStream(outputStream, CompressionMode.Compress, true);
                    // await new MemoryStream(bytes).CopyToAsync(compressor);
                    // await outputStream.CopyToAsync(originalResponseBody);

                    var bytes = Encoding.UTF8.GetBytes(newBodyText);
                    await new MemoryStream(bytes).CopyToAsync(originalResponseBody);
                    context.Response.Body = originalResponseBody;
                    // await context.Response.WriteAsync(newBodyText);
                    // await context.Response.Body.FlushAsync(); //Error: Decompression failed


                    // 重新压缩 01
                    // var bytes = Encoding.UTF8.GetBytes(newBodyText);
                    // var compressBytes = new Span<byte>();
                    // var newStream = new BrotliStream(new MemoryStream(bytes), CompressionMode.Compress);
                    // BrotliEncoder.TryCompress(bytes,out compressBytes,)
                    // await newStream.CopyToAsync(originalResponseBody); //System.NotSupportedException: Stream does not support reading.

                    // 直接写
                    // context.Response.Headers["Content-Encoding"] = "";
                    // await context.Response.WriteAsync(newBodyText);
                    // await context.Response.Body.FlushAsync();
                    // return;
                    context.Response.OnStarting(() =>
                    {
                        return context.Response.WriteAsync(newBodyText);
                    });
                }
                else
                {
                    //重置标识位
                    swapStream.Seek(0, SeekOrigin.Begin);
                    await swapStream.CopyToAsync(originalResponseBody);
                }

                _logger.LogInformation(bodyText);
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

    private async Task<HashSet<string>> GetConverstaionListBySubToken(PandoraAdminContext _dbContext, string userToken)
    {
        return new HashSet<string>();
        
        var userId = await _dbContext.Users.Where(c => c.UserToken == userToken)
            .Select(c => c.Id).FirstOrDefaultAsync();
        var conversationIds = await _dbContext.Conversations.Where(c => c.CreateUserId == userId)
            .Select(c => c.ConversationId).ToListAsync();
        return new HashSet<string>(conversationIds);
    }

    private async Task<string> ConvertSubTokenToOriginToken(PandoraAdminContext _dbContext, string userToken)
    {

        return userToken;
        
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
