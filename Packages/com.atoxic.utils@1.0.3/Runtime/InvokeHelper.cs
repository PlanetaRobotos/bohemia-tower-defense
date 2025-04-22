using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvokeHelper : MonoSingleton<InvokeHelper>
{
    protected struct InvokeData
    {
        private Action m_Action;
        private float m_Time;

        public float endTime;
        public bool isRepeat;

        public InvokeData(Action action, float time, bool repeat)
        {
            m_Action = action;
            m_Time = time;
            endTime = Time.time + time;
            isRepeat = repeat;
        }

        public void Repeat()
        {
            endTime = Time.time + m_Time;
        }

        public void Do()
        {
            m_Action.Invoke();
        }
    }

    protected List<InvokeData> m_Invokes = new List<InvokeData>(64);

    protected void Update()
    {
        int count = m_Invokes.Count;
        float time = Time.time;

        for (int i = count - 1; i >= 0; i--)
        {
            InvokeData data = m_Invokes[i];
            if (data.endTime <= time)
            {
                if (!data.isRepeat)
                {
                    m_Invokes.RemoveAt(i);
                    data.Do();
                }
                else
                {
                    data.Do();
                    data.Repeat();
                    m_Invokes[i] = data;
                }
            }
        }

    }

    /// <summary>
    /// Call method after time
    /// </summary>
    /// <param name="action">Method name</param>
    /// <param name="time">Waiting time</param>
	public static void Invoke(Action action, float time, bool repeat = false)
    {
        Instance.m_Invokes.Add(new InvokeData(action, time, repeat));

        //Instance.StartCoroutine(Instance.InvokeCoroutine(action, time));
    }

    /// <summary>
    /// Call generic method after time
    /// </summary>
    /// <param name="action">Method name</param>
    /// <param name="arg">generic argument</param>
    /// <param name="time">Waiting time</param>
	public static void Invoke<T>(Action<T> action, T arg, float time, bool repeat = false)
    {
        Instance.m_Invokes.Add(new InvokeData(() => { action.Invoke(arg); }, time, repeat));

        //Instance.StartCoroutine(Instance.InvokeCoroutine(action, arg, time));
    }

    /// <summary>
    /// Call method after YieldInstruction
    /// </summary>
    /// <param name="action">Method name</param>
    /// <param name="time">Yield Instruction</param>
    public static void Invoke(Action action, YieldInstruction time)
    {
        Instance.StartCoroutine(Instance.InvokeCoroutine(action, time));
    }

    /// <summary>
    /// Call method after YieldInstruction
    /// </summary>
    /// <param name="action">Method name</param>
    /// <param name="time">Yield Instruction</param>
    public static void Invoke(Action action, CustomYieldInstruction time)
    {
        Instance.StartCoroutine(Instance.InvokeCoroutine(action, time));
    }

    /// <summary>
    /// Call generic method after YieldInstruction
    /// </summary>
    /// <param name="action">Method name</param>
    /// <param name="time">Yield Instruction</param>
    public static void Invoke<T>(Action<T> action, T arg, YieldInstruction time)
    {
        Instance.StartCoroutine(Instance.InvokeCoroutine(action, arg, time));
    }

    /// <summary>
    /// Call generic method after YieldInstruction
    /// </summary>
    /// <param name="action">Method name</param>
    /// <param name="time">Yield Instruction</param>
    public static void Invoke<T>(Action<T> action, T arg, CustomYieldInstruction time)
    {
        Instance.StartCoroutine(Instance.InvokeCoroutine(action, arg, time));
    }

    /// <summary>
    /// Call method after coroutine
    /// </summary>
    /// <param name="coroutine">Coroutine method</param>
    public static void Invoke(Action action, IEnumerator coroutine)
    {
        Instance.StartCoroutine(Instance.InvokeCoroutine(action, coroutine));
    }

    /// <summary>
    /// Call generic method after coroutine
    /// </summary>
    /// <param name="coroutine">Coroutine method</param>
    public static void Invoke<T>(Action<T> action, T arg, IEnumerator coroutine)
    {
        Instance.StartCoroutine(Instance.InvokeCoroutine(action, arg, coroutine));
    }

    /// <summary>
    /// Call coroutine
    /// </summary>
    /// <param name="coroutine">Coroutine method</param>
    public static void Invoke(IEnumerator coroutine)
    {
        Instance.StartCoroutine(coroutine);
    }

    protected IEnumerator InvokeCoroutine(Action action, float time)
    {
        yield return new WaitForSeconds(time);

        action?.Invoke();
    }

    protected IEnumerator InvokeCoroutine(Action action, YieldInstruction time)
    {
        yield return time;
        action?.Invoke();
    }

    protected IEnumerator InvokeCoroutine(Action action, CustomYieldInstruction time)
    {
        yield return time;
        action?.Invoke();
    }

    protected IEnumerator InvokeCoroutine<T>(Action<T> action, T arg, float time)
    {
        yield return new WaitForSeconds(time);

        action?.Invoke(arg);
    }

    protected IEnumerator InvokeCoroutine<T>(Action<T> action, T arg, YieldInstruction time)
    {
        yield return time;
        action?.Invoke(arg);
    }

    protected IEnumerator InvokeCoroutine<T>(Action<T> action, T arg, CustomYieldInstruction time)
    {
        yield return time;
        action?.Invoke(arg);
    }

    protected IEnumerator InvokeCoroutine(Action action, IEnumerator coroutine)
    {
        yield return StartCoroutine(coroutine);
        action?.Invoke();
    }

    protected IEnumerator InvokeCoroutine<T>(Action<T> action, T arg, IEnumerator coroutine)
    {
        yield return StartCoroutine(coroutine);
        action?.Invoke(arg);
    }
}