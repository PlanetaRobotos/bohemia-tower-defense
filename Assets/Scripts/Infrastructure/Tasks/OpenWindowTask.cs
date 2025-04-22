using Cysharp.Threading.Tasks;
using WindowsSystem.Core;
using WindowsSystem.Core.Managers;
using Infrastructure.Extensions.AsyncExtensions;

namespace Infrastructure.Tasks
{
    public class OpenWindowTask<T> : BaseTask where T : BaseWindow
    {
        private readonly WindowsController _windowsController;
        private readonly string _name;
        private readonly bool _waitOpen;

        public OpenWindowTask(WindowsController windowsController, string name, bool waitOpen)
        {
            _windowsController = windowsController;
            _name = name;
            _waitOpen = waitOpen;
        }

        public override ITask Do()
        {
            DoAsync().Forget();
            return this;
        }

        private async UniTask DoAsync()
        {
            if (!_windowsController.TryGetWindowById(_name, out T window)
                || window.Status != Status.Opened && window.Status != Status.Opening)
            {
                window = await _windowsController.OpenWindowAsync<T>(_name, null, immediate: true);
            }

            if (_waitOpen)
            {
                await window.WaitOpened();
            }
            else
            {
                await window.WaitOpen();
            }

            Complete();
        }

        public override string ToString() =>
            $"{nameof(OpenWindowTask<T>)}_{typeof(T).Name}";
    }
}