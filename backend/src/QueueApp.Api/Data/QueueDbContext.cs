using Microsoft.EntityFrameworkCore;
using QueueApp.Api.Models;

namespace QueueApp.Api.Data;

public class QueueDbContext : DbContext
{
    public QueueDbContext(DbContextOptions<QueueDbContext> options) : base(options)
    {
    }

    public DbSet<QueueSetting> QueueSettings => Set<QueueSetting>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<QueueSetting>(entity =>
        {
            entity.ToTable("QueueSettings");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.CurrentIndex).IsRequired();
            entity.Property(e => e.LastActive).IsRequired();
        });
    }
}
