using Microsoft.EntityFrameworkCore;
using UM.Core.entity;

namespace UM.Core.config
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<FileEntity> Files => Set<FileEntity>();
        public DbSet<Server> Servers => Set<Server>();
        public DbSet<ServerMember> ServerMembers => Set<ServerMember>();
        public DbSet<ServerRole> ServerRoles => Set<ServerRole>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<ServerSanction> ServerSanctions => Set<ServerSanction>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Channel> Channels => Set<Channel>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== User =====
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.PublicCode).IsUnique();
            });

            // ===== FileEntity =====
            modelBuilder.Entity<FileEntity>(entity =>
            {
                entity.ToTable("Files");
                entity.HasKey(f => f.Id);
                entity.HasOne(f => f.Owner)
                    .WithMany()
                    .HasForeignKey(f => f.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Server =====
            modelBuilder.Entity<Server>(entity =>
            {
                entity.ToTable("Servers");
                entity.HasKey(s => s.Id);
                entity.Property(s => s.Name).HasColumnType("NVARCHAR(100)");

                entity.HasOne(s => s.Owner)
                    .WithMany(u => u.OwnedServers)
                    .HasForeignKey(s => s.OwnerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.IconFile)
                    .WithMany()
                    .HasForeignKey(s => s.IconFileId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== ServerMember =====
            modelBuilder.Entity<ServerMember>(entity =>
            {
                entity.ToTable("ServerMembers");
                entity.HasKey(sm => sm.Id);

                // Filtered index duy nhất: 1 user chỉ có tối đa 1 membership ACTIVE trong 1 server
                entity.HasIndex(sm => new { sm.ServerId, sm.UserId })
                    .HasFilter("([LeftAt] IS NULL)")
                    .IsUnique();

                entity.HasOne(sm => sm.Server)
                    .WithMany(s => s.ServerMembers)
                    .HasForeignKey(sm => sm.ServerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(sm => sm.User)
                    .WithMany(u => u.ServerMemberships)
                    .HasForeignKey(sm => sm.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sm => sm.ServerRole)
                    .WithMany(sr => sr.ServerMembers)
                    .HasForeignKey(sm => sm.ServerRoleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(sm => sm.BannedByUser)
                    .WithMany()
                    .HasForeignKey(sm => sm.BannedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== ServerRole =====
            modelBuilder.Entity<ServerRole>(entity =>
            {
                entity.ToTable("ServerRoles");
                entity.HasKey(sr => sr.Id);
                entity.HasIndex(sr => new { sr.ServerId, sr.Name }).IsUnique();

                entity.HasOne(sr => sr.Server)
                    .WithMany(s => s.ServerRoles)
                    .HasForeignKey(sr => sr.ServerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Permission =====
            modelBuilder.Entity<Permission>(entity =>
            {
                entity.ToTable("Permissions");
                entity.HasKey(p => p.Id);
                entity.HasIndex(p => p.Code).IsUnique();
            });

            // ===== RolePermission (Composite PK: ServerRoleId, PermissionId) =====
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.ToTable("RolePermissions");
                entity.HasKey(rp => new { rp.ServerRoleId, rp.PermissionId });

                entity.HasOne(rp => rp.ServerRole)
                    .WithMany(sr => sr.RolePermissions)
                    .HasForeignKey(rp => rp.ServerRoleId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== Category =====
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.Id);

                entity.HasOne(c => c.Server)
                    .WithMany(s => s.Categories)
                    .HasForeignKey(c => c.ServerId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===== Channel =====
            modelBuilder.Entity<Channel>(entity =>
            {
                entity.ToTable("Channels");
                entity.HasKey(ch => ch.Id);

                entity.HasOne(ch => ch.Server)
                    .WithMany(s => s.Channels)
                    .HasForeignKey(ch => ch.ServerId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ch => ch.Category)
                    .WithMany(c => c.Channels)
                    .HasForeignKey(ch => ch.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ===== ServerSanction =====
            modelBuilder.Entity<ServerSanction>(entity =>
            {
                entity.ToTable("ServerSanctions");
                entity.HasKey(ss => ss.Id);

                entity.HasOne(ss => ss.Server)
                    .WithMany(s => s.ServerSanctions)
                    .HasForeignKey(ss => ss.ServerId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ss => ss.Issuer)
                    .WithMany()
                    .HasForeignKey(ss => ss.IssuedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(ss => ss.Revoker)
                    .WithMany()
                    .HasForeignKey(ss => ss.RevokedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
