using System;
using Features.Levels.Declaration;
using UnityEngine;
using Utils;
using Infrastructure.Services.ApplicationObservers.Runtime;

namespace Features.Levels.Implementation
{
	/// <summary>
	///     Basic implementation of intro: a delay
	/// </summary>
	public class TimedLevelIntro : LevelIntro
    {
        [Inject] private IUpdater Updater { get; }

	    /// <summary>
	    ///     The delay
	    /// </summary>
	    public float time = 5f;

	    /// <summary>
	    ///     Timer object used to track the delayed
	    /// </summary>
	    protected Timer m_Timer;

	    /// <summary>
	    ///     Set up the timer and make it fire the SafelyCallIntroCompleted event
	    /// </summary>
	    protected virtual void Awake()
        {
            m_Timer = new Timer(time, SafelyCallIntroCompleted);
        }

	    private void Start()
	    {
		    Updater.Subscribe(OnUpdate, 0);
	    }

	    protected virtual void OnDestroy()
        {
            Updater?.Unsubscribe(OnUpdate);
        }

	    /// <summary>
	    ///     Tick the timer and disable it on completion
	    /// </summary>
	    protected virtual void OnUpdate(float _)
        {
            if (m_Timer != null)
                if (m_Timer.Tick(Time.deltaTime))
                    m_Timer = null;
        }
    }
}