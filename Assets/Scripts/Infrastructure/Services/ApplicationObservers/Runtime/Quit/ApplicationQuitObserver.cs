using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure.Services.ApplicationObservers.Runtime.Quit
{
    public class ApplicationQuitObserver : MonoBehaviour, IApplicationQuitObserver
    {
        private readonly List<Action> _subscribers = new();

        private void OnApplicationQuit()
        {
            Publish();
            
            _subscribers.Clear();
        }

        public void AddSubscriber(Action subscriber)
        {
            if(_subscribers.Contains(subscriber))
                return;
            
            _subscribers.Add(subscriber);
        }
        
        public void RemoveSubscriber(Action subscriber) => 
            _subscribers.Remove(subscriber);

        private void Publish()
        {
            for (int i = 0; i < _subscribers.Count; i++)
            {
                _subscribers[i]?.Invoke();
            }
        }
    }
}