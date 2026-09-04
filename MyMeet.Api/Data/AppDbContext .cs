using Microsoft.EntityFrameworkCore;
using MyMeet.Api.Entities;

namespace MyMeet.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
        .Property(u => u.Id)
        .ValueGeneratedOnAdd();
        
        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.Provider, u.ProviderUserId })
            .IsUnique();
    }
}