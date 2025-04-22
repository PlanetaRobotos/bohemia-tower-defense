using System;
using System.Reflection;
using System.Collections.Generic;

public static class ConstantsHelper
{
    public static List<KeyValuePair<string, int>> GetIntConstants(string className)
    {
        List<KeyValuePair<string, int>> constantList = new();

        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            Type type = assembly.GetType(className);

            if (type == null) continue;
            
            FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType == typeof(int) && field.IsLiteral && !field.IsInitOnly)
                {
                    string name = field.Name;
                    int value = (int)field.GetValue(null);

                    constantList.Add(new KeyValuePair<string, int>(name, value));
                }
            }
        }

        return constantList;
    }
}