using System.Collections.Generic;

namespace Plugin
{
    /// <summary>
    /// 插件接口。
    /// </summary>
    public interface IPlugin
    {
        /// <summary>
        /// 名称。
        /// </summary>
        string Name { get; }

        List<string> DependentAssembly { get; }

        /// <summary>
        /// 初始化。
        /// </summary>
        void Initialize();

        /// <summary>
        /// 卸载。
        /// </summary>
        void Unload();
    }
}