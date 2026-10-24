using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TemplateProject.Application.Common.Abstractions;
using TemplateProject.Infrastructure.Identity;

namespace TemplateProject.Infrastructure.Persistence;

public class DatabaseContext : IdentityDbContext<
    ApplicationUser, ApplicationRole, Guid,
    IdentityUserClaim<Guid>,
    ApplicationUserRole,
    IdentityUserLogin<Guid>,
    IdentityRoleClaim<Guid>,
    IdentityUserToken<Guid>>, IUnitOfWork
{
    private const string IdentitySchema = "identity";

    private readonly ICurrentUserProvider _currentUserProvider;

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    public DatabaseContext(
        ICurrentUserProvider currentUserProvider,
        DbContextOptions opt) : base(opt)
    {
        _currentUserProvider = currentUserProvider;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureOpenIdDictTables(modelBuilder);
        ConfigureIdentityTables(modelBuilder);
        ConfigureDictionaries(modelBuilder);
        ConfigureBusinessTables(modelBuilder);
    }

    private void ConfigureBusinessTables(ModelBuilder modelBuilder)
    {
    }

    private void ConfigureDictionaries(ModelBuilder modelBuilder)
    {
    }
    
    private void ConfigureIdentityTables(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>().ToTable("Users", IdentitySchema);
        builder.Entity<ApplicationRole>().ToTable("Roles", IdentitySchema);
        builder.Entity<ApplicationUserRole>(e =>
        {
            e.ToTable("UserRoles", IdentitySchema);
            e.HasKey(ur => new { ur.UserId, ur.RoleId });
            e.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);
            e.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);
        });
        builder.Entity<RefreshToken>(e =>
        {
            e.ToTable("RefreshTokens", IdentitySchema);
            e.HasKey(x => x.Id);
            e.Property(x => x.TokenHash).HasMaxLength(64);
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.HasIndex(x => x.UserId);
            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", IdentitySchema);
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", IdentitySchema);
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", IdentitySchema);
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", IdentitySchema);

        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.NormalizedEmail)
            .HasFilter($"\"{nameof(ApplicationUser.EmailConfirmed)}\" = true")
            .IsUnique();

        builder.Entity<ApplicationUser>()
            .HasIndex(u => u.PhoneNumber)
            .HasFilter($"\"{nameof(ApplicationUser.PhoneNumberConfirmed)}\" = true")
            .IsUnique();
    }

    private void ConfigureOpenIdDictTables(ModelBuilder builder)
    {
    }
}