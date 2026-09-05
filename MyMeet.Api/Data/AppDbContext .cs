using Microsoft.EntityFrameworkCore;
using MyMeet.Api.Entities;

namespace MyMeet.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
    public DbSet<Meeting> Meetings => Set<Meeting>();
    public DbSet<MeetingParticipant> MeetingParticipants => Set<MeetingParticipant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<User>()
            .HasIndex(u => new { u.Provider, u.ProviderUserId })
            .IsUnique();

        modelBuilder.Entity<Meeting>()
            .Property(m => m.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Meeting>()
            .HasIndex(m => m.Code)
            .IsUnique();

        modelBuilder.Entity<Meeting>()
            .HasOne(m => m.HostUser)
            .WithMany()
            .HasForeignKey(m => m.HostUserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MeetingParticipant>()
            .Property(mp => mp.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<MeetingParticipant>()
            .HasIndex(mp => new { mp.MeetingId, mp.UserId })
            .IsUnique(); 

        modelBuilder.Entity<MeetingParticipant>()
            .HasOne(mp => mp.Meeting)
            .WithMany(m => m.Participants)
            .HasForeignKey(mp => mp.MeetingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MeetingParticipant>()
            .HasOne(mp => mp.User)
            .WithMany()
            .HasForeignKey(mp => mp.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}