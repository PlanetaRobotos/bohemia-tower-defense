using System.Threading;
using Constellation.SceneManagement.Config;
using Constellation.SceneManagement.Handlers;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Constellation.SceneManagement.Manager
{
    public class ScenesManager : IScenesManager
    {
        public ISceneManagerHandler AddressablesScenesManagerHandler { get; }
        public ISceneManagerHandler UnitySceneManagerHandler { get; }

        private ScenesLibrary _library;

        public ScenesManager(ScenesLibrary library)
        {
            _library = library;

            UnitySceneManagerHandler = new UnitySceneManagerHandler();
        }

        public UniTask<Scene> LoadScene(byte sceneKey, CancellationToken cancellationToken)
        {
            var config = _library.scenePairsList.Find(x => x.key == sceneKey).sceneConfig;
            return config.isAddressable ? AddressablesScenesManagerHandler.LoadScene(config, cancellationToken) : UnitySceneManagerHandler.LoadScene(config, cancellationToken);
        }

        public UniTask<Scene> LoadScene(int sceneKey, CancellationToken cancellationToken)
        {
            var config = _library.scenePairsList.Find(x => x.key == sceneKey).sceneConfig;
            return config.isAddressable ? AddressablesScenesManagerHandler.LoadScene(config, cancellationToken) : UnitySceneManagerHandler.LoadScene(config, cancellationToken);
        }

        public UniTask UnloadScene(byte sceneKey, CancellationToken cancellationToken)
        {
            var config = _library.scenePairsList.Find(x => x.key == sceneKey).sceneConfig;
            return config.isAddressable ? AddressablesScenesManagerHandler.UnloadScene(config, cancellationToken) : UnitySceneManagerHandler.UnloadScene(config, cancellationToken);
        }

        public UniTask UnloadScene(int sceneKey, CancellationToken cancellationToken)
        {
            var config = _library.scenePairsList.Find(x => x.key == sceneKey).sceneConfig;
            return config.isAddressable ? AddressablesScenesManagerHandler.UnloadScene(config, cancellationToken) : UnitySceneManagerHandler.UnloadScene(config, cancellationToken);
        }
    }
}