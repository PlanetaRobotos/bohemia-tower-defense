using System;

namespace Infrastructure.Services.ApplicationObservers.Runtime.Pause
{
    public interface IApplicationPauseObserver
    {
        void AddSubscriber(Action<bool> subscriber);
        void RemoveSubscriber(Action<bool> subscriber);
    }
}