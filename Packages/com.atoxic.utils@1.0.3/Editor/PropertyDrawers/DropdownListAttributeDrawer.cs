using System.Collections.Generic;
using UnityEditor;

[CustomPropertyDrawer(typeof(DropdownListAttribute))]
public class DropdownListAttributeDrawer : DropdownPropertyDrawer<DropdownListAttribute>
{
    protected override List<string> GetList()
    {
        var targetType = TargetAttribute.type;
        var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

        _items.Add("None");

        for (int j = 0; j < assemblies.Length; j++)
        {
            var assemblyTypes = assemblies[j].GetTypes();

            for (int i = 0; i < assemblyTypes.Length; i++)
            {
                var type = assemblyTypes[i];

                if (targetType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    _items.Add(type.Name);
            }
        }

        return _items;
    }
}