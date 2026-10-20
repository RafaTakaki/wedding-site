using Microsoft.EntityFrameworkCore;
using WeddingGuests.Domain.Entities;

namespace WeddingGuests.Repository.Persistence;

public sealed class WeddingDbContext(DbContextOptions<WeddingDbContext> options) : DbContext(options)
{
    public DbSet<GuestInvitation> Invitations => Set<GuestInvitation>();
    public DbSet<Invitee> Invitees => Set<Invitee>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GuestInvitation>()
            .HasMany(invitation => invitation.People)
            .WithOne()
            .HasForeignKey(person => person.GuestInvitationId)
            .OnDelete(DeleteBehavior.Cascade);
        modelBuilder.Entity<GuestInvitation>().HasIndex(invitation => invitation.NormalizedName);
        modelBuilder.Entity<Invitee>().HasIndex(person => person.GuestInvitationId);
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await Database.EnsureCreatedAsync(cancellationToken);

        await Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await using var command = Database.GetDbConnection().CreateCommand();
            command.CommandText = "PRAGMA table_info('Invitees')";
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            var hasFreeCompanionColumn = false;
            while (await reader.ReadAsync(cancellationToken))
            {
                if (reader.GetString(1) == "IsFreeCompanion") hasFreeCompanionColumn = true;
            }

            await reader.DisposeAsync();
            if (!hasFreeCompanionColumn)
            {
                await using var alterCommand = Database.GetDbConnection().CreateCommand();
                alterCommand.CommandText = "ALTER TABLE Invitees ADD COLUMN IsFreeCompanion INTEGER NOT NULL DEFAULT 0";
                await alterCommand.ExecuteNonQueryAsync(cancellationToken);
            }
        }
        finally
        {
            await Database.CloseConnectionAsync();
        }
    }
}
