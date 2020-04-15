using System.IO;
using System.IO.Compression;
using System.Text;

using PluginInterface;

namespace Plugins
{
    [Name(PluginName = "deflate", PluginExtension = "defl")]
    public class DeflateCompressor : IPlugin
    {
        public void PostProcessString(string InputString, string FilePath)
        {
            byte[] ByteInput = Encoding.Unicode.GetBytes(InputString);
            var MemStream = new MemoryStream();
            MemStream.Write(ByteInput, 0, ByteInput.Length);
            PostProcessStream(MemStream, FilePath);
        }

        public string PreProcessString(string FilePath)
        {
            MemoryStream MemStream = PreProcessStream(FilePath);
            byte[] ByteOutput = MemStream.ToArray();
            return Encoding.Unicode.GetString(ByteOutput);
        }

        public void PostProcessStream(MemoryStream InputStream, string FilePath)
        {
            using (var OutputStream = new FileStream(FilePath, FileMode.Create))
            {
                using (var CompressionStream = new DeflateStream(OutputStream, CompressionMode.Compress))
                {
                    InputStream.Position = 0;
                    InputStream.CopyTo(CompressionStream);
                }
            }
        }

        public MemoryStream PreProcessStream(string FilePath)
        {
            var OutputStream = new MemoryStream();
            using (var InputStream = new FileStream(FilePath, FileMode.Open))
            {
                using (var CompressionStream = new DeflateStream(InputStream, CompressionMode.Decompress))
                {
                    CompressionStream.CopyTo(OutputStream);
                    return OutputStream;
                }
            }
        }
    }
}