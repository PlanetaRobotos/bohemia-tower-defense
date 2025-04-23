using System;
using UnityEngine;
using UnityEngine.Events;
using Utils;
using Infrastructure.Services.ApplicationObservers.Runtime;

namespace Features.Towers
{
	/// <summary>
	///     A helper component for self destruction
	/// </summary>
	public class SelfDestroyTimer : MonoBehaviour
    {
	    /// <summary>
	    ///     The time before destruction
	    /// </summary>
	    public float time = 5;

	    /// <summary>
	    ///     The exposed death callback
	    /// </summary>
	    public UnityEvent death;

	    /// <summary>
	    ///     The controlling timer
	    /// </summary>
	    public Timer timer;

	    [Inject] private IUpdater Updater { get; }

	    public virtual void Start()
	    {
		    // Updater.Subscribe(OnUpdate, 0);
	    }

	    /// <summary>
	    ///     Update the timer
	    /// </summary>
	    protected virtual void OnUpdate(float _)
        {
            if (timer == null) return;
            timer.Tick(Time.deltaTime);
        }

	    private void Update()
	    {
		    if (timer == null) return;
		    timer.Tick(Time.deltaTime);
	    }

	    /// <summary>
	    ///     Potentially initialize the time if necessary
	    /// </summary>
	    protected virtual void OnEnable()
        {
            if (timer == null)
                timer = new Timer(time, OnTimeEnd);
            else
                timer.Reset();
        }

	    /// <summary>
	    ///     Fires at the end of timer
	    /// </summary>
	    protected virtual void OnTimeEnd()
        {
            death.Invoke();
            Poolable.TryPool(gameObject);
            timer.Reset();
        }
    }
}