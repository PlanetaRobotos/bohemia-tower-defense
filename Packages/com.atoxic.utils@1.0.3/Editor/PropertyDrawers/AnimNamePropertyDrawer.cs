using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AnimNameAttribute))]
public class AnimNamePropertyDrawer : DropdownPropertyDrawer<AnimNameAttribute>
{
    private GameObject _targetObject;

    protected override void Init(SerializedProperty property)
    {
        _targetObject = (property.serializedObject.targetObject as Component).gameObject;

        base.Init(property);
    }

    protected override List<string> GetList()
    {
        _items.Add("None");

        var anims = AnimationUtility.GetAnimationClips(_targetObject);

        if (anims != null && anims.Length > 0)
        {
            for (int i = 0; i < anims.Length; i++)
            {
                _items.Add(anims[i].name);
            }
        }

        var objComponents = _targetObject.GetComponents<Component>();

        for (int i = 0; i < objComponents.Length; i++)
        {
            if (objComponents[i] is IAnimationComponentData animationData)
            {
                _items.AddRange(animationData.AvailableAnims);
            }
        }

        return _items;
    }
}