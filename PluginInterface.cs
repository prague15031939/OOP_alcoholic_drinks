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
        void PostProcessString(string InputString, string FilePath);
        void PostProcessStream(MemoryStream InputStream, string FilePath);
        string PreProcessString(string FilePath);
        MemoryStream PreProcessStream(string FilePath);
    }
}
