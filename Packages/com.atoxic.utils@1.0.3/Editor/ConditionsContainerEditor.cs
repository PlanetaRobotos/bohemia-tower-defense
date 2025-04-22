using Conditions;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(ConditionsContainer), true)]
public class ConditionsContainerEditor : ReorderableListGenericEditor
{
    private ConditionsContainer _target;

    protected override void Init()
    {
        _target = (ConditionsContainer)target;

        base.Init();
    }

    protected override List<AbstractReorderableListGeneric> GetReorderableLists()
    {
        List<AbstractReorderableListGeneric> list = new List<AbstractReorderableListGeneric>()
        {
            new ReorderableListGeneric<BaseConditionSO>(serializedObject, _target.conditions, "conditions"),
        };

        return list;
    }
}