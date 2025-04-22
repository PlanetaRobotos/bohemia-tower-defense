using System.Collections.Generic;
using UnityEngine;
using Infrastructure.Services.ApplicationObservers.Runtime;

namespace Utils
{
	/// <summary>
	///     Abstract based class for helping with timing in MonoBehaviours
	/// </summary>
	public abstract class TimedBehaviour : MonoBehaviour
    {
        [Inject] private IUpdater Updater { get; }

	    /// <summary>
	    ///     List of active timers
	    /// </summary>
	    private readonly List<Timer> m_ActiveTimers = new();

        protected virtual void Awake()
        {
            Updater.Subscribe(OnUpdate, 0);
        }

        protected virtual void OnDestroy()
        {
            Updater?.Unsubscribe(OnUpdate);
        }

	    /// <summary>
	    ///     Iterates through the list of active timers and ticks
	    /// </summary>
	    protected virtual void OnUpdate(float _)
        {
            for (var i = m_ActiveTimers.Count - 1; i >= 0; i--)
                if (m_ActiveTimers[i].Tick(Time.deltaTime))
                    StopTimer(m_ActiveTimers[i]);
        }

	    /// <summary>
	    ///     Adds the timer to list  of active timers
	    /// </summary>
	    /// <param name="newTimer">the  timer to be added to the list of active timers</param>
	    protected void StartTimer(Timer newTimer)
        {
            if (m_ActiveTimers.Contains(newTimer))
                Debug.LogWarning("Timer already exists!");
            else
                m_ActiveTimers.Add(newTimer);
        }

	    /// <summary>
	    ///     Removes timer from list of active timers
	    /// </summary>
	    /// <param name="timer">the timer to be removed from the list of active timers</param>
	    protected void PauseTimer(Timer timer)
        {
            if (m_ActiveTimers.Contains(timer)) m_ActiveTimers.Remove(timer);
        }

	    /// <summary>
	    ///     Resets and removes the timer
	    /// </summary>
	    /// <param name="timer">the timer to be stopped</param>
	    protected void StopTimer(Timer timer)
        {
            timer.Reset();
            PauseTimer(timer);
        }
    }
}