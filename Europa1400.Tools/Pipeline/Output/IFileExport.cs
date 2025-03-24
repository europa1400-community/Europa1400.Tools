namespace Europa1400.Tools.Pipeline.Output
{
    public interface IFileExport
    {
        /// <summary>
        ///     Relative path where the file should be written (e.g., "sounds/ambient_01.wav").
        /// </summary>
        string FilePath { get; }

        /// <summary>
        ///     The raw content to be written to disk.
        /// </summary>
        byte[] Content { get; }
    }
}