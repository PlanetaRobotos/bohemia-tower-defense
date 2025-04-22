using System;
using UnityEngine;

namespace Infrastructure.Services.Logging
{
    public class UnityLogger : ICustomLogger
    {
        private readonly LogType _logType;

        public UnityLogger(LogType logType)
        {
            _logType = logType;
        }

        public void Log(string message)
        {
            if(!_logType.HasFlag(LogType.Log)) 
                return;
            
            Debug.Log(message);
        }
        
        public void LogError(string message)
        {
            if(!_logType.HasFlag(LogType.LogError)) 
                return;
            
            Debug.LogError(message);
        }
        
        public void LogWarning(string message)
        {
            if(!_logType.HasFlag(LogType.LogWarning)) 
                return;

            Debug.LogWarning(message);
        }
        
        public void LogMessage(string message, LogType logType)
        {
            switch(logType)
            {
                case LogType.Log:
                    Log(message);
                    break;
                
                case LogType.LogError:
                    LogError(message);
                    break;
                
                case LogType.LogWarning:
                    LogWarning(message);
                    break;
                
                default:
                    Debug.LogException(new ArgumentOutOfRangeException(nameof(logType), logType, null));
                    break;
            };
        }
    }
}