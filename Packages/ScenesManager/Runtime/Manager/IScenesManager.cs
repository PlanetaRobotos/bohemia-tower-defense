using System.Threading;
using Constellation.SceneManagement.Handlers;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Constellation.SceneManagement.Manager
{
    public interface IScenesManager
    {
        public ISceneManagerHandler AddressablesScenesManagerHandler { get; }
        public ISceneManagerHandler UnitySceneManagerHandler { get; }
        
        UniTask<Scene> LoadScene(byte sceneKey, CancellationToken cancellationToken);
        UniTask<Scene> LoadScene(int sceneKey, CancellationToken cancellationToken);
        UniTask UnloadScene(byte sceneKey, CancellationToken cancellationToken);
        UniTask UnloadScene(int sceneKey, CancellationToken cancellationToken);
    }
}