using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Europa1400.Tools.Pipeline.Output;

namespace Europa1400.Tools.Pipeline.Converter
{
    internal class DummyConverter : IConverter<object, List<IFileExport>>
    {
        public async Task<List<IFileExport>> ConvertAsync(object input, CancellationToken cancellationToken = default)
        {
            await Task.Delay(1, cancellationToken);
            return new List<IFileExport> { new FileExport() };
        }
    }
}