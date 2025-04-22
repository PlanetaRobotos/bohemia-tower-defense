using ServiceLocator.Core;
using UnityEngine;

namespace Infrastructure.DI
{
    public class SceneContext : MonoBehaviour
    {
        [SerializeField] private MonoServicesRegistrator _monoServicesRegistrator;

        public void Awake()
        {
            _monoServicesRegistrator.Register();
        }
    }
}
