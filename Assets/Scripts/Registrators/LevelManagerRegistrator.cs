using Features.Levels.Implementation;
using ServiceLocator.Core;
using UnityEngine;

namespace Registrators
{
    public class LevelManagerRegistrator : BaseMonoServicesRegistrator
    {
        [SerializeField] private LevelManager _levelManager;

        public override void Register()
        {
            Locator.Register(_levelManager);
        }
    }
} 