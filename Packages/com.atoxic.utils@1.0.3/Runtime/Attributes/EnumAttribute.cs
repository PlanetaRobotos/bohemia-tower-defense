using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class EnumAttribute : PropertyAttribute
{
    public readonly Type type;

    public EnumAttribute(Type type)
    {
        this.type = type;

        if (!type.IsEnum)
            throw new Exception("Supports only enums");
    }
}