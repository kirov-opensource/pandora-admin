using System;
using System.Collections.Generic;

namespace pandora.admin.webapi.Entities;

public partial class Conversation
{
    public int Id { get; set; }

    public string ConversationId { get; set; } = null!;

    public int AccessTokenId { get; set; }

    public string Remark { get; set; } = null!;

    public DateTime? CreateTime { get; set; }

    public int? CreateUserId { get; set; }

    public DateTime? UpdateTime { get; set; }

    public int? UpdateUserId { get; set; }

    public DateTime? DeleteTime { get; set; }

    public int? DeleteUserId { get; set; }
}
