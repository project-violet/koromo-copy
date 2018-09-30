using System;

namespace Hik.Sps
{
    /// <summary>
    /// Implementation of IApplicationPlugIn interface.
    /// </summary>
    /// <typeparam name="TPlugIn">Type of plugin interface</typeparam>
    internal class ApplicationPlugIn<TPlugIn> : IApplicationPlugIn<TPlugIn>
    {
        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Proxy object to use plugin by application over plugin interface.
        /// </summary>
        public TPlugIn PlugInProxy { get; private set; }

        /// <summary>
        /// Creates a new ApplicationPlugIn object.
        /// </summary>
        /// <param name="plugInApplication">Reference to the application that uses plugin</param>
        /// <param name="plugInType">Type of plugin class (that implements plugin interface)</param>
        public ApplicationPlugIn(IPlugInBasedApplication plugInApplication, Type plugInType)
        {
            PlugInProxy = (TPlugIn)Activator.CreateInstance(plugInType);

            var plugInObjectType = PlugInProxy.GetType();

            var applicationProperty = plugInObjectType.GetProperty("Application");
            var applicationPropertyValue = applicationProperty.GetValue(PlugInProxy, null);
            var applicationPropertyType = applicationPropertyValue.GetType();

            applicationPropertyType.GetProperty("Name").SetValue(applicationPropertyValue, plugInApplication.Name, null);
            applicationPropertyType.GetProperty("ApplicationProxy").SetValue(applicationPropertyValue, plugInApplication, null);

            Name = ((IPlugIn) PlugInProxy).Name;
        }
    }
}
