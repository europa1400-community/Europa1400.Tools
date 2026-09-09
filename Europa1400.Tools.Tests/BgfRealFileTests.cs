using System.IO.Compression;
using Europa1400.Tools.Structs.Bgf;

namespace Europa1400.Tools.Tests;

public class BgfRealFileTests
{
    [Fact]
    public void AllObjects_RuntimeTextureTableResolvesEveryTexturedFace()
    {
        using var archive = ZipFile.OpenRead(Path.Combine(EnvVariables.GameDirectoryPath, "Resources", "objects.bin"));
        var objects = archive.Entries.Where(e => e.FullName.EndsWith(".bgf", StringComparison.OrdinalIgnoreCase)).ToArray();
        Assert.NotEmpty(objects);
        foreach (var entry in objects)
        {
            using var source = entry.Open();
            using var stream = new MemoryStream();
            source.CopyTo(stream);
            stream.Position = 0;
            using var reader = new BinaryReader(stream);
            var bgf = BgfStruct.FromBytes(reader);
            Assert.Equal((int)bgf.MappingObject.TextureCount, bgf.Footer.TextureNames.Length);
            foreach (var face in bgf.MappingObject.PolygonMappings)
            {
                if (face.TextureIndex == byte.MaxValue) continue;
                Assert.True(bgf.ResolveFileTextureIndex(face.TextureIndex) >= 0,
                    $"{entry.FullName}: unresolved runtime material {face.TextureIndex}");
            }
        }
    }
}
