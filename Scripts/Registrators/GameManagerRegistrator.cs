using Features.Data.Implementation;
using ServiceLocator.Core;
using UnityEngine;

namespace Registrators
{
    public class GameManagerRegistrator : BaseMonoServicesRegistrator
    {
        [SerializeField] private GameManager _gameManager;

        public override void Register()
        {
            Locator.Register(_gameManager);
        }
    }
}
