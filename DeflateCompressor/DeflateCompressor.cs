using System.IO;
using System.IO.Compression;
using System.Text;

using PluginInterface;

namespace Plugins
{
    [Name(PluginName = "deflate", PluginExtension = "defl")]
    public class DeflateCompressor : IPlugin
    {
        public void PostProcessString(string input_string, string FilePath)
        {
            byte[] byte_input = Encoding.Unicode.GetBytes(input_string);
            var mem_stream = new MemoryStream();
            mem_stream.Write(byte_input, 0, byte_input.Length);
            PostProcessStream(mem_stream, FilePath);
        }

        public string PreProcessString(string FilePath)
        {
            MemoryStream mem_stream = PreProcessStream(FilePath);
            byte[] byte_output = mem_stream.ToArray();
            return Encoding.Unicode.GetString(byte_output);
        }

        public void PostProcessStream(MemoryStream input_stream, string FilePath)
        {
            using (var output_stream = new FileStream(FilePath, FileMode.Create))
            {
                using (var compression_stream = new DeflateStream(output_stream, CompressionMode.Compress))
                {
                    input_stream.Position = 0;
                    input_stream.CopyTo(compression_stream);
                }
            }
        }

        public MemoryStream PreProcessStream(string FilePath)
        {
            var output_stream = new MemoryStream();
            using (var input_stream = new FileStream(FilePath, FileMode.Open))
            {
                using (var compression_stream = new DeflateStream(input_stream, CompressionMode.Decompress))
                {
                    compression_stream.CopyTo(output_stream);
                    return output_stream;
                }
            }
        }
    }
}