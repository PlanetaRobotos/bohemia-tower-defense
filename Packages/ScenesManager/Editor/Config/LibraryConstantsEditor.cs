using System.IO;
using System.Text;
using Constellation.SceneManagement.Config;
using UnityEditor;
using UnityEngine;

namespace Constellation.SceneManagement.Editor.Editor.Config
{
    [CustomEditor(typeof(ScenesLibraryConstants))]
    public class LibraryConstantsEditor : UnityEditor.Editor
    {
        private const string EntityType = "Scene";

        private string _enumClassName = "libraryConstants";

        private string _newConstantKey = string.Empty;

        private StringBuilder _stringBuilder;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var mapConstants = (ScenesLibraryConstants)target;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Export:", EditorStyles.largeLabel);

            _enumClassName = EditorGUILayout.TextField("Enum Class Name:", _enumClassName);

            EditorGUILayout.BeginHorizontal();

            if (!string.IsNullOrEmpty(mapConstants.enumsPath))
                EditorGUILayout.LabelField("Path", mapConstants.enumsPath, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Pick", GUILayout.Width(96)))
                mapConstants.enumsPath = EditorUtility.OpenFolderPanel("Pick The Folder", "", "");

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"Add New {EntityType} Keys:", EditorStyles.largeLabel);

            _newConstantKey = EditorGUILayout.TextField("New Key:", _newConstantKey).Replace(' ', '_').ToUpper();

            if (GUILayout.Button($"Add New {EntityType} Key", GUILayout.ExpandWidth(true), GUILayout.Height(32)))
                mapConstants.soundMapKeys.Add(_newConstantKey);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"List of {EntityType} Keys:", EditorStyles.largeLabel);

            if (mapConstants.soundMapKeys == null) return;

            for (var i = 0; i < mapConstants.soundMapKeys.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                mapConstants.soundMapKeys[i] = EditorGUILayout.TextField(mapConstants.soundMapKeys[i]);

                if (GUILayout.Button("Remove")) mapConstants.soundMapKeys.RemoveAt(i);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            if (GUILayout.Button("Save Config", GUILayout.ExpandWidth(true), GUILayout.Height(32)))
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Export to enum", GUILayout.ExpandWidth(true), GUILayout.Height(32)))
                ExportKeysToEnum(mapConstants);
        }

        private void ExportKeysToEnum(ScenesLibraryConstants libraryConstants)
        {
            _stringBuilder = new StringBuilder();

            _stringBuilder.Append($"public enum {_enumClassName} : byte\n");

            _stringBuilder.Append("{\n");

            for (var i = 0; i < libraryConstants.soundMapKeys.Count; i++)
            {
                string coma = i < libraryConstants.soundMapKeys.Count - 1 ? "," : string.Empty;
                _stringBuilder.Append($"\t{libraryConstants.soundMapKeys[i].ToUpper()} = {i}{coma}\n");
            }

            _stringBuilder.Append("}");

            var filename = $"{_enumClassName}.cs";
            File.WriteAllText(Path.Combine(libraryConstants.enumsPath, filename), _stringBuilder.ToString());

            AssetDatabase.Refresh();
        }
    }
}