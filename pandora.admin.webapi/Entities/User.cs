using System;
using System.Collections.Generic;

namespace pandora.admin.webapi.Entities;

public partial class User
{
    public int Id { get; set; }

    public string Password { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string? Remark { get; set; }

    public bool? IsAdmin { get; set; }

    public int? DefaultAccessTokenId { get; set; }

    public DateTime? CreateTime { get; set; }

    public int? CreateUserId { get; set; }

    public DateTime? UpdateTime { get; set; }

    public int? UpdateUserId { get; set; }

    public DateTime? DeleteTime { get; set; }

    public int? DeleteUserId { get; set; }
}
