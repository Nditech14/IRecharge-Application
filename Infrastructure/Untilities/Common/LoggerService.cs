using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Untilities.Common
{
    public class LoggerService<T> : ILoggerService<T>
    {
        private readonly ILoggerService<T> _logger;

        public LoggerService(ILoggerService<T> logger)
        {
            _logger = logger;
        }

        public void LogError(Exception ex, string message, params object[] args)
        {
            _logger.LogError(ex, message, args);
        }

        public void LogInfo(string message, params object[] args)
        {
            _logger.LogInfo(message, args);
        }
    }
}
