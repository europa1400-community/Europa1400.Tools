using DotNetEnv;

namespace Europa1400.Tools.Tests
{
    public static class EnvVariables
    {
        static EnvVariables()
        {
            var solutionRoot = FindSolutionRoot() ?? throw new FileNotFoundException("Could not find the solution root directory.");
            var envFilePath = Path.Combine(solutionRoot, ".env");

            if (!File.Exists(envFilePath))
            {
                throw new FileNotFoundException($".env file not found at path: {envFilePath}");
            }

            Env.Load(envFilePath);
        }

        public static string GetEnvVar(string key) => Environment.GetEnvironmentVariable(key) ?? throw new InvalidOperationException($"The {key} environment variable is not set.");

        public static string GameDirectoryPath => GetEnvVar("GAME_DIRECTORY_PATH");

        private static string? FindSolutionRoot()
        {
            var dir = AppContext.BaseDirectory;

            while (!string.IsNullOrEmpty(dir))
            {
                if (File.Exists(Path.Combine(dir, ".env")))
                {
                    return dir;
                }

                dir = Directory.GetParent(dir)?.FullName;
            }

            return null;
        }
    }
}
