using System.Threading;
using Windows.MainMenuWidget.Views;
using Constellation.SceneManagement;
using Constellation.SceneManagement.Manager;
using GameConstants;
using GameTasks;
using GameTasks.Core;
using Infrastructure.DI;
using Infrastructure.Tasks;
using Services.States;
using WindowsSystem.Core.Managers;

namespace GameStates
{
    public class MainMenuState : IState
    {
        [Inject] private readonly ApplicationStateMachine _stateMachine;
        [Inject] private readonly TasksLoader _tasksLoader;
        [Inject] private readonly WindowsController _windowsController;
        [Inject] private readonly IScenesManager _scenesManager;

        private CancellationTokenSource _cts;

        public void Enter()
        {
            _cts = new CancellationTokenSource();

            _tasksLoader.DoTasks(GetTasks());
        }

        public void Exit()
        {
            if (_tasksLoader.enabled)
                _tasksLoader.AbortTasks();
        }

        private ITask[] GetTasks()
        {
            ITask[] tasks =
            {
                new MakeActionTaskAsync(
                    () => _scenesManager.LoadScene((byte)SceneLibraryConstants.MAIN_MENU, _cts.Token)),
                new OpenWindowTask<MainMenuWindow>(_windowsController, WindowsConstants.MAIN_MENU_WINDOW, true),
            };

            return tasks;
        }
    }
}