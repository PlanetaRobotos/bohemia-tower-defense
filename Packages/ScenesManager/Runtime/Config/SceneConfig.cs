using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Constellation.SceneManagement.Config
{
    [CreateAssetMenu(fileName = "SceneConfig", menuName = "Constellation/SceneManagement/SceneConfig")]
    public class SceneConfig : ScriptableObject
    {
        public bool asyncLoad;
        public LoadSceneMode loadSceneMode;
        
        // Scene Reference
        public bool isAddressable;
        public AssetReference sceneReference;
        
        public string scenePath;
    }
}