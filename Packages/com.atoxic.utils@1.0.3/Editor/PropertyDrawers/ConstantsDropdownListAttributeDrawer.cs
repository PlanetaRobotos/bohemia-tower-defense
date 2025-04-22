using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ConstantsDropdownListAttribute))]
public class ConstantsDropdownListAttributeDrawer : DropdownPropertyDrawer<ConstantsDropdownListAttribute>
{
    protected override List<string> GetList()
    {
        Type constantsType = TargetAttribute.type;
        var intConstantList = ConstantsHelper.GetIntConstants(constantsType.Name);

        foreach ((string name, int value) in intConstantList)
        {
            Debug.Log($"{name}: {value}");
            _items.Add(name);
        }


        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        // _items.Add("None");
        
        /*for (int j = 0; j < assemblies.Length; j++)
        {
            var assemblyTypes = assemblies[j].GetTypes();

            for (int i = 0; i < assemblyTypes.Length; i++)
            {
                var type = assemblyTypes[i];

                if (targetType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    _items.Add(type.Name);
            }
        }*/

        return _items;
    }
}