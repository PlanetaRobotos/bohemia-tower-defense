using Windows.Global;
using ServiceLocator.Core;
using UnityEngine;

namespace Registrators
{
    public class GameUIRegistrator : BaseMonoServicesRegistrator
    {
        [SerializeField] private GameUI _gameUI;

        public override void Register()
        {
            Locator.Register(_gameUI);
        }
    }
}
