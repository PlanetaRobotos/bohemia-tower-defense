using System.Collections.Generic;
using UnityEngine;

namespace Constellation.SceneManagement.Config
{
    [CreateAssetMenu(fileName = "ScenesLibraryConstants", menuName = "Constellation/SceneManagement/ScenesLibraryConstants")]
    public class ScenesLibraryConstants : ScriptableObject
    {
        [HideInInspector]
        public string enumsPath;

        [HideInInspector]
        public List<string> soundMapKeys = new List<string>();
    }
}