using System;
using System.Collections.Generic;

namespace Pandora.Admin.WebAPI.Entities;

public partial class AccessToken
{
    public int Id { get; set; }

    public string AccessToken1 { get; set; } = null!;

    public string RefreshToken { get; set; } = null!;

    public string? Email { get; set; }

    public string? Password { get; set; }

    public string? Remark { get; set; }

    public DateTime? ExpireTime { get; set; }

    public DateTime? CreateTime { get; set; }

    public int? CreateUserId { get; set; }

    public DateTime? UpdateTime { get; set; }

    public int? UpdateUserId { get; set; }

    public DateTime? DeleteTime { get; set; }

    public int? DeleteUserId { get; set; }
}
