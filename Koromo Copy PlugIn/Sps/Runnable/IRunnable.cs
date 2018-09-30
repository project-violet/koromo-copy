namespace Hik.Sps.Runnable
{
    /// <summary>
    /// Interface to control modules that can run in self thread.
    /// </summary>
    public interface IRunnable
    {
        /// <summary>
        /// Starts the module.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the module.
        /// </summary>
        void Stop();

        /// <summary>
        /// Waits the module to stop.
        /// </summary>
        void WaitToStop();
    }
}