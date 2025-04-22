using System;

namespace Infrastructure.Services.ApplicationObservers.Runtime.Focus
{
    public interface IApplicationFocusObserver
    {
        void AddSubscriber(Action<bool> subscriber);
        void RemoveSubscriber(Action<bool> subscriber);
    }
}