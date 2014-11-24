using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Plugin
{
    /// <summary>
    /// 插件加载器。
    /// </summary>
    public static class PluginLoader
    {
        #region Const

        public static string InstalledPluginsFilePath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, @"App_Data\InstalledPlugins.txt");
        public static string PluginsPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, @"Plugins");
        public static string ShadowCopyPath = Path.Combine(AppDomain.CurrentDomain.SetupInformation.ApplicationBase, @"App_Data\Plugins");

        #endregion

        /// <summary>
        /// 插件目录。
        /// </summary>
        private static DirectoryInfo PluginFolder;

        /// <summary>
        /// 插件临时目录。
        /// </summary>
        private static DirectoryInfo TempPluginFolder;

        private static List<string> FrameworkPrivateBinFiles;

        /// <summary>
        /// 初始化。
        /// </summary>
        static PluginLoader()
        {
            PluginFolder = new DirectoryInfo(PluginsPath);
            TempPluginFolder = new DirectoryInfo(AppDomain.CurrentDomain.DynamicDirectory);
#if DEBUG
            TempPluginFolder = new DirectoryInfo(ShadowCopyPath);
#endif
            var FrameworkPrivateBin = new DirectoryInfo(System.AppDomain.CurrentDomain.SetupInformation.PrivateBinPath);
            FrameworkPrivateBinFiles = FrameworkPrivateBin.GetFiles().Select(p => p.Name).ToList();

            //foreach (var item in FrameworkPrivateBinFiles)
            //{
            //    Debug.WriteLine(item);
            //}
        }

        /// <summary>
        /// 加载插件。
        /// </summary>
        public static IEnumerable<PluginDescriptor> Load()
        {

            var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(GetInstalledPluginsFilePath());

            List<PluginDescriptor> plugins = new List<PluginDescriptor>();

            if (PluginFolder == null)
                throw new ArgumentNullException("pluginFolder");

            foreach (var pluginFolder in PluginFolder.GetDirectories())
            {
                if (installedPluginSystemNames.Count > 0 && installedPluginSystemNames.Contains(pluginFolder.Name) == false)
                {
                    continue;
                }
                var descriptionFilepath = Path.Combine(pluginFolder.FullName, "Description.txt");
                if (File.Exists(descriptionFilepath))
                {
                    var pluginDescriptor = PluginFileParser.ParsePluginDescriptionFile(descriptionFilepath);
                    pluginDescriptor.Name = pluginFolder.Name;
                    plugins.Add(pluginDescriptor);
                }
                else
                {
                    var pluginDescriptor = new PluginDescriptor();
                    pluginDescriptor.Name = pluginFolder.Name;
                    plugins.Add(pluginDescriptor);
                }
            }
            plugins.Sort((firstPair, nextPair) => firstPair.DisplayOrder.CompareTo(nextPair.DisplayOrder));

            //程序集复制到临时目录。
            CopyToTempPluginFolderDirectory(plugins);

            //加载 bin 目录下的所有程序集。
            IEnumerable<Assembly> assemblies = AppDomain.CurrentDomain.GetAssemblies();
            //加载临时目录下的所有程序集。
            var list = TempPluginFolder.GetFiles("*.dll", SearchOption.AllDirectories).Select(x => Assembly.LoadFile(x.FullName)).ToList().FindAll(p => assemblies.Contains(p) == false);
            InitPlugins(list.Union(assemblies), plugins);

            return plugins.Where(p => p.Plugin != null);
        }

        /// <summary>
        /// Gets the full path of InstalledPlugins.txt file
        /// </summary>
        /// <returns></returns>
        public static string GetInstalledPluginsFilePath()
        {
            var filePath = InstalledPluginsFilePath;
            return filePath;
        }

        /// <summary>
        /// 获得插件信息。
        /// </summary>
        /// <param name="pluginType"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        private static IPlugin GetPluginInstance(Type pluginType, Assembly assembly, IEnumerable<Assembly> assemblies)
        {
            if (pluginType != null)
            {
                var plugin = (IPlugin)Activator.CreateInstance(pluginType);
                return plugin;
                //if (plugin != null)
                //{
                //    var assems = assemblies.Where(p => plugin.DependentAssembly.Contains(p.GetName().Name)).ToList();
                //    return new PluginDescriptor(plugin, assembly, assembly.GetTypes(), assems);//
                //}
            }

            return null;
        }

        /// <summary>
        /// 程序集复制到临时目录。
        /// </summary>
        private static void CopyToTempPluginFolderDirectory(List<PluginDescriptor> pluginDescriptions)
        {

            Directory.CreateDirectory(PluginFolder.FullName);
            Directory.CreateDirectory(TempPluginFolder.FullName);

            //清理临时文件。
            Debug.WriteLine("清理临时文件");
            var pluginsTemp = TempPluginFolder.GetFiles("*.dll", SearchOption.AllDirectories).Where(p => FrameworkPrivateBinFiles.Contains(p.Name) == false);
            foreach (var file in pluginsTemp)
            {
                try
                {
                    Debug.WriteLine(file.FullName);
                    file.Delete();
                }
                catch (Exception)
                {

                }
            }

            //复制插件进临时文件夹。
            foreach (var plugin in pluginDescriptions)
            {
                var PluginFileNames = plugin.PluginFileName == null ? new string[] { } : plugin.PluginFileName.Split(',');
                var dir = new DirectoryInfo(Path.Combine(PluginFolder.FullName, Path.Combine(plugin.Name, "bin")));
                var list = dir.GetFiles("*.dll");
                var plugindlls = new List<FileInfo>();
                foreach (var item in list)
                {
                    if ((PluginFileNames.Length == 0 || PluginFileNames.Length > 0 && PluginFileNames.Contains(item.Name) == true || item.Name.StartsWith("Plugin.")) && FrameworkPrivateBinFiles.Contains(item.Name) == false)
                        plugindlls.Add(item);
                }
                foreach (var plugindll in plugindlls)
                {
                    try
                    {
                        var srcPath = plugindll.FullName;
                        var toPath = Path.Combine(TempPluginFolder.FullName, plugindll.Name);
#if DEBUG
                        Debug.WriteLine(string.Format("from:\t{0}", srcPath));
                        Debug.WriteLine(string.Format("to:\t{0}", toPath));
#endif
                        File.Copy(srcPath, toPath, true);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        /// <summary>
        /// 根据程序集列表获得该列表下的所有插件信息。
        /// </summary>
        /// <param name="assemblies">程序集列表</param>
        /// <returns>插件信息集合。</returns>
        private static void InitPlugins(IEnumerable<Assembly> assemblies, List<PluginDescriptor> descriptors)
        {
            foreach (var assembly in assemblies)
            {
                try
                {
                    var pluginTypes = assembly.GetTypes().Where(type => type.GetInterface(typeof(IPlugin).Name) != null && type.IsClass && !type.IsAbstract);

                    foreach (var pluginType in pluginTypes)
                    {
                        if (pluginType != null)
                        {
                            var plugin = (IPlugin)Activator.CreateInstance(pluginType);
                            var descriptor = descriptors.Where(p => p.Name == plugin.Name).FirstOrDefault();
                            if (descriptor != null)
                            {
                                descriptor.Init(plugin, assemblies);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(assembly.FullName);
                    Debug.WriteLine(ex.Message);
                    //    throw ex;
                }

            }
        }
    }
}