using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Infrastructure.Utils.Editor
{
    internal sealed class ShortcutsEditor : EditorWindow
    {
        public Object[] Components = { };

        private readonly Rect _emptyRect = new(0, 0, 1, 1);

        private bool _readyToDrag;
        private Vector2 _scrollPos;

        private void OnGUI()
        {
            ScriptableObject target = this;
            var so = new SerializedObject(target);
            var objectsProperty = so.FindProperty("Components");
            var fields = new Dictionary<Rect, Object>();

            DropAreaGUI();

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            while (objectsProperty.NextVisible(true))
            {
                if (objectsProperty.propertyType == SerializedPropertyType.ArraySize)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("Size", GUILayout.Width(30));
                    EditorGUILayout.PropertyField(objectsProperty, GUIContent.none, true, GUILayout.MinWidth(20));
                    GUILayout.EndHorizontal();
                }

                if (objectsProperty.propertyType == SerializedPropertyType.ObjectReference)
                {
                    GUILayout.BeginHorizontal();

                    if (objectsProperty.objectReferenceValue != null)
                    {
                        EditorGUILayout.PropertyField(objectsProperty, GUIContent.none, true);
                    }
                    else
                    {
                        EditorGUILayout.PropertyField(objectsProperty, GUIContent.none, true, GUILayout.MaxWidth(24));
                    }

                    GUILayout.EndHorizontal();
                    var rect = GUILayoutUtility.GetLastRect();

                    if (rect != _emptyRect)
                    {
                        fields.Add(GUILayoutUtility.GetLastRect(), objectsProperty.objectReferenceValue);
                    }
                }
            }

            so.ApplyModifiedProperties();

            EditorGUILayout.EndScrollView();

            #region DragAndDrop

            var currentEvent = Event.current;

            if (currentEvent.button == 0 && currentEvent.type == EventType.Used)
            {
                _readyToDrag = true;
            }

            if (currentEvent.type == EventType.MouseUp)
            {
                _readyToDrag = false;
            }

            if (currentEvent.type == EventType.MouseDrag && _readyToDrag)
            {
                var mouseFields = fields.FirstOrDefault(r => r.Key.Contains(currentEvent.mousePosition));

                if (mouseFields.Key != Rect.zero && mouseFields.Value)
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new[] { mouseFields.Value, };

                    if (currentEvent.alt)
                    {
                        Components[fields.ToList().IndexOf(mouseFields)] = null;
                    }

                    DragAndDrop.StartDrag("Dragging");
                }

                _readyToDrag = false;
            }

            #endregion

            #region ContextMenu

            if (currentEvent.type == EventType.ContextClick)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("AddSelected"), false, AddSelected);
                menu.AddItem(new GUIContent("SelectAll"), false, SelectAll);
                menu.AddItem(new GUIContent("Clear"), false, Clear);
                menu.AddItem(new GUIContent("ClearContent"), false, ClearContent);
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("SetComponentsDirty"), false, SetComponentsDirty);
                menu.AddSeparator(string.Empty);
                menu.AddItem(new GUIContent("NewWindow"), false, NewLevelEditorWindow);

                menu.ShowAsContext();
                currentEvent.Use();
            }

            #endregion
        }

        public void DropAreaGUI()
        {
            var currentEvent = Event.current;
            var dropArea = new Rect(Vector2.zero, position.size);

            GUI.Box(dropArea, GUIContent.none, GUIStyle.none);

            switch (currentEvent.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!dropArea.Contains(currentEvent.mousePosition))
                    {
                        return;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (currentEvent.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        ArrayUtility.AddRange(ref Components, DragAndDrop.objectReferences);
                    }

                    break;
            }
        }

        #region Init

        [MenuItem("Tools/Shortcuts")]
        private static void LevelEditorWindow()
        {
            EditorWindow window = GetWindow<ShortcutsEditor>();
            window.titleContent = new GUIContent("Shortcuts");
            window.minSize = new Vector2(65, 30);
        }

        private void NewLevelEditorWindow()
        {
            EditorWindow window = CreateInstance<ShortcutsEditor>();
            window.titleContent = new GUIContent("Shortcuts");
            window.minSize = new Vector2(65, 30);
            window.Show();
        }

        #endregion

        #region Utilities

        private void AddSelected()
        {
            foreach (var obj in Selection.objects)
            {
                ArrayUtility.Add(ref Components, obj);
            }
        }

        private void SelectAll()
        {
            Selection.objects = Components;
        }

        private void Clear()
        {
            ArrayUtility.Clear(ref Components);
        }

        private void ClearContent()
        {
            Components = new Object[Components.Length];
        }

        private void SetComponentsDirty()
        {
            Components.ToList().ForEach(EditorUtility.SetDirty);
        }

        #endregion
    }
}
