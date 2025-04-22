using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class DropdownListAttribute : PropertyAttribute
{
    public readonly Type type;

    public DropdownListAttribute(Type type)
    {
        this.type = type;
    }
}