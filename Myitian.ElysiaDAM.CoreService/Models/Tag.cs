namespace Myitian.ElysiaDAM.CoreService.Models;

public sealed class Tag
{
    public long Id { get; set; }
    public string Namespace { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    // nav
    public List<AlternativeTagName>? AlternativeTagNames { get; set; }
    public List<AssetItem>? AssetItems { get; set; }
    public List<AssetSource>? AssetSources { get; set; }
}