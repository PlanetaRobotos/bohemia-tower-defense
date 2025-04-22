using System.Threading;
using Constellation.SceneManagement;
using Constellation.SceneManagement.Manager;
using Cysharp.Threading.Tasks;
using GameTasks;
using GameTasks.Core;
using Infrastructure.DI;
using Services.States;
using WindowsSystem.Core.Managers;

namespace GameStates
{
    public class LoadApplicationState : IState
    {
        [Inject] private readonly IScenesManager _scenesManager;
        [Inject] private readonly ApplicationStateMachine _stateMachine;
        [Inject] private readonly TasksLoader _tasksLoader;
        [Inject] private readonly WindowsController _windowsController;

        private CancellationTokenSource _cts;

        public void Enter()
        {
            _cts = new CancellationTokenSource();

            _tasksLoader.DoTasks(GetTasks()).OnDone(_ => _stateMachine.Enter<MainMenuState>());
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
                new MakeActionTaskAsync(() => UniTask.CompletedTask)
            };
            return tasks;
        }
    }
}