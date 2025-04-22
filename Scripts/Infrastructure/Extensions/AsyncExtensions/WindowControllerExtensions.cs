#if ASYNC
using Cysharp.Threading.Tasks;
using WindowsSystem.Core;
using WindowsSystem.Core.Managers;

namespace Infrastructure.Extensions.AsyncExtensions
{
    public static class WindowControllerExtensions
    {
        public static UniTask<T> OpenWindowAsync<T>(this WindowsController controller, string name,
            IncomingData data, bool immediate) where T : BaseWindow
        {
            var completionSource = new UniTaskCompletionSource<T>();
            data ??= new IncomingData();
            data.onWindowOpen += view => completionSource.TrySetResult(view as T);

            if (immediate)
                controller.OpenImmediate<T>(name, data);
            else
                controller.Open<T>(name, data);
            return completionSource.Task;
        }

        public static UniTask WaitCloseAllWindows(this WindowsController controller)
            => UniTask.WaitWhile(() => controller.IsSomeWindowOpened() || controller.IsWindowLoading);

        public static UniTask WaitClose(this WindowsController controller, string name)
        {
            if (controller.TryGetWindowById(name, out var windowView))
            {
                var completionSource = new UniTaskCompletionSource();
                windowView.IncomingData.onWindowClose += _ => completionSource.TrySetResult();
                return completionSource.Task;
            }

            return UniTask.CompletedTask;
        }

        public static UniTask WaitClose(this BaseWindow windowView)
        {
            var completionSource = new UniTaskCompletionSource();
            windowView.IncomingData.onWindowClose += _ => completionSource.TrySetResult();
            return completionSource.Task;
        }

        public static UniTask WaitClosed(this BaseWindow windowView)
        {
            var completionSource = new UniTaskCompletionSource();
            windowView.IncomingData.onCloseAnimPlayed += _ => completionSource.TrySetResult();
            return completionSource.Task;
        }

        public static UniTask WaitOpen(this BaseWindow windowView)
        {
            if (windowView.Status == Status.Opened || windowView.Status == Status.Opening)
                return UniTask.CompletedTask;

            var completionSource = new UniTaskCompletionSource();
            windowView.IncomingData ??= new IncomingData();
            windowView.IncomingData.onWindowOpen += _ => completionSource.TrySetResult();
            return completionSource.Task;
        }

        public static UniTask WaitOpened(this BaseWindow windowView)
        {
            if (windowView.Status == Status.Opened)
                return UniTask.CompletedTask;

            var completionSource = new UniTaskCompletionSource();

            windowView.IncomingData ??= new IncomingData();
            windowView.IncomingData.onOpenAnimPlayed += _ => completionSource.TrySetResult();

            return completionSource.Task;
        }

        public static async UniTask WaitOpen(this WindowsController controller, string name)
        {
            var completionSource = new UniTaskCompletionSource();
            if (!controller.TryGetWindowById(name, out _))
            {
                controller.OnWindowOpenEvent += WindowOpenEvent;
                await completionSource.Task;
                controller.OnWindowOpenEvent -= WindowOpenEvent;
            }

            void WindowOpenEvent(IWindowView view)
            {
                if (view.WindowId == name)
                    completionSource.TrySetResult();
            }
        }

        public static bool HasWindow(this WindowsController controller, string name) =>
            controller.TryGetWindowById(name, out _);

        //public static WindowAwaiter GetAwaiter(this BaseWindow window) => new(window);
    }
}
#endif