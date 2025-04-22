using ServiceLocator.Core;
using UnityEngine;
using Utils;

namespace Registrators
{
    public class PoolManagerRegistrator : BaseMonoServicesRegistrator
    {
        [SerializeField] private PoolManager _poolManager;

        public override void Register()
        {
            Locator.Register(_poolManager);
        }
    }
}