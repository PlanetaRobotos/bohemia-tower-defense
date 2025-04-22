using Features.Input.Implementation;
using ServiceLocator.Core;
using UnityEngine;

namespace Registrators
{
    public class InputControllerRegistrator : BaseMonoServicesRegistrator
    {
        [SerializeField] private InputController _inputController;

        public override void Register()
        {
            Locator.Register(_inputController);
        }
    }
} 