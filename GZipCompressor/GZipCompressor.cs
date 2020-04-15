using System.IO;
using System.IO.Compression;
using System.Text;

using PluginInterface;

namespace Plugins
{
    [Name(PluginName = "gzip", PluginExtension = "gzip")]
    public class GZipCompressor : IPlugin
    {
        public void PostProcessString(string InputString, string FilePath)
        {

        }

        public void PostProcessStream(MemoryStream InputStream, string FilePath)
        {

        }

        public string PreProcessString(string FilePath)
        {
            return null;
        }

        public MemoryStream PreProcessStream(string FilePath)
        {
            return null;
        }
    }
}