using System.Collections.Generic;
using UnityEngine;

namespace Conditions
{
    [CreateAssetMenu(fileName = "ConditionsContainer", menuName = "Conditions/ConditionsContainer")]
    public class ConditionsContainer : ScriptableObject
    {
        [HideInInspector] public List<BaseConditionSO> conditions = new List<BaseConditionSO>();

        public bool IsAllConditionsPassed()
        {
            return conditions.TrueForAll(x => x.IsPassed());
        }

        public T GetCondition<T>() where T : BaseConditionSO
        {
            return conditions.Find(x => x is T) as T;
        }
    }
}