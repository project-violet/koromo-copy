namespace Hik.Sps
{
    /// <summary>
    /// Represents a plugin from application perspective.
    /// Application uses a plugin over this interface.
    /// </summary>
    /// <typeparam name="TPlugIn">Type of plugin interface</typeparam>
    public interface IApplicationPlugIn<out TPlugIn>
    {
        /// <summary>
        /// Name of the plugin.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Proxy object to use plugin over plugin interface.
        /// </summary>
        TPlugIn PlugInProxy { get; }
    }
}
