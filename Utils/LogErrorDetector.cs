using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ULDKClient.Utils
{
    internal class LogErrorDetector : ILogEventSink
    {
        volatile bool _errorDetected;
        public bool ErrorDetected { get { return _errorDetected; } }
        public void Emit(LogEvent evt)
        {
            if (evt.Level == LogEventLevel.Error ||
                evt.Level == LogEventLevel.Fatal ||
                evt.Level == LogEventLevel.Information
              )
            {
                _errorDetected = true;
            }
        }
    }
}
