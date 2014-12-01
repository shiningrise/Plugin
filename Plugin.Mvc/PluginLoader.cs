using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml;

namespace PluginMvc
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
            //            TempPluginFolder = new DirectoryInfo(AppDomain.CurrentDomain.DynamicDirectory);
            //#if DEBUG
            TempPluginFolder = new DirectoryInfo(ShadowCopyPath);
            //#endif
            var FrameworkPrivateBin = new DirectoryInfo(System.AppDomain.CurrentDomain.SetupInformation.PrivateBinPath);
            FrameworkPrivateBinFiles = FrameworkPrivateBin.GetFiles().Select(p => p.Name).ToList();

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

            return plugins;
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
        public static string GetShadowCopyPath()
        {
            var filePath = ShadowCopyPath;
            return filePath;
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
            var pluginsTemp = TempPluginFolder.GetFiles("*.*", SearchOption.AllDirectories).Where(p => FrameworkPrivateBinFiles.Contains(p.Name) == false);
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
                    if (FrameworkPrivateBinFiles.Contains(item.Name) == true)
                        continue;
                    if (item.Name.StartsWith("Plugin."))
                        plugindlls.Add(item);
                    else if (PluginFileNames.Length > 0 && PluginFileNames.Contains(item.Name) == true)
                    {
                        plugindlls.Add(item);
                    }
                }
                foreach (var plugindll in plugindlls)
                {
                    try
                    {
                        var srcPath = plugindll.FullName;
                        var toPath = Path.Combine(TempPluginFolder.FullName, plugindll.Name);
                        File.Copy(srcPath, toPath, true);
                        if (!IsAlreadyLoaded(plugindll))
                        {
                            var shadowCopiedAssembly = Assembly.Load(AssemblyName.GetAssemblyName(toPath));
                            System.Web.Compilation.BuildManager.AddReferencedAssembly(shadowCopiedAssembly);
                        }
#if DEBUG
                        if (srcPath.EndsWith(".dll"))
                        {
                            var srcPdbPath = plugindll.FullName.Substring(0, plugindll.FullName.Length - 4) + ".pdb";
                            var pdfName = plugindll.Name.Substring(0, plugindll.Name.Length - 4) + ".pdb";
                            var toPdbPath = Path.Combine(TempPluginFolder.FullName, pdfName);

                            if (File.Exists(srcPdbPath))
                            {
                                File.Copy(srcPdbPath, toPdbPath, true);
                            }
                        }
#endif
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }

        #region Utility 
        
        private static bool IsAlreadyLoaded(FileInfo fileInfo)
        {
            try
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                if (fileNameWithoutExt == null)
                    throw new Exception(string.Format("Cannot get file extnension for {0}", fileInfo.Name));
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    string assemblyName = a.FullName.Split(new[] { ',' }).FirstOrDefault();
                    if (fileNameWithoutExt.Equals(assemblyName, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Cannot validate whether an assembly is already loaded. " + exc);
            }
            return false;
        }

        /// <summary>
        /// Perform file deply
        /// </summary>
        /// <param name="plug">Plugin file info</param>
        /// <returns>Assembly</returns>
        private static Assembly PerformFileDeploy(FileInfo plug)
        {
            if (plug.Directory.Parent == null)
                throw new InvalidOperationException("The plugin directory for the " + plug.Name +
                                                    " file exists in a folder outside of the allowed nopCommerce folder heirarchy");

            FileInfo shadowCopiedPlug;

            if (GetTrustLevel() != AspNetHostingPermissionLevel.Unrestricted)
            {
                //all plugins will need to be copied to ~/Plugins/bin/
                //this is aboslutely required because all of this relies on probingPaths being set statically in the web.config

                //were running in med trust, so copy to custom bin folder
                var shadowCopyPlugFolder = Directory.CreateDirectory(TempPluginFolder.FullName);
                shadowCopiedPlug = InitializeMediumTrust(plug, shadowCopyPlugFolder);
            }
            else
            {
                var directory = AppDomain.CurrentDomain.DynamicDirectory;
                Debug.WriteLine(plug.FullName + " to " + directory);
                //were running in full trust so copy to standard dynamic folder
                shadowCopiedPlug = InitializeFullTrust(plug, new DirectoryInfo(directory));
            }

            //we can now register the plugin definition
            var shadowCopiedAssembly = Assembly.Load(AssemblyName.GetAssemblyName(shadowCopiedPlug.FullName));

            //add the reference to the build manager
            Debug.WriteLine("Adding to BuildManager: '{0}'", shadowCopiedAssembly.FullName);
            System.Web.Compilation.BuildManager.AddReferencedAssembly(shadowCopiedAssembly);

            return shadowCopiedAssembly;
        }

        private static AspNetHostingPermissionLevel? _trustLevel;
        /// <summary>
        /// Finds the trust level of the running application (http://blogs.msdn.com/dmitryr/archive/2007/01/23/finding-out-the-current-trust-level-in-asp-net.aspx)
        /// </summary>
        /// <returns>The current trust level.</returns>
        public static AspNetHostingPermissionLevel GetTrustLevel()
        {
            if (!_trustLevel.HasValue)
            {
                //set minimum
                _trustLevel = AspNetHostingPermissionLevel.None;

                //determine maximum
                foreach (AspNetHostingPermissionLevel trustLevel in new[] {
                                AspNetHostingPermissionLevel.Unrestricted,
                                AspNetHostingPermissionLevel.High,
                                AspNetHostingPermissionLevel.Medium,
                                AspNetHostingPermissionLevel.Low,
                                AspNetHostingPermissionLevel.Minimal 
                            })
                {
                    try
                    {
                        new AspNetHostingPermission(trustLevel).Demand();
                        _trustLevel = trustLevel;
                        break; //we've set the highest permission we can
                    }
                    catch (System.Security.SecurityException)
                    {
                        continue;
                    }
                }
            }
            return _trustLevel.Value;
        }


        /// <summary>
        /// Used to initialize plugins when running in Full Trust
        /// </summary>
        /// <param name="plug"></param>
        /// <param name="shadowCopyPlugFolder"></param>
        /// <returns></returns>
        private static FileInfo InitializeFullTrust(FileInfo plug, DirectoryInfo shadowCopyPlugFolder)
        {
            var shadowCopiedPlug = new FileInfo(Path.Combine(shadowCopyPlugFolder.FullName, plug.Name));
            try
            {
                File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
            }
            catch (IOException)
            {
                Debug.WriteLine(shadowCopiedPlug.FullName + " is locked, attempting to rename");
                //this occurs when the files are locked,
                //for some reason devenv locks plugin files some times and for another crazy reason you are allowed to rename them
                //which releases the lock, so that it what we are doing here, once it's renamed, we can re-shadow copy
                try
                {
                    var oldFile = shadowCopiedPlug.FullName + Guid.NewGuid().ToString("N") + ".old";
                    File.Move(shadowCopiedPlug.FullName, oldFile);
                }
                catch (IOException exc)
                {
                    throw new IOException(shadowCopiedPlug.FullName + " rename failed, cannot initialize plugin", exc);
                }
                //ok, we've made it this far, now retry the shadow copy
                File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
            }
            return shadowCopiedPlug;
        }

        /// <summary>
        /// Used to initialize plugins when running in Medium Trust
        /// </summary>
        /// <param name="plug"></param>
        /// <param name="shadowCopyPlugFolder"></param>
        /// <returns></returns>
        private static FileInfo InitializeMediumTrust(FileInfo plug, DirectoryInfo shadowCopyPlugFolder)
        {
            var shouldCopy = true;
            var shadowCopiedPlug = new FileInfo(Path.Combine(shadowCopyPlugFolder.FullName, plug.Name));

            //check if a shadow copied file already exists and if it does, check if it's updated, if not don't copy
            if (shadowCopiedPlug.Exists)
            {
                //it's better to use LastWriteTimeUTC, but not all file systems have this property
                //maybe it is better to compare file hash?
                var areFilesIdentical = shadowCopiedPlug.CreationTimeUtc.Ticks >= plug.CreationTimeUtc.Ticks;
                if (areFilesIdentical)
                {
                    Debug.WriteLine("Not copying; files appear identical: '{0}'", shadowCopiedPlug.Name);
                    shouldCopy = false;
                }
                else
                {
                    //delete an existing file

                    //More info: http://www.nopcommerce.com/boards/t/11511/access-error-nopplugindiscountrulesbillingcountrydll.aspx?p=4#60838
                    Debug.WriteLine("New plugin found; Deleting the old file: '{0}'", shadowCopiedPlug.Name);
                    File.Delete(shadowCopiedPlug.FullName);
                }
            }

            if (shouldCopy)
            {
                try
                {
                    File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
                }
                catch (IOException)
                {
                    Debug.WriteLine(shadowCopiedPlug.FullName + " is locked, attempting to rename");
                    //this occurs when the files are locked,
                    //for some reason devenv locks plugin files some times and for another crazy reason you are allowed to rename them
                    //which releases the lock, so that it what we are doing here, once it's renamed, we can re-shadow copy
                    try
                    {
                        var oldFile = shadowCopiedPlug.FullName + Guid.NewGuid().ToString("N") + ".old";
                        File.Move(shadowCopiedPlug.FullName, oldFile);
                    }
                    catch (IOException exc)
                    {
                        throw new IOException(shadowCopiedPlug.FullName + " rename failed, cannot initialize plugin", exc);
                    }
                    //ok, we've made it this far, now retry the shadow copy
                    File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
                }
            }

            return shadowCopiedPlug;
        }

        #endregion
    }
}