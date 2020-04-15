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
            var plugin_list = new List<IPlugin>();

            string plugin_path = Path.Combine(Directory.GetCurrentDirectory(), "Plugins");
            DirectoryInfo plugin_directory = new DirectoryInfo(plugin_path);
            if (!plugin_directory.Exists)
                plugin_directory.Create();

            string[] plugin_files = Directory.GetFiles(plugin_path, "*.dll");
            foreach (string file in plugin_files)
            {
                Assembly asm = Assembly.LoadFrom(file);
                Type[] types = asm.GetTypes();
                foreach (Type type in types)
                {
                    IPlugin plugin = asm.CreateInstance(type.FullName) as IPlugin;
                    plugin_list.Add(plugin);
                }
            }

            return plugin_list;
        }
    }
}
