using System;
using System.Collections.Generic;
using UnityEditor;

[CustomPropertyDrawer(typeof(EnumAttribute))]
public class EnumAttributeDrawer : DropdownPropertyDrawer<EnumAttribute>
{
    protected override List<string> GetList()
    {
        var enumValArray = Enum.GetValues(TargetAttribute.type);

        foreach (int val in enumValArray)
            _items.Add(((int)Enum.Parse(TargetAttribute.type, val.ToString())).ToString());

        return _items;
    }
}