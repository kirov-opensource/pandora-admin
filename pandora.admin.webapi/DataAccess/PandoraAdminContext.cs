using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using pandora.admin.webapi.Entities;

namespace pandora.admin.webapi.DataAccess;

public partial class PandoraAdminContext : DbContext
{
    public PandoraAdminContext()
    {
    }

    public PandoraAdminContext(DbContextOptions<PandoraAdminContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AccessToken> AccessTokens { get; set; }

    public virtual DbSet<Conversation> Conversations { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AccessToken>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("access_tokens");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessToken1)
                .HasMaxLength(500)
                .HasColumnName("access_token");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("create_time");
            entity.Property(e => e.CreateUserId).HasColumnName("create_user_id");
            entity.Property(e => e.DeleteTime)
                .HasColumnType("timestamp")
                .HasColumnName("delete_time");
            entity.Property(e => e.DeleteUserId).HasColumnName("delete_user_id");
            entity.Property(e => e.Email)
                .HasMaxLength(500)
                .HasColumnName("email");
            entity.Property(e => e.ExpireTime)
                .HasColumnType("timestamp")
                .HasColumnName("expire_time");
            entity.Property(e => e.Password)
                .HasMaxLength(500)
                .HasColumnName("password");
            entity.Property(e => e.RefreshToken)
                .HasMaxLength(500)
                .HasColumnName("refresh_token");
            entity.Property(e => e.Remark)
                .HasMaxLength(500)
                .HasColumnName("remark");
            entity.Property(e => e.UpdateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("update_time");
            entity.Property(e => e.UpdateUserId).HasColumnName("update_user_id");
        });

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("conversations");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessTokenId).HasColumnName("access_token_id");
            entity.Property(e => e.ConversationId)
                .HasMaxLength(500)
                .HasColumnName("conversation_id");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("create_time");
            entity.Property(e => e.CreateUserId).HasColumnName("create_user_id");
            entity.Property(e => e.DeleteTime)
                .HasColumnType("timestamp")
                .HasColumnName("delete_time");
            entity.Property(e => e.DeleteUserId).HasColumnName("delete_user_id");
            entity.Property(e => e.Remark)
                .HasMaxLength(500)
                .HasColumnName("remark");
            entity.Property(e => e.UpdateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("update_time");
            entity.Property(e => e.UpdateUserId).HasColumnName("update_user_id");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("users");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("create_time");
            entity.Property(e => e.CreateUserId).HasColumnName("create_user_id");
            entity.Property(e => e.DefaultAccessTokenId).HasColumnName("default_access_token_id");
            entity.Property(e => e.DeleteTime)
                .HasColumnType("timestamp")
                .HasColumnName("delete_time");
            entity.Property(e => e.DeleteUserId).HasColumnName("delete_user_id");
            entity.Property(e => e.Email)
                .HasMaxLength(500)
                .HasColumnName("email");
            entity.Property(e => e.IsAdmin)
                .HasDefaultValueSql("'0'")
                .HasColumnName("is_admin");
            entity.Property(e => e.Password)
                .HasMaxLength(500)
                .HasColumnName("password");
            entity.Property(e => e.Remark)
                .HasMaxLength(500)
                .HasColumnName("remark");
            entity.Property(e => e.Role)
                .HasMaxLength(500)
                .HasColumnName("role");
            entity.Property(e => e.UpdateTime)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp")
                .HasColumnName("update_time");
            entity.Property(e => e.UpdateUserId).HasColumnName("update_user_id");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
