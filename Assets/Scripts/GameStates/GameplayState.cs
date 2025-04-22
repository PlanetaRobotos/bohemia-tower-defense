using System.Threading;
using Windows.HUD.Views;
using Windows.Pause.Views;
using Windows.ResultWidget.Views;
using Constellation.SceneManagement;
using Constellation.SceneManagement.Manager;
using Cysharp.Threading.Tasks;
using GameConstants;
using GameTasks;
using GameTasks.Core;
using Infrastructure.DI;
using Infrastructure.Tasks;
using Services.States;
using UnityEngine.SceneManagement;
using WindowsSystem.Core.Managers;

namespace GameStates
{
    public class GameplayState : IState<string>
    {
        [Inject] private readonly ApplicationStateMachine _stateMachine;
        [Inject] private readonly TasksLoader _tasksLoader;
        [Inject] private readonly WindowsController _windowsController;
        [Inject] private readonly IScenesManager _scenesManager;

        private CancellationTokenSource _cts;
        private string _sceneName;

        public void Enter(string data)
        {
            _cts = new CancellationTokenSource();

            _sceneName = data;
            
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
                    () => SceneManager.LoadSceneAsync(_sceneName).ToUniTask()),
                new OpenWindowTask<HUDWindow>(_windowsController, WindowsConstants.HUD_WINDOW, true),
                new OpenWindowTask<PauseWindow>(_windowsController, WindowsConstants.PAUSE_WINDOW, true),
                new OpenWindowTask<EndGameWindow>(_windowsController, WindowsConstants.END_GAME_WINDOW, true),
            };

            return tasks;
        }

        private void OnTasksProgress(float progress)
        {
            // LoadingWindow.progress = progress;
        }
    }
}