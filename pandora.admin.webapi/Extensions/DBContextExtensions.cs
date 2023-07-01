using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Pandora.Admin.WebAPI.DataAccess;

namespace Pandora.Admin.WebAPI.Extensions;

public static class DBContextExtensions
{
    public static async Task<string> GetUserOriginToken(this PandoraAdminContext _dbContext,
        IMemoryCache _cache,
        int userId)
    {
        string tokenKey = $"USER:{userId}:ORIGINTOKEN";
        var hasValue = _cache.TryGetValue<string>(tokenKey, out string? originToken);
        if (hasValue)
        {
            return originToken;
        }

        var userDefaultAccessTokenId = await _dbContext.Users.Where(c => c.Id == userId)
            .Select(c => c.DefaultAccessTokenId).FirstOrDefaultAsync();

        originToken = await _dbContext.AccessTokens.Where(c => c.Id == userDefaultAccessTokenId)
            .Select(c => c.AccessToken1).FirstOrDefaultAsync();

        _cache.Set(tokenKey, originToken);
        return originToken;
    }
}