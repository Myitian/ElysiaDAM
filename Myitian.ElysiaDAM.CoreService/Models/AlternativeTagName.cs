using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Myitian.ElysiaDAM.CoreService.Models;

public sealed class AlternativeTagName : IEntityTypeConfiguration<AlternativeTagName>
{
    public string Name { get; set; } = string.Empty;
    public long TagId { get; set; }

    public Tag? Tag { get; set; }

    public void Configure(EntityTypeBuilder<AlternativeTagName> builder)
    {
        builder
            .HasKey(@this => new { @this.Name, @this.TagId });
        builder
            .HasOne(@this => @this.Tag)
            .WithMany(that => that.AlternativeTagNames)
            .HasForeignKey(@this => @this.TagId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}