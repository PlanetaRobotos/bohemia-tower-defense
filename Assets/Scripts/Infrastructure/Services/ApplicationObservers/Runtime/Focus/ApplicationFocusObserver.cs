using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.Services.ApplicationObservers.Runtime.Focus
{
    public class ApplicationFocusObserver : MonoBehaviour, IApplicationFocusObserver
    {
        private readonly List<Action<bool>> _subscribers = new();

        private void OnApplicationFocus(bool hasFocus)
        {
            Publish(hasFocus);
        }

        public void AddSubscriber(Action<bool> subscriber)
        {
            if(_subscribers.Contains(subscriber))
                return;
            
            _subscribers.Add(subscriber);
        }
        
        public void RemoveSubscriber(Action<bool> subscriber) => 
            _subscribers.Remove(subscriber);

        private void Publish(bool pauseStatus)
        {
            for (int i = 0; i < _subscribers.Count; i++)
            {
                _subscribers[i]?.Invoke(pauseStatus);
            }
        }
    }
}