using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class ReorderableListGenericEditor : Editor
{
    protected List<AbstractReorderableListGeneric> _reorderableLists = new List<AbstractReorderableListGeneric>();
    protected abstract List<AbstractReorderableListGeneric> GetReorderableLists();

    private void OnEnable()
    {
        Init();
    }

    protected virtual void Init()
    {
        _reorderableLists = GetReorderableLists();
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        serializedObject.Update();

        for (int i = 0; i < _reorderableLists.Count; i++)
        {
            GUILayout.Space(10);
            _reorderableLists[i].DoLayoutList();
            _reorderableLists[i].OnInspectorGUI();
        }

        serializedObject.ApplyModifiedProperties();
    }
}