using UnityEngine;
using UnityInput = UnityEngine.Input;

namespace Features.Input.Extensions
{
    public static class TouchHelper
    {
        public static int Count
        {
            get
            {
#if UNITY_WEBGL || UNITY_EDITOR
                return UnityInput.GetMouseButton(0) ? 1 : 0;
#else
            return UnityInput.Count;
#endif
            }
        }

        public static Vector2 Position
        {
            get
            {
#if UNITY_WEBGL || UNITY_EDITOR
                return (Vector2)UnityInput.mousePosition;
#else
            return UnityInput.GetTouch(0).position;
#endif
            }
        }

        public static TouchPhase Phase
        {
            get
            {
#if UNITY_WEBGL || UNITY_EDITOR
                if (UnityInput.GetMouseButtonDown(0)) return TouchPhase.Began;
                if (UnityInput.GetMouseButtonUp(0))   return TouchPhase.Ended;
                if (UnityInput.GetMouseButton(0))     return TouchPhase.Moved;
                return TouchPhase.Canceled;
#else
            return UnityInput.GetTouch(0).phase;
#endif
            }
        }
    }
}