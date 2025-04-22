using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public sealed class ReorderableListGeneric<T> : AbstractReorderableListGeneric where T : ScriptableObject
{
    private const float LINE_MARGIN = 5f;

    private static T _bufferElement;
    private static List<T> _bufferElements;

    private SerializedObject _serializedObject;
    private List<T> _list;
    private string _listName;

    private GUIStyle _guiStyle;
    private GUIStyle _selectedGUIStyle;
    private List<bool> _visable;
    private int _selectedIndex = -1;
    private T _selected;
    private bool _isSelected;
    private bool _canPaste;
    private Rect _tempRect;
    private List<SerializedProperty> _serializedProperties = new List<SerializedProperty>();
    private List<SerializedObject> _serializedObjects = new List<SerializedObject>();
    private List<List<Rect>> _tempRects = new List<List<Rect>>();
    private int _reorderableListCount;

    public ReorderableList ReorderableList { get; private set; }

    public ReorderableListGeneric(SerializedObject serializedObject, List<T> list, string listName)
    {
        _serializedObject = serializedObject;
        _list = list;
        _listName = listName;
        _list.RemoveAll(x => x == null);

        OnInit();
    }

    private void OnInit()
    {
        ReorderableList = new ReorderableList(_serializedObject, _serializedObject.FindProperty(_listName), true, true, true, true);

        SubscribeOnCallbacks();

        _visable = new List<bool>();

        int actionsCount = _list.Count;

        for (int i = 0; i < actionsCount; i++)
            _visable.Add(false);

        _guiStyle = new GUIStyle();
        _guiStyle.fontStyle = FontStyle.Bold;

        if (!EditorGUIUtility.isProSkin)
            _guiStyle.normal.background = new Texture2D(1, 1);

        _selectedGUIStyle = new GUIStyle();
        _selectedGUIStyle.fontStyle = FontStyle.Bold;
        _selectedGUIStyle.normal.textColor = Color.green;

        if (_bufferElements == null)
            _bufferElements = new List<T>();

        CacheSerializedObjects();
    }

    private void CacheSerializedObjects()
    {
        _serializedProperties.Clear();
        _serializedObjects.Clear();
        _tempRects.Clear();

        for (int i = 0; i < ReorderableList.count; i++)
        {
            var property = ReorderableList.serializedProperty.GetArrayElementAtIndex(i);
            _serializedProperties.Add(property);
            SerializedObject serObject = new SerializedObject(property.objectReferenceValue);
            _serializedObjects.Add(serObject);
            _tempRects.Add(new List<Rect>());

            SerializedProperty prop = serObject.GetIterator();
            prop.NextVisible(true);

            while (prop.NextVisible(false))
            {
                _tempRects[i].Add(new Rect(0, 0, 0, EditorGUIUtility.singleLineHeight));
            }
        }

        _reorderableListCount = ReorderableList.count;
    }

    public override void DoLayoutList()
    {
        if (_reorderableListCount != ReorderableList.count)
        {
            _reorderableListCount = ReorderableList.count;
            CacheSerializedObjects();
        }
        ReorderableList.DoLayoutList();
    }

    public override void OnInspectorGUI()
    {
        CheckInputs();
    }

    private void SubscribeOnCallbacks()
    {
        ReorderableList.drawElementCallback = OnDrawElementCallback;
        ReorderableList.elementHeightCallback = OnElementHeightCallback;
        ReorderableList.drawHeaderCallback = OnDrawHeaderCallback;
        ReorderableList.onAddDropdownCallback = OnAddDropdownCallback;
        ReorderableList.onRemoveCallback = OnRemoveCallback;
        ReorderableList.onReorderCallback = OnReorderCallback;
        ReorderableList.onSelectCallback = OnSelectCallback;
    }

    private void OnDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
    {
        _tempRect = rect;
        SerializedProperty property = _serializedProperties[index];
        _tempRect.x += 20;
        EditorGUI.LabelField(_tempRect, property.objectReferenceValue.name.Replace("AX", string.Empty), GetLabelGUIStyle(property));
        _tempRect.x -= 20;
        _tempRect.width = 15;
        _tempRect.height = 15;
        bool toggle = EditorGUI.Toggle(_tempRect, _visable[index]);

        if (_visable[index] != toggle)
        {
            ReorderableList.index = index;
            if (_selectedIndex != index)
            {
                if (_selectedIndex >= 0)
                    _visable[_selectedIndex] = false;

                _selectedIndex = index;
                _visable[_selectedIndex] = true;
            }
            else
            {
                _visable[_selectedIndex] = !_visable[_selectedIndex];
            }
        }

        SerializedObject serObject = _serializedObjects[index];
        serObject.Update();
        SerializedProperty prop = serObject.GetIterator();
        prop.NextVisible(true);

        rect.y += EditorGUI.GetPropertyHeight(prop, GUIContent.none, prop.isExpanded) + LINE_MARGIN;
        var propertyCounter = 0;
        Rect tempRect;

        while (prop.NextVisible(false))
        {
            if (_visable[index])
            {
                tempRect = _tempRects[index][propertyCounter];
                tempRect.x = rect.x;
                tempRect.y = rect.y;
                tempRect.width = rect.width;

                EditorGUI.PropertyField(tempRect, prop, true);
                rect.y += EditorGUI.GetPropertyHeight(prop, GUIContent.none, prop.isExpanded) + LINE_MARGIN;
                propertyCounter++;
            }
        }

        serObject.ApplyModifiedProperties();
        serObject.UpdateIfRequiredOrScript();
    }

    private GUIStyle GetLabelGUIStyle(SerializedProperty property)
    {
        var labelStyle = _guiStyle;

        //if (property.objectReferenceValue is ActionX)
        //{
        //    if (Application.isPlaying)
        //    {
        //        if ((property.objectReferenceValue as ActionX).CurrentState == ActionX.State.Running)
        //            labelStyle = _selectedGUIStyle;
        //    }
        //    else
        //        (property.objectReferenceValue as ActionX).CurrentState = ActionX.State.Waiting;
        //}

        return labelStyle;
    }

    private float OnElementHeightCallback(int index)
    {
        if (ReorderableList.count <= 0)
            return 0f;

        SerializedProperty property = ReorderableList.serializedProperty.GetArrayElementAtIndex(index);
        return CalculatePropertyHeight(property, index);
    }

    private void OnDrawHeaderCallback(Rect rect)
    {
        EditorGUI.LabelField(rect, _listName);
    }

    private void OnAddDropdownCallback(Rect buttonRect, ReorderableList list)
    {
        var menu = new GenericMenu();
        List<Type> nodeTypes = new List<Type>();
        List<string> inheritedList = new List<string>();

        var targetType = typeof(T);
        var assemlies = AppDomain.CurrentDomain.GetAssemblies();
        var assemblyTypes = assemlies.First(x => x.GetName().Name == "Assembly-CSharp").GetTypes();
        var types = assemblyTypes.Where(x => targetType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

        inheritedList.AddRange(types.Select(x => x.Name));
        inheritedList.Sort();

        foreach (var typeName in inheritedList)
            menu.AddItem(new GUIContent(typeName[0].ToString() + "/" + typeName), false, OnClickHandler, typeName);

        menu.DropDown(buttonRect);
    }

    private void OnClickHandler(object target)
    {
        _visable.Add(false);
        CreateNewAction(target);
    }

    private void OnRemoveCallback(ReorderableList list)
    {
        int index = list.index;

        if (EditorUtility.DisplayDialog("Warning!", "Are you sure you want to delete the action? " + _list[index].name, "Yes", "No"))
        {
            _list.RemoveAt(index);
            UnityEngine.Object.DestroyImmediate(list.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue, true);
            _visable.RemoveAt(index);
            OnReorderCallback(list);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private void OnReorderCallback(ReorderableList list)
    {
        for (int i = 0; i < Mathf.Clamp(list.count, 0, _list.Count); i++)
            _list[i].name = _list[i].GetType().Name + i.ToString();
    }

    private void OnSelectCallback(ReorderableList list)
    {
        _isSelected = true;
        _selected = _list[list.index];
        _canPaste = _bufferElement != null && _bufferElement.GetType().Equals(_selected.GetType()) ? true : false;
    }

    private T CreateNewAction(object target, T scObjFrom = null)
    {
        var so = ScriptableObject.CreateInstance((string)target);
        T scObj = so as T;
        scObj.name = (string)target + _list.Count;
        scObj.hideFlags = HideFlags.HideInHierarchy;

        if (scObjFrom != null)
            scObj = CopyParams(scObjFrom, scObj);

        AssetDatabase.AddObjectToAsset(scObj, _serializedObject.targetObject);

        if (ReorderableList.index > 0)
            _list.Insert(ReorderableList.index + 1, scObj);
        else
            _list.Add(scObj);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        return scObj;
    }

    private T CopyParams(T source, T output)
    {
        if (output == null)
            output = (T)ScriptableObject.CreateInstance(source.GetType().Name);

        SerializedObject serObjSource = new SerializedObject(source);
        SerializedObject serObj = new SerializedObject(output);
        SerializedProperty prop = serObjSource.GetIterator();
        prop.NextVisible(true);

        while (prop.NextVisible(false))
            serObj.CopyFromSerializedProperty(prop);

        serObj.ApplyModifiedProperties();

        return output;
    }

    private float CalculatePropertyHeight(SerializedProperty property, int index)
    {
        if (property.objectReferenceValue != null)
        {
            if (!_visable[index])
                return EditorGUIUtility.singleLineHeight + LINE_MARGIN;

            SerializedObject objectSerialized = new SerializedObject(property.objectReferenceValue);
            SerializedProperty prop = objectSerialized.GetIterator();
            prop.NextVisible(true);

            float height = (prop.isExpanded ? EditorGUI.GetPropertyHeight(prop, GUIContent.none, prop.isExpanded) : EditorGUIUtility.singleLineHeight) + LINE_MARGIN;

            while (prop.NextVisible(false))
                height += EditorGUI.GetPropertyHeight(prop) + LINE_MARGIN;

            return height;
        }

        return 0f;
    }

    #region Tools
    private void CheckInputs()
    {
        Event e = Event.current;

        if (_canPaste && e.control && e.keyCode == KeyCode.V)
            OnPasteClick();

        if (_isSelected)
        {
            if (e.control && e.keyCode == KeyCode.C)
                OnCopyClick();
            if (e.control && e.keyCode == KeyCode.D)
                OnDuplicateClick();
            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Delete)
                OnDeleteClick();
        }

        if (e.type == EventType.ContextClick)
            ItemContextMenu(e);
    }

    private void OnCopyClick()
    {
        RefreshButParams();
        TryCopy(_selected, null);
    }

    private void OnPasteClick()
    {
        RefreshButParams();
        TryPaste(_bufferElement, _selected);
        _bufferElement = null;
        _selected = null;
    }

    private void OnDuplicateClick()
    {
        RefreshButParams();
        TryDuplicate(_selected);
        _bufferElement = null;
        _selected = null;
    }

    private void OnDeleteClick()
    {
        OnRemoveCallback(ReorderableList);
        RefreshButParams();
        _selected = null;
    }

    private void RefreshButParams()
    {
        _isSelected = false;
        _canPaste = false;
    }

    private void ItemContextMenu(Event e)
    {
        if (ReorderableList.index != -1 && _selected != null)
        {
            GenericMenu menu = new GenericMenu();
            string selectedElementName = _selected.name.Replace("AX", string.Empty);

            List<Type> nodeTypes = new List<Type>();
            List<string> inheritedList = new List<string>();

            var targetType = typeof(T);
            var assemlies = AppDomain.CurrentDomain.GetAssemblies();
            var assemblyTypes = assemlies.First(x => x.GetName().Name == "Assembly-CSharp").GetTypes();
            var types = assemblyTypes.Where(x => targetType.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);

            inheritedList.AddRange(types.Select(x => x.Name));
            inheritedList.Sort();

            foreach (var typeName in inheritedList)
                menu.AddItem(new GUIContent("Add" + "/" + typeName[0].ToString() + "/" + typeName), false, OnClickHandler, typeName);

            menu.AddItem(new GUIContent(string.Format("Copy {0}", selectedElementName)), false, OnCopyClick);

            if (_bufferElement)
            {
                if (_bufferElement.GetType() == _selected.GetType())
                    menu.AddItem(new GUIContent(string.Format("Paste {0}", selectedElementName)), false, OnPasteClick);

                menu.AddItem(new GUIContent(string.Format("Paste Above {0}", selectedElementName)), false, () => TryDuplicate(_bufferElement, _selectedIndex > 0 ? _selectedIndex - 1 : _selectedIndex));
                menu.AddItem(new GUIContent(string.Format("Paste Under {0}", selectedElementName)), false, () => TryDuplicate(_bufferElement, _selectedIndex + 1));
            }

            menu.AddItem(new GUIContent(string.Format("Delete {0}", selectedElementName)), false, OnDeleteClick);

            menu.AddSeparator(string.Empty);

            if (!_bufferElements.Contains(_selected))
                menu.AddItem(new GUIContent(string.Format("Add {0} To Buffer", selectedElementName)), false, () => _bufferElements.Add(_selected));

            if (_bufferElements.Count > 0)
            {
                menu.AddItem(new GUIContent(string.Format("{0}\t({1})", "Paste Elements from Buffer", _bufferElements.Count)), false, () =>
                {
                    EditorUtility.DisplayProgressBar("Paste Elements from Buffer", "Info", 1.0f);

                    for (int i = 0; i < _bufferElements.Count; i++)
                        TryDuplicate(_bufferElements[i], _selectedIndex + 1 + i);

                    EditorUtility.ClearProgressBar();
                });
                menu.AddItem(new GUIContent("Clear Buffer"), false, _bufferElements.Clear);
            }

            menu.ShowAsContext();
            e.Use();
        }
    }

    private void TryDuplicate(T scObjFrom)
    {
        _visable.Add(false);
        CreateNewAction(scObjFrom.GetType().Name, scObjFrom);
    }

    private void TryDuplicate(T scObjFrom, int index)
    {
        _visable.Add(false);
        CreateNewAction(scObjFrom.GetType().Name, index, scObjFrom);
    }

    private T CreateNewAction(object target, int index, T scObjFrom = null)
    {
        T scObj = (T)ScriptableObject.CreateInstance((string)target);
        scObj.name = (string)target + _list.Count;
        scObj.hideFlags = HideFlags.HideInHierarchy;
        if (scObjFrom != null)
            scObj = CopyParams(scObjFrom, scObj);
        AssetDatabase.AddObjectToAsset(scObj, _serializedObject.targetObject);
        _list.Insert(index, scObj);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return scObj;
    }

    private T TryCopy(T scObjFrom, T scObjTo = null)
    {
        if (scObjFrom != null)
        {
            _bufferElement = CopyParams(scObjFrom, scObjTo);
            return _bufferElement;
        }
        return null;
    }

    private void TryPaste(T scObjFrom, T scObjTo)
    {
        if (scObjFrom != null)
        {
            CopyParams(scObjFrom, scObjTo);
            EditorUtility.SetDirty(_serializedObject.targetObject);
        }
    }
    #endregion
}
