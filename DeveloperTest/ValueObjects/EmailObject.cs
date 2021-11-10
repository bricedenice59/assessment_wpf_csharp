using System;
using System.Threading;

namespace DeveloperTest.ValueObjects
{
    public class EmailObject
    {
        #region Fields

        private long _isBodyBeingDownloaded = 0;

        #endregion

        #region Properties

        public string Uid { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public string Date { get; set; }
        public string Body { get; set; }

        public bool IsBodyBeingDownloaded
        {
            get => Interlocked.Read(ref _isBodyBeingDownloaded) == 1;
            set => Interlocked.Exchange(ref _isBodyBeingDownloaded, Convert.ToInt64(value));
        }

        public bool IsBodyDownloaded { get; set; }

        #endregion
    }
}