using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Myitian.ElysiaDAM.CoreService.Models;

public sealed class AssetItem : IEntityTypeConfiguration<AssetItem>
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
    public bool IsSynthetic { get; set; }

    // nav
    public List<Tag>? Tags { get; set; }
    public List<AssetSource>? Sources { get; set; }
    public AssetItem? Parent { get; set; }
    public List<AssetItem>? Children { get; set; }
    public ExtraMetadataString? ExtraMetadataAsString { get; set; }
    public ExtraMetadataJson? ExtraMetadataAsJson { get; set; }

    public void Configure(EntityTypeBuilder<AssetItem> builder)
    {
        builder
            .ToTable("Asset_item");
        builder
            .HasKey(@this => @this.Id);
        builder
            .HasMany(@this => @this.Children)
            .WithOne(that => that.Parent)
            .HasForeignKey(@this => @this.ParentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasMany(@this => @this.Tags)
            .WithMany(that => that.AssetItems)
            .UsingEntity(b => b.ToTable("Asset_item_tag"));
    }

    public sealed class ExtraMetadataString : IEntityTypeConfiguration<ExtraMetadataString>
    {
        public long Id { get; set; }
        [Column(TypeName = "jsonb")]
        public string? ExtraMetadata { get; set; }

        // nav
        public AssetItem? AssetItem { get; set; }

        public void Configure(EntityTypeBuilder<ExtraMetadataString> builder)
        {
            builder
                .ToTable("Asset_item");
            builder
                .HasKey(@this => @this.Id);
            builder
                .HasOne(@this => @this.AssetItem)
                .WithOne(that => that.ExtraMetadataAsString)
                .HasForeignKey<ExtraMetadataString>(@this => @this.Id);
        }
    }
    public sealed class ExtraMetadataJson : IEntityTypeConfiguration<ExtraMetadataJson>
    {
        public long Id { get; set; }
        [Column(TypeName = "jsonb")]
        public JsonDocument? ExtraMetadata { get; set; }

        // nav
        public AssetItem? AssetItem { get; set; }

        public void Configure(EntityTypeBuilder<ExtraMetadataJson> builder)
        {
            builder
                .ToTable("Asset_item");
            builder
                .HasKey(@this => @this.Id);
            builder
                .HasOne(@this => @this.AssetItem)
                .WithOne(that => that.ExtraMetadataAsJson)
                .HasForeignKey<ExtraMetadataJson>(@this => @this.Id);
        }
    }
}