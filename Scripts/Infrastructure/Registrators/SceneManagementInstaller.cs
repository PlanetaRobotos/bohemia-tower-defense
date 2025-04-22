using Constellation.SceneManagement;
using Constellation.SceneManagement.Config;
using Constellation.SceneManagement.Manager;
using ServiceLocator.Core;
using UnityEngine;

namespace Infrastructure.Registrators
{
    public class SceneManagementInstaller : BaseMonoServicesRegistrator
    {
        [SerializeField] private ScenesLibrary scenesLibrary;

        public override void Register()
        {
            Locator.Register(scenesLibrary);
            Locator.Register<IScenesManager>(new ScenesManager(scenesLibrary));
        }
    }
}