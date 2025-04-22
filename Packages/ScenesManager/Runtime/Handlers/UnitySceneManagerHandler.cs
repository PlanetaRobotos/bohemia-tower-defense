using System.Threading;
using Constellation.SceneManagement.Config;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace Constellation.SceneManagement.Handlers
{
    public class UnitySceneManagerHandler : ISceneManagerHandler
    {
        public async UniTask<Scene> LoadScene(SceneConfig sceneConfig, CancellationToken cancellationToken)
{
    var sceneIndex = SceneUtility.GetBuildIndexByScenePath(sceneConfig.scenePath);
    var asyncOperation = SceneManager.LoadSceneAsync(sceneIndex, sceneConfig.loadSceneMode);

    var tcs = new UniTaskCompletionSource<Scene>();

    using (cancellationToken.Register(() =>
    {
        tcs.TrySetCanceled(cancellationToken);
    }))
    {
        asyncOperation.completed += operation =>
        {
            if (operation.isDone && !cancellationToken.IsCancellationRequested)
            {
                tcs.TrySetResult(SceneManager.GetSceneByBuildIndex(sceneIndex));
            }
            else
            {
                tcs.TrySetException(new System.Exception("Failed to load scene: " + sceneIndex));
            }
        };

        return await tcs.Task;
    }
}


        public UniTask UnloadScene(SceneConfig sceneConfig, CancellationToken cancellationToken)
        {
            var sceneIndex = SceneUtility.GetBuildIndexByScenePath(sceneConfig.scenePath);
            return SceneManager.UnloadSceneAsync(sceneIndex).ToUniTask(cancellationToken: cancellationToken);
        }
    }
}