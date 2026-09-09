using System.Reflection;
using DotNetEnv;
using Europa1400.Tools.Pipeline;

namespace Europa1400.Tools.Verifier;

internal static class VerifierPaths
{
    public static string SolutionRoot { get; } = FindSolutionRoot();
    public static string GameDirectory => _gameDirectory.Value;

    private static readonly Lazy<string> _gameDirectory = new(LoadGameDirectory);
    public static string CacheRoot { get; } = Path.Combine(SolutionRoot, "cache");
    public static string VerifierCacheRoot { get; } = Path.Combine(CacheRoot, "verifier");

    public static string LibraryFingerprint { get; } =
        BuildFingerprint(typeof(PipelineSettings).Assembly);

    public static void ConfigurePipelineCache()
    {
        PipelineSettings.CacheRoot = CacheRoot;
        Directory.CreateDirectory(VerifierCacheRoot);
    }

    private static string LoadGameDirectory()
    {
        var envPath = Path.Combine(SolutionRoot, ".env");
        if (File.Exists(envPath))
            Env.Load(envPath);

        var path = Environment.GetEnvironmentVariable("GAME_DIRECTORY_PATH");
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            throw new InvalidOperationException(
                "Set GAME_DIRECTORY_PATH in the solution-root .env to a valid Europa 1400 install.");

        return path;
    }

    private static string BuildFingerprint(Assembly assembly)
    {
        var location = assembly.Location;
        if (string.IsNullOrEmpty(location) || !File.Exists(location))
            return assembly.FullName ?? "unknown";

        var info = new FileInfo(location);
        return $"{info.LastWriteTimeUtc.Ticks}:{info.Length}";
    }

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Europa1400.Tools.sln")))
                return dir.FullName;
            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not find Europa1400.Tools.sln");
    }
}
