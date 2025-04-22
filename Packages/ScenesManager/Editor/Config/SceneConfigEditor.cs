using Constellation.SceneManagement.Config;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Constellation.SceneManagement.Editor.Editor.Config
{
    [CustomEditor(typeof(SceneConfig))]
    public class SceneConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _sceneReference;
        private SerializedProperty _scenePath;

        private void OnEnable()
        {
            _sceneReference = serializedObject.FindProperty("sceneReference");
            _scenePath = serializedObject.FindProperty("scenePath");
        }
        
        public override void OnInspectorGUI()
        {
            var sceneConfig = (SceneConfig)target;
            
            serializedObject.Update();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Scene Reference", EditorStyles.largeLabel);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Set true in case you a want to load the scene as an addressable.", EditorStyles.miniLabel);
            sceneConfig.isAddressable = EditorGUILayout.Toggle("Is Addressable", sceneConfig.isAddressable);

            if (sceneConfig.isAddressable)
            {
                EditorGUILayout.PropertyField(_sceneReference);
            }
            else
            {
                var oldScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneConfig.scenePath);
                
                EditorGUI.BeginChangeCheck();
                var newScene = EditorGUILayout.ObjectField("Build-In Scene", oldScene, typeof(SceneAsset), false) as SceneAsset;

                if (EditorGUI.EndChangeCheck())
                {
                    var newScenePath = AssetDatabase.GetAssetPath(newScene);
                    _scenePath.stringValue = newScenePath;
                }
            }
            
            EditorGUILayout.EndVertical();
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Scene Loading Properties", EditorStyles.largeLabel);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Set true in case you a want to load the scene in async mode.", EditorStyles.miniLabel);
            sceneConfig.asyncLoad = EditorGUILayout.Toggle("Async Load", sceneConfig.asyncLoad);
            
            EditorGUILayout.Space();
            sceneConfig.loadSceneMode = (LoadSceneMode)EditorGUILayout.EnumPopup("Loading Mode", sceneConfig.loadSceneMode);

            EditorGUILayout.EndVertical();
            
            serializedObject.ApplyModifiedProperties();
            
            EditorGUILayout.Space();

            if (GUILayout.Button("Save Config", GUILayout.ExpandWidth(true), GUILayout.Height(32))) {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
        }
    }
}