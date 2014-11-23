using System.Collections.Generic;

namespace Plugin
{
    
    /// <summary>
    /// 插件加载器。
    /// </summary>
    public interface IPluginLoader
    {
        /// <summary>
        /// 加载插件。
        /// </summary>
        /// <returns></returns>
        IEnumerable<PluginDescriptor> Load();
    }
}