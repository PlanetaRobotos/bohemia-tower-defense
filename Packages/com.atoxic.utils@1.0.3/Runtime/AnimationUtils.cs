using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace Galaxy4Games.Utils
{
    public class AnimationUtils
    {
        public static IEnumerator AnimateValueCoroutine(float startValue, float endValue, float duration, Action<float> progressCallback)
        {
            var passedTime = 0f;
            var progress = 0f;

            while (progress < 1f)
            {
                passedTime += Time.deltaTime;
                progress = passedTime / duration;
                progressCallback(Mathf.Lerp(startValue, endValue, progress));
                yield return null;
            }

            progressCallback(endValue);
        }

        public static IEnumerator AnimateValueCoroutine(int startValue, int endValue, float duration, Action<int> progressCallback)
        {
            var passedTime = 0f;
            var progress = 0f;

            while (progress < 1f)
            {
                passedTime += Time.deltaTime;
                progress = passedTime / duration;
                progressCallback((int)Mathf.Lerp(startValue, endValue, progress));
                yield return null;
            }

            progressCallback(endValue);
        }

        public static IEnumerator AnimateValueCoroutine(Vector2 startValue, Vector2 endValue, float duration, Action<Vector2> progressCallback)
        {
            var passedTime = 0f;
            var progress = 0f;

            while (progress < 1f)
            {
                passedTime += Time.deltaTime;
                progress = passedTime / duration;
                progressCallback(Vector2.Lerp(startValue, endValue, progress));
                yield return null;
            }

            progressCallback(endValue);
        }

        public static IEnumerator AnimateValueCoroutine(Vector3 startValue, Vector3 endValue, float duration, Action<Vector3> progressCallback)
        {
            var passedTime = 0f;
            var progress = 0f;

            while (progress < 1f)
            {
                passedTime += Time.deltaTime;
                progress = passedTime / duration;
                progressCallback(Vector3.Lerp(startValue, endValue, progress));
                yield return null;
            }

            progressCallback(endValue);
        }

        public static IEnumerator RotateVector3Coroutine(Vector3 startValue, Vector3 endValue, float duration, Action<Vector3> progressCallback)
        {
            var passedTime = 0f;
            var progress = 0f;

            if (Mathf.Abs(endValue.y - startValue.y) > 180)
            {
                if (endValue.y < startValue.y)
                    endValue.y += 360;
                else
                    startValue.y += 360;
            }

            while (progress < 1f)
            {
                passedTime += Time.deltaTime;
                progress = passedTime / duration;
                progressCallback(Vector3.Lerp(startValue, endValue, progress));
                yield return null;
            }

            progressCallback(endValue);
        }

        public static async void AnimateValueAsync(float startValue, float endValue, float duration, Action<float> progressCallback)
        {
            var passedTime = 0f;
            var progress = 0f;

            while (progress < 1f)
            {
                passedTime += Time.deltaTime;
                progress = passedTime / duration;
                progressCallback(Mathf.Lerp(startValue, endValue, progress));
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            progressCallback(endValue);
        }

        public static async UniTaskVoid AnimateValueAsync(float startValue, float endValue, float duration, Action<float> progressCallback, CancellationToken cancellationToken = default)
        {
            var passedTime = 0f;
            var progress = 0f;

            while (progress < 1f)
            {
                passedTime += Time.deltaTime;
                progress = passedTime / duration;
                progressCallback(Mathf.Lerp(startValue, endValue, progress));
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            progressCallback(endValue);
        }

        public static async void AnimateValueAsync(int startValue, int endValue, float duration, Action<int> progressCallback)
        {
            var passedTime = 0f;
            var progress = 0f;

            while (progress < 1f)
            {
                passedTime += Time.deltaTime;
                progress = passedTime / duration;
                progressCallback((int)Mathf.Lerp(startValue, endValue, progress));
                await UniTask.Yield(PlayerLoopTiming.Update);
            }

            progressCallback(endValue);
        }

        public static async UniTaskVoid AnimateValueAsync(int startValue, int endValue, float duration, Action<int> progressCallback, CancellationToken cancellationToken = default)
        {
            var passedTime = 0f;
            var progress = 0f;

            while (progress < 1f)
            {
                passedTime += Time.deltaTime;
                progress = passedTime / duration;
                progressCallback((int)Mathf.Lerp(startValue, endValue, progress));
                await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
            }

            progressCallback(endValue);
        }

        public static IEnumerator MoveCoroutine(Vector3 startValue, Transform target, float duration, Action<Vector3> progressCallback)
        {
            var passedTime = 0f;
            var progress = 0f;
            var endValue = target.position;

            while (progress < 1f)
            {
                passedTime += Time.deltaTime;
                progress = passedTime / duration;

                if (target)
                    endValue = target.position;

                progressCallback(Vector3.Lerp(startValue, endValue, progress));
                yield return null;
            }

            progressCallback(endValue);
        }
    }
}