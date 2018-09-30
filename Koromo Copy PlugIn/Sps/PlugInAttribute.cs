using System;

namespace Hik.Sps
{
    /// <summary>
    /// This attribute is used to mark a class as a PlugIn.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PlugInAttribute : Attribute
    {
        /// <summary>
        /// Name of the PlugIn.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Name of the PlugIn.
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">Name of the PlugIn</param>
        public PlugInAttribute(string name, string version)
        {
            Name = name;
            Version = version;
        }
    }
}
