using System.Threading;
using Constellation.SceneManagement.Config;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Constellation.SceneManagement.Handlers
{
    public interface ISceneManagerHandler
    {
        UniTask<Scene> LoadScene(SceneConfig sceneConfig, CancellationToken cancellationToken);
        UniTask UnloadScene(SceneConfig sceneConfig, CancellationToken cancellationToken);
    }
}