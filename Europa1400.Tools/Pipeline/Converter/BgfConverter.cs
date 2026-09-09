using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Europa1400.Tools.Pipeline;
using Europa1400.Tools.Structs.Txs;
using Europa1400.Tools.Gltf;
using Europa1400.Tools.Structs.Bgf;
using ImageMagick;
using ImageMagick.Formats;

namespace Europa1400.Tools.Pipeline.Converter;

public class BgfConverter
{
    public required string PathToGameFiles { get; init; }
    public required string TargetDirectory { get; init; }
    private Dictionary<string, string> ExtractedTextures { get; set; }
    private Dictionary<string, ExtractedObject> ExtractedObjects { get; set; }
    private string TexturesBinPath => Path.Combine(PathToGameFiles, "Resources\\textures.bin");
    private string ObjectsBinPath => Path.Combine(PathToGameFiles, "Resources\\objects.bin");
    private string ExtractedTexturesPath => Path.Combine(TargetDirectory, "textures");
    private string ExtractedObjectsPath => Path.Combine(TargetDirectory, "objects");
    private string ConvertedObjectsPath => Path.Combine(TargetDirectory, "converted_objects");

    private void ExtractTextures()
    {
        ExtractedTextures = EnsureTextures(PathToGameFiles, ExtractedTexturesPath);
    }

    public static Dictionary<string, string> EnsureTextures(string gamePath, string extractedTexturesPath)
    {
        var archivePath = Path.Combine(gamePath, "Resources", "textures.bin");
        ExtractionHelper.EnsureExtracted(archivePath, extractedTexturesPath);

        var extractedTextures = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var png in Directory.GetFiles(extractedTexturesPath, "*.png", SearchOption.AllDirectories))
        {
            var textureName = Path.GetFileNameWithoutExtension(png);
            extractedTextures[textureName] = png;
        }

        if (extractedTextures.Count > 0)
            return extractedTextures;

        foreach (var file in Directory.GetFiles(extractedTexturesPath, "*.BMP", SearchOption.AllDirectories))
        {
            var textureName = Path.GetFileNameWithoutExtension(file);
            extractedTextures[textureName] = file;
        }

        var corruptedTextures = new List<string>();
        foreach (var (name, path) in extractedTextures.ToArray())
            try
            {
                var pngPath = Path.ChangeExtension(path, ".png");
                using var bmp = new MagickImage(path, new MagickReadSettings(new BmpReadDefines
                {
                    IgnoreFileSize = true
                }));
                bmp.Write(pngPath);
                extractedTextures[name] = pngPath;
            }
            catch (MagickCorruptImageErrorException e)
            {
                Console.WriteLine(e);
                corruptedTextures.Add(name);
            }

        foreach (var name in corruptedTextures)
            extractedTextures.Remove(name);

        return extractedTextures;
    }

    private void ExtractObjects()
    {
        var extractedObjects = new Dictionary<string, ExtractedObject>();

        if (!File.Exists(ObjectsBinPath))
            throw new FileNotFoundException("The objects.bin file was not found.", ObjectsBinPath);

        Directory.CreateDirectory(ExtractedObjectsPath);

        using var zipArchive = ZipFile.OpenRead(ObjectsBinPath);
        foreach (var entry in zipArchive.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name)) continue;

            var extractedFilePath = Path.Combine(ExtractedObjectsPath, entry.FullName);
            var extractedFileDirectory = Path.GetDirectoryName(extractedFilePath);

            if (extractedFileDirectory is not null && !Directory.Exists(extractedFileDirectory))
                Directory.CreateDirectory(extractedFileDirectory);

            if (!entry.FullName.EndsWith('/')) entry.ExtractToFile(extractedFilePath, true);
        }

        foreach (var file in Directory.GetFiles(ExtractedObjectsPath, "*.bgf", SearchOption.AllDirectories))
        {
            var relativeFilePath = Path.GetRelativePath(ExtractedObjectsPath, file);
            var objectName = Path.GetFileNameWithoutExtension(relativeFilePath);
            var txsPath = Path.ChangeExtension(file, ".TXS");
            var hasTxs = File.Exists(txsPath);

            extractedObjects.Add(relativeFilePath, new ExtractedObject
            {
                BgfName = objectName,
                BgfPath = file,
                RelativeBgfFile = Path.GetRelativePath(ExtractedObjectsPath, file),
                RelativeBgfDirectory = Path.GetDirectoryName(relativeFilePath) ?? string.Empty,
                TxsPath = hasTxs ? txsPath : null,
                RelativeTxsFile = hasTxs ? Path.GetRelativePath(ExtractedObjectsPath, txsPath) : null,
                RelativeTxsDirectory = hasTxs ? Path.GetDirectoryName(relativeFilePath) : null
            });
        }

        ExtractedObjects = extractedObjects;
    }

    private void Preprocess()
    {
        // foreach (var (name, extractedObject) in ExtractedObjects)
        // {
        //     using var bgfStream = File.OpenRead(extractedObject.BgfPath);
        //     using var br = new BinaryReader(bgfStream);
        //     var bgf = BgfStruct.FromBytes(br);
        // }
    }

    public void Convert()
    {
        ExtractTextures();
        ExtractObjects();

        Preprocess();

        foreach (var (relativePath, extractedObject) in ExtractedObjects)
        {
            var convertedDirectory = Path.Combine(ConvertedObjectsPath, extractedObject.RelativeBgfDirectory);
            var convertedFilePath =
                Path.Combine(convertedDirectory, Path.ChangeExtension(extractedObject.BgfName, "glb"));

            if (!Path.Exists(convertedDirectory)) Directory.CreateDirectory(convertedDirectory);

            using var bgfStream = File.OpenRead(extractedObject.BgfPath);
            using var br = new BinaryReader(bgfStream);
            using var txsStream = extractedObject.TxsPath is not null
                ? File.OpenRead(extractedObject.TxsPath)
                : null;
            using var txsBr = txsStream is not null ? new BinaryReader(txsStream) : null;

            var bgfStruct = BgfStruct.FromBytes(br);
            var txsStruct = txsBr is not null
                ? TxsStruct.FromBytes(txsBr)
                : null;

            var gltfModelData = GltfUtil.GetModelData(bgfStruct, null, txsStruct, ExtractedTextures);
            var scene = GltfUtil.CreateModel(gltfModelData);
            scene.ToGltf2().SaveGLB(convertedFilePath);
        }
    }

    private class ExtractedObject
    {
        internal required string BgfName { get; init; }
        internal required string BgfPath { get; init; }
        internal required string RelativeBgfFile { get; init; }
        internal required string RelativeBgfDirectory { get; init; }
        internal required string? TxsPath { get; init; }
        internal required string? RelativeTxsFile { get; init; }
        internal required string? RelativeTxsDirectory { get; init; }
    }
}