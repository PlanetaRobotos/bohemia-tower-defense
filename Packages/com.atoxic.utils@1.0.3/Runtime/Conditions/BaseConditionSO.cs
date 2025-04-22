using UnityEngine;

namespace Conditions
{
    public abstract class BaseConditionSO : ScriptableObject, ICondition
    {
        public abstract bool IsPassed();

        public virtual float GetProgress()
        {
            return IsPassed() ? 1f : 0f;
        }
    }
}