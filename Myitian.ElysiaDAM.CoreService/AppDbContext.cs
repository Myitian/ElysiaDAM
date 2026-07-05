using Microsoft.EntityFrameworkCore;
using Myitian.ElysiaDAM.CoreService.Models;
using System.Reflection;

namespace Myitian.ElysiaDAM.CoreService;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<AssetItem> AssetItems { get; set; }
    public DbSet<AssetSource> AssetSources { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<AlternativeTagName> AlternativeTagNames { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}