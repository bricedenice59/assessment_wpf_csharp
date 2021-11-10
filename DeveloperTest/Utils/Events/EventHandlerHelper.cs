using System;
using GalaSoft.MvvmLight.Threading;

namespace DeveloperTest.Utils.Events
{
    public static class EventHandlerHelper
    {
        public static EventHandler<T> SafeEventHandler<T>(Action<object, T> action) where T : EventArgs
        {
            return (s, e) => { DispatcherHelper.RunAsync(() => { action(s, e); }); };
        }

        public static EventHandler SafeEventHandler(Action<object, EventArgs> action)
        {
            return (s, e) => { DispatcherHelper.RunAsync(() => { action(s, e); }); };
        }
    }
}
