using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Myitian.ElysiaDAM.CoreService.Models;

public sealed class AssetSource : IEntityTypeConfiguration<AssetSource>
{
    public long Id { get; set; }
    public byte[]? Hash { get; set; }
    public long Size { get; set; }
    public DateTimeOffset? CreateTime { get; set; }
    public DateTimeOffset? UpdateTime { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long? ParentId { get; set; }
    public int? Index { get; set; }
    public long? AssetItemId { get; set; }

    // nav
    public List<Tag>? Tags { get; set; }
    public AssetSource? Parent { get; set; }
    public List<AssetSource>? Children { get; set; }
    public AssetItem? AssetItem { get; set; }
    public List<ExtraMetadataBytes>? ExtraMetadata { get; set; }

    public void Configure(EntityTypeBuilder<AssetSource> builder)
    {
        builder
            .HasKey(@this => @this.Id);
        builder
            .HasMany(@this => @this.Children)
            .WithOne(that => that.Parent)
            .HasForeignKey(@this => @this.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne(@this => @this.AssetItem)
            .WithMany(that => that.Sources)
            .HasForeignKey(@this => @this.AssetItemId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasMany(@this => @this.Tags)
            .WithMany(that => that.AssetSources)
            .UsingEntity(b => b.ToTable("Asset_source_tag"));
    }

    public sealed class ExtraMetadataBytes : IEntityTypeConfiguration<ExtraMetadataBytes>
    {
        public long Id { get; set; }
        public long AssetSourceId { get; set; }
        public string Namespace { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsSensitive { get; set; }
        public byte[] Content { get; set; } = [];

        // nav
        public AssetSource? Parent { get; set; }

        public void Configure(EntityTypeBuilder<ExtraMetadataBytes> builder)
        {
            builder
                .HasKey(@this => @this.Id);
            builder
                .HasOne(@this => @this.Parent)
                .WithMany(that => that.ExtraMetadata)
                .HasForeignKey(@this => @this.AssetSourceId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}