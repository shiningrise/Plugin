using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PluginMvc
{
    /// <summary>
    /// 插件管理器。
    /// </summary>
    public static class PluginManager
    {

        /// <summary>
        /// 插件字典。
        /// </summary>
        private readonly static IDictionary<string, PluginDescriptor> _plugins = new Dictionary<string, PluginDescriptor>();

        /// <summary>
        /// 初始化。
        /// </summary>
        public static void Initialize()
        {
            _plugins.Clear();
            //遍历所有插件描述。
            var plugins = PluginLoader.Load();
            
            foreach (var plugin in plugins)
            {
                _plugins.Add(plugin.Name,plugin);
            }
        }

        /// <summary>
        /// 获得当前系统所有插件描述。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<PluginDescriptor> GetPlugins()
        {
            return _plugins.Select(m => m.Value).ToList();
        }

        /// <summary>
        /// 根据插件名称获得插件描述。
        /// </summary>
        /// <param name="name">插件名称。</param>
        /// <returns>插件描述。</returns>
        public static PluginDescriptor GetPlugin(string name)
        {
            return GetPlugins().SingleOrDefault(plugin => plugin.Name == name);
        }

        #region Install
        
        /// <summary>
        /// Mark plugin as installed
        /// </summary>
        /// <param name="systemName">Plugin system name</param>
        public static void MarkPluginAsInstalled(string systemName)
        {
            if (String.IsNullOrEmpty(systemName))
                throw new ArgumentNullException("systemName");

            var filePath = PluginLoader.GetInstalledPluginsFilePath();
            if (!File.Exists(filePath))
                using (File.Create(filePath))
                {
                    //we use 'using' to close the file after it's created
                }


            var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(PluginLoader.GetInstalledPluginsFilePath());
            bool alreadyMarkedAsInstalled = installedPluginSystemNames
                                .FirstOrDefault(x => x.Equals(systemName, StringComparison.InvariantCultureIgnoreCase)) != null;
            if (!alreadyMarkedAsInstalled)
                installedPluginSystemNames.Add(systemName);
            PluginFileParser.SaveInstalledPluginsFile(installedPluginSystemNames,filePath);
        }

        /// <summary>
        /// Mark plugin as uninstalled
        /// </summary>
        /// <param name="systemName">Plugin system name</param>
        public static void MarkPluginAsUninstalled(string systemName)
        {
            if (String.IsNullOrEmpty(systemName))
                throw new ArgumentNullException("systemName");

            var filePath = PluginLoader.GetInstalledPluginsFilePath();
            if (!File.Exists(filePath))
                using (File.Create(filePath))
                {
                    //we use 'using' to close the file after it's created
                }


            var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(PluginLoader.GetInstalledPluginsFilePath());
            bool alreadyMarkedAsInstalled = installedPluginSystemNames
                                .FirstOrDefault(x => x.Equals(systemName, StringComparison.InvariantCultureIgnoreCase)) != null;
            if (alreadyMarkedAsInstalled)
                installedPluginSystemNames.Remove(systemName);
            PluginFileParser.SaveInstalledPluginsFile(installedPluginSystemNames,filePath);
        }

        /// <summary>
        /// Mark plugin as uninstalled
        /// </summary>
        public static void MarkAllPluginsAsUninstalled()
        {
            var filePath = PluginLoader.GetInstalledPluginsFilePath();
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        #endregion
    }
}