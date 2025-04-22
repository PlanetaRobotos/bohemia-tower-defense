using System;

namespace Infrastructure.Services.Logging
{
    [Flags]
    public enum LogType
    {
        Log = 0,
        LogError,
        LogWarning,
    }
}