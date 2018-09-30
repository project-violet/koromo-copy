using System;

namespace Hik.Sps.Runnable
{
    /// <summary>
    /// This class is used as a base class for plugin applications that they and their plugins can be run (start/stop).
    /// </summary>
    /// <typeparam name="TPlugIn">Type of PlugIn that is used by plugin application</typeparam>
    public abstract class RunnablePlugInBasedApplication<TPlugIn> : PlugInBasedApplication<TPlugIn> where TPlugIn : IRunnablePlugIn
    {
        /// <summary>
        /// This event is raised when plugin application is being started (before starting of plugins).
        /// </summary>
        public event EventHandler Starting;

        /// <summary>
        /// This event is raised when plugin application is started (after starting of plugins).
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// This event is raised when plugin application is being stopped (before stopping of plugins).
        /// </summary>
        public event EventHandler Stopping;

        /// <summary>
        /// This event is raised when plugin application is stopped (after stopping of plugins).
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Starts the plugin application and all plugins.
        /// </summary>
        public void Start()
        {
            OnStarting();
            
            foreach (var plugIn in PlugIns)
            {
                plugIn.PlugInProxy.Start();
            }

            OnStarted();
        }

        /// <summary>
        /// Stops the plugin application and all plugins.
        /// </summary>
        public void Stop()
        {
            OnStopping();

            foreach (var plugIn in PlugIns)
            {
                plugIn.PlugInProxy.Stop();
            }

            OnStopped();
        }

        /// <summary>
        /// Waits stopping of the plugin application and all plugins.
        /// </summary>
        public void WaitToStop()
        {
            foreach (var plugIn in PlugIns)
            {
                plugIn.PlugInProxy.WaitToStop();
            }

            OnWaitToStop();
        }

        /// <summary>
        /// This method is called when plugin application is started and raises Starting event.
        /// A plugin application may override this method or register to Starting event
        /// to perform operations on startup (before starting of plugins).
        /// </summary>
        protected virtual void OnStarting()
        {
            if (Starting != null)
            {
                Starting(this, new EventArgs());
            }
        }

        /// <summary>
        /// This method is called when plugin application is started and raises Started event.
        /// A plugin application may override this method or register to Started event
        /// to perform operations on startup (after starting of plugins).
        /// </summary>
        protected virtual void OnStarted()
        {
            if (Started != null)
            {
                Started(this, new EventArgs());
            }
        }

        /// <summary>
        /// This method is called when plugin application is stopped and raises Stopping event.
        /// A pluging application may override this method or register to Stopping event
        /// to perform operations on stopping (before stopping of plugins).
        /// </summary>
        protected virtual void OnStopping()
        {
            if (Stopping != null)
            {
                Stopping(this, new EventArgs());
            }
        }

        /// <summary>
        /// This method is called when plugin application is stopped and raises Stopped event.
        /// A pluging application may override this method or register to Stopped event
        /// to perform operations on stopping (after stopping of plugins).
        /// </summary>
        protected virtual void OnStopped()
        {
            if (Stopped != null)
            {
                Stopped(this, new EventArgs());
            }
        }

        /// <summary>
        /// This method is called when plugin application is being waited to stop.
        /// A pluging application may override this method to perform additional operations after stopped.
        /// </summary>
        protected virtual void OnWaitToStop()
        {
            //No default action
        }
    }
}
