using Constellation.SceneManagement.Config;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Constellation.SceneManagement.Editor.Editor.Config
{
    [CustomEditor(typeof(ScenesLibrary))]
    public class LibraryEditor : UnityEditor.Editor
    {
        private ReorderableList _reorderableList;

        private ScenesLibrary Library => target as ScenesLibrary;

        private string[] _keys;

        private void OnEnable()
        {
            if (Library == null) return;

            _reorderableList = new ReorderableList(Library.scenePairsList, typeof(SceneNamePair), true, true, true,
                true);

            // This could be used aswell, but I only advise this your class inherrits from UnityEngine.Object or has a CustomPropertyDrawer
            // Since you'll find your item using: serializedObject.FindProperty("list").GetArrayElementAtIndex(index).objectReferenceValue
            // which is a UnityEngine.Object
            // reorderableList = new ReorderableList(serializedObject, serializedObject.FindProperty("list"), true, true, true, true);

            // Add listeners to draw events
            _reorderableList.drawHeaderCallback += DrawHeader;
            _reorderableList.drawElementCallback += DrawElement;

            _reorderableList.onAddCallback += AddItem;
            _reorderableList.onRemoveCallback += RemoveItem;
        }

        private void OnDisable()
        {
            if (_reorderableList == null) return;

            // Make sure we don't get memory leaks etc.
            _reorderableList.drawHeaderCallback -= DrawHeader;
            _reorderableList.drawElementCallback -= DrawElement;

            _reorderableList.onAddCallback -= AddItem;
            _reorderableList.onRemoveCallback -= RemoveItem;
        }

        /// <summary>
        /// Draws the header of the list
        /// </summary>
        /// <param name="rect"></param>
        private void DrawHeader(Rect rect)
        {
            GUI.Label(rect, "Dependencies between sound name and IcarusClip", EditorStyles.boldLabel);
        }

        /// <summary>
        /// Draws one element of the list (ListItemExample)
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="index"></param>
        /// <param name="active"></param>
        /// <param name="focused"></param>
        private void DrawElement(Rect rect, int index, bool active, bool focused)
        {
            var item = Library.scenePairsList[index];

            EditorGUI.BeginChangeCheck();

            // item.key = EditorGUI.TextField(new Rect(rect.x, rect.y, rect.width * 0.5f, EditorGUIUtility.singleLineHeight), item.key);

            item.key = EditorGUI.Popup(
                new Rect(rect.x, rect.y, rect.width * 0.5f, EditorGUIUtility.singleLineHeight),
                item.key,
                _keys
            );

            item.sceneConfig = (SceneConfig)EditorGUI.ObjectField(
                new Rect(rect.x + rect.width * 0.5f + 8f, rect.y, rect.width * 0.5f - 8f,
                    EditorGUIUtility.singleLineHeight), item.sceneConfig, typeof(SceneConfig), false);

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
            }

            // If you are using a custom PropertyDrawer, this is probably better
            // EditorGUI.PropertyField(rect, serializedObject.FindProperty("list").GetArrayElementAtIndex(index));
            // Although it is probably smart to cach the list as a private variable ;)
        }

        private void AddItem(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoAddButton(list);
        }

        private void RemoveItem(ReorderableList list)
        {
            Library.scenePairsList.RemoveAt(list.index);

            EditorUtility.SetDirty(target);
        }

        public override void OnInspectorGUI()
        {
            var IcarusLibrary = (ScenesLibrary)target;

            if (IcarusLibrary.scenesLibraryConstants != null)
            {
                _keys = IcarusLibrary.scenesLibraryConstants.soundMapKeys.ToArray();
                ConvertIntValuesFromKeys(_keys);
            }

            base.OnInspectorGUI();

            if (_reorderableList == null) return;

            // Actually draw the list in the inspector
            _reorderableList.DoLayoutList();

            EditorGUILayout.Space();

            if (GUILayout.Button("Save Library", GUILayout.ExpandWidth(true), GUILayout.Height(32f)))
            {
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            }
        }

        private void ConvertIntValuesFromKeys(string[] input)
        {
            var options = new int[input.Length];

            for (int i = 0; i < options.Length; i++)
            {
                options[i] = i;
            }
        }
    }
}