using System.Collections.Generic;
using UnityEngine;

namespace Constellation.SceneManagement.Config
{
    [CreateAssetMenu(fileName = "ScenesLibrary", menuName = "Constellation/SceneManagement/ScenesLibrary")]
    public class ScenesLibrary : ScriptableObject
    {
        public ScenesLibraryConstants scenesLibraryConstants;

        [HideInInspector]
        public List<SceneNamePair> scenePairsList = new();
    }
}