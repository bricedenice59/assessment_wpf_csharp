using System;

namespace DeveloperTest.ConnectionUtils
{
    public abstract class AbstractConnectionResource : IDisposable
    {
        /// <summary>
        /// When implementing an AbstractConnection, I want to ensure connection items being retrieved
        /// are still valid (e.g. a connection is still connected)
        /// </summary>
        /// <returns></returns>
        public abstract bool IsAlive();

        public abstract void Dispose();
    }
}
