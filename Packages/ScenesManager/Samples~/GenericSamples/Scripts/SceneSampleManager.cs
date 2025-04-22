using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Constellation.SceneManagement.Samples.GenericSamples
{
    public class SceneSampleManager : MonoBehaviour
    {
        [SerializeField] private Button _changeSceneButton;
        
        private IScenesManager _scenesManager;

        [Inject]
        private void Construct(IScenesManager scenesManager)
        {
            _scenesManager = scenesManager;
        }
        
        private void Start()
        {
            _changeSceneButton.onClick.AddListener(ChangeScene);
        }

        private void ChangeScene()
        {
            _scenesManager.LoadScene((byte)SceneLibraryConstants.BOOTSCENE_SCENEMANAGEMENTSAMPLE, CancellationToken.None);
        }
    }
}