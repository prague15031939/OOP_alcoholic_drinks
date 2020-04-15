using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

using PluginInterface;

namespace AlcoholicDrinks
{
    class PluginLoader
    {
        public List<IPlugin> RefreshPlugins()
        {
            var PluginList = new List<IPlugin>();

            string PluginPath = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            DirectoryInfo PluginDirectory = new DirectoryInfo(PluginPath);
            if (!PluginDirectory.Exists)
                PluginDirectory.Create();

            string[] PluginFiles = Directory.GetFiles(PluginPath, "*.dll");
            foreach (string file in PluginFiles)
            {
                Assembly asm = Assembly.LoadFrom(file);
                Type[] types = asm.GetTypes();
                foreach (Type type in types)
                {
                    IPlugin plugin = asm.CreateInstance(type.FullName) as IPlugin;
                    PluginList.Add(plugin);
                }
            }

            return PluginList;
        }
    }
}
