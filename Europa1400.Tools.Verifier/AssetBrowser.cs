using Europa1400.Tools.Pipeline.Assets;

namespace Europa1400.Tools.Verifier;

internal sealed class CatalogAsset
{
    public required FormatKind Format { get; init; }
    public required GameAsset Asset { get; init; }
    public string DisplayPath => Asset.RelativePath;
}

internal sealed class CatalogCategory
{
    public required string Name { get; init; }
    public required FormatKind Format { get; init; }
    public required IReadOnlyList<CatalogAsset> Assets { get; init; }
    public string Label => $"{Name} ({Assets.Count})";
}

internal sealed class CatalogLoadResult
{
    public required IReadOnlyList<CatalogCategory> Categories { get; init; }
    public required IReadOnlyList<string> Warnings { get; init; }
}

internal static class AssetBrowser
{
    public static CatalogLoadResult Load(string gamePath)
    {
        var categories = new List<CatalogCategory>();
        var warnings = new List<string>();

        foreach (var format in FormatCatalog.All)
            try
            {
                var assets = format.Discover(gamePath)
                    .Select(asset => new CatalogAsset { Format = format, Asset = asset })
                    .OrderBy(item => item.DisplayPath, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                categories.Add(new CatalogCategory { Name = format.Category, Format = format, Assets = assets });
            }
            catch (Exception ex)
            {
                warnings.Add($"{format.Category}: {ex.Message}");
                categories.Add(new CatalogCategory { Name = format.Category, Format = format, Assets = [] });
            }

        return new CatalogLoadResult { Categories = categories, Warnings = warnings };
    }
}
