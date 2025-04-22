using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.Services.ApplicationObservers.Runtime.Pause
{
    public class ApplicationPauseObserver : MonoBehaviour, IApplicationPauseObserver
    {
        private readonly List<Action<bool>> _subscribers = new();

        private void OnApplicationPause(bool pauseStatus)
        {
            Debug.Log($"Base - ApplicationPauseObserver.OnApplicationPause({pauseStatus})");
            Publish(pauseStatus);
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