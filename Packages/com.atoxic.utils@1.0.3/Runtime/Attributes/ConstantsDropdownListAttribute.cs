using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class ConstantsDropdownListAttribute : PropertyAttribute
{
    public readonly Type type;

    public ConstantsDropdownListAttribute(Type type)
    {
        this.type = type;
    }
}