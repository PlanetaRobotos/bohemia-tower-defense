using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public struct StringDrawerValuePair
{
    public string str;
    public SerializedProperty property;

    public StringDrawerValuePair(string val, SerializedProperty property)
    {
        str = val;
        this.property = property;
    }
}

public abstract class DropdownPropertyDrawer<T> : PropertyDrawer where T : PropertyAttribute
{
    protected T TargetAttribute { get { return (T)attribute; } }

    protected List<string> _items = new List<string>();

    private bool _init;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, "ERROR:", "May only apply to type string");
            return;
        }

        if (!_init)
        {
            _init = !_init;
            Init(property);
        }

        position = EditorGUI.PrefixLabel(position, label);

        if (GUI.Button(position, new GUIContent() { text = property.stringValue }, EditorStyles.popup))
        {
            var menu = new GenericMenu();

            _items.Clear();
            _items = GetList();

            for (int i = 0; i < _items.Count; i++)
            {
                var parsedNames = _items[i].Split('/');
                string name = parsedNames[parsedNames.Length - 1];
                menu.AddItem(new GUIContent(_items[i]), name == property.stringValue, HandleSelect, new StringDrawerValuePair(_items[i], property));
            }

            menu.ShowAsContext();
        }
    }

    protected virtual void Init(SerializedProperty property)
    {

    }

    protected abstract List<string> GetList();

    protected virtual void HandleSelect(object val)
    {
        var pair = (StringDrawerValuePair)val;
        var parsedNames = pair.str.Split('/');
        string name = parsedNames[parsedNames.Length - 1];

        pair.property.stringValue = name == "None" ? string.Empty : name;
        pair.property.serializedObject.ApplyModifiedProperties();
    }
}