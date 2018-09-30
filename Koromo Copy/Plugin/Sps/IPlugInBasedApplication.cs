namespace Hik.Sps
{
    /// <summary>
    /// All plugin-based applications implements this interface.
    /// Applications does not directly implement this interface, but they implement by inheriting PlugInBasedApplication class.
    /// </summary>
    public interface IPlugInBasedApplication : IPluggable
    {
        /// <summary>
        /// Gets the name of this Application.
        /// </summary>
        string Name { get; }
    }
}
