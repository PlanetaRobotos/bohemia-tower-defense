using UnityEngine;
using UnityEngine.Events;
using Utils;

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

	    /// <summary>
	    ///     Update the timer
	    /// </summary>
	    protected virtual void Update()
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