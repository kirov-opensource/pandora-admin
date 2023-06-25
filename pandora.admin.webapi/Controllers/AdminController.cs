using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using pandora.admin.webapi.DataAccess;
using pandora.admin.webapi.Entities;

namespace pandora.admin.webapi.Controllers;

[ApiController]
[Route("/pandora_admin")]
public class AdminController : ControllerBase
{
    private readonly PandoraAdminContext _dbContext;

    private readonly Logger<AdminController> _logger;

    //TODO,fill user id
    private int userId { get; set; } = 0;

    public AdminController(PandoraAdminContext dbContext, Logger<AdminController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #region user

    /// <summary>
    /// Get all users,no pagination
    /// </summary>
    /// <returns></returns>
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _dbContext.Users.ToListAsync();
        return Ok(users);
    }

    /// <summary>
    /// create user
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    [HttpPost("user")]
    public async Task<IActionResult> CreateUser([FromBody] User user)
    {
        user.CreateTime = DateTime.Now;
        user.CreateUserId = userId;
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("user/{id}")]
    public async Task<IActionResult> DeleteUser([FromRoute] int id)
    {
        var user = await _dbContext.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        user.DeleteTime = DateTime.Now;
        user.DeleteUserId = userId;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("user/{id}")]
    public async Task<IActionResult> UpdateUser([FromRoute] int id, [FromBody] User user)
    {
        var userInDb = await _dbContext.Users.FindAsync(id);
        if (userInDb == null)
        {
            return NotFound();
        }

        userInDb.Email = user.Email;
        userInDb.Password = user.Password;
        userInDb.Role = user.Role;
        userInDb.Remark = user.Remark;
        userInDb.UpdateTime = DateTime.Now;
        userInDb.UpdateUserId = userId;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    #endregion


    #region access token

    [HttpGet("access_tokens")]
    public async Task<IActionResult> GetTokens()
    {
        var tokens = await _dbContext.AccessTokens.ToListAsync();
        return Ok(tokens);
    }

    [HttpPost("access_token")]
    public async Task<IActionResult> CreateToken([FromBody] AccessToken token)
    {
        await _dbContext.AccessTokens.AddAsync(token);
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPut("access_token/{id}")]
    public async Task<IActionResult> UpdateToken([FromRoute] int id, [FromBody] AccessToken token)
    {
        var tokenInDb = await _dbContext.AccessTokens.FindAsync(id);
        if (tokenInDb == null)
        {
            return NotFound();
        }

        tokenInDb.AccessToken1 = token.AccessToken1;
        tokenInDb.ExpireTime = token.ExpireTime;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("access_token/{id}")]
    public async Task<IActionResult> DeleteToken([FromRoute] int id)
    {
        var token = await _dbContext.AccessTokens.FindAsync(id);
        if (token == null)
        {
            return NotFound();
        }

        token.DeleteTime = DateTime.Now;
        token.DeleteUserId = userId;
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    [HttpPatch("access_token/{id}/refresh")]
    public async Task<IActionResult> RefershToken([FromRoute] int id)
    {
        var token = await _dbContext.AccessTokens.FindAsync(id);
        if (token == null)
        {
            return NotFound();
        }

        //TODO,refresh token
        
        await _dbContext.SaveChangesAsync();
        return Ok();
    }

    #endregion
}