using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Windows.Global
{
	/// <summary>
	/// Simple component to load scenes by name
	/// </summary>
	public class SceneLoader : MonoBehaviour
	{
		/// <summary>
		/// Name of the scene to load
		/// </summary>
		public string sceneToLoadName = "LevelSelect";

		/// <summary>
		/// Loads the scene from <see cref="sceneToLoadName" />
		/// if a scene with that name exists
		/// </summary>
		public void LoadScene()
		{
			StartCoroutine(LoadSceneAsync(sceneToLoadName));
		}

		private IEnumerator LoadSceneAsync(string sceneName)
		{
			AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
			asyncLoad.allowSceneActivation = false;
			while (asyncLoad.progress < 0.9f) yield return null;
			asyncLoad.allowSceneActivation = true;
			while (!asyncLoad.isDone) yield return null;
		}

		/// <summary>
		/// Restarts the current scene
		/// </summary>
		public void RestartCurrentScene()
		{
			Scene activeScene = SceneManager.GetActiveScene();
			StartCoroutine(LoadSceneAsync(activeScene.name));
		}
	}
}