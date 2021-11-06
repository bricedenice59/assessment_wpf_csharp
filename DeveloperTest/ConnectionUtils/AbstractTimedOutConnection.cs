using System;

namespace DeveloperTest.ConnectionUtils
{
    public abstract class AbstractTimedOutConnection : AbstractConnectionResource
    {
        System.Timers.Timer _timer;
        bool _timedOut;

        public void SetTimeout(TimeSpan timeout)
        {
            _timer = new System.Timers.Timer(timeout.TotalMilliseconds);
            _timer.Elapsed += Timer_Elapsed;
            _timer.Start();
        }

        void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _timer.Stop();
            _timer.Dispose();
            _timedOut = true;
            Dispose();
        }

        public override bool IsAlive()
        {
            return !_timedOut;
        }
    }
}
