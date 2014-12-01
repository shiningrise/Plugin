using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Linq;

namespace PluginMvc
{
    /// <summary>
    /// 插件信息。
    /// </summary>
    public class PluginDescriptor
    {
        /// <summary>
        /// 控制器类型字典。
        /// </summary>
        private readonly IDictionary<string, Type> _controllerTypes = new Dictionary<string, Type>();

        /// <summary>
        /// 构造器。
        /// </summary>
        public PluginDescriptor(Assembly assembly, IEnumerable<Type> types, IEnumerable<Assembly> dependentAssemblys = null)
        {
            this.Assembly = assembly;
            this.Types = types;

            if (DependentAssemblys == null)
            {
                DependentAssemblys = new List<Assembly>();
            }
            DependentAssemblys.AddRange(dependentAssemblys);
        }

        public PluginDescriptor()
        {
            // TODO: Complete member initialization
            this._controllerTypes = new Dictionary<string, Type>();
        }

        public void Init(IEnumerable<System.Reflection.Assembly> assemblies)
        {
            if (DependentAssemblys == null)
            {
                DependentAssemblys = new List<Assembly>();
            }
            DependentAssemblys.AddRange(assemblies);

        }

        /// <summary>
        /// 程序集。
        /// </summary>
        public Assembly Assembly { get; private set; }

        public List<Assembly> DependentAssemblys { get; private set; }

        /// <summary>
        /// 类型。
        /// </summary>
        public IEnumerable<Type> Types { get; private set; }

        /// <summary>
        /// Plugin type
        /// </summary>
        public virtual string PluginFileName { get; set; }

        /// <summary>
        /// Plugin type
        /// </summary>
        public virtual Type PluginType { get; set; }

        /// <summary>
        /// The assembly that has been shadow copied that is active in the application
        /// </summary>
        public virtual Assembly ReferencedAssembly { get; internal set; }

        /// <summary>
        /// The original assembly file that a shadow copy was made from it
        /// </summary>
        public virtual FileInfo OriginalAssemblyFile { get; internal set; }

        /// <summary>
        /// Gets or sets the plugin group
        /// </summary>
        public virtual string Group { get; set; }

        /// <summary>
        /// Gets or sets the friendly name
        /// </summary>
        public virtual string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the Plugin name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the version
        /// </summary>
        public virtual string Version { get; set; }

        /// <summary>
        /// Gets or sets the supported versions of nopCommerce
        /// </summary>
        public virtual IList<string> SupportedVersions { get; set; }

        /// <summary>
        /// Gets or sets the author
        /// </summary>
        public virtual string Author { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public virtual int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the list of store identifiers in which this plugin is available. If empty, then this plugin is available in all stores
        /// </summary>
        public virtual IList<int> LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether plugin is installed
        /// </summary>
        public virtual bool Installed { get; set; }



        public int CompareTo(PluginDescriptor other)
        {
            if (DisplayOrder != other.DisplayOrder)
                return DisplayOrder.CompareTo(other.DisplayOrder);

            return FriendlyName.CompareTo(other.FriendlyName);
        }

        public override string ToString()
        {
            return FriendlyName;
        }

        public override bool Equals(object obj)
        {
            var other = obj as PluginDescriptor;
            return other != null &&
                Name != null &&
                Name.Equals(other.Name);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

    }
}