using Microsoft.EntityFrameworkCore;

namespace ZeferiniPersonApi.Models;

public class EventsDbContext : DbContext
{
    public EventsDbContext(DbContextOptions<EventsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AggregateId).IsRequired();
            entity.Property(e => e.AggregateType).IsRequired();
            entity.Property(e => e.EventType).IsRequired();
            entity.Property(e => e.EventData).IsRequired();
            entity.Property(e => e.Version).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            // Se Metadata for um dicionário, pode ser necessário configurar como JSON
            // Exemplo para SQL Server 2016+ e EF Core 8:
            // entity.Property(e => e.Metadata).HasColumnType("nvarchar(max)").HasConversion(
            //     v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
            //     v => JsonSerializer.Deserialize<Dictionary<string, object?>>(v, (JsonSerializerOptions)null));
        });
    }
}