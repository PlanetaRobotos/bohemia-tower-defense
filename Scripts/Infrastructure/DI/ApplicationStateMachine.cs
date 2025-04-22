using GameStates;
using ServiceLocator.Core;
using Services.States;
using UnityEngine;

namespace Infrastructure.DI
{
    public class ApplicationStateMachine : StateMachineMonoBehaviour
    {
        [SerializeField] private MonoServicesRegistrator _monoServicesRegistrator;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _monoServicesRegistrator.Register();
        }

        private void Start()
        {
            AddState(new InitializeApplicationState());
            AddState(new LoadApplicationState());
            AddState(new MainMenuState());
            AddState(new GameplayState());

            Enter<InitializeApplicationState>();
        }
    }
}