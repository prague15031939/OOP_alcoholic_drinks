using System.IO;
using System.Collections.Generic;

using PluginInterface;
using Alcohol;

namespace Plugins
{
    [Name(Name = "Second")]
    public class LZWCompressor : IPlugin
    {
        public string PostProcess(string input)
        {
            return input.ToUpper();
        }

        public MemoryStream PostProcess(MemoryStream input)
        {
            byte[] byte_array = input.ToArray();
            for (int i = 0; i < byte_array.Length; i++)
                if (!(byte_array[i] == 0 || byte_array[i] == 255))
                    byte_array[i] += 1;
            var output = new MemoryStream();
            output.Write(byte_array, 0, byte_array.Length);
            output.Position = 0;
            return output;
        }

        public string PreProcess(string input)
        {
            return null;
        }

        public MemoryStream PreProcess(MemoryStream input)
        {
            byte[] byte_array = input.ToArray();
            for (int i = 0; i < byte_array.Length; i++)
                if (!(byte_array[i] == 0 || byte_array[i] == 255))
                    byte_array[i] -= 1;
            var output = new MemoryStream();
            output.Write(byte_array, 0, byte_array.Length);
            output.Position = 0;
            return output;
        }
    }
}