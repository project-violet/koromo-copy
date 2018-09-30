using System;

namespace Hik.Sps.Runnable
{
    /// <summary>
    /// This class is used to make possible to create a plugin that implements
    /// IRunnablePlugInApplication interface. A plugin must inherit this class to be used by
    /// main runnable application.
    /// </summary>
    /// <typeparam name="TApp">Type of main runnable application interface</typeparam>
    public abstract class RunnablePlugIn<TApp> : PlugIn<TApp>, IRunnablePlugIn
    {
        /// <summary>
        /// This event is raised when plugin is started by main application.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// This event is raised when plugin is stopped by main application.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Starts the plugin.
        /// </summary>
        public void Start()
        {
            OnStart();
        }

        /// <summary>
        /// Stops the plugin.
        /// </summary>
        public void Stop()
        {
            OnStop();
        }

        /// <summary>
        /// Waits stopping of the plugin.
        /// </summary>
        public void WaitToStop()
        {
            OnWaitToStop();
        }

        /// <summary>
        /// This method is called when plugin is started and raises Started event.
        /// A plugin may override this method or register to Started event
        /// to perform operations on startup.
        /// </summary>
        protected virtual void OnStart()
        {
            if (Started != null)
            {
                Started(this, new EventArgs());
            }
        }

        /// <summary>
        /// This method is called when plugin is stopped and raises Stopped event.
        /// A pluging may override this method or register to Stopped event
        /// to perform operations on stopping.
        /// </summary>
        protected virtual void OnStop()
        {
            if (Started != null)
            {
                Stopped(this, new EventArgs());
            }
        }

        /// <summary>
        /// This method is called when plugin is being waited to stop.
        /// A pluging may override this method to perform additional operations after all plugins are stopped.
        /// </summary>
        protected virtual void OnWaitToStop()
        {
            //No default action
        }
    }
}
