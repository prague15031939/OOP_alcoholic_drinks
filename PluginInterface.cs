using System;
using System.IO;

namespace PluginInterface
{
    public class NameAttribute : Attribute
    {
        public string PluginName;
        public string PluginExtension;
    }

    public interface IPlugin
    {
        void PostProcessString(string input_string, string FilePath);
        void PostProcessStream(MemoryStream input_stream, string FilePath);
        string PreProcessString(string FilePath);
        MemoryStream PreProcessStream(string FilePath);
    }
}
